param(
  [Parameter(Mandatory=$true)][string]$ZipPath,
  [Parameter(Mandatory=$true)][string]$SitePath,
  [Parameter(Mandatory=$true)][string]$AppPool,
  [string]$HealthUrl,
  [int]$HealthTimeoutSec = 20
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

  if ($HealthUrl) {
    Write-Host "Health check: $HealthUrl (timeout ${HealthTimeoutSec}s)"
    $ok = $false
    $sw = [Diagnostics.Stopwatch]::StartNew()
    while ($sw.Elapsed.TotalSeconds -lt $HealthTimeoutSec) {
      try {
        $resp = Invoke-WebRequest -Uri $HealthUrl -UseBasicParsing -TimeoutSec 5
        if ($resp.StatusCode -ge 200 -and $resp.StatusCode -lt 500) { $ok = $true; break }
      } catch { Start-Sleep -Seconds 2 }
    }
    $sw.Stop()
    if (-not $ok) {
      Write-Warning "Health check failed. Rolling back to backup..."
      Stop-WebAppPool -Name $AppPool -ErrorAction SilentlyContinue
      # Clear new contents
      Get-ChildItem -Force -Path $SitePath | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
      # Restore backup
      if (Test-Path $backup) {
        Get-ChildItem -Force -Path $backup | Move-Item -Destination $SitePath
        Remove-Item -Force -Recurse $backup -ErrorAction SilentlyContinue
      }
      Start-WebAppPool -Name $AppPool
      throw "Deployment rolled back due to failing health check."
    } else {
      Write-Host "Health check OK."
    }
  } else {
    Write-Host "Deployment complete. No health check URL provided."
  }
} catch {
  Write-Error $_
  throw
}

