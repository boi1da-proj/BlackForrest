param(
  [string]$SiteName = "Rhino.Compute",
  [string]$PayloadSrc = "C:\apps\rhino.compute\current",
  [string]$WebConfigSrc = "$PSScriptRoot\..\deploy\compute\web.config",
  [string]$HealthUrl = "http://localhost/version",  # switch to "/" if /version isn't available
  [int]$BackupRetentionDays = 14
)

$ErrorActionPreference = "Stop"
Import-Module WebAdministration

# Resolve IIS site info
$site = Get-Website -Name $SiteName
if (-not $site) { throw "IIS site not found: $SiteName" }
$SiteRoot = $ExecutionContext.InvokeCommand.ExpandString($site.physicalPath)
$AppPool  = $site.applicationPool

if (-not (Test-Path $PayloadSrc))   { throw "Payload source not found: $PayloadSrc" }
if (-not (Test-Path $WebConfigSrc)) { throw "web.config not found: $WebConfigSrc" }

function Ensure-Dir([string]$p) { if (!(Test-Path $p)) { New-Item -ItemType Directory -Path $p | Out-Null } }

Write-Host "SiteName:  $SiteName"
Write-Host "SiteRoot:  $SiteRoot"
Write-Host "AppPool:   $AppPool"
Write-Host "Payload:   $PayloadSrc"
Write-Host "WebConfig: $WebConfigSrc"

Ensure-Dir $SiteRoot
Ensure-Dir "C:\logs\rhino-compute"

# Grant filesystem permissions to the app pool identity (idempotent)
try {
  $poolPath = "IIS:\AppPools\$AppPool"
  if (Test-Path $poolPath) {
    $identity = "IIS AppPool\$AppPool"
    Write-Host "Granting file permissions to '$identity'..."
    & icacls $PayloadSrc /grant "$identity:(RX)" /T /C | Out-Null
    & icacls "C:\logs\rhino-compute" /grant "$identity:(M)" /T /C | Out-Null
    & icacls $SiteRoot /grant "$identity:(R)" /T /C | Out-Null
    Set-ItemProperty $poolPath -Name processModel.loadUserProfile -Value True -ErrorAction SilentlyContinue
  }
} catch {
  Write-Warning "Permission grant step warning: $($_.Exception.Message)"
}

# Backup current site
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backup = Join-Path $SiteRoot ("backup_" + $timestamp)
Ensure-Dir $backup

Write-Host "Stopping app pool $AppPool"
Stop-WebAppPool -Name $AppPool -ErrorAction SilentlyContinue

Write-Host "Backing up current site to $backup"
Get-ChildItem -Path $SiteRoot -Force -Exclude 'backup_*' | ForEach-Object {
  Move-Item -Path $_.FullName -Destination $backup
}

# Mirror payload into site (exclude web.config from payload)
Write-Host "Mirroring payload -> site"
$null = robocopy $PayloadSrc $SiteRoot /MIR /R:1 /W:2 /NFL /NDL /NP /XF web.config
if ($LASTEXITCODE -ge 8) { throw "robocopy failed with exit code $LASTEXITCODE" }

# Place web.config from repo
Write-Host "Copying web.config"
Copy-Item $WebConfigSrc -Destination (Join-Path $SiteRoot "web.config") -Force

# Start and health-check
Write-Host "Starting app pool $AppPool"
Start-WebAppPool -Name $AppPool
Start-Sleep -Seconds 3

try {
  Write-Host "Health check: $HealthUrl"
  $resp = Invoke-WebRequest -Uri $HealthUrl -UseBasicParsing -TimeoutSec 20
  Write-Host "Health OK: HTTP $($resp.StatusCode)"
} catch {
  Write-Warning "Health check failed: $($_.Exception.Message)"
  Write-Warning "Rolling back from $backup"
  Stop-WebAppPool -Name $AppPool -ErrorAction SilentlyContinue
  Get-ChildItem -Path $SiteRoot -Force -Exclude 'backup*' | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
  $null = robocopy $backup $SiteRoot /MIR /R:1 /W:2 /NFL /NDL /NP
  Start-WebAppPool -Name $AppPool
  throw
}

# Prune old backups
$threshold = (Get-Date).AddDays(-$BackupRetentionDays)
Get-ChildItem -Path $SiteRoot -Directory -Filter "backup_*" |
  Where-Object { $_.LastWriteTime -lt $threshold } |
  Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Deployment complete."


