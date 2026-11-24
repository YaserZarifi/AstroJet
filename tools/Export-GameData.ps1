param([string]$GameDirectory = (Resolve-Path "$PSScriptRoot\..\..").Path)
$ErrorActionPreference = 'Stop'
$workspace = (Resolve-Path "$PSScriptRoot\..").Path
$gameCore = Get-Content -Raw -LiteralPath "$workspace\recovered\JnG2\Rake2D\GameCore.cs"
$passwordMatch = [regex]::Match($gameCore, 'StringCipher\.SetPassword\("([^"]+)"\)')
if (!$passwordMatch.Success) { throw 'The content password could not be located in recovered GameCore.cs.' }
Add-Type -Path "$PSScriptRoot\GameDataExporter.cs" -WarningAction SilentlyContinue
$destination = "$workspace\research\decoded-content"
$result = [AndroidPort.Tools.GameDataExporter]::Export(
    "$GameDirectory\content.zip", $destination, $passwordMatch.Groups[1].Value)
$result | Set-Content -LiteralPath "$workspace\research\decoded-content-result.txt"
Write-Output $result
