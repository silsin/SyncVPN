$appFolder = "C:\Program Files\SyncVPN"
$appUninstallExe = $appFolder + "\unins000.exe"

if (Test-Path -Path $appFolder) {
    Start-Process -FilePath $appUninstallExe -ArgumentList "/verysilent" -Wait -ErrorAction Ignore

    # Sometimes the callout driver is stuck running
    net stop SyncVPNCallout 2>$null

    Remove-Item $appFolder -Recurse -ErrorAction Ignore
}

# If the uninstaller failed for any reason, clean the registry manually
$uninstallKey = "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\SyncVPN_is1"
if (Test-Path $uninstallKey) {
    Remove-Item $uninstallKey -Recurse -Force -ErrorAction Ignore
}
