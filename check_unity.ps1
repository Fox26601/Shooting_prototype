Write-Host "🎬 Checking Unity process..." -ForegroundColor Green

$unityProcess = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($unityProcess) {
    Write-Host "✅ Unity is running (PID: $($unityProcess.Id))" -ForegroundColor Green
    Write-Host "📁 Unity path: $($unityProcess.Path)" -ForegroundColor Cyan
} else {
    Write-Host "⚠️ Unity is not running" -ForegroundColor Yellow
    Write-Host "💡 Start Unity Editor to extract animations" -ForegroundColor Cyan
}

Write-Host "🎬 Unity check complete!" -ForegroundColor Green

