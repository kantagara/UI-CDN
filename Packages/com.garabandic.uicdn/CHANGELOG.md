# Changelog

## 0.4.0 — 2026-11-26

- Checkbox draws a contrasting tick on a filled square for the checked state and a dash for mixed values across all twelve themes.
- Preview any component or genre screen in an interactive portrait or landscape phone viewport. Rotating or returning to full width keeps the current example's state.

## 0.3.0 — 2026-11-25

- Drag a project folder onto **Install folder** in the registry to choose where components, themes, and examples are installed. Clear the field to restore `Assets/UI`.
- The installer checks for an existing copy of the same component namespace and shows its location before a second install can introduce duplicate classes.

## 0.2.1 — 2026-11-25

- Armory can refresh repeatedly when an equipment slot is empty.
- Selecting a component from the genre showcase now redraws its preview once.
- Simplified installation planning and file writes. Edited component files are preserved, and failed writes still roll back.
- Contributors can edit Royal Puzzle and Pixel Forge directly in their USS files.

## 0.2.0 — 2026-11-25

- Choose **Pixel Forge** in the visual style picker for pixel-art equipment, shop items, and a forge backdrop. The installer preserves point filtering for the pixel textures.
- Install **Armory** to inspect six weapons and shields, compare attack and guard with the equipped item, and equip a selection using pointer or gamepad controls.
- Switch between the RPG equipment screen and the casual treasure shop in **Game UI showcase → Genre / screen**. The GitHub README includes screenshots of both screens captured in Unity.
- Select **Jersey 10** independently in the display font picker. It is the default typeface for Pixel Forge.

## 0.1.0 — 2026-11-25

- First public UPM release for Unity 6. The Editor registry copies C#, UXML and USS into the project's Assets/UI directory.
- Includes fifteen selectable components and screens, their shared Core source, and interactive previews.
- Eleven game themes, an illustrated Treasure Shop, and independent Coiny, Fredoka and Nunito display font choices.
- Dependency resolution, namespace configuration, keyboard and gamepad navigation, and protection for locally edited files.
- Installed components compile and their UXML loads after removal of the Editor package.
