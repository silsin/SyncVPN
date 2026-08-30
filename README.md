# SyncVPN Windows app

Copyright (c) 2023 Proton AG

This repository holds the SyncVPN Windows app.
For a detailed build information see [BUILD](BUILD.md).
For licensing information see [COPYING](COPYING.md).
For contribution policy see [CONTRIBUTING](CONTRIBUTING.md).

## Description

The [SyncVPN](https://protonvpn.com) Windows app is intended for every SyncVPN service user,
paid or free and supports all functionalities available to authenticated users (user signup instead happens on the web site).

You can download the latest stable release, either on [SyncVPN official website](https://protonvpn.com/download) or directly on the [official GitHub repository](https://github.com/SyncVPN/win-app/releases/latest).

### The application

The app consists of these interacting parts:
- SyncVPN GUI application
- SyncVPN Service
- OpenVPN
- TAP adapter
- Split Tunnel driver

#### GUI application

The SyncVPN GUI app is installed into "C:\Program Files\Proton\VPN\<version>" directory by default. 
The main executable is "SyncVPN.exe".

SyncVPN GUI app starts SyncVPN Service when launched and stops the service
when closed.

App logs are saved to "%LOCALAPPDATA%\SyncVPN\Logs" directory.

The SyncVPN build using Debug configuration optionally loads its configuration from file
"SyncVPN.config" in the app directory. This file is not deployed during install. If the configuration
file doesn't exist or contains not valid values the app tries to save default configuration
used in the app.

To monitor Http traffic of SyncVPN GUI app using Fiddler or another tool, you might need to disable
TLS certificate pinning. To disable TLS certificate pinning the configuration file with empty
"TlsPinningConfig" value should be provided:
```
    ...
    "TlsPinningConfig": {}
    ...
```

#### SyncVPN Service

The Windows service "SyncVPN Service" is installed into
"C:\Program Files\Proton\VPN\<version>" directory by default. Service
executable is "SyncVPNService.exe". The service is started and stopped by the SyncVPN
GUI app.

During installation, the service is configured to be started and stopped by the unprivileged
interactive users.

Service executable supports installation and uninstallation of service. Passing "install" on
command line to "SyncVPNService.exe" installs the service, passing "uninstall" - uninstalls.
This installation method doesn't configure service security settings.

Service is responsible for interaction with OpenVPN, managing Windows firewall and Split Tunnel
driver.

Service logs are saved to "%ALLUSERSPROFILE%\SyncVPN\Logs" directory.

#### OpenVPN

The SyncVPN uses OpenVPN for maintaining a VPN tunnel. The new OpenVPN process is started on each
connect to a VPN and closed on disconnect. Communication with the OpenVPN process is maintained through
TCP management interface.

OpenVPN is installed into "C:\Program Files\Proton\VPN\<version>\Resources\"
directory by default. The OpenVPN config file is static, it doesn't change for each VPN server.

The OpenVPN is built from official source by applying a patch to support SyncVPN specific
TAP adapter. See [win-openvpn](https://github.com/SyncVPN/win-openvpn) repository.

#### TAP adapter

TAP adapter "TAP-SyncVPN Windows Adapter V9" is used by the OpenVPN.

The TAP adapter is built from official source by applying a patch to have SyncVPN specific
name and identification. See [win-tap-adapter](https://github.com/SyncVPN/win-tap-adapter) repository.

#### Callout driver

The kernel-mode driver "SyncVPN Callout Driver" is used for redirecting socket bindings when
Split Tunnel is enabled and preventing DNS leak by sending SERVFAIL response packet for DNS
requests which were made from other interfaces than SyncVPN uses.

The driver is installed as a system service. It is started when connecting to VPN and stopped
when disconnecting by SyncVPN Service.

## Folder structure

The main repository folder contains the .NET Visual Studio solution of the
SyncVPN Windows app named SyncVPN.

### Folder "ci"

Contains continuous integration scripts.

### Folder "packages"

It contains NuGet packages of the SyncVPN solution.

### Folder "Setup"

This folder contains Advanced Installer setup project files, resources included in the installer,
and built installer files. Subfolders contain:

- "Images" - images for inclusion into the installer.
- "Installers" - built SyncVPN installer files.
- "SyncVPNTap-SetupFiles" - built TAP adapter installer files. The latest successfully
  built TAP adapter installer file is required to build the SyncVPN installer.
- "SplitTunnel" - SplitTunnel Callout driver for inclusion into the installer.

### Folder "src"

This folder contains Visual Studio solution projects.

### Folder "src\bin"

This folder contains Visual Studio project build output. This folder can be safely
deleted as it's content is recreated by building the solution.

### Folder "src\srp"

This folder contains GIT submodule of [ProtonMail SRP library](https://github.com/ProtonMail/go-srp).

### Folder "test"

This folder contains test projects of the SyncVPN solution.

## Solution

SyncVPN Windows app is created using C# and C++ programming languages, WPF and MVVM
technologies. The Visual Studio solution consists of a series of projects:
- **SyncVPN.App** - the main project which builds to SyncVPN GUI app executable.
  It contains startup logic and GUI (view models and views).
- **SyncVPN.CalloutDriver** - the callout driver written in C++ used for split tunneling and DNS leak protection.
- **SyncVPN.Common** - the classes shared between projects.
- **SyncVPN.Core** - the business logic of the application.
- **SyncVPN.ErrorMessage** - displays an error message when the application cannot be run. Builds to an executable.
- **SyncVPN.InstallActions** - the C++ actions used by the app installer.
- **SyncVPN.IpFilter** - the C++ library for configuring Windows firewall filters.
- **SyncVPN.Native** - the C# wrapper around Windows system libraries.
- **SyncVPN.NetworkFilter** - the C# wrapper around C++ library for configuring Windows firewall.
- **SyncVPN.NetworkUtil** - the C++ library for changing network configuration.
- **SyncVPN.Resource** - contains resources shared between projects.
- **SyncVPN.Service** - the Windows service which handles VPN, Windows firewall and Split Tunneling.
- **SyncVPN.Service.Contract** - contains the service contract.
- **SyncVPN.TapInstaller** - the TAP install action used in the app installer.
- **SyncVPN.TlsVerify** - the command line utility which verifies the VPN server certificate.
- **SyncVPN.Update** - the application update module used in the update service.
- **SyncVPN.UpdateService** - the Windows service which handles the app updates.
- **SyncVPN.UpdateServiceContract** - contains the update service contract.
- **SyncVPN.Vpn** - the OpenVPN management module used in the service.

Solution folder "Test" contains test projects.