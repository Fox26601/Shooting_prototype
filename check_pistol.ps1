Write-Host "🎬 Checking Pistol.fbx..." -ForegroundColor Green

$pistolPath = "Assets\Resources\Prefabs\Pistol.fbx"
if (Test-Path $pistolPath) {
    Write-Host "✅ Found Pistol.fbx" -ForegroundColor Green
    $fileInfo = Get-Item $pistolPath
    Write-Host "📁 File size: $($fileInfo.Length) bytes" -ForegroundColor Cyan
    Write-Host "📅 Last modified: $($fileInfo.LastWriteTime)" -ForegroundColor Cyan
} else {
    Write-Host "❌ Pistol.fbx not found" -ForegroundColor Red
}

Write-Host "🎬 Ready for animation extraction!" -ForegroundColor Green

