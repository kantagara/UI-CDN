// Optional visible Editor review, copied to an isolated project's Assets/Editor.
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Garabandic.UiCdn.Editor;
using Garabandic.UiCdn.Editor.Preview;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class UiCdnVisualVerification
{
    private static UiCdnRegistryWindow window;
    private static string output;
    private static int assertions;
    public static void Run()
    {
        output = Environment.GetEnvironmentVariable("UICDN_REVIEW_OUTPUT") ?? Path.Combine(Application.dataPath, "../Review");
        Directory.CreateDirectory(output);
        UiCdnSettings.instance.SelectTheme("dark");
        UiCdnSettings.instance.SelectFont(UiCdnDisplayFont.ThemeDefault);
        foreach (var name in new[] { "failure.txt", "result.txt" })
            if (File.Exists(Path.Combine(output, name))) File.Delete(Path.Combine(output, name));
        window = ScriptableObject.CreateInstance<UiCdnRegistryWindow>();
        window.titleContent = new GUIContent("UiCdn Review");
        window.position = new Rect(160, 120, 1080, 800);
        window.ShowUtility();
        window.Focus();
        Later(2000, ReviewButton);
    }
    private static void Later(int delay, Action action) => window.rootVisualElement.schedule.Execute(() =>
    {
        try { action(); }
        catch (Exception ex) { Debug.LogException(ex); File.WriteAllText(Path.Combine(output, "failure.txt"), ex.ToString()); EditorApplication.Exit(1); }
    }).StartingIn(delay);
    private static void Check(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new Exception(message);
    }
    private static void Until(Func<bool> condition, Action action, int remaining = 100)
    {
        if (condition()) { action(); return; }
        if (remaining == 0) throw new Exception("Timed out waiting for the panel transition to settle.");
        Later(50, () => Until(condition, action, remaining - 1));
    }
    private static void Select(string id)
    {
        var catalog = window.rootVisualElement.Q("catalog");
        var button = catalog.Query<Button>().ToList().First(b => b.text.StartsWith(id + "  /", StringComparison.OrdinalIgnoreCase));
        button.Focus();
        using (var submit = NavigationSubmitEvent.GetPooled()) button.SendEvent(submit);
    }
    private static void Capture(string name)
    {
        window.Repaint();
        // Capture only our native window, even if another application covers it.
        // A desktop-region capture can accidentally record unrelated user content.
        var handle = FindWindow(null, "UiCdn Review");
        if (handle == IntPtr.Zero) throw new Exception("Cannot find the dedicated review window.");
        GetWindowRect(handle, out var bounds);
        var width = bounds.right - bounds.left;
        var height = bounds.bottom - bounds.top;
        var info = new BitmapInfo { size = 40, width = width, height = height, planes = 1, bitCount = 32 };
        var screen = GetDC(IntPtr.Zero);
        var context = CreateCompatibleDC(screen);
        var bitmap = CreateDIBSection(context, ref info, 0, out var bits, IntPtr.Zero, 0);
        var previous = SelectObject(context, bitmap);
        byte[] pixels;
        try
        {
            if (!PrintWindow(handle, context, 2)) throw new Exception("Native window capture failed.");
            pixels = new byte[width * height * 4];
            Marshal.Copy(bits, pixels, 0, pixels.Length);
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var blue = pixels[i]; pixels[i] = pixels[i + 2]; pixels[i + 2] = blue; pixels[i + 3] = 255;
            }
        }
        finally
        {
            SelectObject(context, previous); DeleteObject(bitmap); DeleteDC(context); ReleaseDC(IntPtr.Zero, screen);
        }
        var image = new Texture2D(width, height, TextureFormat.RGBA32, false);
        image.LoadRawTextureData(pixels); image.Apply();
        File.WriteAllBytes(Path.Combine(output, name + ".png"), image.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(image);
    }
    [StructLayout(LayoutKind.Sequential)] private struct Bounds { public int left, top, right, bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct BitmapInfo
    {
        public uint size; public int width, height; public ushort planes, bitCount;
        public uint compression, sizeImage; public int xPixels, yPixels; public uint colorsUsed, colorsImportant;
    }
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr FindWindow(string className, string title);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr windowHandle, out Bounds bounds);
    [DllImport("user32.dll")] private static extern bool PrintWindow(IntPtr windowHandle, IntPtr context, uint flags);
    [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr windowHandle);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr windowHandle, IntPtr context);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr context);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateDIBSection(IntPtr context, ref BitmapInfo info, uint usage, out IntPtr bits, IntPtr section, uint offset);
    [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr context, IntPtr obj);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr obj);
    [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr context);
    private static void ReviewButton()
    {
        var button = window.rootVisualElement.Q<UiButton>("sample-button");
        Check(button != null && button.worldBound.width > 100 && button.worldBound.height >= 32, "Button layout");
        Check(button.resolvedStyle.backgroundColor.b > 0.6f, "Theme variables resolved");
        button.Focus();
        using (var submit = NavigationSubmitEvent.GetPooled()) button.SendEvent(submit);
        Later(200, () =>
        {
            Check(button.text == "Ready!", "Native navigation submit triggers button");
            Capture("01-button");
            var preview = window.rootVisualElement.Q("preview");
            preview.AddToClassList("ui-reduced-motion");
            Later(100, () =>
            {
                Check(button.resolvedStyle.transitionDuration.All(t => Mathf.Approximately(t.value, 0)), "Reduced motion overrides component timings");
                Select("tabs");
                Later(300, ReviewTabs);
            });
        });
    }
    private static void ReviewTabs()
    {
        var root = window.rootVisualElement;
        var tabs = root.Q<UiTabs>();
        var trigger = tabs.Query<UiTabsTrigger>().ToList()[1];
        trigger.Focus();
        using (var submit = NavigationSubmitEvent.GetPooled()) trigger.SendEvent(submit);
        Check(tabs.Value == "settings", "Tab submit selects content");
        Check(trigger.ClassListContains("ui-tabs-trigger--active"), "Tab modifier state");
        Later(250, () =>
        {
            Capture("02-tabs");
            using (var move = NavigationMoveEvent.GetPooled(Vector2.left)) trigger.SendEvent(move);
            Check(tabs.Value == "profile", "Directional navigation changes tabs");
            Select("dialog"); Later(250, ReviewDialog);
        });
    }
    private static void ReviewDialog()
    {
        var root = window.rootVisualElement;
        var dialog = root.Q<UiDialog>();
        var opener = root.Q<UiButton>("open-dialog");
        opener.Focus();
        dialog.Show();
        Until(() => dialog.resolvedStyle.opacity > 0.99f && dialog.panel.focusController.focusedElement == root.Q<UiButton>("close-dialog"), () =>
        {
            var closer = root.Q<UiButton>("close-dialog");
            Check(dialog.Open && dialog.resolvedStyle.opacity > 0.99f, "Dialog enter transition");
            Check(dialog.panel.focusController.focusedElement == closer, "Dialog initial focus");
            opener.Focus();
            Check(dialog.Contains(dialog.panel.focusController.focusedElement as VisualElement), "Modal contains programmatic focus");
            Capture("03-dialog-open");
            using (var cancel = NavigationCancelEvent.GetPooled()) closer.SendEvent(cancel);
            Until(() => dialog.resolvedStyle.display == DisplayStyle.None, () =>
            {
                Check(!dialog.Open && dialog.resolvedStyle.display == DisplayStyle.None, "Cancel closes dialog after transition");
                Check(opener.panel.focusController.focusedElement == opener, "Dialog restores focus");
                Select("checkbox");
                ReviewCheckbox(0);
            });
        });
    }
    private static void ReviewCheckbox(int index)
    {
        var root = window.rootVisualElement;
        var themes = UiCdnManifest.Load().themes;
        if (index == themes.Length)
        {
            root.Q<DropdownField>("theme-picker").value = "Studio Dark";
            Select("switch");
            Later(250, () => { Capture("04-switch"); ReviewRemaining(); });
            return;
        }
        var theme = themes[index];
        root.Q<DropdownField>("theme-picker").value = theme.name;
        Later(250, () =>
        {
            var boxes = root.Q("preview").Query<UiCheckbox>().ToList();
            var check = boxes[0];
            var tick = check.Q(className: "ui-checkbox__tick");
            var box = tick.parent;
            Check(check.value && tick.resolvedStyle.display == DisplayStyle.Flex && tick.worldBound.width > 10,
                "UXML checked indicator " + theme.name);
            Check(!boxes[1].value && boxes[1].Q(className: "ui-checkbox__tick").resolvedStyle.display == DisplayStyle.None,
                "unchecked box has no tick " + theme.name);
            var ink = tick.resolvedStyle.borderBottomColor;
            var fill = box.resolvedStyle.backgroundColor;
            Check(ink.a > 0.99f && fill.a > 0.99f && Contrast(ink, fill) >= 3f && box.resolvedStyle.backgroundImage == default(Background),
                "opaque, contrasting tick on a solid fill " + theme.name);
            if (theme.id != "casual-royal") { ReviewCheckbox(index + 1); return; }
            Capture("04-checkbox");
            int changes = 0;
            check.OnChanged(_ => changes++);
            check.Focus();
            using (var submit = NavigationSubmitEvent.GetPooled()) check.SendEvent(submit);
            Later(150, () =>
            {
                Check(!check.value && changes == 1 && tick.resolvedStyle.display == DisplayStyle.None, "submit clears checkmark");
                check.SetValueWithoutNotify(true);
                Later(150, () =>
                {
                    Check(changes == 1 && tick.resolvedStyle.display == DisplayStyle.Flex, "silent update restores checkmark");
                    check.showMixedValue = true;
                    Later(150, () =>
                    {
                        Check(tick.resolvedStyle.height == 3 && tick.resolvedStyle.borderLeftWidth == 0, "mixed checkbox shows a dash");
                        ReviewCheckbox(index + 1);
                    });
                });
            });
        });
    }
    private static float Contrast(Color a, Color b)
    {
        float Luminance(Color color)
        {
            var linear = color.linear;
            return 0.2126f * linear.r + 0.7152f * linear.g + 0.0722f * linear.b;
        }
        float first = Luminance(a), second = Luminance(b);
        return (Mathf.Max(first, second) + 0.05f) / (Mathf.Min(first, second) + 0.05f);
    }
    private static void ReviewRemaining()
    {
        Select("input");
        Later(250, () =>
        {
            Capture("05-input"); Select("accordion");
            Later(300, () =>
            {
                var first = window.rootVisualElement.Q<UiAccordionItem>();
                Check(first.Open && first.Q<UiAccordionContent>().Open, "Declarative accordion state");
                Capture("06-accordion"); Select("toast");
                Later(200, () =>
                {
                    var toast = window.rootVisualElement.Q<UiToast>();
                    toast.DurationMs = 0; toast.Show();
                    Until(() => toast.resolvedStyle.opacity > 0.99f, () =>
                    {
                        Check(toast.resolvedStyle.opacity > 0.99f, "Toast enter transition");
                        Capture("07-toast");
                        ReviewFont(0);
                    });
                });
            });
        });
    }

    private static void ReviewFont(int index)
    {
        var choices = new[] { UiCdnDisplayFont.Fredoka, UiCdnDisplayFont.Nunito, UiCdnDisplayFont.Coiny, UiCdnDisplayFont.Jersey10, UiCdnDisplayFont.ThemeDefault };
        if (index == choices.Length) { ReviewTheme(0); return; }
        var value = choices[index];
        var root = window.rootVisualElement;
        window.position = new Rect(160, 30, 1080, 1000);
        root.Q<DropdownField>("theme-picker").value = "Candy Pop";
        root.Q<Toggle>("theme-showcase").value = true;
        root.Q<EnumField>("font-picker").value = value;
        Later(250, () =>
        {
            var price = root.Q("preview").Q<UiButton>("pocket");
            string expected = value == UiCdnDisplayFont.ThemeDefault ? "Coiny" : value.ToString();
            var actual = price.resolvedStyle.unityFont?.name;
            Check(actual != null && actual.StartsWith(expected, StringComparison.OrdinalIgnoreCase), $"price font {expected}, resolved {actual}");
            Check(UiCdnSettings.instance.DisplayFont == value, "font selection persists");
            if (value != UiCdnDisplayFont.ThemeDefault) Capture("font-" + value.ToString().ToLowerInvariant());
            ReviewFont(index + 1);
        });
    }

    private static void ReviewTheme(int index)
    {
        var themes = UiCdnManifest.Load().themes;
        if (index == themes.Length)
        {
            ReviewArmory(); return;
        }
        var theme = themes[index];
        window.Focus();
        window.position = new Rect(160, 30, 1080, 1000);
        window.rootVisualElement.Q<DropdownField>("theme-picker").value = theme.name;
        window.rootVisualElement.Q<Toggle>("theme-showcase").value = true;
        Later(250, () =>
        {
            var preview = window.rootVisualElement.Q("preview");
            Check(preview.ClassListContains(theme.cssClass), "preview theme class " + theme.id);
            Check(UiCdnSettings.instance.SelectedTheme == theme.id, "theme selection persists " + theme.id);
            var action = preview.Query<UiButton>().ToList().First(b => b.ClassListContains("ui-game-action"));
            string selectedOffer = null;
            preview.Q<UiTreasureShop>().OfferSelected += id => selectedOffer = id;
            Check(action.resolvedStyle.height >= 43, "shop control dimensions " + theme.id);
            Check(action.resolvedStyle.color.a > 0.9f, "theme text visible " + theme.id);
            var cap = action.resolvedStyle.backgroundImage.texture;
            Check(cap != null && cap.name == (theme.family == "royal" ? "royal-action" : "cap-" + theme.family), "theme button artwork " + theme.id);
            var feature = preview.Q(className: "treasury-feature");
            string cover = theme.family == "pixel" ? "forge" : theme.family == "royal" ? "treasury-enamel" : "cover-" + theme.id;
            Check(feature.resolvedStyle.backgroundImage.texture?.name == cover, "themed cover " + theme.id);
            var prices = preview.Query<UiButton>(className: "treasury-price").ToList();
            Check(prices.Count == 4 && prices.All(b => Mathf.Abs(b.worldBound.width - 103) < 1), "consistent price width " + theme.id);
            Check(prices.Skip(1).All(b => Mathf.Abs(b.worldBound.xMax - prices[1].worldBound.xMax) < 1), "aligned prices " + theme.id);
            Check(action.resolvedStyle.unityFont?.name.StartsWith(theme.family == "pixel" ? "Jersey10" : "Coiny") == true, "default display lettering " + theme.id);
            var initialBounds = action.layout;
            action.Focus();
            using (var submit = NavigationSubmitEvent.GetPooled()) action.SendEvent(submit);
            Later(250, () =>
            {
                Check(selectedOffer == "treasure-bundle" && action.ClassListContains("treasury-price--selected"), "showcase action works " + theme.id);
                Check(action.resolvedStyle.backgroundImage.texture == cap && action.resolvedStyle.borderBottomWidth > 0 && action.layout == initialBounds, "focus preserves price geometry and artwork " + theme.id);
                Capture("theme-" + theme.id);
                var shop = preview.Q<UiTreasureShop>();
                shop.style.width = 320;
                Later(120, () =>
                {
                    Check(prices.All(b => b.worldBound.xMin >= shop.worldBound.xMin && b.worldBound.xMax <= shop.worldBound.xMax && b.worldBound.width >= 100), "320px price controls " + theme.id);
                    Check(shop.Query<Label>(className: "treasury-count").ToList().All(amount =>
                        amount.MeasureTextSize(amount.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined).x <= amount.contentRect.width), "320px coin amounts fit " + theme.id);
                    ReviewTheme(index + 1);
                });
            });
        });
    }

    private static void ReviewArmory()
    {
        var root = window.rootVisualElement;
        root.Q<DropdownField>("theme-picker").value = "Royal Puzzle";
        root.Q<EnumField>("font-picker").value = UiCdnDisplayFont.Fredoka;
        root.Q<DropdownField>("genre-picker").value = "RPG · Equipment";
        Later(350, () =>
        {
            var armory = root.Q<UiArmory>();
            var blade = armory.Q<UiGearItem>("dawn");
            Check(armory.Selected == blade && blade.Icon != null, "initial weapon and original artwork");
            Check(armory.Q(className: "armory-banner").resolvedStyle.backgroundImage.texture?.name == "forge", "forge illustration");
            Check(armory.Q(className: "armory-heading").resolvedStyle.unityFont?.name.StartsWith("Fredoka") == true, "armory display font override");
            Check(armory.Query<Label>(className: "armory-delta").ToList().Select(l => l.text).SequenceEqual(new[] { "+14", "-3" }), "meaningful item tradeoff");
            Capture("genre-rpg-weapons");
            var equip = armory.Q<UiButton>("equip");
            equip.Focus();
            using (var submit = NavigationSubmitEvent.GetPooled()) equip.SendEvent(submit);
            var shieldTab = armory.Q<Button>("slot-Shield");
            shieldTab.Focus();
            using (var submit = NavigationSubmitEvent.GetPooled()) shieldTab.SendEvent(submit);
            var moon = armory.Q<UiGearItem>("moon");
            moon.Focus();
            using (var submit = NavigationSubmitEvent.GetPooled()) moon.SendEvent(submit);
            Later(240, () =>
            {
                Check(armory.Selected == moon && moon.Icon != null && armory.GetEquipped(GearSlot.Weapon) == blade, "shield inspection preserves equipped weapon");
                Check(armory.GetEquipped(GearSlot.Shield).name == "oak", "inspection leaves current shield equipped");
                Capture("genre-rpg-shields");
                armory.style.width = 320;
                using (var cancel = NavigationCancelEvent.GetPooled()) moon.SendEvent(cancel);
                Later(180, () =>
                {
                    Check(armory.Selected.name == "oak" && armory.panel.focusController.focusedElement == shieldTab, "cancel restores equipped selection and slot focus");
                    Check(armory.Query<UiGearItem>().ToList().Where(i => i.Slot == GearSlot.Shield).All(i => i.worldBound.xMin >= armory.worldBound.xMin && i.worldBound.xMax <= armory.worldBound.xMax), "320px inventory fits");
                    ReviewPixelArmory();
                });
            });
        });
    }

    private static void ReviewPixelArmory()
    {
        var root = window.rootVisualElement;
        root.Q<EnumField>("font-picker").value = UiCdnDisplayFont.ThemeDefault;
        root.Q<DropdownField>("theme-picker").value = "Pixel Forge";
        Later(350, () =>
        {
            var gear = root.Q<UiArmory>();
            var blade = gear.Selected;
            Check(blade.DisplayedIcon != blade.Icon && blade.DisplayedIcon.width == 24 && blade.DisplayedIcon.filterMode == FilterMode.Point, "themed weapon sprite");
            Check(gear.Q(className: "armory-portrait").resolvedStyle.backgroundImage.texture == blade.DisplayedIcon, "portrait matches the selected sprite");
            Check(gear.Q(className: "armory-heading").resolvedStyle.unityFont?.name.StartsWith("Jersey10") == true, "retro title font");
            Capture("pixel-armory");
            // Swap a class on the same attached tree, as a game can do at runtime.
            var host = root.Q("preview");
            host.RemoveFromClassList("ui-theme-pixel-forge");
            host.AddToClassList("ui-theme-royal-puzzle");
            Later(180, () =>
            {
                Check(blade.DisplayedIcon == blade.Icon && gear.Q(className: "armory-portrait").resolvedStyle.backgroundImage.texture == blade.Icon,
                    "removing the theme restores user artwork on the same item");
                Select("card");
                Check(!root.Q<Toggle>("theme-showcase").value && !root.Q("genre-picker").enabledSelf && root.Q<UiCard>() != null,
                    "selecting a catalog entry exits the genre preview");
                ReviewPhone();
            });
        });
    }

    private static void CheckPhoneSize(float width, float height)
    {
        var root = window.rootVisualElement;
        var screen = root.Q<ScrollView>("phone-screen").contentViewport;
        var stage = root.Q("preview-stage");
        Check(Mathf.Abs(screen.layout.width - width) < 1 && Mathf.Abs(screen.layout.height - height) < 1,
            $"phone lays out at {width} × {height}, got {screen.layout.size}");
        var frame = root.Q("phone-frame");
        Check(frame.worldBound.width <= stage.worldBound.width && frame.worldBound.height <= stage.worldBound.height,
            "scaled device fits the available preview");
    }

    private static void ReviewPhone()
    {
        var root = window.rootVisualElement;
        var detail = root.Q<ScrollView>("detail");
        root.Q<Toggle>("theme-showcase").value = true;
        var gear = root.Q<UiArmory>();
        var axe = gear.Q<UiGearItem>("cinder");
        gear.Inspect(axe);
        var picker = root.Q<DropdownField>("preview-size");
        picker.index = 1;
        Later(300, () =>
        {
            CheckPhoneSize(390, 844);
            Check(gear == root.Q<UiArmory>() && gear.Selected == axe, "changed preview size keeps the live selection");
            detail.ScrollTo(root.Q("preview-stage"));
            Later(200, () =>
            {
                var equip = gear.Q<UiButton>("equip");
                var picked = root.panel.Pick(equip.worldBound.center);
                Check(picked == equip || equip.Contains(picked), "scaled portrait maps pointer coordinates to Equip");
                equip.Focus();
                using (var submit = NavigationSubmitEvent.GetPooled()) equip.SendEvent(submit);
                Check(gear.GetEquipped(GearSlot.Weapon) == axe, "equip in the phone preview");
                Capture("mobile-armory");
                picker.index = 2;
                Later(250, () =>
                {
                    CheckPhoneSize(844, 390);
                    Check(gear.GetEquipped(GearSlot.Weapon) == axe, "rotation preserves the equipped item");
                    Capture("mobile-landscape");
                    var screen = root.Q<ScrollView>("phone-screen");
                    screen.ScrollTo(gear.Q<UiButton>("equip"));
                    Later(150, () =>
                    {
                        Check(screen.scrollOffset.y > 0 && screen.contentViewport.worldBound.Contains(equip.worldBound.center),
                            "overflowing landscape content can scroll to the last action");
                        ReviewPhoneFormsAndShop();
                    });
                });
            });
        });
    }

    private static void ReviewPhoneFormsAndShop()
    {
        var root = window.rootVisualElement;
        Select("checkbox");
        var checkbox = root.Q<UiCheckbox>();
        var picker = root.Q<DropdownField>("preview-size");
        Check(picker.index == 2, "another catalog entry keeps the selected orientation");
        picker.index = 0;
        Check(checkbox == root.Q<UiCheckbox>() && root.Q("phone-frame") == null, "full width restores the live component");
        picker.index = 1;
        checkbox.Focus();
        using (var submit = NavigationSubmitEvent.GetPooled()) checkbox.SendEvent(submit);
        Check(!checkbox.value, "checkbox accepts submit on the phone");
        root.Q<DropdownField>("theme-picker").value = "Royal Puzzle";
        root.Q<Toggle>("theme-showcase").value = true;
        root.Q<DropdownField>("genre-picker").value = "Casual · Treasure shop";
        Later(300, () =>
        {
            CheckPhoneSize(390, 844);
            var shop = root.Q<UiTreasureShop>();
            Check(shop != null && shop.layout.width <= 390, "shop reflows at the mobile width after theme and screen changes");
            root.Q<ScrollView>("detail").ScrollTo(root.Q("preview-stage"));
            Later(200, () =>
            {
                Capture("mobile-shop");
                var previousWidth = root.Q("phone-frame").worldBound.width;
                window.position = new Rect(120, 30, 780, 580);
                Later(250, () =>
                {
                    CheckPhoneSize(390, 844);
                    Check(root.Q("phone-frame").worldBound.width < previousWidth, "smaller editor scales the frame while preserving logical width");
                    UiCdnSettings.instance.SelectTheme("dark");
                    File.WriteAllText(Path.Combine(output, "result.txt"), $"PASS: {assertions} attached-panel assertions.");
                    window.Close(); EditorApplication.Exit(0);
                });
            });
        });
    }
}
