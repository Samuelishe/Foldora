#!/usr/bin/env pwsh
param(
    [switch]$RemoveUserData
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$installRoot = Join-Path $env:LOCALAPPDATA "Programs\Foldora"
$cliExecutable = Join-Path $installRoot "Foldora.Cli.exe"
$userDataRoot = Join-Path $env:APPDATA "Foldora"

function Get-FullPath([string]$Path) {
    return [System.IO.Path]::GetFullPath($Path)
}

function Test-IsExpectedInstallRoot([string]$Path) {
    $localPrograms = Get-FullPath (Join-Path $env:LOCALAPPDATA "Programs")
    $expected = Get-FullPath (Join-Path $localPrograms "Foldora")
    $actual = Get-FullPath $Path
    return $actual.Equals($expected, [System.StringComparison]::OrdinalIgnoreCase)
}

function Remove-FoldoraRegistryRoots {
    $roots = @(
        "HKCU\Software\Classes\Directory\shell\Foldora",
        "HKCU\Software\Classes\Directory\Background\shell\Foldora"
    )

    foreach ($root in $roots) {
        & reg.exe delete $root /f *> $null
        if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne 1) {
            Write-Host "Registry cleanup returned exit code $LASTEXITCODE for $root."
        }
    }
}

if (-not (Test-IsExpectedInstallRoot $installRoot)) {
    throw "Refusing to clean unexpected install directory: $installRoot"
}

try {
    Write-Host "Foldora per-user uninstall"
    Write-Host "Install:   $installRoot"
    Write-Host "User data: $userDataRoot"
    Write-Host ""

    if (Test-Path -LiteralPath $cliExecutable -PathType Leaf) {
        Write-Host "Unregistering Explorer integration through installed CLI..."
        & $cliExecutable unregister-menu
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Installed CLI unregister failed with exit code $LASTEXITCODE. Falling back to Foldora-owned HKCU root cleanup."
            Remove-FoldoraRegistryRoots
        }
    }
    else {
        Write-Host "Installed CLI was not found. Removing Foldora-owned HKCU registry roots directly..."
        Remove-FoldoraRegistryRoots
    }

    if (Test-Path -LiteralPath $installRoot) {
        Write-Host ""
        Write-Host "Removing installed binaries..."
        Remove-Item -LiteralPath $installRoot -Recurse -Force
    }
    else {
        Write-Host ""
        Write-Host "Install directory is already absent."
    }

    if ($RemoveUserData) {
        Write-Host ""
        Write-Host "WARNING: -RemoveUserData deletes settings, imported icons and logs under %AppData%\Foldora."
        Write-Host "WARNING: Existing styled folders can lose custom icons if desktop.ini references imported AppData icons."
        if (Test-Path -LiteralPath $userDataRoot) {
            Remove-Item -LiteralPath $userDataRoot -Recurse -Force
            Write-Host "User data removed: $userDataRoot"
        }
        else {
            Write-Host "User data directory is already absent."
        }
    }
    else {
        Write-Host ""
        Write-Host "User data was preserved: $userDataRoot"
        Write-Host "Run with -RemoveUserData only if you intentionally want to delete settings, imported icons and logs."
    }

    Write-Host ""
    Write-Host "Uninstall complete."
}
catch {
    Write-Error "Uninstall failed. Close running Foldora executables and retry. $($_.Exception.Message)"
    exit 1
}
