param([string]$EditorPath = 'C:/Program Files/Unity/Hub/Editor/6000.3.7f1/Editor/Unity.exe')
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$checkRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('uicdn-validation-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path "$checkRoot/Assets/Editor", "$checkRoot/Packages", "$checkRoot/ProjectSettings" | Out-Null
$packageRoot = (Join-Path $repoRoot 'Packages/com.garabandic.uicdn').Replace('\', '/')
@{ dependencies = @{ 'com.garabandic.uicdn' = "file:$packageRoot"; 'com.unity.modules.uielements' = '1.0.0' } } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath "$checkRoot/Packages/manifest.json"
'm_EditorVersion: 6000.3.7f1' | Set-Content -LiteralPath "$checkRoot/ProjectSettings/ProjectVersion.txt"
Copy-Item -LiteralPath "$PSScriptRoot/Verification.cs" -Destination "$checkRoot/Assets/Editor/Verification.cs"
Write-Output "Validation project: $checkRoot"
function Invoke-Check([string]$stage, [string]$method) {
    $arguments = "-batchmode -nographics -projectPath `"$checkRoot`" -executeMethod $method -logFile `"$checkRoot/$stage.log`" -accept-apiupdate"
    $editorProcess = Start-Process -FilePath $EditorPath -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
    if ($editorProcess.ExitCode -ne 0) { throw "$stage failed. See $checkRoot/$stage.log" }
}
Invoke-Check 'stage1' 'UiCdnVerification.Run'
Get-Content -LiteralPath "$checkRoot/stage1-result.txt"
# Installed source is compiled in Assembly-CSharp after the installer triggers a refresh.
@'
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
public static class InstalledVerification
{
    public static void Run()
    {
        try
        {
            var button = new Game.UI.UiButton("Owned by the game");
            if (button.GetType().Assembly.GetReferencedAssemblies().Any(a => a.Name.Contains("UiCdn"))) throw new Exception("Runtime assembly references UiCdn");
            foreach (var path in Directory.GetFiles(Application.dataPath + "/UI", "*.uxml", SearchOption.AllDirectories))
            {
                var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets" + path.Substring(Application.dataPath.Length).Replace(Path.DirectorySeparatorChar, '/'));
                if (tree == null || !tree.CloneTree().Query<VisualElement>().ToList().Any(e => e.GetType().Namespace == "Game.UI") && !path.Contains("CoreExample")) throw new Exception("Installed UXML failed: " + path);
            }
            var shopAssets = AssetDatabase.GetDependencies("Assets/UI/Components/TreasureShop/TreasureShopExample.uxml", true);
            if (!shopAssets.Contains("Assets/UI/Themes/font-fredoka.uss") || !shopAssets.Contains("Assets/UI/Themes/Assets/Fredoka-SemiBold.ttf") ||
                shopAssets.Any(p => p.StartsWith("Packages/com.garabandic.uicdn/", StringComparison.Ordinal)))
                throw new Exception("Installed shop must import the selected project-owned font and styles.");
            File.WriteAllText(Application.dataPath + "/../installed-result.txt", "PASS: installed C#, source-generated UXML, zero package references");
            EditorApplication.Exit(0);
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
    }
}
'@ | Set-Content -LiteralPath "$checkRoot/Assets/Editor/Verification.cs"
Invoke-Check 'stage2' 'InstalledVerification.Run'
Get-Content -LiteralPath "$checkRoot/installed-result.txt"
# Remove the Editor tool from this throwaway project and compile again.
'{"dependencies":{"com.unity.modules.uielements":"1.0.0"}}' | Set-Content -LiteralPath "$checkRoot/Packages/manifest.json"
Invoke-Check 'stage3' 'InstalledVerification.Run'
Write-Output 'PASS: installed components still compile and UXML still clones after package removal.'
Write-Output "Logs retained: $checkRoot"
