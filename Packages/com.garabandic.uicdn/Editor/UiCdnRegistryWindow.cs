using System;
using System.IO;
using System.Linq;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Garabandic.UiCdn.Editor.Preview;

namespace Garabandic.UiCdn.Editor
{
    public sealed class UiCdnRegistryWindow : EditorWindow
    {
        private UiCdnManifest manifest;
        private VisualElement catalog;
        private VisualElement detail;
        private TextField namespaceField;
        private Label status;
        private string query = "";
        private string selectedId;
        private string themeId;
        private bool gamePreview;

        [MenuItem("Window/UiCdn/Component Registry")]
        public static void Open() => GetWindow<UiCdnRegistryWindow>("UiCdn");

        public void CreateGUI()
        {
            minSize = new Vector2(780, 580);
            rootVisualElement.Clear();
            try
            {
                manifest = UiCdnManifest.Load();
                themeId = manifest.themes.Any(t => t.id == UiCdnSettings.instance.SelectedTheme) ? UiCdnSettings.instance.SelectedTheme : "royal-puzzle";
                var package = PackageInfo.FindForAssembly(typeof(UiCdnRegistryWindow).Assembly);
                var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(package.assetPath + "/Editor/UiCdnRegistryWindow.uss");
                if (sheet != null) rootVisualElement.styleSheets.Add(sheet);
                rootVisualElement.AddToClassList("registry");
                rootVisualElement.Add(new Label("UiCdn / Component Registry") { name = "heading" });
                rootVisualElement.Add(new Label("Copy the components. Keep the code.") { name = "subtitle" });
                namespaceField = new TextField("Installed namespace") { value = UiCdnSettings.instance.ComponentNamespace };
                rootVisualElement.Add(namespaceField);
                var themePicker = new DropdownField("Visual style", manifest.themes.Select(t => t.name).ToList(), Array.FindIndex(manifest.themes, t => t.id == themeId)) { name = "theme-picker" };
                themePicker.RegisterValueChangedCallback(evt =>
                {
                    themeId = manifest.themes.First(t => t.name == evt.newValue).id;
                    UiCdnSettings.instance.SelectTheme(themeId);
                    if (detail != null) Select(manifest.components.First(c => c.id == selectedId));
                });
                var appearance = new VisualElement { name = "appearance" };
                appearance.Add(themePicker);
                var fontPicker = new EnumField("Display font", UiCdnSettings.instance.DisplayFont) { name = "font-picker" };
                fontPicker.RegisterValueChangedCallback(evt =>
                {
                    UiCdnSettings.instance.SelectFont((UiCdnDisplayFont)evt.newValue);
                    Select(manifest.components.First(c => c.id == selectedId));
                });
                appearance.Add(fontPicker);
                rootVisualElement.Add(appearance);
                var showcaseToggle = new Toggle("Game UI showcase") { name = "theme-showcase", value = gamePreview };
                showcaseToggle.RegisterValueChangedCallback(evt => { gamePreview = evt.newValue; Select(manifest.components.First(c => c.id == selectedId)); });
                rootVisualElement.Add(showcaseToggle);
                var search = new ToolbarSearchField();
                search.RegisterValueChangedCallback(evt => { query = evt.newValue; RenderCatalog(); });
                rootVisualElement.Add(search);
                var body = new VisualElement { name = "body" };
                catalog = new ScrollView { name = "catalog" };
                detail = new ScrollView { name = "detail" };
                body.Add(catalog);
                body.Add(detail);
                rootVisualElement.Add(body);
                status = new Label("Installs to Assets/UI. Existing differing files are preserved.") { name = "status" };
                rootVisualElement.Add(status);
                RenderCatalog();
                Select(manifest.components.First(c => c.id != "core"));
            }
            catch (Exception ex) { rootVisualElement.Add(new HelpBox(ex.Message, HelpBoxMessageType.Error)); }
        }

        private void RenderCatalog()
        {
            catalog.Clear();
            foreach (var component in manifest.components.Where(c => c.id != "core" &&
                         (c.name + " " + c.description + " " + c.category).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var button = new Button(() => Select(component)) { text = component.name + "  /  " + component.category };
                button.AddToClassList("catalog-item");
                button.EnableInClassList("catalog-item--selected", component.id == selectedId);
                catalog.Add(button);
            }
            if (catalog.childCount == 0) catalog.Add(new Label("No matching components."));
        }

        private void Select(UiCdnComponent component)
        {
            selectedId = component.id;
            RenderCatalog();
            detail.Clear();
            var componentIds = gamePreview ? new[] { "treasureshop" } : new[] { component.id };
            var displayed = manifest.Resolve(componentIds);
            detail.Add(new Label(gamePreview ? "Game UI showcase" : component.name) { name = "component-title" });
            var description = new Label(gamePreview ? "Choose a bundle or a coin pack." : component.description);
            description.AddToClassList("wrap"); detail.Add(description);
            var dependencies = displayed.Where(c => gamePreview || c.id != component.id).Select(c => c.name);
            detail.Add(new Label("Dependencies: " + string.Join(" → ", dependencies)));
            var theme = manifest.Theme(themeId);
            var themeDescription = new Label(theme.name + " · " + theme.family + "\n" + theme.description);
            themeDescription.AddToClassList("wrap"); detail.Add(themeDescription);
            var preview = new VisualElement { name = "preview" };
            preview.AddToClassList("ui-root"); preview.AddToClassList(theme.cssClass);
            var package = PackageInfo.FindForAssembly(typeof(UiCdnRegistryWindow).Assembly);
            foreach (var foundation in manifest.foundations.Where(f => f.target == "Themes/theme-tokens.uss"))
                preview.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(package.assetPath + "/" + foundation.source));
            foreach (var style in displayed.SelectMany(c => c.files).Where(f => f.source.EndsWith(".uss", StringComparison.Ordinal)))
                preview.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(package.assetPath + "/" + style.source));
            var sample = gamePreview ? manifest.components.First(c => c.id == "treasureshop") : component;
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(package.assetPath + "/" + sample.preview);
            if (tree != null) tree.CloneTree(preview);
            else preview.Add(new HelpBox("Preview could not be loaded.", HelpBoxMessageType.Warning));
            if (sample.id == "treasureshop") preview.Q<UiTreasureShop>()?.parent.RemoveFromClassList("ui-theme-royal-puzzle");
            // Same cascade order as the installed aggregate: tokens, components, then theme overrides.
            foreach (var foundation in manifest.foundations.Where(f => f.target.StartsWith("Themes/theme-", StringComparison.Ordinal) && f.target != "Themes/theme-tokens.uss"))
                preview.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(package.assetPath + "/" + foundation.source));
            var fontSheet = UiCdnSettings.FontSheet(UiCdnSettings.instance.DisplayFont);
            if (fontSheet != null)
                preview.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(package.assetPath + "/Editor/Registry/" + fontSheet));
            WirePreview(preview);
            detail.Add(preview);
            detail.Add(new Label("Live preview · Tab / arrows / submit / cancel"));
            var paths = new Label("Files copied:\n" + string.Join("\n", displayed.SelectMany(c => c.files).Select(f => "Assets/UI/" + f.target)));
            paths.AddToClassList("wrap");
            var actions = new VisualElement(); actions.AddToClassList("actions");
            actions.Add(new Button(() => Install(componentIds)) { text = gamePreview ? "Install shop screen" : "Install " + component.name });
            actions.Add(new Button(() => Install(manifest.components.Select(c => c.id).ToArray())) { text = "Install all" });
            actions.Add(new Button(() => Install(Array.Empty<string>())) { text = "Apply appearance" });
            detail.Add(actions);
            var files = new Foldout { text = "Copied files", value = false };
            files.Add(paths);
            detail.Add(files);
        }

        private static void WirePreview(VisualElement root)
        {
            var dialog = root.Q<UiDialog>();
            var show = root.Q<UiButton>("open-dialog");
            if (show != null && dialog != null) show.clicked += dialog.Show;
            var close = root.Q<UiButton>("close-dialog");
            if (close != null && dialog != null) close.clicked += dialog.Hide;
            var toast = root.Q<UiToast>();
            var notify = root.Q<UiButton>("show-toast");
            if (notify != null && toast != null) notify.clicked += toast.Show;
            var sample = root.Q<UiButton>("sample-button");
            if (sample != null) sample.clicked += () => sample.text = "Ready!";
        }

        private void Install(string[] ids)
        {
            try
            {
                var plan = UiCdnInstaller.Plan(manifest, ids, namespaceField.value.Trim(), themeId);
                UiCdnSettings.instance.Configure(plan.Namespace);
                var count = UiCdnInstaller.Apply(plan);
                var fontName = ObjectNames.NicifyVariableName(UiCdnSettings.instance.DisplayFont.ToString());
                status.text = $"{manifest.Theme(themeId).name} · {fontName}. Wrote {count} files and preserved {plan.Conflicts.Count}. Use ui-root ui-theme-selected with Assets/UI/ui.uss.";
                if (plan.Conflicts.Count > 0) Debug.Log("UiCdn preserved user-owned files:\n" + string.Join("\n", plan.Conflicts));
            }
            catch (Exception ex) { status.text = ex.Message; Debug.LogException(ex); }
        }
    }
}
