// Catalog source and deterministic UXML example generator. Run: node tools/build-registry.mjs
import { mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { themes, themeUss } from './theme-presets.mjs';
import { themeAssets } from './build-theme-assets.mjs';
import { shopMarkup } from './treasure-shop.mjs';
const root = resolve('Packages/com.garabandic.uicdn');
const entries = [
  ['Core', 'Foundation', 'Fluent composition helpers and overridable motion lifecycle.', [], '<ui:Label text="Compose your UI with .With(...) and .Class(...)" />'],
  ['Button', 'Actions', 'Primary, secondary, ghost, and danger actions with native submit navigation.', ['core'], '<c:UiButton name="sample-button" text="Start adventure" /><c:UiButton text="Secondary" variant="secondary" /><c:UiButton text="Ghost" variant="ghost" /><c:UiButton text="Disabled" enabled="false" />'],
  ['Card', 'Layout', 'Composable header, title, description, content, and footer.', ['core'], '<c:UiCard><c:UiCardHeader><c:UiCardTitle text="Your next adventure" /><c:UiCardDescription text="Build something that belongs to your game." /></c:UiCardHeader><c:UiCardContent><ui:Label text="Everything here is yours to edit." /></c:UiCardContent><c:UiCardFooter><ui:Label text="No runtime package required" /></c:UiCardFooter></c:UiCard>'],
  ['Dialog', 'Overlay', 'Modal overlay with focus containment, focus restoration, cancel, and replaceable transitions.', ['core', 'button'], '<c:UiButton name="open-dialog" text="Open dialog" /><c:UiDialog><c:UiDialogOverlay /><c:UiDialogContent><c:UiDialogHeader><c:UiDialogTitle text="Leave this session?" /><c:UiDialogDescription text="Your progress has been saved. Press Escape or cancel to return." /></c:UiDialogHeader><c:UiDialogFooter><c:UiButton name="close-dialog" text="Stay here" /></c:UiDialogFooter></c:UiDialogContent></c:UiDialog>'],
  ['Tabs', 'Navigation', 'Composable tabs with value selection, nested-tab isolation, and left/right navigation.', ['core', 'button'], '<c:UiTabs value="profile"><c:UiTabsList><c:UiTabsTrigger value="profile" text="Profile" /><c:UiTabsTrigger value="settings" text="Settings" /></c:UiTabsList><c:UiTabsContent value="profile"><ui:Label text="Your player profile" /></c:UiTabsContent><c:UiTabsContent value="settings"><ui:Label text="Your game settings" /></c:UiTabsContent></c:UiTabs>'],
  ['Accordion', 'Disclosure', 'Single or multiple expandable items with animated content.', ['core', 'button'], '<c:UiAccordion><c:UiAccordionItem open="true"><c:UiAccordionTrigger text="Do I own the code?" /><c:UiAccordionContent><ui:Label text="Yes. Every installed file lives in your Assets directory." /></c:UiAccordionContent></c:UiAccordionItem><c:UiAccordionItem><c:UiAccordionTrigger text="Can I change the animation?" /><c:UiAccordionContent><ui:Label text="Override PlayEnterAnimation and PlayExitAnimation." /></c:UiAccordionContent></c:UiAccordionItem></c:UiAccordion>'],
  ['Checkbox', 'Forms', 'Native boolean field with change events and submit support.', ['core'], '<c:UiCheckbox text="Enable subtitles" value="true" /><c:UiCheckbox text="Share anonymous diagnostics" />'],
  ['Switch', 'Forms', 'Compact toggle for immediate settings.', ['core'], '<c:UiSwitch text="Music" value="true" /><c:UiSwitch text="Vibration" />'],
  ['Input', 'Forms', 'Native text editing with validation styling and change events.', ['core'], '<c:UiInput label="Player name" value="Wayfarer" /><c:UiInput label="Invalid example" value="Name already taken" invalid="true" />'],
  ['Slider', 'Forms', 'Native range selection with gamepad navigation and change events.', ['core'], '<c:UiSlider label="Music volume" low-value="0" high-value="100" value="65" />'],
  ['Badge', 'Status', 'Compact default, success, and danger status labels.', ['core'], '<c:UiBadge text="Early access" /><c:UiBadge text="Online" variant="success" /><c:UiBadge text="Disconnected" variant="danger" />'],
  ['Alert', 'Feedback', 'Composable inline informational, success, and danger messages.', ['core'], '<c:UiAlert variant="success"><c:UiAlertTitle text="Cloud save complete" /><c:UiAlertDescription text="Your progress is ready on your other devices." /></c:UiAlert>'],
  ['Progress', 'Status', 'Animated determinate progress with native range clamping.', ['core'], '<c:UiProgress value="64" title="Loading world: 64%" />'],
  ['Separator', 'Layout', 'Horizontal and vertical layout dividers.', ['core'], '<ui:Label text="Inventory" /><c:UiSeparator /><ui:Label text="Equipment" />'],
  ['Toast', 'Feedback', 'Dismissible notifications with hover/focus pause and motion lifecycle.', ['core', 'button'], '<c:UiButton name="show-toast" text="Show notification" /><c:UiToast duration-ms="4000"><c:UiToastTitle text="Quest complete" /><c:UiToastDescription text="The next chapter is ready. Submit or cancel to dismiss." /></c:UiToast>'],
];
entries.push(['TreasureShop', 'Screens', 'Illustrated portrait shop with a treasure bundle and three coin packs.', ['core','button'], shopMarkup]);
function write(path, content) { const full = resolve(root, path); mkdirSync(dirname(full), { recursive: true }); writeFileSync(full, content); }
for (const theme of themes) write(`Editor/Registry/Themes/theme-${theme.id}.uss`, themeUss(theme));
const fonts = { coiny: 'Coiny-Regular', nunito: 'Nunito-SemiBold', fredoka: 'Fredoka-SemiBold' };
for (const [id, font] of Object.entries(fonts))
  write(`Editor/Registry/Themes/font-${id}.uss`, `/* Display type override. SPDX-License-Identifier: MIT */\n.ui-root { --ui-display-font: url("Assets/${font}.ttf"); }\n`);
const components = entries.map(([name, category, description, dependencies, markup]) => {
  const id = name.toLowerCase();
  const base = `Editor/Registry/Components/${name}`;
  const example = `<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:c="Garabandic.UiCdn.Editor.Preview">\n  <ui:Style src="../../Themes/theme-tokens.uss" />\n  <ui:Style src="../../Themes/theme-dark.uss" />\n${[...dependencies.map(d => entries.find(e => e[0].toLowerCase() === d)[0]), name].map(n => `  <ui:Style src="../${n}/Ui${n}.uss" />`).join('\n')}\n  <ui:VisualElement class="ui-root ui-stack ui-example">\n    ${markup}\n  </ui:VisualElement>\n</ui:UXML>\n`;
  const document = name === 'TreasureShop' ? example.replace('class="ui-root ui-stack ui-example"', 'class="ui-root ui-theme-royal-puzzle"').replace('  <ui:VisualElement class="ui-root', '  <ui:Style src="../../Themes/theme-royal-puzzle.uss" />\n  <ui:VisualElement class="ui-root') : example;
  write(`${base}/${name}Example.uxml`, document);
  return { id, name, category, description, dependencies, preview: `${base}/${name}Example.uxml`, files: ['cs', 'uss'].map(ext => ({ source: `${base}/Ui${name}.${ext}`, target: `Components/${name}/Ui${name}.${ext}` })).concat({ source: `${base}/${name}Example.uxml`, target: `Components/${name}/${name}Example.uxml` }) };
});
const foundations = ['Themes/theme-tokens.uss', 'Themes/theme-game.uss', ...themes.map(t => `Themes/theme-${t.id}.uss`), ...Object.keys(fonts).map(id => `Themes/font-${id}.uss`), 'Docs/AGENTS.md'].map(p => ({ source: `Editor/Registry/${p}`, target: p === 'Docs/AGENTS.md' ? 'AGENTS.md' : p }));
foundations.push({ source: 'LICENSE.md', target: 'LICENSE.md' });
for (const font of Object.keys(fonts)) {
  const target = 'Themes/Assets/' + font[0].toUpperCase() + font.slice(1) + '-License.txt';
  foundations.push({ source: 'Editor/Registry/' + target, target });
}
foundations.push(...themeAssets.map(target => ({ source: `Editor/Registry/${target}`, target, binary: true })));
const { version } = JSON.parse(readFileSync(resolve(root, 'package.json'), 'utf8'));
write('Editor/Registry/manifest.json', JSON.stringify({ schemaVersion: 1, version, foundations, themes: themes.map(({id,name,family,description}) => ({id,name,family,description, cssClass: `ui-theme-${id}`, source: `Editor/Registry/Themes/theme-${id}.uss`})), components }, null, 2) + '\n');
console.log(`Generated ${components.length} catalog entries and UXML examples.`);
