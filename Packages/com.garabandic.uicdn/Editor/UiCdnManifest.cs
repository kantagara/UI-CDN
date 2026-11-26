using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Garabandic.UiCdn.Editor
{
    [Serializable]
    public sealed class UiCdnTheme
    {
        public string id;
        public string name;
        public string family;
        public string description;
        public string cssClass;
        public string source;
    }

    [Serializable]
    public sealed class UiCdnFile
    {
        public string source;
        public string target;
        public bool binary;
        public bool pointFilter;
    }

    [Serializable]
    public sealed class UiCdnComponent
    {
        public string id;
        public string name;
        public string category;
        public string genre;
        public string description;
        public string[] dependencies = Array.Empty<string>();
        public UiCdnFile[] files = Array.Empty<UiCdnFile>();
        public string preview;
    }

    [Serializable]
    public sealed class UiCdnManifest
    {
        public int schemaVersion;
        public string version;
        public UiCdnFile[] foundations;
        public UiCdnComponent[] components;
        public UiCdnTheme[] themes;
        public const string PreviewNamespace = "Garabandic.UiCdn.Editor.Preview";

        public static string PackageRoot => PackageInfo.FindForAssembly(typeof(UiCdnManifest).Assembly)?.resolvedPath
            ?? throw new InvalidOperationException("UiCdn must be installed as a local, embedded, or Git UPM package.");

        public static UiCdnManifest Load()
        {
            var manifest = JsonUtility.FromJson<UiCdnManifest>(File.ReadAllText(Path.Combine(PackageRoot, "Editor/Registry/manifest.json")));
            manifest.Validate();
            return manifest;
        }

        public void Validate()
        {
            if (schemaVersion != 1 || components == null || foundations == null || themes == null || themes.Length == 0)
                throw new InvalidDataException("Unsupported or incomplete registry manifest.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var c in components)
            {
                if (string.IsNullOrWhiteSpace(c.id) || !ids.Add(c.id) || c.files == null || c.dependencies == null)
                    throw new InvalidDataException("Invalid or duplicate component ID: " + c.id);
            }
            var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var packageRoot = PackageRoot;
            foreach (var file in foundations.Concat(components.SelectMany(c => c.files)))
            {
                var source = UiCdnInstaller.SafePath(packageRoot, file.source);
                UiCdnInstaller.ValidateRelative(file.target);
                if (!targets.Add(file.target)) throw new InvalidDataException("Duplicate target: " + file.target);
                if (!File.Exists(source)) throw new FileNotFoundException(file.source);
            }
            Resolve(components.Select(c => c.id));
            var themeIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var theme in themes)
            {
                if (theme == null || string.IsNullOrEmpty(theme.id) || !System.Text.RegularExpressions.Regex.IsMatch(theme.id, "^[a-z][a-z0-9-]*$") ||
                    new[] { "active", "selected", "tokens", "game" }.Contains(theme.id) || !themeIds.Add(theme.id) || theme.cssClass != "ui-theme-" + theme.id ||
                    !foundations.Any(f => f.source == theme.source && f.target == "Themes/theme-" + theme.id + ".uss"))
                    throw new InvalidDataException("Invalid theme entry.");
            }
        }

        public UiCdnTheme Theme(string id) => themes.FirstOrDefault(t => t.id == id) ?? throw new InvalidDataException("Unknown theme: " + id);

        public IReadOnlyList<UiCdnComponent> Resolve(IEnumerable<string> requested)
        {
            var byId = components.ToDictionary(c => c.id, StringComparer.Ordinal);
            var visiting = new HashSet<string>();
            var done = new HashSet<string>();
            var result = new List<UiCdnComponent>();
            void Visit(string id)
            {
                if (done.Contains(id)) return;
                if (!byId.TryGetValue(id, out var component)) throw new InvalidDataException("Unknown dependency: " + id);
                if (!visiting.Add(id)) throw new InvalidDataException("Dependency cycle: " + id);
                foreach (var dependency in component.dependencies) Visit(dependency);
                visiting.Remove(id);
                done.Add(id);
                result.Add(component);
            }
            foreach (var id in requested) Visit(id);
            return result;
        }
    }
}
