param(
    [Parameter(Mandatory = $true)][string]$ProjectPath,
    [string]$EditorPath = 'C:/Program Files/Unity/Hub/Editor/6000.3.7f1/Editor/Unity.exe'
)
$ErrorActionPreference = 'Stop'
# Only accept an existing disposable verification project created by verify.ps1.
$resolvedProject = (Resolve-Path -LiteralPath $ProjectPath).Path
$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
if (-not $resolvedProject.StartsWith($tempRoot + '\', [StringComparison]::OrdinalIgnoreCase) -or
    -not (Split-Path $resolvedProject -Leaf).StartsWith('uicdn-validation-') -or
    -not (Test-Path -LiteralPath "$resolvedProject/stage1-result.txt")) {
    throw 'Pass a disposable uicdn-validation-* project produced by verify.ps1.'
}
$repoRoot = Split-Path $PSScriptRoot -Parent
$packageRoot = (Join-Path $repoRoot 'Packages/com.garabandic.uicdn').Replace('\', '/')
@{ dependencies = @{ 'com.garabandic.uicdn' = "file:$packageRoot"; 'com.unity.modules.uielements' = '1.0.0' } } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath "$resolvedProject/Packages/manifest.json"
Copy-Item -LiteralPath "$PSScriptRoot/VisualVerification.cs" -Destination "$resolvedProject/Assets/Editor/VisualVerification.cs"
$previousOutput = $env:UICDN_REVIEW_OUTPUT
$env:UICDN_REVIEW_OUTPUT = Join-Path $repoRoot 'artifacts/ui-review'
try {
    $arguments = "-projectPath `"$resolvedProject`" -executeMethod UiCdnVisualVerification.Run -logFile `"$resolvedProject/visual.log`""
    $reviewProcess = Start-Process -FilePath $EditorPath -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
    if ($reviewProcess.ExitCode -ne 0) { throw "Visual review failed. See $resolvedProject/visual.log" }
    Get-Content -LiteralPath "$env:UICDN_REVIEW_OUTPUT/result.txt"
} finally { $env:UICDN_REVIEW_OUTPUT = $previousOutput }
