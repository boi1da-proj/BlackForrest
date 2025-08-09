param(
  [Parameter(Mandatory=$true)] [string]$ReleaseName,

  [string]$ReleasesRoot = "C:\apps\rhino.compute\releases",
  [string]$CurrentLink = "C:\apps\rhino.compute\current",
  [string]$SiteName = "Rhino.Compute",
  [string]$HealthUrl = "http://localhost/version",

  [switch]$ValidateChecksums,
  [switch]$RequireChecksums,

  [string]$AliasesJson = "",

  [switch]$DeployLocal,
  [string]$DeployScriptPath = "$PSScriptRoot\deploy-compute.ps1",

  [switch]$TriggerDeploy,
  [string]$GitHubOwner = "",
  [string]$GitHubRepo  = "",
  [string]$Branch      = "main",
  [string]$WorkflowFile = "deploy-rhino-compute.yml",
  [string]$GitHubTokenEnv = "GH_WORKFLOW_PAT"
)

$ErrorActionPreference = "Stop"

function Resolve-ReleasePath([string]$name) {
  $root = $script:ReleasesRoot
  $direct = Join-Path $root $name
  if (Test-Path $direct) { return $direct }

  $aliasesPath = if ([string]::IsNullOrWhiteSpace($script:AliasesJson)) { Join-Path $root "aliases.json" } else { $script:AliasesJson }
  if (Test-Path $aliasesPath) {
    try {
      $aliases = Get-Content $aliasesPath -Raw | ConvertFrom-Json
      $mapped = $aliases.$name
      if ($mapped) {
        $cand = Join-Path $root $mapped
        if (Test-Path $cand) { return $cand }
        throw "aliases.json maps '$name' -> '$mapped', but folder not found: $cand"
      }
    } catch { Write-Warning "Could not parse aliases.json: $($_.Exception.Message)" }
  }

  $dirs = Get-ChildItem $root -Directory -ErrorAction SilentlyContinue
  foreach ($d in $dirs) {
    $j = Join-Path $d.FullName "release.json"
    $t = Join-Path $d.FullName "semver.txt"
    if (Test-Path $j) {
      try {
        $obj = Get-Content $j -Raw | ConvertFrom-Json
        if ($obj.semver -eq $name) { return $d.FullName }
      } catch {}
    }
    if (Test-Path $t) {
      $sv = (Get-Content $t -Raw).Trim()
      if ($sv -eq $name) { return $d.FullName }
    }
  }

  throw "Could not resolve release path for '$name'. Checked folder name, aliases.json, release.json, semver.txt."
}

function Validate-ChecksumsSha256([string]$releasePath, [string]$file) {
  $errors = 0
  $lines = Get-Content $file -Encoding UTF8
  foreach ($line in $lines) {
    $l = $line.Trim()
    if ($l -eq "" -or $l.StartsWith("#")) { continue }
    $m = [Regex]::Match($l, '^\s*([0-9a-fA-F]{64})\s+\*?(.*)$')
    if (-not $m.Success) { Write-Warning "Bad line in manifest: $l"; continue }
    $expected = $m.Groups[1].Value.ToLower()
    $rel = $m.Groups[2].Value.Trim()
    $path = Join-Path $releasePath $rel
    if (-not (Test-Path $path)) { Write-Error "Missing file from manifest: $rel"; $errors++; continue }
    $actual = (Get-FileHash -Algorithm SHA256 -Path $path).Hash.ToLower()
    if ($actual -ne $expected) {
      Write-Error "Checksum mismatch: $rel expected $expected got $actual"
      $errors++
    }
  }
  if ($errors -gt 0) { throw "Checksum validation failed for $errors file(s)." }
  Write-Host "Checksum validation passed."
}

function Validate-Checksums([string]$releasePath, [bool]$require) {
  $sha = Join-Path $releasePath "checksums.sha256"
  if (Test-Path $sha) { return (Validate-ChecksumsSha256 $releasePath $sha) }
  if ($require) { throw "Checksum manifest not found: $sha" }
  Write-Warning "No checksum manifest present. Skipping validation."
}

function New-DirJunction([string]$LinkPath, [string]$TargetPath) {
  if (Test-Path $LinkPath) { throw "Junction already exists: $LinkPath" }
  try {
    New-Item -ItemType Junction -Path $LinkPath -Target $TargetPath | Out-Null
  } catch {
    & cmd.exe /c "mklink /J \"$LinkPath\" \"$TargetPath\"" | Out-Null
  }
  $item = Get-Item -LiteralPath $LinkPath -Force
  if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -eq 0) {
    throw "Failed to create junction: $LinkPath"
  }
}

function Try-Trigger-GitHub() {
  $gh = Get-Command gh -ErrorAction SilentlyContinue
  if ($gh) {
    & gh workflow run $script:WorkflowFile -r $script:Branch | Out-Null
    Write-Host "Triggered GitHub workflow via gh CLI."
    return
  }
  $token = [Environment]::GetEnvironmentVariable($script:GitHubTokenEnv, "Process")
  if (-not $token) { $token = [Environment]::GetEnvironmentVariable($script:GitHubTokenEnv, "Machine") }
  if (-not $token) { $token = [Environment]::GetEnvironmentVariable($script:GitHubTokenEnv, "User") }
  if (-not $token) { Write-Warning "No gh CLI and no token in $($script:GitHubTokenEnv). Skipping GitHub trigger."; return }
  $uri = "https://api.github.com/repos/$($script:GitHubOwner)/$($script:GitHubRepo)/actions/workflows/$($script:WorkflowFile)/dispatches"
  $headers = @{ "Authorization" = "Bearer $token"; "Accept" = "application/vnd.github+json"; "User-Agent" = "promote-compute-release" }
  $body = @{ ref = $script:Branch } | ConvertTo-Json
  Invoke-RestMethod -Method Post -Uri $uri -Headers $headers -Body $body
  Write-Host "Triggered GitHub workflow via REST API."
}

# Resolve and validate release
$ReleasePath = Resolve-ReleasePath -name $ReleaseName
Write-Host "Resolved release '$ReleaseName' -> $ReleasePath"

if ($ValidateChecksums) {
  Validate-Checksums -releasePath $ReleasePath -require:[bool]$RequireChecksums
}

# Swap junction atomically
$base = Split-Path $CurrentLink -Parent
$newLink = Join-Path $base "current_new"
$prevLink = Join-Path $base ("current_prev_" + (Get-Date -Format "yyyyMMdd_HHmmss"))

if (Test-Path $newLink) { Remove-Item $newLink -Recurse -Force }
New-DirJunction -LinkPath $newLink -TargetPath $ReleasePath

if (Test-Path $CurrentLink) {
  Rename-Item -LiteralPath $CurrentLink -NewName (Split-Path $prevLink -Leaf)
  Write-Host "Saved previous link as: $prevLink"
}
Rename-Item -LiteralPath $newLink -NewName (Split-Path $CurrentLink -Leaf)
Write-Host "Current -> $ReleasePath"

if ($DeployLocal) {
  if (-not (Test-Path $DeployScriptPath)) {
    $guess = Join-Path $PSScriptRoot "deploy-compute.ps1"
    if (Test-Path $guess) { $DeployScriptPath = $guess }
  }
  if (-not (Test-Path $DeployScriptPath)) { throw "deploy-compute.ps1 not found at $DeployScriptPath" }
  Write-Host "Running local deploy script..."
  & $DeployScriptPath -SiteName $SiteName -PayloadSrc $CurrentLink -HealthUrl $HealthUrl
}

if ($TriggerDeploy) {
  if (-not $GitHubOwner -or -not $GitHubRepo) { Write-Warning "Owner/repo not set; skipping GitHub trigger." }
  else { Try-Trigger-GitHub }
}

Write-Host "Promotion complete."


