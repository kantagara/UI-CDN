using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Garabandic.UiCdn.Editor
{
    public enum UiCdnDisplayFont { ThemeDefault, Coiny, Fredoka, Nunito, Jersey10 }

    [FilePath("ProjectSettings/UiCdnSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class UiCdnSettings : ScriptableSingleton<UiCdnSettings>
    {
        public const string DefaultInstallFolder = "Assets/UI";
        [SerializeField] private string installFolder = DefaultInstallFolder;
        public string InstallFolder => string.IsNullOrEmpty(installFolder) ? DefaultInstallFolder : installFolder;
        [SerializeField] private string componentNamespace = "Game.UI";
        public string ComponentNamespace => componentNamespace;
        [SerializeField] private string selectedTheme = "royal-puzzle";
        public string SelectedTheme => selectedTheme;
        [SerializeField] private UiCdnDisplayFont displayFont;
        public UiCdnDisplayFont DisplayFont => displayFont;

        internal static string FontSheet(UiCdnDisplayFont choice)
        {
            if (!Enum.IsDefined(typeof(UiCdnDisplayFont), choice)) throw new ArgumentOutOfRangeException(nameof(choice));
            return choice == UiCdnDisplayFont.ThemeDefault ? null : "Themes/font-" + choice.ToString().ToLowerInvariant() + ".uss";
        }

        public void SelectFont(UiCdnDisplayFont choice)
        {
            FontSheet(choice);
            displayFont = choice;
            Save(true);
        }

        public void SelectTheme(string id)
        {
            UiCdnManifest.Load().Theme(id);
            selectedTheme = id;
            Save(true);
        }

        public void SelectInstallFolder(string assetPath)
        {
            assetPath = assetPath?.Replace('\\', '/');
            UiCdnInstaller.ResolveInstallFolder(assetPath);
            installFolder = assetPath;
            Save(true);
        }

        public void Configure(string targetNamespace)
        {
            ValidateNamespace(targetNamespace);
            componentNamespace = targetNamespace;
            Save(true);
        }

        public static void ValidateNamespace(string value)
        {
            const string keywords = "abstract as base bool break byte case catch char checked class const continue decimal default delegate do double else enum event explicit extern false finally fixed float for foreach goto if implicit in int interface internal is lock long namespace new null object operator out override params private protected public readonly ref return sbyte sealed short sizeof stackalloc static string struct switch this throw true try typeof uint ulong unchecked unsafe ushort using virtual void volatile while record required file scoped";
            if (string.IsNullOrWhiteSpace(value) || value.Split('.').Any(segment =>
                    !Regex.IsMatch(segment, @"^[A-Za-z_][A-Za-z0-9_]*$") ||
                    keywords.Split(' ').Contains(segment) || segment.IndexOf("unity", StringComparison.OrdinalIgnoreCase) >= 0) ||
                value.StartsWith("Garabandic.UiCdn.Editor", StringComparison.Ordinal))
                throw new ArgumentException("Use a valid C# namespace, for example Game.UI. Reserved words, Unity names, and the preview namespace are unavailable.");
        }
    }
}
