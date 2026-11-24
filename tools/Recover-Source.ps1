param([string]$GameDirectory = (Resolve-Path "$PSScriptRoot\..\..").Path)
$ErrorActionPreference = 'Stop'
Add-Type -Path "$PSScriptRoot\decompiler\lib\netstandard2.0\ICSharpCode.Decompiler.dll"
$outputRoot = Join-Path $GameDirectory 'AndroidPort\recovered'
$settings = [ICSharpCode.Decompiler.DecompilerSettings]::new()
$settings.ThrowOnAssemblyResolveErrors = $false
foreach ($assemblyName in @('JnG2.exe', 'FarseerPhysics MonoGame.dll', 'spine-csharp.dll')) {
    $assemblyPath = Join-Path $GameDirectory $assemblyName
    $resolver = [ICSharpCode.Decompiler.Metadata.UniversalAssemblyResolver]::new(
        $assemblyPath, $false, '.NETFramework,Version=v4.6', $null,
        [System.Reflection.PortableExecutable.PEStreamOptions]::PrefetchEntireImage,
        [System.Reflection.Metadata.MetadataReaderOptions]::ApplyWindowsRuntimeProjections)
    $resolver.AddSearchDirectory($GameDirectory)
    $project = [ICSharpCode.Decompiler.CSharp.ProjectDecompiler.WholeProjectDecompiler]::new(
        $settings, $resolver, $null, $null, $null)
    $module = [ICSharpCode.Decompiler.Metadata.PEFile]::new(
        $assemblyPath, [System.Reflection.PortableExecutable.PEStreamOptions]::PrefetchEntireImage,
        [System.Reflection.Metadata.MetadataReaderOptions]::ApplyWindowsRuntimeProjections, $null)
    try {
        $destination = Join-Path $outputRoot ([IO.Path]::GetFileNameWithoutExtension($assemblyName))
        New-Item -ItemType Directory -Force -Path $destination | Out-Null
        $project.DecompileProject($module, $destination, [Threading.CancellationToken]::None)
        $errors = @($project.Errors)
        Write-Output "$assemblyName recovered to $destination ($($errors.Count) reported errors)"
        $errors | Out-String | Set-Content -LiteralPath (Join-Path $destination 'recovery-errors.txt')
    } finally {
        $module.Dispose()
    }
}
