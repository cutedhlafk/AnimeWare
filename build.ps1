$ErrorActionPreference = 'Stop'
$projectFile = Join-Path $PSScriptRoot 'VelocityStudio.csproj'
$outputDir = Join-Path $PSScriptRoot 'App'
dotnet build $projectFile -c Release -o $outputDir --configfile (Join-Path $PSScriptRoot 'NuGet.Config')
if ($LASTEXITCODE -ne 0) { throw 'Kompilacja nie powiodła się.' }
Write-Host "Gotowe: $outputDir\VelocityStudio.exe"
