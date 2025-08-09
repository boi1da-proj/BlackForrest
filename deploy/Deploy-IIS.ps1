param(
  [Parameter(Mandatory=$true)][string]$ZipPath,
  [Parameter(Mandatory=$true)][string]$SitePath,
  [Parameter(Mandatory=$true)][string]$AppPool
)

Import-Module WebAdministration
Write-Host "Deploying $ZipPath to $SitePath (AppPool: $AppPool)"
if (!(Test-Path $ZipPath)) { throw "Zip not found: $ZipPath" }
New-Item -ItemType Directory -Path $SitePath -ErrorAction SilentlyContinue | Out-Null
$backup = Join-Path $SitePath ("backup_" + (Get-Date -Format "yyyyMMdd_HHmmss"))

try {
  Stop-WebAppPool -Name $AppPool -ErrorAction SilentlyContinue
  if (Get-ChildItem $SitePath -Recurse -ErrorAction SilentlyContinue) {
    New-Item -ItemType Directory -Path $backup | Out-Null
    Move-Item -Path (Join-Path $SitePath '*') -Destination $backup
  }
  Expand-Archive -Path $ZipPath -DestinationPath $SitePath -Force
  Start-WebAppPool -Name $AppPool
  Start-Sleep -Seconds 5
  Write-Host "Deployment complete. Consider verifying logs and performing an HTTP health check."
} catch {
  Write-Error $_
  throw
}

