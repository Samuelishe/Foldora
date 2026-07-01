#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDirectory = Split-Path -Parent $PSCommandPath
$repositoryRoot = Resolve-Path (Join-Path $scriptDirectory "..")
$publishRoot = Join-Path $repositoryRoot "artifacts\publish"
$publishDirectory = Join-Path $publishRoot "Foldora"

$resolvedRepositoryRoot = [System.IO.Path]::GetFullPath($repositoryRoot)
$resolvedPublishDirectory = [System.IO.Path]::GetFullPath($publishDirectory)
$expectedPrefix = [System.IO.Path]::GetFullPath((Join-Path $resolvedRepositoryRoot "artifacts\publish\Foldora"))

if (-not $resolvedPublishDirectory.Equals($expectedPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to clean unexpected publish directory: $resolvedPublishDirectory"
}

Write-Host "Foldora dev publish"
Write-Host "Repository: $resolvedRepositoryRoot"
Write-Host "Output:     $resolvedPublishDirectory"

if (Test-Path -LiteralPath $resolvedPublishDirectory) {
    Write-Host "Cleaning existing publish directory..."
    Remove-Item -LiteralPath $resolvedPublishDirectory -Recurse -Force
}

New-Item -ItemType Directory -Path $resolvedPublishDirectory -Force | Out-Null

$projects = @(
    "src\Foldora.App\Foldora.App.csproj",
    "src\Foldora.Cli\Foldora.Cli.csproj",
    "src\Foldora.MenuHost\Foldora.MenuHost.csproj"
)

foreach ($project in $projects) {
    $projectPath = Join-Path $resolvedRepositoryRoot $project
    Write-Host "Publishing $project..."
    dotnet publish $projectPath --configuration Release --output $resolvedPublishDirectory --self-contained false
}

$requiredExecutables = @(
    "Foldora.App.exe",
    "Foldora.Cli.exe",
    "Foldora.MenuHost.exe"
)

foreach ($executable in $requiredExecutables) {
    $path = Join-Path $resolvedPublishDirectory $executable
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Publish output is missing required executable: $path"
    }
}

Write-Host ""
Write-Host "Publish layout is ready:"
foreach ($executable in $requiredExecutables) {
    Write-Host "  $(Join-Path $resolvedPublishDirectory $executable)"
}

Write-Host ""
Write-Host "Next steps:"
$appExecutable = Join-Path $resolvedPublishDirectory "Foldora.App.exe"
$cliExecutable = Join-Path $resolvedPublishDirectory "Foldora.Cli.exe"
$hostExecutable = Join-Path $resolvedPublishDirectory "Foldora.MenuHost.exe"

Write-Host "  1. Start $appExecutable for manual WPF testing."
Write-Host "  2. Enable Explorer integration from the app, or run:"
Write-Host "     $cliExecutable register-menu --host-path `"$hostExecutable`""
Write-Host "  3. Before deleting the publish folder, unregister the menu:"
Write-Host "     $cliExecutable unregister-menu"
Write-Host ""
Write-Host "This script does not register the Explorer menu and does not start the app."
