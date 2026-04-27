# Path to the executable
$exePath = "C:\Proj\JewelleryManagementApp.WPF\bin\Debug\net10.0-windows\JewelleryManagementApp.WPF.exe"

if (Test-Path $exePath) {
    Write-Host "Unblocking: $exePath"
    Unblock-File -Path $exePath
    Write-Host "Successfully unblocked."
} else {
    Write-Error "File not found at: $exePath"
}
