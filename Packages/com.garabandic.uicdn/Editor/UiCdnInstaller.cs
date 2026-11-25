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
        public static string OutputRoot => Path.GetFullPath(Path.Combine(Application.dataPath, "UI"));

        // Reject traversal and links, including links in an existing ancestor of the root.
        public static string SafePath(string root, string relative)
        {
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.Contains(":") ||
                relative.Replace('\\', '/').Split('/').Any(p => p == ".." || p == "." || p.Length == 0))
                throw new InvalidDataException("Unsafe registry path: " + relative);
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var full = Path.GetFullPath(Path.Combine(fullRoot, relative));
            if (!full.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Path escapes registry root: " + relative);
            for (var current = full; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
                if ((Directory.Exists(current) || File.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("UiCdn does not follow symbolic links: " + current);
            return full;
        }

        public static UiCdnInstallPlan Plan(UiCdnManifest manifest, IEnumerable<string> ids, string targetNamespace, string themeId = null, UiCdnDisplayFont? displayFont = null)
        {
            UiCdnSettings.ValidateNamespace(targetNamespace);
            manifest.Validate();
            var theme = manifest.Theme(themeId ?? UiCdnSettings.instance.SelectedTheme);
            var fontSheet = UiCdnSettings.FontSheet(displayFont ?? UiCdnSettings.instance.DisplayFont);
            var examples = new HashSet<string>(manifest.components.Select(c => c.preview), StringComparer.Ordinal);
            var plan = new UiCdnInstallPlan { Namespace = targetNamespace, Root = OutputRoot, Theme = theme.id };
            var namespacePath = SafePath(plan.Root, ".uicdn-namespace");
            if (File.Exists(namespacePath) && File.ReadAllText(namespacePath).Trim() != targetNamespace)
                throw new InvalidOperationException("Assets/UI already uses another namespace. Migrate the existing files and .uicdn-namespace together before changing it.");
            foreach (var file in manifest.foundations.Concat(manifest.Resolve(ids).SelectMany(c => c.files)))
            {
                var target = SafePath(plan.Root, file.target);
                if (file.binary)
                {
                    plan.BinaryFiles.Add(target, File.ReadAllBytes(SafePath(UiCdnManifest.PackageRoot, file.source)));
                    if (file.pointFilter) plan.PointFiltered.Add(target);
                    if (File.Exists(target) && !File.ReadAllBytes(target).SequenceEqual(plan.BinaryFiles[target])) plan.Conflicts.Add(target);
                    continue;
                }
                var contents = File.ReadAllText(SafePath(UiCdnManifest.PackageRoot, file.source));
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
            // Rebuild imports from the union of installed and requested component styles.
            var styles = new SortedSet<string>(StringComparer.Ordinal);
            var componentsRoot = SafePath(plan.Root, "Components");
            if (Directory.Exists(componentsRoot))
            {
                CollectStyles(componentsRoot, plan.Root, styles);
            }
            foreach (var file in plan.Files.Keys.Where(p => p.EndsWith(".uss", StringComparison.Ordinal) && p.Contains(Path.DirectorySeparatorChar + "Components" + Path.DirectorySeparatorChar)))
                styles.Add(file.Substring(plan.Root.Length + 1).Replace('\\', '/'));
            var imports = new StringBuilder("/* UiCdn generated imports. Customize themes and component files directly. */\n@import url(\"Themes/theme-tokens.uss\");\n");
            foreach (var style in styles) imports.Append("@import url(\"").Append(style).Append("\");\n");
            imports.Append("@import url(\"Themes/theme-game.uss\");\n");
            foreach (var item in manifest.themes) imports.Append("@import url(\"Themes/theme-").Append(item.id).Append(".uss\");\n");
            imports.Append("@import url(\"Themes/theme-active.uss\");\n");
            if (fontSheet != null) imports.Append("@import url(\"").Append(fontSheet).Append("\");\n");
            plan.Files[SafePath(plan.Root, "ui.uss")] = imports.ToString();
            foreach (var file in plan.Files)
            {
                if (Directory.Exists(file.Key)) throw new IOException("A directory occupies a registry file path: " + file.Key);
                if (!File.Exists(file.Key) || File.ReadAllText(file.Key) == file.Value) continue;
                // The aggregate stylesheet is tool-owned; component/theme/docs files are user-owned.
                if (!IsGenerated(plan.Root, file.Key)) plan.Conflicts.Add(file.Key);
            }
            return plan;
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
            if (plan.Root != OutputRoot) throw new InvalidOperationException("Install plans may only write to Assets/UI.");
            var payload = plan.Files.Select(file => new KeyValuePair<string, byte[]>(file.Key, new UTF8Encoding(false).GetBytes(file.Value)))
                .Concat(plan.BinaryFiles).ToArray();
            if (payload.Select(p => p.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count() != payload.Length)
                throw new InvalidOperationException("Duplicate text and binary install targets.");
            foreach (var path in payload.Select(p => p.Key))
            {
                if (!path.StartsWith(plan.Root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("Install target is outside Assets/UI: " + path);
                SafePath(plan.Root, path.Substring(plan.Root.Length + 1));
                if (Directory.Exists(path)) throw new IOException("A directory occupies a registry file path: " + path);
                for (var parent = Path.GetDirectoryName(path); parent != null; parent = Path.GetDirectoryName(parent))
                    if (File.Exists(parent)) throw new IOException("A file occupies an install directory: " + parent);
            }
            var namespacePath = SafePath(plan.Root, ".uicdn-namespace");
            if (File.Exists(namespacePath) && File.ReadAllText(namespacePath).Trim() != plan.Namespace)
                throw new InvalidOperationException("The project namespace changed since this install was planned.");
            var written = new List<string>();
            var backups = new Dictionary<string, byte[]>();
            var refresh = false;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var file in payload)
                {
                    SafePath(plan.Root, file.Key.Substring(plan.Root.Length + 1));
                    var exists = File.Exists(file.Key);
                    if (exists && (!IsGenerated(plan.Root, file.Key) || File.ReadAllBytes(file.Key).SequenceEqual(file.Value))) continue;
                    backups[file.Key] = exists ? File.ReadAllBytes(file.Key) : null;
                    Directory.CreateDirectory(Path.GetDirectoryName(file.Key));
                    File.WriteAllBytes(file.Key, file.Value);
                    written.Add(file.Key);
                }
                refresh = true;
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
                    throw new AggregateException("Install failed and some files could not be restored. Inspect Assets/UI before retrying.", new[] { original }.Concat(failures));
                throw;
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                if (refresh) AssetDatabase.Refresh();
            }
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
            return written.Count;
        }
    }
}
