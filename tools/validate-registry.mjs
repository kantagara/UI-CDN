import { readFileSync, existsSync, readdirSync } from 'node:fs';
import { resolve, dirname, relative, isAbsolute } from 'node:path';
import assert from 'node:assert/strict';
const root = resolve('Packages/com.garabandic.uicdn');
const read = p => readFileSync(resolve(root, p), 'utf8');
const pkg = JSON.parse(read('package.json'));
const manifest = JSON.parse(read('Editor/Registry/manifest.json'));
assert.equal(pkg.name, 'com.garabandic.uicdn');
assert.equal(pkg.license, 'MIT');
assert.equal(manifest.schemaVersion, 1);
assert.equal(manifest.version, pkg.version, 'registry and UPM versions must match');
assert.deepEqual(JSON.parse(read('Editor/Garabandic.UiCdn.Editor.asmdef')).includePlatforms, ['Editor']);
const entries = new Map();
assert.equal(manifest.themes.length, 12);
const themeIds = new Set();
for (const theme of manifest.themes) {
  assert(!themeIds.has(theme.id)); themeIds.add(theme.id);
  assert.equal(theme.cssClass, `ui-theme-${theme.id}`);
  assert(manifest.foundations.some(f => f.source === theme.source));
  const source = read(theme.source);
  assert(source.includes(`.${theme.cssClass} {`));
  assert(source.includes('--ui-radius-md:') && source.includes('--ui-control-height:'));
  assert(!/box-shadow|linear-gradient|:last-child/.test(source));
  for (const match of source.matchAll(/url\("([^"]+)"\)/g)) {
    assert(existsSync(resolve(dirname(resolve(root, theme.source)), match[1])), `missing theme artwork: ${match[1]}`);
    assert(manifest.foundations.some(f => f.binary && f.target === 'Themes/' + match[1]), `uncopied theme asset: ${match[1]}`);
  }
}
for (const f of manifest.foundations.filter(f => f.binary && f.target.endsWith(".png")))
  assert.deepEqual([...readFileSync(resolve(root, f.source)).subarray(0, 8)], [137,80,78,71,13,10,26,10]);
for (const f of manifest.foundations.filter(f => f.pointFilter))
  assert.match(read(f.source + '.meta'), /filterMode: 0/, 'point filtering in the preview: ' + f.target);
const targets = new Set();
for (const c of manifest.components) { assert(!entries.has(c.id)); entries.set(c.id, c); }
function safe(path) { assert(path && !isAbsolute(path) && !path.split(/[\\/]/).some(p => ['..', '.', ''].includes(p))); }
for (const f of [...manifest.foundations, ...manifest.components.flatMap(c => c.files)]) {
  safe(f.source); safe(f.target); assert(existsSync(resolve(root, f.source)), f.source);
  assert(!targets.has(f.target.toLowerCase()), `duplicate ${f.target}`); targets.add(f.target.toLowerCase());
  if (f.source.endsWith('.cs')) {
    const source = read(f.source);
    assert(!/using UnityEditor|UnityEditor\.|UxmlFactory|UxmlTraits|Resources\.Load|AssetDatabase/.test(source), f.source);
    assert(source.includes('namespace Garabandic.UiCdn.Editor.Preview'), f.source);
    for (const cls of source.matchAll(/\[UxmlElement\]\s*public\s+(.*?)class\s+(\w+)/g)) assert(cls[1].includes('partial'), cls[2]);
  }
  if (f.source.endsWith('.uxml')) {
    const source = read(f.source);
    assert(source.includes('xmlns:c="Garabandic.UiCdn.Editor.Preview"'));
    for (const match of source.matchAll(/<ui:Style src="([^"]+)"/g)) {
      const full = resolve(dirname(resolve(root, f.source)), match[1]);
      assert(existsSync(full), `${f.source}: ${match[1]}`);
      const targetImport = resolve('/installed', dirname(f.target), match[1]);
      assert([...targets, ...manifest.foundations.map(f => f.target.toLowerCase()), ...manifest.components.flatMap(c => c.files).map(f => f.target.toLowerCase())].includes(relative('/installed', targetImport).replaceAll('\\', '/').toLowerCase()), `uncopied import: ${targetImport}`);
    }
  }
}
const done = new Set(), visiting = new Set();
function visit(id) {
  assert(entries.has(id), `unknown ${id}`);
  if (done.has(id)) return;
  assert(!visiting.has(id), `cycle ${id}`); visiting.add(id);
  entries.get(id).dependencies.forEach(visit); visiting.delete(id); done.add(id);
}
entries.forEach(c => visit(c.id));
function walk(path) {
  for (const e of readdirSync(path, { withFileTypes: true })) {
    const full = resolve(path, e.name);
    assert(!e.name.toLowerCase().includes('unity'), `forbidden package path ${full}`);
    if (e.isDirectory()) walk(full);
  }
}
walk(root);
assert(read('Editor/UiCdnRegistryWindow.cs').includes('Window/UiCdn/Component Registry'));
console.log(`PASS: ${entries.size} catalog entries; dependency graph, paths, UXML imports, source contracts, naming, Editor-only assembly.`);
