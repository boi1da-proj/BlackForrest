param(
  [string]$ZipPath = "shadow.compute.zip",
  [string]$InstallDir = "C:\\ShadowCompute"
)

Write-Host "Installing ShadowCompute to $InstallDir from $ZipPath"
if (-Not (Test-Path $ZipPath)) { throw "Zip not found: $ZipPath" }
New-Item -ItemType Directory -Path $InstallDir -ErrorAction SilentlyContinue | Out-Null
Expand-Archive -Path $ZipPath -DestinationPath $InstallDir -Force

# Optional: setup venv and install requirements
$req = Join-Path $InstallDir 'softlyplease\requirements.txt'
if (Test-Path $req) {
  Write-Host "Installing Python requirements..."
  pip install -r $req
}

Write-Host "Done. To run API:"
Write-Host "  cd $InstallDir\softlyplease"
Write-Host "  uvicorn api.server:app --host 0.0.0.0 --port 8000"

