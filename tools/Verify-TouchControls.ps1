$ErrorActionPreference = 'Stop'
$control = Get-Content -Raw -LiteralPath "$PSScriptRoot\..\src\Mobile\TouchControls.cs"
$checks = Get-Content -Raw -LiteralPath "$PSScriptRoot\..\verification\TouchControlChecks.cs"
Add-Type -TypeDefinition "using JnG2.Mobile;`n$control`n$($checks -replace '(?m)^using .+;\r?\n', '')"
[JnG2.Verification.TouchControlChecks]::Run()
