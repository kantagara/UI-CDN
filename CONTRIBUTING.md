# Contributing

The package targets Unity 6. Keep package and project namespaces free of `Unity` branding. The Editor namespace is `Garabandic.UiCdn.Editor`; previews use its `.Preview` child namespace and installed source defaults to `Game.UI`.

1. Add source under `Packages/com.garabandic.uicdn/Editor/Registry/Components/<Name>`.
2. Use `[UxmlElement] partial` classes and `[UxmlAttribute]` properties. Retain a public parameterless constructor for UXML. Expose constructors or fluent helpers for code-first use.
3. Use runtime `UnityEngine.UIElements` APIs only in templates. No asset loads, editor APIs, runtime package references, or legacy factory/traits implementations.
4. Split complex controls into composable parts. Add semantic USS tokens, BEM state classes, focus/disabled styling, and submit/cancel behavior for interactive controls.
5. Add catalog metadata, dependencies, and working UXML markup to `tools/build-registry.mjs`. Run it to regenerate the manifest and examples. Do not rely on dependencies' transitive stylesheet imports; generated examples include their own style list.
6. Run `node tools/validate-registry.mjs` and `./tools/verify.ps1`. Pass `-EditorPath` when your Unity Editor is installed elsewhere. Review live previews in an Editor, including pointer, Tab, arrow/D-pad, submit, cancel, nesting, focus restoration, and disabled states.
7. Commit `.meta` files for package assets so GUIDs stay stable. Do not commit the generated validation project or engine cache.

Verification uses the installed engine directly; no third-party test or runtime dependency is required. The engine test runner intentionally lives outside the UPM folder and is copied into a disposable test project. CI validates catalog contracts without an engine license; engine verification requires an installed, licensed Editor.

For rendered review and attached-panel interaction tests, run `./tools/visual-review.ps1 -ProjectPath '<temp verification project>'` after `verify.ps1`. This briefly opens the registry in a separate Editor, checks focus, submit/cancel, directional navigation, and transitions, saves screenshots under `artifacts/ui-review`, and closes that Editor. Inspect the images before publishing. It accepts only disposable verification projects created by the test script.

Theme definitions live in `tools/theme-presets.mjs`. Shop palettes and button profiles are in `tools/game-finishes.mjs`. The artwork generator in `tools/build-theme-assets.mjs` builds the normal and pressed surfaces, frames and shop covers. Keep their committed importer metadata configured for GUI use, no compression, no mipmaps, clamp wrapping, and no NPOT resizing. Installation initializes these settings for newly copied PNGs. The visual checker captures the dedicated native Editor window.

Keep public changes documented in `CHANGELOG.md`. Contributions are provided under MIT.

## Publishing a version

Update `Packages/com.garabandic.uicdn/package.json` and add a matching version heading to both changelogs. Run `node tools/build-registry.mjs` and `node tools/validate-registry.mjs`, then commit the changes on `main`. Select **Actions → Release UiCdn → Run workflow** or run `gh workflow run release.yml --ref main`. The workflow validates the catalog, compares the unpacked tarball with the package directory, creates the matching `v<version>` tag, and attaches the archive to a GitHub Release. Pushing an exact version tag also starts the workflow. Tags such as `v0.2.0-preview.1` produce a GitHub prerelease.

After OpenUPM accepts the catalog entry, it discovers new versions by tag and publishes the archive attached to the matching GitHub Release. Verify availability with `npm view com.garabandic.uicdn@<version> version --registry=https://package.openupm.com`.

For an engine check before publishing, run `npm pack ./Packages/com.garabandic.uicdn --ignore-scripts` from the repository root and test the resulting archive with `./tools/verify.ps1 -PackageArchive ./com.garabandic.uicdn-<version>.tgz`. [OpenUPM publishing guide](https://openupm.com/docs/adding-upm-package.html)
