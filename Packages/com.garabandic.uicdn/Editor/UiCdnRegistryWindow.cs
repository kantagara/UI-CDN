using System;
using System.Collections.Generic;
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
        private UiCdnComponent selected, showcase;
        private UiCdnTheme theme;
        private string packagePath;
        private Toggle showcaseToggle;
        private DropdownField genres;
        private UiCdnComponent ActiveComponent => showcaseToggle.value ? showcase : selected;

        [MenuItem("Window/UiCdn/Component Registry")]
        public static void Open() => GetWindow<UiCdnRegistryWindow>("UiCdn");

        public void CreateGUI()
        {
            minSize = new Vector2(780, 580);
            rootVisualElement.Clear();
            try
            {
                manifest = UiCdnManifest.Load();
                theme = manifest.themes.FirstOrDefault(t => t.id == UiCdnSettings.instance.SelectedTheme) ?? manifest.Theme("royal-puzzle");
                packagePath = PackageInfo.FindForAssembly(typeof(UiCdnRegistryWindow).Assembly).assetPath;
                var sheet = Load<StyleSheet>("Editor/UiCdnRegistryWindow.uss");
                if (sheet != null) rootVisualElement.styleSheets.Add(sheet);
                rootVisualElement.AddToClassList("registry");
                rootVisualElement.Add(new Label("UiCdn / Component Registry") { name = "heading" });
                rootVisualElement.Add(new Label("Copy the components. Keep the code.") { name = "subtitle" });
                namespaceField = new TextField("Installed namespace") { value = UiCdnSettings.instance.ComponentNamespace };
                rootVisualElement.Add(namespaceField);
                rootVisualElement.Add(CreateInstallFolderField());
                rootVisualElement.Add(CreateAppearanceControls());
                rootVisualElement.Add(CreateShowcaseControls());
                var search = new ToolbarSearchField();
                search.RegisterValueChangedCallback(evt => { query = evt.newValue; RenderCatalog(); });
                rootVisualElement.Add(search);
                catalog = new ScrollView { name = "catalog" };
                detail = new ScrollView { name = "detail" };
                rootVisualElement.Add(new VisualElement { name = "body" }.With(catalog, detail));
                status = new Label("Drop a Project folder into Install folder to choose where files are copied.") { name = "status" };
                rootVisualElement.Add(status);
                selected = manifest.components.First(c => c.id != "core");
                RenderSelection();
            }
            catch (Exception ex) { rootVisualElement.Add(new HelpBox(ex.Message, HelpBoxMessageType.Error)); }
        }

        private TextField CreateInstallFolderField()
        {
            var settings = UiCdnSettings.instance;
            var field = new TextField("Install folder")
            {
                name = "install-folder", value = settings.InstallFolder, isDelayed = true,
                tooltip = "Drop a folder from the Project window or enter its Assets path. Clear to restore Assets/UI."
            };
            void SelectFolder(string path)
            {
                try
                {
                    settings.SelectInstallFolder(string.IsNullOrEmpty(path) ? UiCdnSettings.DefaultInstallFolder : path);
                    status.text = $"Install destination: {settings.InstallFolder}";
                    RenderDetail();
                }
                catch (Exception ex) { status.text = ex.Message; }
                field.SetValueWithoutNotify(settings.InstallFolder);
            }
            field.RegisterValueChangedCallback(evt => SelectFolder(evt.newValue));
            field.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                var paths = DragAndDrop.paths;
                DragAndDrop.visualMode = paths.Length == 1 && AssetDatabase.IsValidFolder(paths[0]) &&
                    (paths[0] == "Assets" || paths[0].StartsWith("Assets/", StringComparison.Ordinal))
                    ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);
            field.RegisterCallback<DragPerformEvent>(evt =>
            {
                if (DragAndDrop.paths.Length == 1)
                {
                    var path = DragAndDrop.paths[0];
                    SelectFolder(path);
                    if (settings.InstallFolder == path) DragAndDrop.AcceptDrag();
                }
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);
            return field;
        }

        private VisualElement CreateAppearanceControls()
        {
            var themePicker = new DropdownField("Visual style", manifest.themes.Select(t => t.name).ToList(), theme.name) { name = "theme-picker" };
            themePicker.RegisterValueChangedCallback(evt =>
            {
                theme = manifest.themes.First(t => t.name == evt.newValue);
                UiCdnSettings.instance.SelectTheme(theme.id);
                RenderDetail();
            });
            var fontPicker = new EnumField("Display font", UiCdnSettings.instance.DisplayFont) { name = "font-picker" };
            fontPicker.RegisterValueChangedCallback(evt =>
            {
                UiCdnSettings.instance.SelectFont((UiCdnDisplayFont)evt.newValue);
                RenderDetail();
            });
            return new VisualElement { name = "appearance" }.With(themePicker, fontPicker);
        }

        private VisualElement CreateShowcaseControls()
        {
            var screens = manifest.components.Where(c => !string.IsNullOrEmpty(c.genre)).ToList();
            showcase = screens.FirstOrDefault(c => c.id == showcase?.id) ?? screens.First();
            showcaseToggle = new Toggle("Game UI showcase") { name = "theme-showcase", value = showcaseToggle?.value ?? false };
            genres = new DropdownField("Genre / screen", screens.Select(c => c.genre).ToList(), showcase.genre) { name = "genre-picker" };
            showcaseToggle.RegisterValueChangedCallback(_ => RenderSelection());
            genres.RegisterValueChangedCallback(evt =>
            {
                showcase = screens.First(c => c.genre == evt.newValue);
                RenderSelection();
            });
            return new VisualElement { name = "showcase" }.With(showcaseToggle, genres);
        }

        private void RenderCatalog()
        {
            catalog.Clear();
            foreach (var component in manifest.components.Where(c => c.id != "core" &&
                         (c.name + " " + c.description + " " + c.category + " " + c.genre).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var button = new Button(() => Select(component)) { text = component.name + "  /  " + component.category };
                button.AddToClassList("catalog-item");
                button.EnableInClassList("catalog-item--selected", component.id == ActiveComponent.id);
                catalog.Add(button);
            }
            if (catalog.childCount == 0) catalog.Add(new Label("No matching components."));
        }

        private void Select(UiCdnComponent component)
        {
            selected = component;
            showcaseToggle.SetValueWithoutNotify(false);
            RenderSelection();
        }

        private void RenderSelection()
        {
            genres.SetEnabled(showcaseToggle.value);
            RenderCatalog();
            RenderDetail();
        }

        private void RenderDetail()
        {
            detail.Clear();
            var sample = ActiveComponent;
            var componentIds = new[] { sample.id };
            var displayed = manifest.Resolve(componentIds);
            detail.Add(new Label(showcaseToggle.value ? sample.genre : sample.name) { name = "component-title" });
            detail.Add(new Label(sample.description).Class("wrap"));
            var dependencies = displayed.Where(c => c.id != sample.id).Select(c => c.name);
            detail.Add(new Label("Dependencies: " + string.Join(" → ", dependencies)));
            detail.Add(new Label(theme.name + " · " + theme.family + "\n" + theme.description).Class("wrap"));
            detail.Add(CreatePreview(sample, displayed));
            detail.Add(new Label("Live preview · Tab / arrows / submit / cancel"));
            var actions = new VisualElement(); actions.AddToClassList("actions");
            actions.Add(new Button(() => Install(componentIds)) { text = "Install " + sample.name });
            actions.Add(new Button(() => Install(manifest.components.Select(c => c.id).ToArray())) { text = "Install all" });
            actions.Add(new Button(() => Install(Array.Empty<string>())) { text = "Apply appearance" });
            detail.Add(actions);
            var files = new Foldout { text = "Copied files", value = false };
            var paths = displayed.SelectMany(c => c.files).Select(f => UiCdnSettings.instance.InstallFolder + "/" + f.target);
            files.Add(new Label("Files copied:\n" + string.Join("\n", paths)).Class("wrap"));
            detail.Add(files);
        }

        private T Load<T>(string path) where T : UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>(packagePath + "/" + path);

        private VisualElement CreatePreview(UiCdnComponent sample, IReadOnlyList<UiCdnComponent> displayed)
        {
            var preview = new VisualElement { name = "preview" }.Class("ui-root").Class(theme.cssClass);
            void AddSheet(string source) => preview.styleSheets.Add(Load<StyleSheet>(source));
            foreach (var file in manifest.foundations.Where(f => f.target == "Themes/theme-tokens.uss")) AddSheet(file.source);
            foreach (var file in displayed.SelectMany(c => c.files).Where(f => f.source.EndsWith(".uss", StringComparison.Ordinal))) AddSheet(file.source);
            var tree = Load<VisualTreeAsset>(sample.preview);
            if (tree != null) tree.CloneTree(preview);
            else preview.Add(new HelpBox("Preview could not be loaded.", HelpBoxMessageType.Warning));
            if (!string.IsNullOrEmpty(sample.genre))
                foreach (var host in preview.Children()) host.RemoveFromClassList("ui-theme-royal-puzzle");
            foreach (var file in manifest.foundations.Where(f => f.target.StartsWith("Themes/theme-", StringComparison.Ordinal) && f.target != "Themes/theme-tokens.uss"))
                AddSheet(file.source);
            var fontSheet = UiCdnSettings.FontSheet(UiCdnSettings.instance.DisplayFont);
            if (fontSheet != null) AddSheet("Editor/Registry/" + fontSheet);
            WirePreview(preview);
            return preview;
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
                var plan = UiCdnInstaller.Plan(manifest, ids, namespaceField.value.Trim(), theme.id);
                UiCdnSettings.instance.Configure(plan.Namespace);
                var count = UiCdnInstaller.Apply(plan);
                var fontName = ObjectNames.NicifyVariableName(UiCdnSettings.instance.DisplayFont.ToString());
                status.text = $"{theme.name} · {fontName}. Wrote {count} files and preserved {plan.Conflicts.Count}. Use ui-root ui-theme-selected with {UiCdnSettings.instance.InstallFolder}/ui.uss.";
                if (plan.Conflicts.Count > 0) Debug.Log("UiCdn preserved user-owned files:\n" + string.Join("\n", plan.Conflicts));
            }
            catch (Exception ex) { status.text = ex.Message; Debug.LogException(ex); }
        }
    }
}
