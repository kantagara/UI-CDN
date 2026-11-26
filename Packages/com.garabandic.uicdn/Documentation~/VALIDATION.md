# Validation record

Validated on Windows with Unity **6000.3.7f1**, 2026-11-26. The minimum declared package version is 6000.0; this run did not execute another Editor version or a physical gamepad.

## Engine checks

`tools/verify.ps1` passed **171 assertions** in an isolated project.

- Catalog parsing, topological dependency resolution, deduplication, missing dependencies, and cycle detection.
- Unsafe path rejection, namespace validation, namespace drift prevention, and directory collision preflight.
- Project folder drag-and-drop, destination reset, relative paths under a nested folder with spaces, stale plans and duplicate namespace detection across folders.
- Fluent composition, button variants, native fields, progress clamping, motion lifecycle order and idempotence, and tab selection.
- Import and instantiate every catalog UXML example using the actual source-generated controls.
- Install the RPG screen with its own art, equip through the fluent API, switch slots, and handle an empty slot.
- Inherited declarative motion attributes, initial toast state, and non-first UXML tab selection.
- Custom namespace replacement in C#, UXML, and the copied agent guide.
- Installation, conflict reporting, preservation of edited code, and incremental stylesheet import union.
- Rollback after a simulated write failure against a read-only aggregate stylesheet.
- Registry EditorWindow construction.
- Twelve-theme catalog, generated selected-theme aliases, theme-only application, and preservation of edited palettes.
- Verified point filtering for every installed pixel sprite, preservation of its native dimensions, and project-owned Pixel Forge dependencies.
- Independent typeface selection, generated font imports and installed examples that follow the selected appearance.
- Binary PNG installation without text conversion and initial GUI import settings with no compression, mipmaps, or NPOT resizing.

Two subsequent Editor launches confirmed installed C# compilation, source-generated UXML instantiation, and no UiCdn reference in the runtime assembly. The last launch removed the UiCdn package entirely and repeated those checks successfully. The engine's native UIElements module remained enabled.

## Rendered checks

`tools/visual-review.ps1` passed **250 attached-panel assertions**. It captured eight component views, twelve themes, four font choices, RPG equipment comparisons, and phone previews of the armory and shop.

- Actual layout and resolved theme variables.
- Checkbox tick visibility and at least 3:1 contrast against its fill in every preset, plus navigation submit, silent value changes and the mixed-state dash.
- Native NavigationSubmit activation and tab selection.
- Directional NavigationMove tab switching.
- Reduced-motion token overrides.
- Dialog enter/exit transitions, initial button focus, focus containment, NavigationCancel dismissal, and focus restoration.
- Declarative accordion expansion and toast enter transitions.
- Theme and typeface selection persistence, resolved font on a live price button, normal and focused geometry, and interactive shop offer selection.
- Per-theme artwork resolution, aligned price columns and space for the largest coin amount on a 320 px shop.
- The genre picker, armory artwork, stat gains and losses, equipment changes by submit, cancel focus and a 320 px inventory.
- Portrait and landscape viewport dimensions, pointer hit testing after scaling, retained equipment selection on rotation, scrolling to an action in landscape, checkbox submit, return to full width, and resizing the Editor window.

Reviewed Button, Tabs, Dialog, Switch, Input, Accordion, Toast and the shop across all presets. Screenshots come from the dedicated native Editor window and are generated locally in `artifacts/ui-review`. The package includes the [alternate theme gallery](Images/theme-gallery.png) and [three-font comparison](Images/font-choices.png).

The shop checks cover the example's English copy in the Windows Editor. Localized prices, translated titles and device scaling depend on each game's content and Panel Settings.

No component compiler errors, compiler warnings, UXML type errors, or USS property warnings were found in the successful verification logs. The Editor emitted unrelated cloud telemetry DNS failures in this environment. Physical controller mappings, platform player builds, and additional engine versions remain release-matrix checks for maintainers.

## Reference pages consulted

- [Create a custom control](https://docs.unity.com/en-us/engine/6000.0/manual/uitoolkits/uielements/uie-structure-ui/uie-custom-controls/uie-create-custom-controls)
- [UxmlElementAttribute](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/uielements/uxmlelementattribute)
- [UxmlAttributeAttribute](https://docs.unity.cn/6000.0/Documentation/ScriptReference/UIElements.UxmlAttributeAttribute.html)
- [Navigation events](https://docs.unity.com/en-us/engine/6000.0/manual/uitoolkits/uielements/uie-events/reference/uie-navigation-events)
- [USS transitions](https://docs.unity.com/en-us/engine/6000.0/manual/uitoolkits/uielements/uie-uss/properties/uie-transitions)
- [USS properties reference](https://docs.unity.com/en-us/engine/6000.0/manual/uitoolkits/uielements/uie-uss/properties/reference)
- [USS custom properties](https://docs.unity.com/en-us/engine/6000.0/manual/uitoolkits/uielements/uie-uss/variables/custom-properties)
