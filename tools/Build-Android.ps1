param([string]$AndroidSdkDirectory, [string]$JavaSdkDirectory, [string]$Configuration = 'Debug')
$ErrorActionPreference = 'Stop'
$workspace = (Resolve-Path "$PSScriptRoot\..").Path
$dotnet = Join-Path $PSScriptRoot 'dotnet\dotnet.exe'
if (!(Test-Path -LiteralPath $dotnet)) { $dotnet = (Get-Command dotnet -ErrorAction Stop).Source }
if (!(Test-Path -LiteralPath "$workspace\packed\content.zip")) {
    throw 'Prepare the local game content with package_content.py before building.'
}
foreach ($effect in (Get-ChildItem -LiteralPath "$workspace\packed\shader" -Filter '*.mgfxo')) {
    $bytes = [IO.File]::ReadAllBytes($effect.FullName)
    if ($bytes.Length -lt 10 -or $bytes[4] -ne 10) { throw 'Run convert_effects.py after packaging the game content.' }
}
$arguments = @('build', "$workspace\src\Android\JnG2.Android.csproj", '-c', $Configuration)
if ($AndroidSdkDirectory) { $arguments += "-p:AndroidSdkDirectory=$AndroidSdkDirectory" }
if ($JavaSdkDirectory) { $arguments += "-p:JavaSdkDirectory=$JavaSdkDirectory" }
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
& $dotnet @arguments
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
