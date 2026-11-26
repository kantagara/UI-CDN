using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace Garabandic.UiCdn.Editor
{
    public sealed class UiCdnInstallPlan
    {
        public readonly Dictionary<string, string> Files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, byte[]> BinaryFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        internal readonly HashSet<string> PointFiltered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly List<string> Conflicts = new List<string>();
        public string Namespace { get; internal set; }
        public string Root { get; internal set; }
        public string Theme { get; internal set; }
    }

    public static class UiCdnInstaller
    {
        public static string OutputRoot => ResolveInstallFolder(UiCdnSettings.instance.InstallFolder);

        internal static string ResolveInstallFolder(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || (assetPath != "Assets" && !assetPath.StartsWith("Assets/", StringComparison.Ordinal)))
                throw new ArgumentException("Choose a project folder under Assets.");
            var full = SafePath(Path.GetDirectoryName(Application.dataPath), assetPath);
            var segments = assetPath.Split('/');
            if (segments.Contains("Editor", StringComparer.OrdinalIgnoreCase) ||
                (segments.Length > 1 && segments[1].Equals("StreamingAssets", StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Choose a runtime asset folder outside Editor and StreamingAssets.");
            if (!AssetDatabase.IsValidFolder(assetPath) && (assetPath != UiCdnSettings.DefaultInstallFolder || File.Exists(full)))
                throw new ArgumentException("Choose an existing folder from the Project window.");
            return full;
        }

        // Reject traversal and links, including links in an existing ancestor of the root.
        public static string SafePath(string root, string relative)
        {
            ValidateRelative(relative);
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var full = Path.GetFullPath(Path.Combine(fullRoot, relative));
            if (!full.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Path escapes registry root: " + relative);
            for (var current = full; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
                if ((Directory.Exists(current) || File.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("UiCdn does not follow symbolic links: " + current);
            return full;
        }

        /// <summary>
        /// Lexical check of a registry-relative path, without touching the file system. Validation must
        /// not resolve a real root: on macOS the temp folder lives under /var, itself a symbolic link.
        /// </summary>
        public static void ValidateRelative(string relative)
        {
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.Contains(":") ||
                relative.Replace('\\', '/').Split('/').Any(p => p == ".." || p == "." || p.Length == 0))
                throw new InvalidDataException("Unsafe registry path: " + relative);
        }

        public static UiCdnInstallPlan Plan(UiCdnManifest manifest, IEnumerable<string> ids, string targetNamespace, string themeId = null, UiCdnDisplayFont? displayFont = null)
        {
            UiCdnSettings.ValidateNamespace(targetNamespace);
            manifest.Validate();
            var theme = manifest.Theme(themeId ?? UiCdnSettings.instance.SelectedTheme);
            var fontSheet = UiCdnSettings.FontSheet(displayFont ?? UiCdnSettings.instance.DisplayFont);
            var examples = new HashSet<string>(manifest.components.Select(c => c.preview), StringComparer.Ordinal);
            var packageRoot = UiCdnManifest.PackageRoot;
            var plan = new UiCdnInstallPlan { Namespace = targetNamespace, Root = OutputRoot, Theme = theme.id };
            var namespacePath = SafePath(plan.Root, ".uicdn-namespace");
            if (File.Exists(namespacePath) && File.ReadAllText(namespacePath).Trim() != targetNamespace)
                throw new InvalidOperationException($"{UiCdnSettings.instance.InstallFolder} already uses another namespace. Migrate the existing files and .uicdn-namespace together before changing it.");
            var components = manifest.Resolve(ids);
            if (components.Count > 0 && !File.Exists(SafePath(plan.Root, "Components/Core/UiCore.cs")))
            {
                const string corePath = "/Components/Core/UiCore.cs";
                foreach (var guid in AssetDatabase.FindAssets("UiCore t:MonoScript", new[] { "Assets" }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.EndsWith(corePath, StringComparison.Ordinal)) continue;
                    var folder = path.Substring(0, path.Length - corePath.Length);
                    var marker = SafePath(Path.GetDirectoryName(Application.dataPath), folder + "/.uicdn-namespace");
                    if (File.Exists(marker) && File.ReadAllText(marker).Trim() == targetNamespace)
                        throw new InvalidOperationException($"{targetNamespace} components are already installed in {folder}. To relocate them, move the installed folder in the Project window and select its new location.");
                }
            }
            foreach (var file in manifest.foundations.Concat(components.SelectMany(c => c.files)))
            {
                var target = SafePath(plan.Root, file.target);
                var source = SafePath(packageRoot, file.source);
                if (file.binary)
                {
                    var bytes = File.ReadAllBytes(source);
                    plan.BinaryFiles.Add(target, bytes);
                    if (file.pointFilter) plan.PointFiltered.Add(target);
                    if (File.Exists(target) && !File.ReadAllBytes(target).SequenceEqual(bytes)) plan.Conflicts.Add(target);
                    continue;
                }
                var contents = File.ReadAllText(source);
                contents = contents.Replace(UiCdnManifest.PreviewNamespace, targetNamespace).Replace("{{NAMESPACE}}", targetNamespace);
                if (examples.Contains(file.source)) contents = InstalledExample(contents, file.target);
                plan.Files.Add(target, contents);
            }
            plan.Files[namespacePath] = targetNamespace + "\n";
            var presetPath = SafePath(plan.Root, "Themes/theme-" + theme.id + ".uss");
            // Respect edits to the installed preset when generating its selected-theme alias.
            var preset = File.Exists(presetPath) ? File.ReadAllText(presetPath) : plan.Files[presetPath];
            plan.Files[SafePath(plan.Root, "Themes/theme-active.uss")] = "/* Generated selected theme. Edit the named preset instead. */\n" + preset.Replace("." + theme.cssClass, ".ui-theme-selected");
            plan.Files[SafePath(plan.Root, ".uicdn-theme")] = theme.id + "\n";
            plan.Files[SafePath(plan.Root, "ui.uss")] = BuildStylesheet(plan, manifest, fontSheet);
            foreach (var file in plan.Files)
            {
                if (Directory.Exists(file.Key)) throw new IOException("A directory occupies a registry file path: " + file.Key);
                if (!File.Exists(file.Key) || File.ReadAllText(file.Key) == file.Value) continue;
                // The aggregate stylesheet is tool-owned; component/theme/docs files are user-owned.
                if (!IsGenerated(plan.Root, file.Key)) plan.Conflicts.Add(file.Key);
            }
            return plan;
        }

        private static string BuildStylesheet(UiCdnInstallPlan plan, UiCdnManifest manifest, string fontSheet)
        {
            var styles = new SortedSet<string>(StringComparer.Ordinal);
            var directory = SafePath(plan.Root, "Components");
            if (Directory.Exists(directory)) CollectStyles(directory, plan.Root, styles);
            foreach (var path in plan.Files.Keys)
            {
                var relative = path.Substring(plan.Root.Length + 1).Replace('\\', '/');
                if (relative.StartsWith("Components/", StringComparison.Ordinal) && relative.EndsWith(".uss", StringComparison.Ordinal))
                    styles.Add(relative);
            }
            var imports = new List<string> { "Themes/theme-tokens.uss" };
            imports.AddRange(styles);
            imports.Add("Themes/theme-game.uss");
            imports.AddRange(manifest.themes.Select(t => $"Themes/theme-{t.id}.uss"));
            imports.Add("Themes/theme-active.uss");
            if (fontSheet != null) imports.Add(fontSheet);
            return "/* UiCdn generated imports. Customize themes and component files directly. */\n" +
                string.Concat(imports.Select(path => $"@import url(\"{path}\");\n"));
        }

        // Package previews load their local sheets individually. Owned examples follow the
        // project's aggregate so later theme and font changes reach existing documents.
        private static string InstalledExample(string source, string target)
        {
            var document = XDocument.Parse(source);
            var root = document.Root;
            var ui = root.Name.Namespace;
            root.Elements(ui + "Style").Remove();
            var host = root.Elements().First(e => ((string)e.Attribute("class") ?? "").Split(' ').Contains("ui-root"));
            var classes = ((string)host.Attribute("class")).Split(' ').Where(c => !c.StartsWith("ui-theme-", StringComparison.Ordinal));
            host.SetAttributeValue("class", string.Join(" ", classes.Append("ui-theme-selected")));
            var import = string.Concat(Enumerable.Repeat("../", target.Count(c => c == '/'))) + "ui.uss";
            root.AddFirst(new XElement(ui + "Style", new XAttribute("src", import)));
            return document + "\n";
        }

        private static bool IsGenerated(string root, string path) => path == Path.Combine(root, "ui.uss") ||
            path == Path.Combine(root, "Themes", "theme-active.uss") || path == Path.Combine(root, ".uicdn-theme");

        private static void CollectStyles(string directory, string root, ISet<string> styles)
        {
            foreach (var path in Directory.GetFileSystemEntries(directory))
            {
                var relative = path.Substring(root.Length + 1).Replace('\\', '/');
                SafePath(root, relative);
                if (Directory.Exists(path)) CollectStyles(path, root, styles);
                else if (path.EndsWith(".uss", StringComparison.Ordinal))
                {
                    if (relative.IndexOfAny(new[] { '"', '\n', '\r', '(', ')' }) >= 0) throw new InvalidDataException("Unsafe stylesheet name: " + relative);
                    styles.Add(relative);
                }
            }
        }

        // Existing differing files are always preserved. No silent upgrade or overwrite mode.
        public static int Apply(UiCdnInstallPlan plan)
        {
            if (plan.Root != OutputRoot) throw new InvalidOperationException("The install folder changed. Create a new plan before installing.");
            var payload = plan.Files.Select(file => new KeyValuePair<string, byte[]>(file.Key, Encoding.UTF8.GetBytes(file.Value)))
                .Concat(plan.BinaryFiles).ToArray();
            if (payload.Select(p => p.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count() != payload.Length)
                throw new InvalidOperationException("Duplicate text and binary install targets.");
            ValidateTargets(plan.Root, payload.Select(p => p.Key));
            var namespacePath = SafePath(plan.Root, ".uicdn-namespace");
            if (File.Exists(namespacePath) && File.ReadAllText(namespacePath).Trim() != plan.Namespace)
                throw new InvalidOperationException("The project namespace changed since this install was planned.");
            var backups = new Dictionary<string, byte[]>();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var file in payload)
                {
                    SafePath(plan.Root, file.Key.Substring(plan.Root.Length + 1));
                    var exists = File.Exists(file.Key);
                    if (exists && !IsGenerated(plan.Root, file.Key)) continue;
                    var previous = exists ? File.ReadAllBytes(file.Key) : null;
                    if (previous != null && previous.SequenceEqual(file.Value)) continue;
                    backups.Add(file.Key, previous);
                    Directory.CreateDirectory(Path.GetDirectoryName(file.Key));
                    File.WriteAllBytes(file.Key, file.Value);
                }
            }
            catch (Exception original)
            {
                var failures = new List<Exception>();
                foreach (var entry in backups)
                {
                    try
                    {
                        if (entry.Value == null) { if (File.Exists(entry.Key)) File.Delete(entry.Key); }
                        else if (!File.Exists(entry.Key) || !File.ReadAllBytes(entry.Key).SequenceEqual(entry.Value)) File.WriteAllBytes(entry.Key, entry.Value);
                    }
                    catch (Exception rollbackFailure) { failures.Add(rollbackFailure); }
                }
                if (failures.Count > 0)
                    throw new AggregateException($"Install failed and some files could not be restored. Inspect {plan.Root} before retrying.", new[] { original }.Concat(failures));
                throw;
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            AssetDatabase.Refresh();
            ImportTextures(plan, backups.Keys);
            return backups.Count;
        }

        private static void ValidateTargets(string root, IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                    throw new IOException("Install target is outside the selected folder: " + path);
                SafePath(root, path.Substring(root.Length + 1));
                if (Directory.Exists(path)) throw new IOException("A directory occupies a registry file path: " + path);
                for (var parent = Path.GetDirectoryName(path); parent != null; parent = Path.GetDirectoryName(parent))
                    if (File.Exists(parent)) throw new IOException("A file occupies an install directory: " + parent);
            }
        }

        private static void ImportTextures(UiCdnInstallPlan plan, IEnumerable<string> written)
        {
            foreach (var path in written.Where(p => plan.BinaryFiles.ContainsKey(p) && p.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
            {
                var assetPath = "Assets/" + path.Substring(Path.GetFullPath(Application.dataPath).Length + 1).Replace('\\', '/');
                if (!(AssetImporter.GetAtPath(assetPath) is TextureImporter importer)) continue;
                importer.textureType = TextureImporterType.GUI;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = plan.PointFiltered.Contains(path) ? FilterMode.Point : FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
    }
}
