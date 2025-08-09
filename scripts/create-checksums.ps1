param(
  [Parameter(Mandatory=$true)][string]$ReleaseName,
  [string]$ReleasesRoot = "C:\apps\rhino.compute\releases",
  [string]$ManifestFile = "checksums.sha256",
  [switch]$IncludeHidden
)

$ErrorActionPreference = "Stop"

$ReleasePath = Join-Path $ReleasesRoot $ReleaseName
if (-not (Test-Path $ReleasePath)) { throw "Release not found: $ReleasePath" }

$manifestPath = Join-Path $ReleasePath $ManifestFile
if (Test-Path $manifestPath) { Remove-Item $manifestPath -Force }

$flags = @()
if ($IncludeHidden) { $flags += "-Force" }

$files = Get-ChildItem -Path $ReleasePath -File -Recurse @flags |
  Where-Object { $_.Name -ne $ManifestFile }

foreach ($f in $files) {
  $hash = (Get-FileHash -Algorithm SHA256 -Path $f.FullName).Hash.ToLower()
  $rel = $f.FullName.Substring($ReleasePath.Length + 1).Replace("\\","/")
  "$hash *$rel" | Out-File -FilePath $manifestPath -Append -Encoding utf8
}

Write-Host "Wrote manifest: $manifestPath"


