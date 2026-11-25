# Project UI reference

These are project-owned Unity 6 runtime UI Toolkit components, licensed MIT.
Namespace: `{{NAMESPACE}}`. No UiCdn package reference is required at runtime.

- Compose in UXML or C#. Use `[UxmlElement]`, `[UxmlAttribute]`, and `partial` classes.
- Add `Assets/UI/ui.uss` to the document and `ui-root ui-theme-selected` to its root.
- `ui.uss`, `Themes/theme-active.uss`, and `.uicdn-theme` are installer-owned. Edit named theme presets, tokens, component styles, or a separate override stylesheet.
- Eleven named themes are installed. Use `ui-theme-selected` for the installer selection or one explicit `ui-theme-<id>` class to choose a preset in UXML or C#. Keep one theme class per root.
- The registry's Display font picker controls headings, buttons and prices through `--ui-display-font`. Apply appearance updates the aggregate for existing screens. Keep the OFL notices with the three bundled typefaces.
- Royal Puzzle is the default for new configurations. Its 9-slice PNG surfaces and coin are project-owned files under `Themes/Assets`; preserve their relative USS URLs. UI textures are installed uncompressed, without mipmaps or NPOT resizing.
- Casual Royal, Candy Pop, Toy Box, and Jungle Quest provide chunky game styling; Sunny Puzzle and Mint Cloud are soft; Neon Arcade, Paper Play, Studio Dark, and Studio Light offer other directions.
- `theme-game.uss` includes game layout utilities and common/rare/epic/legendary rarity classes. Illustration sprites and font assets are project-owned, separate from USS styling.
- Use semantic `--ui-*` tokens and BEM classes. USS is not browser CSS.
- Fluent construction: `new UiCard().With(new UiCardTitle("Inventory"))`.
- Buttons: `new UiButton("Start", OnStart).WithVariant("primary")`.
- Native form values support `RegisterValueChangedCallback`; prefer `SetValueWithoutNotify` for synchronization.
- Tabs require matching unique trigger/content `value` strings. Call `Refresh()` after dynamic changes.
- Dialogs require `UiDialogOverlay` and `UiDialogContent` children. Add the dialog last to a full-size root.
- Close buttons are application logic: subscribe `clicked += dialog.Hide`. UXML cannot bind event handlers.
- Keep interactive controls focusable. Preserve submit/cancel and `:focus` styling.
- Motion elements expose `OnBeforeOpen`, `OnAfterOpen`, `OnBeforeClose`, `OnAfterClose`.
- Override `PlayEnterAnimation` / `PlayExitAnimation`, toggle the open class, then call `CompleteEnter` / `CompleteExit` when your tween finishes. Cancel your own old tweens when the direction changes or the element detaches.
- USS panel transitions default to 180ms. Match `AnimationDurationMs` to `--ui-duration-panel`; set both to zero for reduced motion (also add `ui-reduced-motion` to the root).
- Toast `DurationMs <= 0` means persistent; submit/cancel dismisses it. Timers pause on hover or focus.
- Never add UnityEditor, Resources loads, package paths, tween packages, or installer dependencies to these runtime files.
- Reinstallation preserves existing differing files, including themes and this guide. Merge upstream changes manually.
- Preserve LICENSE.md when redistributing these files.
