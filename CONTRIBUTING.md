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

Edit Royal Puzzle and Pixel Forge directly in `Editor/Registry/Themes/theme-royal-puzzle.uss` and `theme-pixel-forge.uss` inside the package. The other presets share palettes in `tools/theme-presets.mjs` and shop finishes in `tools/game-finishes.mjs`. The artwork generator in `tools/build-theme-assets.mjs` builds the normal and pressed surfaces, frames and shop covers. Keep their committed importer metadata configured for GUI use, no compression, no mipmaps, clamp wrapping, and no NPOT resizing. Installation initializes these settings for newly copied PNGs. The visual checker captures the dedicated native Editor window.

Keep public changes documented in `CHANGELOG.md`. Contributions are provided under MIT.

## Commit titles

Start every new commit title with `feat: `, `fix: `, or `chore: ` followed by a description. For example, `feat: choose an install folder`. This applies to merge, squash and release commits too. Enable the local check once per clone with `git config --local core.hooksPath .githooks`. GitHub Actions checks the new commits in each branch push and pull request.

## Publishing a version

Update `Packages/com.garabandic.uicdn/package.json` and add a matching version heading to both changelogs. Run `node tools/build-registry.mjs` and `node tools/validate-registry.mjs`, then commit the changes on `main`. Select **Actions → Release UiCdn → Run workflow** or run `gh workflow run release.yml --ref main`. The workflow validates the catalog, compares the unpacked tarball with the package directory, creates the matching `v<version>` tag, and attaches the archive to a GitHub Release. Pushing an exact version tag also starts the workflow. Tags such as `v0.2.0-preview.1` produce a GitHub prerelease.

OpenUPM discovers new versions by tag and publishes the archive attached to the matching GitHub Release. The release workflow waits for the registry version, downloads its tarball and compares the SHA-256 checksum with the release asset. You can repeat that check independently with `gh workflow run openupm.yml --ref main -f tag=v0.1.0`, substituting the tag you want to verify.

For an engine check before publishing, run `npm pack ./Packages/com.garabandic.uicdn --ignore-scripts` from the repository root and test the resulting archive with `./tools/verify.ps1 -PackageArchive ./com.garabandic.uicdn-<version>.tgz`. [OpenUPM publishing guide](https://openupm.com/docs/adding-upm-package.html)
