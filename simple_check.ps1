Write-Host "Checking Unity process..." -ForegroundColor Green

$unityProcess = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($unityProcess) {
    Write-Host "Unity is running" -ForegroundColor Green
} else {
    Write-Host "Unity is not running" -ForegroundColor Yellow
}

Write-Host "Check complete!" -ForegroundColor Green

