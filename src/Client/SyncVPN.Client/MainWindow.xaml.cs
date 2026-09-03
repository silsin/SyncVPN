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

using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Extensions;
using SyncVPN.Client.Core.Messages;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Services.Activation;
using SyncVPN.Client.UI.Main.Components;
using Windows.Foundation;
using Windows.Graphics;
using WinRT.Interop;
using static SyncVPN.Client.Common.Interop.WindowHelper;

namespace SyncVPN.Client;

public sealed partial class MainWindow : IFocusAware
{
    private const double TITLE_BAR_HEIGHT = 38.0;

    public MainWindowActivator WindowActivator { get; }
    public MainWindowOverlayActivator OverlayActivator { get; }
    private IEventMessageSender EventMessageSender { get; }
    private IUserAuthenticator UserAuthenticator { get; }

    private IntPtr _hWnd;
    private WindowProc? _newWndProc;
    private IntPtr _oldWndProc;

    public MainWindow()
    {
        WindowActivator = App.GetService<MainWindowActivator>();
        OverlayActivator = App.GetService<MainWindowOverlayActivator>();
        EventMessageSender = App.GetService<IEventMessageSender>();
        UserAuthenticator = App.GetService<IUserAuthenticator>();

        InitializeComponent();

        WindowActivator.Initialize(this);
        OverlayActivator.Initialize(this);
    }

    protected override void OnActivated(object sender, WindowActivatedEventArgs e)
    {
        base.OnActivated(sender, e);

        // Self-heals the min/max/resize caption buttons in case some earlier page
        // (e.g. NoServersPage) disabled them and the Home page never got a chance
        // to re-enable them afterwards.
        if (UserAuthenticator.IsLoggedIn)
        {
            InvalidateWindowResizeCapabilities(canResize: true);
        }

        if (_hWnd != IntPtr.Zero)
        {
            return;
        }

        _hWnd = WindowNative.GetWindowHandle(this);
        // It is important to have this reference as a class field, otherwise
        // garbage collector deletes the reference which causes the app to crash.
        _newWndProc = new(CustomWndProc);
        _oldWndProc = SetWindowLongPtr(_hWnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));

        // Force the native minimize/maximize caption buttons to be present.
        // Setting WindowEx.IsMaximizable/IsMinimizable alone isn't reliably
        // reflected in the actual window style bits, so set them directly too.
        EnsureMinimizeMaximizeWindowStyle();
    }

    private void EnsureMinimizeMaximizeWindowStyle()
    {
        long style = GetWindowLongPtr(_hWnd, GWL_STYLE).ToInt64();
        style |= WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU;
        SetWindowLongPtr(_hWnd, GWL_STYLE, new IntPtr(style));

        // WinUI3's AppWindow/OverlappedPresenter owns the actual caption-button
        // chrome and ignores the legacy GWL_STYLE bits above, so set it there too.
        if (AppWindow?.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsMinimizable = true;
            presenter.IsMaximizable = true;
            presenter.IsResizable = true;
        }
    }

    private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_ENDSESSION:
                if (wParam != IntPtr.Zero)
                {
                    OnSessionEnded();
                }
                break;
            case WM_NCLBUTTONDBLCLK:
                if (!IsMaximizable && wParam.ToInt32() == HTCAPTION)
                {
                    return IntPtr.Zero;
                }
                break;
        }

        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    private void OnSessionEnded()
    {
        EventMessageSender.Send<WindowsSessionEndingMessage>();
    }

    public void OnFocusChanged()
    {
        if (WindowContainer != null)
        {
            WindowContainer.TitleBarOpacity = this.GetTitleBarOpacity();
        }
    }

    public bool IsFocused()
    {
        return WindowActivator.IsWindowFocused;
    }

    public void InvalidateTitleBarVisibility(bool isTitleBarVisible)
    {
        if (WindowContainer != null)
        {
            WindowContainer.IsTitleBarVisible = isTitleBarVisible;
        }

        InvalidateWindowResizeCapabilities(isTitleBarVisible);

        InvalidateTitleDragArea();
    }

    public void InvalidateWindowResizeCapabilities(bool canResize)
    {
        bool isTitleBarVisible = WindowContainer?.IsTitleBarVisible ?? false;

        if (!isTitleBarVisible)
        {
            // When title bar is not visible, the window should not be resizable
            canResize = false;
        }

        IsMaximizable = canResize;
        IsMinimizable = canResize;
        IsResizable = canResize;

        if (AppWindow?.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsMinimizable = canResize;
            presenter.IsMaximizable = canResize;
            presenter.IsResizable = canResize;
        }
    }

    public void InvalidateTitleDragArea()
    {
        bool isTitleBarVisible = WindowContainer?.IsTitleBarVisible ?? false;

        if (isTitleBarVisible && TitleBarMenuComponent != null)
        {
            Point position = this.GetRelativePosition(TitleBarMenuComponent);
            Size size = TitleBarMenuComponent.RenderSize;

            RectInt32 interactiveArea = new(
                _X: (int)position.X,
                _Y: (int)position.Y,
                _Width: (int)size.Width,
                _Height: (int)size.Height);

            this.SetDragArea(Width, TITLE_BAR_HEIGHT, interactiveArea);
        }
        else
        {
            this.SetDragArea(Width, TITLE_BAR_HEIGHT);
        };
    }

    protected override bool OnSizeChanged(Size newSize)
    {
        InvalidateTitleDragArea();

        return base.OnSizeChanged(newSize);
    }

    private void OnTitleBarMenuComponentSizeChanged(object sender, SizeChangedEventArgs e)
    {
        InvalidateTitleDragArea();
    }
}