# PowerShell script to extract animations from Pistol.fbx
Write-Host "🎬 Starting animation extraction from Pistol.fbx..." -ForegroundColor Green

# Check if Unity is running
$unityProcess = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($unityProcess) {
    Write-Host "✅ Unity is running" -ForegroundColor Green
} else {
    Write-Host "⚠️ Unity is not running - animations will be extracted when Unity starts" -ForegroundColor Yellow
}

# Check if Pistol.fbx exists
$pistolPath = "Assets\Resources\Prefabs\Pistol.fbx"
if (Test-Path $pistolPath) {
    Write-Host "✅ Found Pistol.fbx at: $pistolPath" -ForegroundColor Green
} else {
    Write-Host "❌ Pistol.fbx not found at: $pistolPath" -ForegroundColor Red
    exit 1
}

# Create animations folder
$animFolder = "Assets\Animations\Pistol"
if (!(Test-Path $animFolder)) {
    New-Item -ItemType Directory -Path $animFolder -Force
    Write-Host "✅ Created animation folder: $animFolder" -ForegroundColor Green
} else {
    Write-Host "✅ Animation folder already exists: $animFolder" -ForegroundColor Green
}

Write-Host "🎬 Animation extraction setup complete!" -ForegroundColor Green
Write-Host "📝 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Open Unity Editor" -ForegroundColor White
Write-Host "   2. Run Tools menu" -ForegroundColor White
Write-Host "   3. Use automatic extraction in GameManager" -ForegroundColor White
