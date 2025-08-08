#!/bin/bash
set -e

echo "Restoring and building Compute.Shadow + Fancy FE UI"
dotnet restore
dotnet build -c Release

echo "Run the app (WinForms)"
dotnet run --project src/Compute.Shadow.App/Compute.Shadow.App.csproj
