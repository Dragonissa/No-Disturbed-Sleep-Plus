param(
    [Parameter(Mandatory = $true)]
    [string]$SourceDll,

    [Parameter(Mandatory = $true)]
    [string]$ModName
)

Write-Host ""
Write-Host "===== RimWorld Mod Deployment ====="
Write-Host "Source DLL : $SourceDll"
Write-Host "Mod Name   : $ModName"
Write-Host ""

$steamPath = (Get-ItemProperty `
    -Path "HKCU:\Software\Valve\Steam" `
    -Name SteamPath `
    -ErrorAction Stop).SteamPath

$libraryFile = Join-Path $steamPath "steamapps\libraryfolders.vdf"

if (-not (Test-Path $libraryFile))
{
    throw "libraryfolders.vdf not found: $libraryFile"
}

$libraries = @()

# Main Steam library
$libraries += $steamPath

# Additional libraries
Get-Content $libraryFile | ForEach-Object {
    if ($_ -match '"path"\s+"([^"]+)"')
    {
        $libraries += $matches[1].Replace('\\', '\')
    }
}

$libraries = $libraries | Select-Object -Unique

foreach ($library in $libraries)
{
    $manifest = Join-Path $library "steamapps\appmanifest_294100.acf"

    if (Test-Path $manifest)
    {
        $rimWorldPath = Join-Path $library "steamapps\common\RimWorld"

        if (-not (Test-Path $rimWorldPath))
        {
            continue
        }

        $assemblyFolder = Join-Path $rimWorldPath "Mods\$ModName\Assemblies"

        New-Item `
            -ItemType Directory `
            -Force `
            -Path $assemblyFolder | Out-Null

        Copy-Item `
            -Path $SourceDll `
            -Destination $assemblyFolder `
            -Force

        Write-Host "RimWorld found: $rimWorldPath"
        Write-Host "Copied to: $assemblyFolder"

        exit 0
    }
}

throw "RimWorld (AppID 294100) not found in any Steam library."
