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
using System.Runtime.InteropServices;

namespace SyncVPN.OperatingSystems.Ras;

// P/Invoke surface for rasapi32.dll (Windows Remote Access Service), covering just enough to create
// an ephemeral L2TP/IPsec-PSK or SSTP VPN phonebook entry, dial it, read back its local IP, and tear
// it down.
//
// UNVERIFIED AGAINST THE WINDOWS SDK: this was written from documented Win32 RAS API shapes without
// access to a machine that could compile/run it against a real ras.h and a real RAS server. Struct
// layouts for RASENTRYW/RASDIALPARAMSW/RASCREDENTIALSW/RASPPPIP follow the long-stable "classic"
// field set (unchanged since Windows XP) rather than the newest SDK version, specifically to avoid
// guessing at the exact position of fields added in later Windows releases - dwSize tells RAS which
// struct version was passed, and this smaller/older size is a documented, still-supported form. The
// specific values flagged "VERIFY" below are the ones with the least certain provenance and should
// be checked against a real ras.h (or https://github.com/dahall/Vanara's Rasapi32 P/Invoke, which
// wasn't available as a NuGet package to reference directly here) before this is trusted in
// production - a wrong struct layout here can access-violate the hosting Windows service process.
internal static class NativeMethods
{
    private const string RASAPI32 = "rasapi32.dll";

    private const int RAS_MaxEntryName = 256;
    private const int RAS_MaxDeviceType = 16;
    private const int RAS_MaxDeviceName = 128;
    private const int RAS_MaxPhoneNumber = 128;
    private const int RAS_MaxCallbackNumber = RAS_MaxPhoneNumber;
    private const int RAS_MaxAreaCode = 10;
    private const int RAS_MaxPadType = 32;
    private const int RAS_MaxX25Address = 200;
    private const int RAS_MaxFacilities = 200;
    private const int RAS_MaxUserData = 200;
    private const int RAS_MaxIpAddress = 15;
    private const int UNLEN = 256;
    private const int PWLEN = 256;
    private const int DNLEN = 15;
    private const int MAX_PATH = 260;

    // RASENTRY.dwType
    public const uint RASET_Vpn = 3;

    // RASENTRY.dwEncryptionType
    public const uint ET_Optional = 0;

    // RASENTRY.dwfNetProtocols
    public const uint RASNP_Ip = 0x00000001;

    // RASENTRY.dwFramingProtocol
    public const uint RASFP_Ppp = 0x00000001;

    // RASENTRY.dwfOptions - route all traffic through the VPN ("use default gateway on remote network").
    public const uint RASEO_RemoteDefaultGateway = 0x00000001;

    // RASENTRY.dwVpnStrategy. VERIFY: PPTP/L2TP values are widely and consistently documented; the
    // SSTP value is recalled with materially less confidence and must be checked against ras.h.
    public const uint VS_L2tpOnly = 3;
    public const uint VS_SstpOnly = 6;

    // Device name RAS expects for a VPN entry - the built-in "WAN Miniport" device selects the
    // strategy given in dwVpnStrategy rather than a specific protocol.
    public const string RASDT_Vpn = "vpn";
    public const string DeviceName = "WAN Miniport (IKEv2)"; // VERIFY: device name string is best-effort; RAS may resolve devices by dwVpnStrategy alone for VPN entries and ignore this.

    // RASCREDENTIALS.dwMask. VERIFY: RASCM_PreSharedKey's bit value is recalled with moderate
    // confidence from third-party samples, not a primary source.
    public const uint RASCM_UserName = 0x1;
    public const uint RASCM_Password = 0x2;
    public const uint RASCM_PreSharedKey = 0x8;

    // RasDial dwNotifierType - selects the RasDialFunc2 callback shape (dwError/dwExtendedError
    // included). VERIFY: recalled with moderate confidence; some references use 0xFFFFFFFF for the
    // older 3-argument RasDialFunc instead.
    public const uint RASDIALEVENT_RasDialFunc2 = 2;

    // RASCS_* connection-state values reported to the dial callback. The two terminal values below
    // are stable/high-confidence; RASCS_Connected in particular is used ubiquitously.
    public const uint RASCS_Connected = 0x2000;
    public const uint RASCS_Disconnected = 0x2001;

    // RASP_PppIp - RasGetProjectionInfo info-type requesting PPP IP projection results.
    public const uint RASP_PppIp = 0x2000001;

    [StructLayout(LayoutKind.Sequential)]
    public struct RASIPADDR
    {
        public byte a;
        public byte b;
        public byte c;
        public byte d;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RASENTRY
    {
        public uint dwSize;
        public uint dwfOptions;
        public uint dwCountryID;
        public uint dwCountryCode;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxAreaCode + 1)]
        public string szAreaCode;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxPhoneNumber + 1)]
        public string szLocalPhoneNumber;

        public uint dwAlternateOffset;
        public RASIPADDR ipaddr;
        public RASIPADDR ipaddrDns;
        public RASIPADDR ipaddrDnsAlt;
        public RASIPADDR ipaddrWins;
        public RASIPADDR ipaddrWinsAlt;
        public uint dwFrameSize;
        public uint dwfNetProtocols;
        public uint dwFramingProtocol;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        public string szScript;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        public string szAutodialDll;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        public string szAutodialFunc;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxDeviceType + 1)]
        public string szDeviceType;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxDeviceName + 1)]
        public string szDeviceName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxPadType + 1)]
        public string szX25PadType;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxX25Address + 1)]
        public string szX25Address;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxFacilities + 1)]
        public string szX25Facilities;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxUserData + 1)]
        public string szX25UserData;

        public uint dwChannels;
        public uint dwReserved1;
        public uint dwReserved2;
        public uint dwSubEntries;
        public uint dwDialMode;
        public uint dwDialExtraPercent;
        public uint dwDialExtraSampleSeconds;
        public uint dwHangUpExtraPercent;
        public uint dwHangUpExtraSampleSeconds;
        public uint dwIdleDisconnectSeconds;
        public uint dwType;
        public uint dwEncryptionType;
        public uint dwCustomAuthKey;
        public Guid guidId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        public string szCustomDialDll;

        public uint dwVpnStrategy;

        public static RASENTRY CreateDefault()
        {
            return new RASENTRY
            {
                dwSize = (uint)Marshal.SizeOf<RASENTRY>(),
                szAreaCode = string.Empty,
                szLocalPhoneNumber = string.Empty,
                szScript = string.Empty,
                szAutodialDll = string.Empty,
                szAutodialFunc = string.Empty,
                szDeviceType = RASDT_Vpn,
                szDeviceName = DeviceName,
                szX25PadType = string.Empty,
                szX25Address = string.Empty,
                szX25Facilities = string.Empty,
                szX25UserData = string.Empty,
                szCustomDialDll = string.Empty,
                dwType = RASET_Vpn,
                dwEncryptionType = ET_Optional,
                dwfNetProtocols = RASNP_Ip,
                dwFramingProtocol = RASFP_Ppp,
                dwfOptions = RASEO_RemoteDefaultGateway,
                dwSubEntries = 1,
                dwDialMode = 1, // RASEDM_DialAll
                guidId = Guid.Empty,
            };
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RASDIALPARAMS
    {
        public uint dwSize;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxEntryName + 1)]
        public string szEntryName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxPhoneNumber + 1)]
        public string szPhoneNumber;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxCallbackNumber + 1)]
        public string szCallbackNumber;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = UNLEN + 1)]
        public string szUserName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PWLEN + 1)]
        public string szPassword;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DNLEN + 1)]
        public string szDomain;

        public uint dwSubEntry;
        public uint dwCallbackId;

        public static RASDIALPARAMS Create(string entryName, string username, string password)
        {
            return new RASDIALPARAMS
            {
                dwSize = (uint)Marshal.SizeOf<RASDIALPARAMS>(),
                szEntryName = entryName,
                szPhoneNumber = string.Empty,
                szCallbackNumber = string.Empty,
                szUserName = username,
                szPassword = password,
                szDomain = string.Empty,
                dwSubEntry = 1,
                dwCallbackId = 0,
            };
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RASCREDENTIALS
    {
        public uint dwSize;
        public uint dwMask;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = UNLEN + 1)]
        public string szUserName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PWLEN + 1)]
        public string szPassword;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DNLEN + 1)]
        public string szDomain;

        public static RASCREDENTIALS Create(uint mask, string userName, string password)
        {
            return new RASCREDENTIALS
            {
                dwSize = (uint)Marshal.SizeOf<RASCREDENTIALS>(),
                dwMask = mask,
                szUserName = userName,
                szPassword = password,
                szDomain = string.Empty,
            };
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RASPPPIP
    {
        public uint dwSize;
        public uint dwError;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxIpAddress + 1)]
        public string szIpAddress;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxIpAddress + 1)]
        public string szServerIpAddress;

        public uint dwOptions;
        public uint dwServerOptions;
    }

    // RASDIALFUNC2 - fired repeatedly on a RAS-owned thread as a dial attempt progresses.
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void RasDialFunc2(
        uint dwCallbackId,
        uint dwSubEntry,
        nint hrasconn,
        uint unMsg,
        uint rascs,
        uint dwError,
        uint dwExtendedError);

    [DllImport(RASAPI32, EntryPoint = "RasSetEntryPropertiesW", CharSet = CharSet.Unicode)]
    public static extern uint RasSetEntryProperties(
        string? lpszPhonebook,
        string lpszEntry,
        ref RASENTRY lpbEntry,
        uint dwEntryInfoSize,
        nint lpbDeviceInfo,
        uint dwDeviceInfoSize);

    [DllImport(RASAPI32, EntryPoint = "RasSetCredentialsW", CharSet = CharSet.Unicode)]
    public static extern uint RasSetCredentials(
        string? lpszPhonebook,
        string lpszEntry,
        ref RASCREDENTIALS lpCredentials,
        [MarshalAs(UnmanagedType.Bool)] bool fClearCredentials);

    [DllImport(RASAPI32, EntryPoint = "RasDialW", CharSet = CharSet.Unicode)]
    public static extern uint RasDial(
        nint lpRasDialExtensions,
        string? lpszPhonebook,
        ref RASDIALPARAMS lpRasDialParams,
        uint dwNotifierType,
        RasDialFunc2 lpvNotifier,
        out nint lphRasConn);

    [DllImport(RASAPI32, EntryPoint = "RasHangUpW")]
    public static extern uint RasHangUp(nint hrasconn);

    [DllImport(RASAPI32, EntryPoint = "RasDeleteEntryW", CharSet = CharSet.Unicode)]
    public static extern uint RasDeleteEntry(string? lpszPhonebook, string lpszEntry);

    [DllImport(RASAPI32, EntryPoint = "RasGetProjectionInfoW")]
    public static extern uint RasGetProjectionInfo(nint hrasconn, uint rasprojection, ref RASPPPIP lpprojection, ref uint lpcb);

    [DllImport(RASAPI32, EntryPoint = "RasGetErrorStringW", CharSet = CharSet.Unicode)]
    public static extern uint RasGetErrorString(uint uErrorValue, System.Text.StringBuilder lpszErrorString, uint cBufSize);
}
