<#
.SYNOPSIS
    Packs a project by name and pushes the resulting .nupkg to the NuGet
    source configured in nuget.push.config.json.

.DESCRIPTION
    Reads the source URL, API key and solution directory from
    nuget.push.config.json (which must live next to this script and is
    excluded from source control via .gitignore).

    The script then:
      1. Locates the .csproj whose name matches -Project (searching under
         the configured solution directory and skipping bin/ and obj/).
      2. Packs it into a temporary folder in the system temp directory.
      3. Pushes every produced .nupkg (and .snupkg, if any) to the
         configured NuGet source.
      4. Removes the temporary folder, leaving nothing on disk.

.PARAMETER Project
    The project name (without .csproj), e.g. "KernelAbstractions.Patterns".

.PARAMETER Configuration
    Build configuration used by dotnet pack. Defaults to "Release".

.PARAMETER ConfigFile
    Optional path to the JSON configuration file. Defaults to
    nuget.push.config.json next to this script.

.EXAMPLE
    .\nuget-push.ps1 -Project KernelAbstractions.Patterns

.EXAMPLE
    .\nuget-push.ps1 -Project KernelAbstractions.AspNetCore -Configuration Debug
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Project,

    [string]$Configuration = "Release",

    [string]$ConfigFile
)

$ErrorActionPreference = "Stop"

# ──────────────────────────────────────────────────────────────
# 1. Load configuration
# ──────────────────────────────────────────────────────────────
$scriptRoot = $PSScriptRoot
if (-not $ConfigFile) {
    $ConfigFile = Join-Path $scriptRoot "nuget.push.config.json"
}

if (-not (Test-Path $ConfigFile)) {
    throw "Configuration file not found: $ConfigFile"
}

$config = Get-Content $ConfigFile -Raw | ConvertFrom-Json

if (-not $config.source)            { throw "Missing 'source' in $ConfigFile" }
if (-not $config.apiKey)            { throw "Missing 'apiKey' in $ConfigFile" }
if (-not $config.solutionDirectory) { $config.solutionDirectory = "." }

$searchRoot = if ([System.IO.Path]::IsPathRooted($config.solutionDirectory)) {
    $config.solutionDirectory
} else {
    Join-Path $scriptRoot $config.solutionDirectory
}

Write-Host "Config   : $ConfigFile"            -ForegroundColor Cyan
Write-Host "Source   : $($config.source)"     -ForegroundColor Cyan
Write-Host "Search   : $searchRoot"           -ForegroundColor Cyan
Write-Host "Project  : $Project"              -ForegroundColor Cyan
Write-Host ""

# ──────────────────────────────────────────────────────────────
# 2. Locate the .csproj
# ──────────────────────────────────────────────────────────────
$matches = Get-ChildItem -Path $searchRoot -Recurse -Filter "$Project.csproj" -File |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }

if (-not $matches) {
    throw "No .csproj named '$Project.csproj' found under $searchRoot."
}

if ($matches.Count -gt 1) {
    Write-Host "Multiple candidates found:" -ForegroundColor Yellow
    $matches | ForEach-Object { Write-Host "  $($_.FullName)" -ForegroundColor Yellow }
    throw "Project name is ambiguous. Please specify a more specific -Project value."
}

$csproj = $matches[0].FullName
Write-Host "Found    : $csproj" -ForegroundColor Green
Write-Host ""

# ──────────────────────────────────────────────────────────────
# 3. Pack into a temporary folder
# ──────────────────────────────────────────────────────────────
$tempOut = Join-Path ([System.IO.Path]::GetTempPath()) "kab-pkg-$([Guid]::NewGuid().ToString('N').Substring(0,8))"
New-Item -ItemType Directory -Path $tempOut | Out-Null

try {
    Write-Host "Packing ($Configuration)..." -ForegroundColor Yellow
    dotnet pack $csproj `
        --configuration $Configuration `
        --output $tempOut
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed." }

    $nupkgs = Get-ChildItem -Path $tempOut -Filter "*.nupkg" -File
    if ($nupkgs.Count -eq 0) {
        throw "No .nupkg produced in $tempOut"
    }

    Write-Host "Produced:" -ForegroundColor Green
    $nupkgs | ForEach-Object { Write-Host "  $($_.Name)" -ForegroundColor Green }

    # ──────────────────────────────────────────────────────────
    # 4. Push every produced package
    # ──────────────────────────────────────────────────────────
    Write-Host ""
    Write-Host "Pushing to $($config.source)..." -ForegroundColor Yellow
    foreach ($pkg in $nupkgs) {
        dotnet nuget push $pkg.FullName `
            --source $config.source `
            --api-key $config.apiKey `
            --skip-duplicate
        if ($LASTEXITCODE -ne 0) { throw "dotnet nuget push failed for $($pkg.Name)." }
    }

    Write-Host ""
    Write-Host "Done." -ForegroundColor Green
}
finally {
    if (Test-Path $tempOut) {
        Remove-Item $tempOut -Recurse -Force -ErrorAction SilentlyContinue
    }
}