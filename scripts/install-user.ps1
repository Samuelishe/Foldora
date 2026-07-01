#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDirectory = Split-Path -Parent $PSCommandPath
$repositoryRoot = Resolve-Path (Join-Path $scriptDirectory "..")
$publishScript = Join-Path $scriptDirectory "publish-dev.ps1"
$publishDirectory = Join-Path $repositoryRoot "artifacts\publish\Foldora"
$installRoot = Join-Path $env:LOCALAPPDATA "Programs\Foldora"

function Get-FullPath([string]$Path) {
    return [System.IO.Path]::GetFullPath($Path)
}

function Test-IsExpectedInstallRoot([string]$Path) {
    $localPrograms = Get-FullPath (Join-Path $env:LOCALAPPDATA "Programs")
    $expected = Get-FullPath (Join-Path $localPrograms "Foldora")
    $actual = Get-FullPath $Path
    return $actual.Equals($expected, [System.StringComparison]::OrdinalIgnoreCase)
}

if (-not (Test-IsExpectedInstallRoot $installRoot)) {
    throw "Refusing to clean unexpected install directory: $installRoot"
}

try {
    Write-Host "Foldora per-user install"
    Write-Host "Repository: $repositoryRoot"
    Write-Host "Install:    $installRoot"
    Write-Host ""

    Write-Host "Creating fresh dev publish output..."
    & $publishScript

    $resolvedPublishDirectory = Get-FullPath $publishDirectory
    if (-not (Test-Path -LiteralPath $resolvedPublishDirectory -PathType Container)) {
        throw "Publish output was not found: $resolvedPublishDirectory"
    }

    if (Test-Path -LiteralPath $installRoot) {
        Write-Host ""
        Write-Host "Cleaning existing install directory..."
        Remove-Item -LiteralPath $installRoot -Recurse -Force
    }

    New-Item -ItemType Directory -Path $installRoot -Force | Out-Null
    Get-ChildItem -LiteralPath $resolvedPublishDirectory -Force |
        Copy-Item -Destination $installRoot -Recurse -Force

    $appExecutable = Join-Path $installRoot "Foldora.App.exe"
    $cliExecutable = Join-Path $installRoot "Foldora.Cli.exe"
    $hostExecutable = Join-Path $installRoot "Foldora.MenuHost.exe"

    foreach ($path in @($appExecutable, $cliExecutable, $hostExecutable)) {
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "Install output is missing required executable: $path"
        }
    }

    Write-Host ""
    Write-Host "Foldora was installed for the current user:"
    Write-Host "  App:      $appExecutable"
    Write-Host "  CLI:      $cliExecutable"
    Write-Host "  MenuHost: $hostExecutable"
    Write-Host ""
    Write-Host "Next steps:"
    Write-Host "  1. Start the installed app:"
    Write-Host "     $appExecutable"
    Write-Host "  2. Enable Explorer integration from the app."
    Write-Host "  3. Explorer commands will use the installed MenuHost sibling:"
    Write-Host "     $hostExecutable"
    Write-Host "  4. To uninstall binaries and unregister the menu:"
    Write-Host "     pwsh scripts/uninstall-user.ps1"
    Write-Host ""
    Write-Host "This script does not register the Explorer menu and does not start the app."
    Write-Host "User data remains under %AppData%\Foldora."
}
catch {
    Write-Error "Install failed. Close running Foldora executables and retry. $($_.Exception.Message)"
    exit 1
}
