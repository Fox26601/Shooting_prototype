Write-Host "🎬 Creating animation folder..." -ForegroundColor Green

$animFolder = "Assets\Animations\Pistol"
if (!(Test-Path $animFolder)) {
    New-Item -ItemType Directory -Path $animFolder -Force
    Write-Host "✅ Created animation folder: $animFolder" -ForegroundColor Green
} else {
    Write-Host "✅ Animation folder already exists: $animFolder" -ForegroundColor Green
}

Write-Host "🎬 Animation folder ready!" -ForegroundColor Green

