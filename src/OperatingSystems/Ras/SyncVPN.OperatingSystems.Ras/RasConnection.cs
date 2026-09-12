/*
 * Copyright (c) 2026 Proton AG
 *
 * This file is part of SyncVPN.
 *
 * SyncVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * SyncVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with SyncVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.ConnectLogs;
using SyncVPN.Logging.Contracts.Events.DisconnectLogs;
using SyncVPN.Logging.Contracts.Events.NetworkLogs;
using SyncVPN.OperatingSystems.Ras.Contracts;

namespace SyncVPN.OperatingSystems.Ras;

// See NativeMethods for the caveat that this whole engine is unverified against a real Windows SDK
// and a real RAS server - this class is the orchestration layer on top of that P/Invoke surface.
public class RasConnection : IRasConnection
{
    private readonly ILogger _logger;

    // Kept as a field, not a local, so the delegate isn't eligible for GC while RAS still holds an
    // unmanaged pointer to it between Connect() and the connection ending - a local would risk the
    // native callback firing into a collected delegate.
    private readonly NativeMethods.RasDialFunc2 _dialCallback;

    private string? _entryName;
    private nint _rasConnectionHandle;
    private bool _disposed;

    public RasConnection(ILogger logger)
    {
        _logger = logger;
        _dialCallback = OnDialCallback;
    }

    public event EventHandler<RasStateChangedEventArgs>? StateChanged;

    public void Connect(RasEntryOptions options)
    {
        string phonebook = RasPhonebookPathProvider.GetPath();
        _entryName = options.EntryName;

        NativeMethods.RASENTRY entry = NativeMethods.RASENTRY.CreateDefault();
        entry.dwVpnStrategy = options.DeviceType == RasDeviceType.L2tp
            ? NativeMethods.VS_L2tpOnly
            : NativeMethods.VS_SstpOnly;

        uint entryResult = NativeMethods.RasSetEntryProperties(phonebook, options.EntryName, ref entry, entry.dwSize, 0, 0);
        if (entryResult != 0)
        {
            RaiseError(entryResult, "Failed to create the RAS phonebook entry.");
            return;
        }

        if (options.DeviceType == RasDeviceType.L2tp)
        {
            if (string.IsNullOrEmpty(options.PreSharedKey))
            {
                RaiseError(0, "L2TP requires a pre-shared key.");
                return;
            }

            NativeMethods.RASCREDENTIALS pskCredentials = NativeMethods.RASCREDENTIALS.Create(
                NativeMethods.RASCM_PreSharedKey, string.Empty, options.PreSharedKey);
            uint pskResult = NativeMethods.RasSetCredentials(phonebook, options.EntryName, ref pskCredentials, fClearCredentials: false);
            if (pskResult != 0)
            {
                RaiseError(pskResult, "Failed to set the L2TP pre-shared key.");
                return;
            }
        }

        NativeMethods.RASCREDENTIALS userCredentials = NativeMethods.RASCREDENTIALS.Create(
            NativeMethods.RASCM_UserName | NativeMethods.RASCM_Password, options.Username, options.Password);
        uint credentialsResult = NativeMethods.RasSetCredentials(phonebook, options.EntryName, ref userCredentials, fClearCredentials: false);
        if (credentialsResult != 0)
        {
            RaiseError(credentialsResult, "Failed to set RAS username/password credentials.");
            return;
        }

        NativeMethods.RASDIALPARAMS dialParams = NativeMethods.RASDIALPARAMS.Create(options.EntryName, options.Username, options.Password);

        _logger.Info<ConnectLog>($"Dialing RAS entry '{options.EntryName}' ({options.DeviceType}) to {options.Server}.");

        uint dialResult = NativeMethods.RasDial(
            0, phonebook, ref dialParams, NativeMethods.RASDIALEVENT_RasDialFunc2, _dialCallback, out nint connectionHandle);

        if (dialResult != 0)
        {
            RaiseError(dialResult, "RasDial failed to start.");
            return;
        }

        _rasConnectionHandle = connectionHandle;
        StateChanged?.Invoke(this, new RasStateChangedEventArgs(RasConnectionState.Connecting));
    }

    public void Disconnect()
    {
        if (_rasConnectionHandle != 0)
        {
            NativeMethods.RasHangUp(_rasConnectionHandle);
            _rasConnectionHandle = 0;
        }

        CleanUpEntry();
    }

    public string? GetLocalIpAddress()
    {
        if (_rasConnectionHandle == 0)
        {
            return null;
        }

        NativeMethods.RASPPPIP projection = new() { dwSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.RASPPPIP>() };
        uint size = projection.dwSize;

        uint result = NativeMethods.RasGetProjectionInfo(_rasConnectionHandle, NativeMethods.RASP_PppIp, ref projection, ref size);
        if (result != 0 || string.IsNullOrEmpty(projection.szIpAddress))
        {
            _logger.Warn<NetworkLog>($"Failed to read the RAS PPP IP projection (error {result}).");
            return null;
        }

        return projection.szIpAddress;
    }

    private void OnDialCallback(uint dwCallbackId, uint dwSubEntry, nint hrasconn, uint unMsg, uint rascs, uint dwError, uint dwExtendedError)
    {
        if (dwError != 0)
        {
            RaiseError(dwError, $"RAS dial callback reported error {dwError} (extended {dwExtendedError}) at state {rascs:X}.");
            return;
        }

        if (rascs == NativeMethods.RASCS_Connected)
        {
            _logger.Info<ConnectLog>("RAS connection established.");
            StateChanged?.Invoke(this, new RasStateChangedEventArgs(RasConnectionState.Connected));
        }
        else if (rascs == NativeMethods.RASCS_Disconnected)
        {
            _logger.Info<DisconnectLog>("RAS connection disconnected.");
            CleanUpEntry();
            StateChanged?.Invoke(this, new RasStateChangedEventArgs(RasConnectionState.Disconnected));
        }
    }

    private void RaiseError(uint errorCode, string message)
    {
        _logger.Error<NetworkLog>($"{message} (RAS error {errorCode}).");
        CleanUpEntry();
        StateChanged?.Invoke(this, new RasStateChangedEventArgs(RasConnectionState.Disconnected, errorCode, message));
    }

    private void CleanUpEntry()
    {
        if (_entryName is null)
        {
            return;
        }

        NativeMethods.RasDeleteEntry(RasPhonebookPathProvider.GetPath(), _entryName);
        _entryName = null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Disconnect();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
