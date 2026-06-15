param(
    [int]$ApiPort = 5199
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Split-Path -Parent $scriptRoot
$apiProject = Join-Path $root "Learnix.Api\Learnix.API\Learnix.API.csproj"
$apiBin = Join-Path $root "Learnix.Api\Learnix.API\bin\Debug\net10.0"
$apiExe = Join-Path $apiBin "Learnix.API.exe"
$apiDll = Join-Path $apiBin "Learnix.API.dll"
$mauiProject = Join-Path $root "Learnix\Learnix\Learnix.csproj"
$composeFile = Join-Path $root "Learnix.Api\docker-compose.yml"
$logsDir = Join-Path $root "logs"
$apiOut = Join-Path $logsDir "learnix-api.out.log"
$apiErr = Join-Path $logsDir "learnix-api.err.log"

function Resolve-ToolPath {
    param(
        [string]$Name,
        [string[]]$Candidates
    )

    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    foreach ($candidate in $Candidates) {
        if ($candidate -and (Test-Path $candidate)) {
            return $candidate
        }
    }

    return $null
}

function Invoke-Native {
    param(
        [string]$FilePath,
        [string[]]$Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
    }
}

function Test-Docker {
    docker info *> $null
    return $LASTEXITCODE -eq 0
}

function Test-Api {
    try {
        $response = Invoke-WebRequest -Uri "http://127.0.0.1:$ApiPort/api/catalog/subjects" -UseBasicParsing -TimeoutSec 1
        return $response.StatusCode -ge 200 -and $response.StatusCode -lt 500
    }
    catch {
        return $false
    }
}

function Get-ApiProcesses {
    try {
        return @(Get-CimInstance Win32_Process |
            Where-Object {
                $_.Name -in @("dotnet.exe", "Learnix.API.exe") -and
                ($_.CommandLine -like "*Learnix.API.dll*" -or $_.CommandLine -like "*Learnix.API.exe*")
            })
    }
    catch {
        return @()
    }
}

function Wait-Api {
    for ($i = 0; $i -lt 180; $i++) {
        if (Test-Api) {
            return
        }

        Start-Sleep -Seconds 1
    }

    throw "Learnix.API did not start on http://localhost:$ApiPort. First database seed can take a few minutes; check $apiErr"
}

function Wait-AndroidDevice {
    param([string]$Adb)

    for ($i = 0; $i -lt 90; $i++) {
        $devices = & $Adb devices
        $ready = $devices | Where-Object { $_ -match "\tdevice$" } | Select-Object -First 1
        if ($ready) {
            return
        }

        Start-Sleep -Seconds 2
    }

    throw "Android emulator is not ready. Open it in Visual Studio or Android Device Manager, then run start-learnix.cmd again."
}

function Start-DetachedCommand {
    param(
        [string]$CommandLine,
        [string]$WorkingDirectory
    )

    $commandProcessor = $env:ComSpec
    if (-not $commandProcessor) {
        $commandProcessor = "C:\Windows\System32\cmd.exe"
    }

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $commandProcessor
    $startInfo.Arguments = "/d /c $CommandLine"
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true

    [System.Diagnostics.Process]::Start($startInfo) | Out-Null
}

New-Item -ItemType Directory -Force -Path $logsDir | Out-Null

Write-Host "== Learnix auto start =="
Write-Host "Project: $root"

Write-Host "Starting PostgreSQL container..."
if (-not (Test-Docker)) {
    $dockerDesktop = "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    if (Test-Path $dockerDesktop) {
        Write-Host "Docker Desktop is not running. Starting it..."
        Start-Process -FilePath $dockerDesktop -WindowStyle Hidden
        for ($i = 0; $i -lt 60; $i++) {
            if (Test-Docker) {
                break
            }

            Start-Sleep -Seconds 2
        }
    }
}

if (-not (Test-Docker)) {
    throw "Docker is not available. Start Docker Desktop and run start-learnix.cmd again."
}

Invoke-Native "docker" @("compose", "-f", $composeFile, "up", "-d", "learnix-postgres")

Write-Host "Building Learnix.API..."
Invoke-Native "dotnet" @("build", $apiProject, "--no-restore", "--nologo")

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:LEARNIX_DB_CONNECTION = "Host=127.0.0.1;Port=55432;Database=learnix_db;Username=postgres;Password=jara130308;SSL Mode=Disable;GSS Encryption Mode=Disable"

if (Test-Api) {
    Write-Host "Learnix.API is already running on http://localhost:$ApiPort"
}
elseif ((Get-ApiProcesses).Count -gt 0) {
    Write-Host "Learnix.API is already starting. Waiting for it to become ready..."
    Wait-Api
}
else {
    Write-Host "Starting Learnix.API on http://0.0.0.0:$ApiPort ..."
    $apiCommand = "dotnet `"$apiDll`" --urls http://0.0.0.0:$ApiPort > `"$apiOut`" 2> `"$apiErr`""
    Start-DetachedCommand -CommandLine $apiCommand -WorkingDirectory $apiBin
    Wait-Api
}

$adb = Resolve-ToolPath "adb" @(
    "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe",
    "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe"
)
$emulator = Resolve-ToolPath "emulator" @(
    "$env:LOCALAPPDATA\Android\Sdk\emulator\emulator.exe",
    "C:\Program Files (x86)\Android\android-sdk\emulator\emulator.exe"
)

if (-not $adb) {
    throw "adb.exe was not found. Install Android SDK Platform Tools from Visual Studio Installer."
}

$device = (& $adb devices) | Where-Object { $_ -match "\tdevice$" } | Select-Object -First 1
if (-not $device) {
    if (-not $emulator) {
        throw "No Android emulator is running and emulator.exe was not found."
    }

    $avd = (& $emulator -list-avds | Select-Object -First 1)
    if (-not $avd) {
        throw "No Android virtual devices were found. Create one in Visual Studio Android Device Manager."
    }

    Write-Host "Starting Android emulator: $avd"
    Start-Process -FilePath $emulator -ArgumentList "-avd", $avd -WorkingDirectory (Split-Path $emulator)
}

Wait-AndroidDevice -Adb $adb

Write-Host "Building, installing and launching Learnix on the emulator..."
Invoke-Native "dotnet" @("build", $mauiProject, "-f", "net9.0-android", "-t:Run", "--no-restore", "--nologo")

Write-Host "Ready. Learnix is installed on the emulator with the L icon."
