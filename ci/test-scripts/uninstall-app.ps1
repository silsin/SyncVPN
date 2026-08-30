$protonFolder = "C:\Program Files\Proton\VPN"
$protonUninstallExe = $protonFolder + "\unins000.exe"

if (Test-Path -Path $protonFolder) {
    Start-Process -FilePath $protonUninstallExe -ArgumentList "/verysilent" -Wait -ErrorAction Ignore

    # Sometimes the callout driver is stuck running
    net stop SyncVPNCallout 2>$null

    Remove-Item $protonFolder -Recurse -ErrorAction Ignore
}

# If the uninstaller failed for any reason, clean the registry manually
$uninstallKey = "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\SyncVPN_is1"
if (Test-Path $uninstallKey) {
    Remove-Item $uninstallKey -Recurse -Force -ErrorAction Ignore
}
