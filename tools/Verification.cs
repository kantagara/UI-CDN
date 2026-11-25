// Copied into an isolated test project's Assets/Editor by verify.ps1.
using System;
using System.IO;
using System.Linq;
using Garabandic.UiCdn.Editor;
using Garabandic.UiCdn.Editor.Preview;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEngine;
using UnityEngine.UIElements;

public static class UiCdnVerification
{
    private static int count;
    private static void Check(bool condition, string message)
    {
        count++;
        if (!condition) throw new Exception("FAILED: " + message);
    }
    private static void Throws(Action action, string message)
    {
        try { action(); } catch { count++; return; }
        throw new Exception("FAILED (expected exception): " + message);
    }
    public static void Run()
    {
        try
        {
            var manifest = UiCdnManifest.Load();
            Check(manifest.themes.Length == 11, "theme catalog size");
            foreach (UiCdnDisplayFont font in Enum.GetValues(typeof(UiCdnDisplayFont)))
            {
                var appearance = UiCdnInstaller.Plan(manifest, new[] { "treasureshop" }, "Game.UI", "candy-pop", font);
                var aggregate = appearance.Files[Path.Combine(UiCdnInstaller.OutputRoot, "ui.uss")];
                Check(font == UiCdnDisplayFont.ThemeDefault ? !aggregate.Contains("Themes/font-") : aggregate.Contains("Themes/font-" + font.ToString().ToLowerInvariant() + ".uss"), "aggregate display font " + font);
                var example = appearance.Files[Path.Combine(UiCdnInstaller.OutputRoot, "Components", "TreasureShop", "TreasureShopExample.uxml")];
                Check(example.Contains("../../ui.uss") && example.Contains("ui-theme-selected"), "installed shop follows appearance");
            }
            Throws(() => manifest.Theme("missing-theme"), "unknown theme rejected");
            foreach (var theme in manifest.themes)
            {
                var themedPlan = UiCdnInstaller.Plan(manifest, new[] { "button" }, "Game.UI", theme.id);
                var activeSheet = themedPlan.Files[Path.Combine(UiCdnInstaller.OutputRoot, "Themes", "theme-active.uss")];
                Check(activeSheet.Contains(".ui-theme-selected") && !activeSheet.Contains("." + theme.cssClass), "selected alias generated for " + theme.id);
                Check(themedPlan.Files[Path.Combine(UiCdnInstaller.OutputRoot, "ui.uss")].Contains("theme-" + theme.id + ".uss"), "theme import " + theme.id);
            }
            Check(manifest.components.Length == 16, "catalog size");
            var resolved = manifest.Resolve(new[] { "dialog", "tabs" });
            Check(resolved[0].id == "core" && resolved.Count(c => c.id == "button") == 1, "topological order and deduplication");
            Throws(() => manifest.Resolve(new[] { "missing" }), "missing dependency");
            var core = manifest.components.First(c => c.id == "core");
            core.dependencies = new[] { "dialog" };
            Throws(() => manifest.Resolve(new[] { "dialog" }), "cycles");
            core.dependencies = Array.Empty<string>();
            foreach (var path in new[] { "../outside", "C:/outside", "a/../../outside", "a\\..\\outside", "a//b" })
                Throws(() => UiCdnInstaller.SafePath(Application.dataPath, path), "path traversal " + path);
            foreach (var ns in new[] { "", "Game.class", "Game..UI", "Bad-Name", "Unity.UI", UiCdnManifest.PreviewNamespace })
                Throws(() => UiCdnSettings.ValidateNamespace(ns), "invalid namespace " + ns);
            UiCdnSettings.ValidateNamespace("Studio.Game.UI");

            var card = new UiCard().With(new UiCardTitle("Inventory"));
            Check(card.childCount == 1 && card.Q<UiCardTitle>().text == "Inventory", "fluent composition");
            var button = new UiButton("Test").WithVariant("ghost");
            Check(button.focusable && button.ClassListContains("ui-button--ghost") && !button.ClassListContains("ui-button--primary"), "button variants");
            int changes = 0;
            var checkbox = new UiCheckbox("Subtitles").OnChanged(_ => changes++);
            checkbox.value = true;
            Check(checkbox.value, "checkbox native value");
            var input = new UiInput("Name", "Player") { Invalid = true };
            Check(input.value == "Player" && input.ClassListContains("ui-input--invalid"), "input validation state");
            var progress = new UiProgress(500);
            Check(progress.value == 100, "progress clamps range");
            var dialog = new UiDialog();
            var lifecycle = "";
            dialog.OnBeforeOpen += () => lifecycle += "B";
            dialog.OnAfterOpen += () => lifecycle += "A";
            dialog.OnBeforeClose += () => lifecycle += "b";
            dialog.OnAfterClose += () => lifecycle += "a";
            dialog.Show(); dialog.Show(); dialog.Hide(); dialog.Hide();
            Check(lifecycle == "BAba" && dialog.style.display.value == DisplayStyle.None, "motion lifecycle and idempotence");
            var tabs = new UiTabs().With(new UiTabsList().With(new UiTabsTrigger("a", "A"), new UiTabsTrigger("b", "B")), new UiTabsContent("a"), new UiTabsContent("b"));
            tabs.Refresh(); tabs.Select("b");
            Check(tabs.Value == "b" && tabs.Query<UiTabsContent>().ToList().Last().style.display.value == DisplayStyle.Flex, "tab selection");
            tabs.Select("absent"); Check(tabs.Value == "b", "invalid tab ignored");

            var package = PackageInfo.FindForAssembly(typeof(UiCdnManifest).Assembly);
            foreach (var component in manifest.components)
            {
                var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(package.assetPath + "/" + component.preview);
                Check(tree != null, "import UXML " + component.id);
                var clone = tree.CloneTree();
                Check(clone.childCount > 0, "clone UXML " + component.id);
                if (component.id != "core") Check(clone.Query<VisualElement>().ToList().Any(e => e.GetType().Namespace == UiCdnManifest.PreviewNamespace), "custom UXML source generation " + component.id);
            }
            File.WriteAllText(Path.Combine(Application.dataPath, "Attributes.uxml"), "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\" xmlns:c=\"Garabandic.UiCdn.Editor.Preview\"><c:UiDialog open=\"true\" animation-duration-ms=\"7\" /><c:UiToast open=\"true\" duration-ms=\"0\" /><c:UiTabs value=\"b\"><c:UiTabsList><c:UiTabsTrigger value=\"a\" text=\"A\" /><c:UiTabsTrigger value=\"b\" text=\"B\" /></c:UiTabsList></c:UiTabs></ui:UXML>");
            AssetDatabase.ImportAsset("Assets/Attributes.uxml", ImportAssetOptions.ForceSynchronousImport);
            var attributes = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Attributes.uxml").CloneTree();
            Check(attributes.Q<UiDialog>().Open && attributes.Q<UiDialog>().AnimationDurationMs == 7, "inherited UXML motion attributes");
            Check(attributes.Q<UiToast>().Open && attributes.Q<UiToast>().DurationMs == 0, "toast declarative open state");
            attributes.Q<UiTabs>().Refresh();
            Check(attributes.Q<UiTabs>().Value == "b", "UXML initial non-first tab survives child deserialization");
            AssetDatabase.DeleteAsset("Assets/Attributes.uxml");
            var alternate = UiCdnInstaller.Plan(manifest, new[] { "dialog" }, "Studio.Interface");
            Check(alternate.Files.Values.Any(s => s.Contains("namespace Studio.Interface")), "custom C# namespace");
            Check(alternate.Files.Values.Any(s => s.Contains("xmlns:c=\"Studio.Interface\"")), "custom UXML namespace");
            Check(alternate.Files.Values.Any(s => s.Contains("Namespace: `Studio.Interface`")), "custom agent guide namespace");
            var plan = UiCdnInstaller.Plan(manifest, manifest.components.Select(c => c.id), "Game.UI");
            Check(plan.BinaryFiles.Count == manifest.foundations.Count(f => f.binary), "every bundled font, illustration and surface copied as binary");
            Check(plan.Files.Values.All(s => !s.Contains(UiCdnManifest.PreviewNamespace)), "namespace replacement");
            Check(UiCdnInstaller.Apply(plan) > 0, "installation writes files");
            var artPath = Path.Combine(UiCdnInstaller.OutputRoot, "Themes", "Assets", "royal-action.png");
            Check(File.ReadAllBytes(artPath).SequenceEqual(plan.BinaryFiles[artPath]), "artwork copied without text conversion");
            var artImporter = AssetImporter.GetAtPath("Assets/UI/Themes/Assets/royal-action.png") as TextureImporter;
            Check(artImporter != null && !artImporter.mipmapEnabled && artImporter.npotScale == TextureImporterNPOTScale.None && artImporter.textureCompression == TextureImporterCompression.Uncompressed, "UI artwork imported without resizing or compression");
            var customPath = Path.GetFullPath(Path.Combine(Application.dataPath, "UI/Components/Button/UiButton.cs"));
            File.AppendAllText(customPath, "\n// user customization\n");
            var repeat = UiCdnInstaller.Plan(manifest, new[] { "button" }, "Game.UI");
            Check(repeat.Conflicts.Contains(customPath), "edited files reported");
            UiCdnInstaller.Apply(repeat);
            Check(File.ReadAllText(customPath).Contains("// user customization"), "edited files preserved");
            Check(File.ReadAllText(Path.Combine(Application.dataPath, "UI/ui.uss")).Contains("Components/Dialog/UiDialog.uss"), "incremental import union");
            Throws(() => UiCdnInstaller.Plan(manifest, new[] { "card" }, "Other.UI"), "namespace drift blocked");
            var directoryCollision = UiCdnInstaller.Plan(manifest, new[] { "core" }, "Game.UI");
            var occupied = Path.Combine(UiCdnInstaller.OutputRoot, "directory-collision.cs");
            Directory.CreateDirectory(occupied);
            directoryCollision.Files.Add(occupied, "// never written");
            Throws(() => UiCdnInstaller.Apply(directoryCollision), "directory collision preflight");
            Directory.Delete(occupied);
            var rollback = UiCdnInstaller.Plan(manifest, new[] { "core" }, "Game.UI");
            var aggregatePath = Path.Combine(UiCdnInstaller.OutputRoot, "ui.uss");
            var originalAggregate = File.ReadAllText(aggregatePath);
            rollback.Files.Remove(aggregatePath);
            var rolledBack = Path.Combine(UiCdnInstaller.OutputRoot, "rollback-probe.txt");
            rollback.Files.Add(rolledBack, "must be rolled back");
            rollback.Files.Add(aggregatePath, originalAggregate + "\n/* simulated update */\n");
            var attributesBefore = File.GetAttributes(aggregatePath);
            try
            {
                File.SetAttributes(aggregatePath, attributesBefore | FileAttributes.ReadOnly);
                Throws(() => UiCdnInstaller.Apply(rollback), "simulated write failure");
                Check(!File.Exists(rolledBack) && File.ReadAllText(aggregatePath) == originalAggregate, "transaction rolled back without changing readonly file");
            }
            finally { File.SetAttributes(aggregatePath, attributesBefore); }
            var presetPath = Path.Combine(UiCdnInstaller.OutputRoot, "Themes", "theme-casual-royal.uss");
            File.AppendAllText(presetPath, "\n/* owned palette edit */\n");
            var themeOnly = UiCdnInstaller.Plan(manifest, Array.Empty<string>(), "Game.UI", "casual-royal", UiCdnDisplayFont.Fredoka);
            Check(themeOnly.Conflicts.Contains(presetPath), "customized palette reported");
            UiCdnInstaller.Apply(themeOnly);
            Check(File.ReadAllText(presetPath).Contains("owned palette edit"), "theme edit preserved");
            Check(File.ReadAllText(Path.Combine(UiCdnInstaller.OutputRoot, "Themes", "theme-active.uss")).Contains("owned palette edit"), "selected alias uses owned palette");
            Check(File.ReadAllText(Path.Combine(UiCdnInstaller.OutputRoot, ".uicdn-theme")).Trim() == "casual-royal", "theme-only apply changes selection");
            Check(File.ReadAllText(aggregatePath).Contains("Themes/font-fredoka.uss"), "display font applied to existing screens");
            Check(File.ReadAllText(customPath).Contains("// user customization"), "theme switching preserves runtime component edits");
            // Confirm a real EditorWindow can construct its full catalog and preview.
            var window = ScriptableObject.CreateInstance<UiCdnRegistryWindow>();
            window.CreateGUI();
            Check(window.rootVisualElement.Q("preview") != null, "registry window builds");
            UnityEngine.Object.DestroyImmediate(window);
            File.WriteAllText(Path.Combine(Application.dataPath, "../stage1-result.txt"), $"PASS: {count} assertions\n");
            Debug.Log($"UICDN PASS: {count} assertions");
            EditorApplication.Exit(0);
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
    }
}
