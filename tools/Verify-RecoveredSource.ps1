param([string]$GameDirectory = (Resolve-Path "$PSScriptRoot\..\..").Path, [switch]$AndroidCompatibility, [switch]$PortSource)
$ErrorActionPreference = 'Stop'
Add-Type -Path "$PSHOME\Microsoft.CodeAnalysis.dll"
Add-Type -Path "$PSHOME\Microsoft.CodeAnalysis.CSharp.dll"
$sourceRoot = Join-Path $GameDirectory 'AndroidPort\recovered\JnG2'
if ($PortSource) { $sourceRoot = Join-Path $GameDirectory 'AndroidPort\src\Game' }
$outputRoot = Join-Path $GameDirectory 'AndroidPort\verification'
New-Item -ItemType Directory -Force -Path $outputRoot | Out-Null
$trees = [Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
foreach ($file in (Get-ChildItem -LiteralPath $sourceRoot -Recurse -Filter '*.cs')) {
    if ($file.FullName -match '[\\/]obj[\\/]|[\\/]bin[\\/]') { continue }
    if ($PortSource -and $file.Name -in @('SoundDataBass.cs','Sound2DBass.cs','Program.cs')) { continue }
    $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
        [IO.File]::ReadAllText($file.FullName), [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default,
        $file.FullName, [Text.Encoding]::UTF8, [Threading.CancellationToken]::None))
}
if ($PortSource) {
    foreach ($file in (Get-ChildItem -LiteralPath "$GameDirectory\AndroidPort\src\Mobile","$GameDirectory\AndroidPort\src\Android" -Filter '*.cs')) {
        $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
            [IO.File]::ReadAllText($file.FullName), [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default,
            $file.FullName, [Text.Encoding]::UTF8, [Threading.CancellationToken]::None))
    }
}
$references = [Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$frameworkNames = @('mscorlib','System','System.Core','System.Xml','System.Xml.Linq',
    'System.Data','System.Drawing','System.IO.Compression','System.IO.Compression.FileSystem',
    'System.Runtime.Serialization','System.Configuration','System.Numerics','Microsoft.CSharp')
if ($AndroidCompatibility) {
    foreach ($path in ([AppContext]::GetData('TRUSTED_PLATFORM_ASSEMBLIES') -split [IO.Path]::PathSeparator)) {
        $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile(
            $path, [Microsoft.CodeAnalysis.MetadataReferenceProperties]::Assembly, $null))
    }
    $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile(
        "$PSScriptRoot\monogame-android\lib\net8.0-android34.0\MonoGame.Framework.dll",
        [Microsoft.CodeAnalysis.MetadataReferenceProperties]::Assembly, $null))
    foreach ($file in (Get-ChildItem -LiteralPath "$PSScriptRoot\android-ref\ref" -Recurse -Filter '*.dll')) {
        $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile(
            $file.FullName, [Microsoft.CodeAnalysis.MetadataReferenceProperties]::Assembly, $null))
    }
} else {
foreach ($name in $frameworkNames) {
    $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile(
        (Join-Path $framework "$name.dll"), [Microsoft.CodeAnalysis.MetadataReferenceProperties]::Assembly, $null))
}
    $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile(
        (Join-Path $GameDirectory 'MonoGame.Framework.dll'), [Microsoft.CodeAnalysis.MetadataReferenceProperties]::Assembly, $null))
}
foreach ($name in @('FarseerPhysics MonoGame.dll','spine-csharp.dll',
    'ManagedBass.dll','Steamworks.NET.dll')) {
    $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile(
        (Join-Path $GameDirectory $name), [Microsoft.CodeAnalysis.MetadataReferenceProperties]::Assembly, $null))
}
if (!$AndroidCompatibility) {
$runtimeFacade = Get-ChildItem -LiteralPath "$env:WINDIR\Microsoft.NET\assembly\GAC_MSIL\System.Runtime" -Recurse -Filter 'System.Runtime.dll' | Select-Object -First 1
$references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile(
    $runtimeFacade.FullName, [Microsoft.CodeAnalysis.MetadataReferenceProperties]::Assembly, $null))
}
$options = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new(
    [Microsoft.CodeAnalysis.OutputKind]::WindowsApplication).WithAllowUnsafe($true).WithPlatform(
    [Microsoft.CodeAnalysis.Platform]::X86).WithOptimizationLevel([Microsoft.CodeAnalysis.OptimizationLevel]::Release)
if ($PortSource) { $options = $options.WithOutputKind([Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary).WithPlatform([Microsoft.CodeAnalysis.Platform]::AnyCpu) }
$compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create('JnG2', $trees, $references, $options)
$diagnostics = @($compilation.GetDiagnostics() | Where-Object Severity -in @('Error','Warning'))
$logName = if ($AndroidCompatibility) { 'android-compatibility-diagnostics.txt' } else { 'compile-diagnostics.txt' }
$diagnostics | ForEach-Object ToString | Set-Content -LiteralPath (Join-Path $outputRoot $logName)
$errors = @($diagnostics | Where-Object Severity -eq 'Error')
Write-Output "Checked $($trees.Count) recovered C# files: $($errors.Count) errors, $($diagnostics.Count - $errors.Count) warnings."
$errors | Select-Object -First 20 | ForEach-Object ToString
if ($errors.Count -gt 0) { exit 1 }
# This checks compilation only. A runnable build also needs resources and runtime validation.
