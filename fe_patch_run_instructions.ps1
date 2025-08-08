# fe_patch_run_instructions.ps1
param(
  [string]$Configuration = "Release"
)

Write-Host "Restoring and building Compute.Shadow + Fancy FE UI" -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed" }

dotnet build -c $Configuration
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

Write-Host "Run the app (WinForms)" -ForegroundColor Cyan
dotnet run --project src/Compute.Shadow.App/Compute.Shadow.App.csproj
