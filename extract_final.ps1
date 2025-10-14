Write-Host "Starting animation extraction..." -ForegroundColor Green

# Check Pistol.fbx
$pistolPath = "Assets\Resources\Prefabs\Pistol.fbx"
if (Test-Path $pistolPath) {
    Write-Host "Found Pistol.fbx" -ForegroundColor Green
} else {
    Write-Host "Pistol.fbx not found" -ForegroundColor Red
    exit 1
}

# Create animations folder
$animFolder = "Assets\Animations\Pistol"
if (!(Test-Path $animFolder)) {
    New-Item -ItemType Directory -Path $animFolder -Force
    Write-Host "Created animation folder" -ForegroundColor Green
} else {
    Write-Host "Animation folder exists" -ForegroundColor Green
}

Write-Host "Setup complete!" -ForegroundColor Green
Write-Host "Now run Unity and use Tools menu to extract animations" -ForegroundColor Cyan

