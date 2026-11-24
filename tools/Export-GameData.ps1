param([string]$GameDirectory = (Resolve-Path "$PSScriptRoot\..\..").Path)
$ErrorActionPreference = 'Stop'
$gameCore = Get-Content -Raw -LiteralPath "$GameDirectory\AndroidPort\recovered\JnG2\Rake2D\GameCore.cs"
$passwordMatch = [regex]::Match($gameCore, 'StringCipher\.SetPassword\("([^"]+)"\)')
if (!$passwordMatch.Success) { throw 'The content password could not be located in recovered GameCore.cs.' }
Add-Type -Path "$PSScriptRoot\GameDataExporter.cs" -WarningAction SilentlyContinue
$destination = "$GameDirectory\AndroidPort\research\decoded-content"
$result = [AndroidPort.Tools.GameDataExporter]::Export(
    "$GameDirectory\content.zip", $destination, $passwordMatch.Groups[1].Value)
$result | Set-Content -LiteralPath "$GameDirectory\AndroidPort\research\decoded-content-result.txt"
Write-Output $result
