param(
    [string]$ItchTarget = $env:ITCH_TARGET,
    [string]$UnityEditor = 'D:\UnityEditors\6000.3.8f1\Editor\Unity.exe'
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$validationLog = Join-Path $projectRoot 'countdown-validation-final.log'
$buildLog = Join-Path $projectRoot 'countdown-build.log'
$webglOutput = Join-Path $projectRoot 'Builds\WebGL'
$indexFile = Join-Path $webglOutput 'index.html'
$butlerDirectory = Join-Path $projectRoot '.tools\butler'
$butlerExecutable = Join-Path $butlerDirectory 'butler.exe'
$butlerCredentials = Join-Path $env:USERPROFILE '.config\itch\butler_creds'

if (-not (Test-Path -LiteralPath $UnityEditor -PathType Leaf)) {
    throw "Unity Editor was not found: $UnityEditor"
}

if ([string]::IsNullOrWhiteSpace($ItchTarget)) {
    throw 'ITCH_TARGET is missing. Expected format: user/game:webgl'
}

if ($ItchTarget -notmatch '^[^/:]+/[^/:]+:[^/:]+$') {
    throw "Invalid itch.io target '$ItchTarget'. Expected: user/game:webgl"
}

if ([string]::IsNullOrWhiteSpace($env:BUTLER_API_KEY) -and
    -not (Test-Path -LiteralPath $butlerCredentials -PathType Leaf)) {
    throw 'itch.io authentication is missing. Set BUTLER_API_KEY or run butler login.'
}

Write-Host 'Validating COUNT DOWN runtime assembly...'
$validationArguments = @(
    '-batchmode',
    '-projectPath', "`"$projectRoot`"",
    '-executeMethod', 'CountDownValidation.ValidateRuntimeAssembly',
    '-logFile', "`"$validationLog`""
)
$validationProcess = Start-Process `
    -FilePath $UnityEditor `
    -ArgumentList $validationArguments `
    -Wait `
    -PassThru `
    -WindowStyle Hidden

if ($validationProcess.ExitCode -ne 0 -or
    -not (Select-String -LiteralPath $validationLog `
        -Pattern 'COUNT DOWN runtime validation passed' -Quiet)) {
    throw "Unity runtime validation failed. See $validationLog"
}

Write-Host 'Building WebGL with the locally activated Unity Personal license...'
$buildArguments = @(
    '-batchmode',
    '-projectPath', "`"$projectRoot`"",
    '-executeMethod', 'WebGLProjectSetup.Build',
    '-logFile', "`"$buildLog`"",
    '-quit'
)
$buildProcess = Start-Process `
    -FilePath $UnityEditor `
    -ArgumentList $buildArguments `
    -Wait `
    -PassThru `
    -WindowStyle Hidden

if ($buildProcess.ExitCode -ne 0 -or
    -not (Select-String -LiteralPath $buildLog `
        -Pattern 'Build Finished, Result: Success.' -Quiet) -or
    -not (Test-Path -LiteralPath $indexFile -PathType Leaf)) {
    throw "Unity WebGL build failed. See $buildLog"
}

if (-not (Test-Path -LiteralPath $butlerExecutable -PathType Leaf)) {
    Write-Host 'Installing itch.io butler locally...'
    New-Item -ItemType Directory -Force -Path $butlerDirectory | Out-Null
    $archive = Join-Path $butlerDirectory 'butler.zip'
    Invoke-WebRequest `
        -Uri 'https://broth.itch.ovh/butler/windows-amd64/LATEST/archive/default' `
        -OutFile $archive
    Expand-Archive -LiteralPath $archive -DestinationPath $butlerDirectory -Force
    Remove-Item -LiteralPath $archive -Force
}

$commit = (& git -C $projectRoot rev-parse --short HEAD).Trim()
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to determine the Git commit for the itch.io version.'
}

$version = "$(Get-Date -Format 'yyyyMMdd-HHmm')-$commit"
Write-Host "Publishing $webglOutput to $ItchTarget as $version..."
& $butlerExecutable push $webglOutput $ItchTarget --userversion $version

if ($LASTEXITCODE -ne 0) {
    throw 'butler failed to publish the WebGL build.'
}

Write-Host "COUNT DOWN published successfully: $ItchTarget ($version)"
