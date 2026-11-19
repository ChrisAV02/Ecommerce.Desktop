param([switch]$SkipBuild)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
if (-not $SkipBuild) {
    & dotnet build (Join-Path $projectRoot 'Ecommerce.Desktop.csproj')
    if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación del Desktop.' }
}

$dotnetRoot = Split-Path (Get-Command dotnet).Source -Parent
$sdkVersion = (& dotnet --version).Trim()
$compiler = Join-Path $dotnetRoot "sdk/$sdkVersion/Roslyn/bincore/csc.dll"
$references = foreach ($packName in @('Microsoft.NETCore.App.Ref', 'Microsoft.WindowsDesktop.App.Ref')) {
    $pack = Get-ChildItem (Join-Path $dotnetRoot "packs/$packName") -Directory |
        Where-Object { $_.Name -like '10.*' } | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
    Get-ChildItem (Join-Path $pack.FullName 'ref/net10.0') -Filter '*.dll'
}
$output = Join-Path $projectRoot 'bin/Debug/net10.0-windows'
$testDirectory = Join-Path ([IO.Path]::GetTempPath()) ('DespensaCerca-Ui-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDirectory | Out-Null
Copy-Item (Join-Path $output 'Ecommerce.Desktop.dll') $testDirectory
Get-ChildItem $output -Filter 'Microsoft.Web.WebView2.*.dll' | Copy-Item -Destination $testDirectory
Copy-Item (Join-Path $output 'Resources') $testDirectory -Recurse
$mapLoader = Join-Path $output ('runtimes/win-' + [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString().ToLowerInvariant() + '/native/WebView2Loader.dll')
if (Test-Path $mapLoader) { Copy-Item $mapLoader $testDirectory }
Copy-Item (Join-Path $output 'Ecommerce.Desktop.runtimeconfig.json') (Join-Path $testDirectory 'UiSmokeTests.runtimeconfig.json')
$testAssembly = Join-Path $testDirectory 'UiSmokeTests.dll'
$arguments = @('-nologo', '-target:exe', '-nullable:enable', '-langversion:latest', "-out:`"$testAssembly`"")
$arguments += $references | ForEach-Object { '-r:"' + $_.FullName + '"' }
$arguments += '-r:"' + (Join-Path $output 'Ecommerce.Desktop.dll') + '"'
$arguments += Get-ChildItem $output -Filter 'Microsoft.Web.WebView2.*.dll' | ForEach-Object { '-r:"' + $_.FullName + '"' }
$arguments += '"' + (Join-Path $PSScriptRoot 'UiSmokeTests.cs') + '"'
$responseFile = Join-Path $testDirectory 'compiler.rsp'
Set-Content -LiteralPath $responseFile -Value $arguments -Encoding utf8
& dotnet $compiler "@$responseFile"
if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación de las verificaciones UI.' }
& dotnet $testAssembly
if ($LASTEXITCODE -ne 0) { throw 'Falló la verificación UI.' }
Write-Host "Artefactos temporales de verificación: $testDirectory"
