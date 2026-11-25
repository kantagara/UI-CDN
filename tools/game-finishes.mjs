// All measurements are shared by the PNG renderer and the stylesheet that slices it.
export const profiles = {
  chunky:  { radius: 14, depth: 6, edge: 2, gleam: 0.55, height: 52 },
  soft:    { radius: 18, depth: 4, edge: 1, gleam: 0.38, height: 46 },
  neon:    { radius: 7,  depth: 4, edge: 2, gleam: 0.76, height: 44 },
  paper:   { radius: 5,  depth: 3, edge: 2, gleam: 0.16, height: 44 },
  minimal: { radius: 8,  depth: 3, edge: 1, gleam: 0.30, height: 42 },
};

// The cover, offer ribbon and pack stock are independent surfaces.
// Ink is specified for the surface it is printed on, even on a very light canvas.
export const shopFinishes = {
  'casual-royal': { board: '#146bbb', title: '#fff4c5', cover: '#1467b4', rim: '#75c7ff', ribbon: '#0e497f', stock: '#ddf4ff', ink: '#17436b' },
  'candy-pop':    { board: '#ffe0ef', title: '#751b60', cover: '#a23d91', rim: '#ff99c9', ribbon: '#782469', stock: '#fff1f7', ink: '#5c285a' },
  'toy-box':      { board: '#ffe7ac', title: '#6b3620', cover: '#bb4540', rim: '#ffac68', ribbon: '#8e2f35', stock: '#fff8dc', ink: '#2d4261' },
  'jungle-quest': { board: '#255a40', title: '#ffe391', cover: '#327359', rim: '#a8cd76', ribbon: '#194e43', stock: '#edf1cc', ink: '#284a37' },
  'sunny-puzzle': { board: '#ffedcc', title: '#783d2e', cover: '#ac5240', rim: '#ffc47b', ribbon: '#804430', stock: '#fff9e8', ink: '#643f2f' },
  'mint-cloud':   { board: '#d9f3eb', title: '#245c58', cover: '#347775', rim: '#a2ddd0', ribbon: '#255a59', stock: '#f3fff9', ink: '#285854' },
  'neon-arcade':  { board: '#20183e', title: '#9df8ed', cover: '#503781', rim: '#be8bfc', ribbon: '#33215b', stock: '#332850', ink: '#efe9ff' },
  'paper-play':   { board: '#eee1c8', title: '#39322e', cover: '#796347', rim: '#483d33', ribbon: '#574939', stock: '#fff6dc', ink: '#3c3734' },
  'dark':         { board: '#1a2333', title: '#dce9ff', cover: '#344864', rim: '#748da9', ribbon: '#25344a', stock: '#293548', ink: '#e2ebfa' },
  'studio-light': { board: '#e6edf6', title: '#293d61', cover: '#49668e', rim: '#a6bdd9', ribbon: '#344f73', stock: '#f9fbff', ink: '#2d4263' },
};

export function gameFinishUss(theme) {
  const scope = '.ui-theme-' + theme.id;
  const p = profiles[theme.family];
  const shop = shopFinishes[theme.id];
  const rules = [];
  function rule(selectors, declarations) {
    rules.push(selectors.split(', ').map(s => scope + ' ' + s).join(',\n') + ' {\n    ' + declarations.join('\n    ') + '\n}');
  }
  const image = name => 'background-image: url("Assets/' + name + '.png");';
  const slice = ['left','right','top','bottom'].map(side => '-unity-slice-' + side + ': 20;');
  const cap = image('cap-' + theme.family);
  const frame = image('frame-' + theme.family);
  const radius = 'border-radius: ' + p.radius + 'px;';
  const inkOnCanvas = theme.id === 'casual-royal' ? '#123453' : shop.title;
  rules.push(scope + ' {\n' + Object.entries(shop).map(([k,v]) => '    --ui-shop-' + k + ': ' + v + ';').join('\n') + '\n}');
  rule('.ui-root, .ui-card-description, .ui-dialog-description, .ui-alert-description, .ui-toast-description, .ui-input, .ui-switch, .ui-checkbox, .ui-slider, .ui-game-caption, .ui-shop Label, .treasury Label', [
    '-unity-font-definition: url("Assets/Nunito-SemiBold.ttf");', '-unity-font: url("Assets/Nunito-SemiBold.ttf");', '-unity-font-style: normal;',
  ]);
  rule('.ui-button, .ui-heading, .ui-game-title, .ui-card-title, .ui-dialog-title, .ui-alert-title, .ui-toast-title, .ui-badge, .ui-game-currency, .ui-game-slot-name, .ui-shop .ui-shop-amount, .ui-shop .ui-card-title, .treasury .treasury-title, .treasury .treasury-feature-title, .treasury .treasury-prize, .treasury .treasury-section, .treasury .treasury-count', [
    '-unity-font-definition: var(--ui-display-font, url("Assets/Coiny-Regular.ttf"));', '-unity-font: var(--ui-display-font, url("Assets/Coiny-Regular.ttf"));', '-unity-font-style: normal;',
  ]);
  rule('.ui-button', [cap, ...slice, radius, 'border-width: 2px;', 'border-color: transparent;', 'min-height: ' + p.height + 'px;',
    'padding: 5px 16px ' + (5 + p.depth) + 'px 16px;', 'font-size: 18px;', '-unity-text-align: middle-center;',
    'transition-property: background-color, border-color, scale, translate;']);
  rule('.ui-button--secondary', ['background-color: var(--ui-surface-hover);', 'color: var(--ui-foreground);']);
  rule('.ui-button--danger', ['background-color: var(--ui-danger);', 'color: ' + (['studio-light','sunny-puzzle','mint-cloud','paper-play'].includes(theme.id) ? '#ffffff' : '#241e2f') + ';']);
  rule('.ui-button--ghost', ['background-image: none;', 'background-color: transparent;', 'color: var(--ui-foreground);']);
  rule('.ui-button--secondary:hover', ['background-color: var(--ui-surface);']);
  rule('.ui-button--danger:hover', ['background-color: var(--ui-danger);', '-unity-background-image-tint-color: #ffe4e4;']);
  rule('.ui-button--ghost:hover', ['background-color: var(--ui-surface-hover);']);
  rule('.ui-tabs-trigger--active, .ui-tabs-trigger--active:hover', [cap, 'background-color: var(--ui-primary);', 'color: var(--ui-primary-foreground);']);
  rule('.ui-button:focus, .treasury .treasury-price--selected', ['border-width: 2px;', 'border-color: var(--ui-primary-foreground);']);
  rule('.ui-button--ghost:focus, .ui-button--secondary:focus', ['border-color: var(--ui-foreground);']);
  rule('.ui-button--primary:active, .ui-button--secondary:active, .ui-button--danger:active, .ui-tabs-trigger--active:active', [image('cap-' + theme.family + '-pressed')]);
  rule('.ui-button:active', ['translate: 0 2px;', 'scale: 0.99 0.96;']);
  rule('.ui-accordion-trigger', ['-unity-text-align: middle-left;']);
  rule('.ui-card, .ui-toast, .ui-dialog-content, .ui-game-currency, .ui-shop-offer, .treasury-pack', [frame, ...slice, radius, 'border-width: 1px;', 'border-color: var(--ui-border);']);
  rule('.ui-toast:focus', ['border-color: var(--ui-primary);']);
  rule('.ui-game > .ui-game-title, .ui-game > .ui-game-caption', ['color: ' + inkOnCanvas + ';']);
  rule('.ui-tabs-list', ['padding: 4px;', radius]);
  rule('.ui-input .unity-base-text-field__input', ['background-color: var(--ui-surface);']);
  rule('.ui-heading, .ui-game-caption', ['white-space: normal;']);
  rule('.ui-shop-coin', [image('royal-coin'), 'background-color: transparent;']);
  rule('.treasury', ['background-color: var(--ui-shop-board);', 'border-width: 2px;', 'border-color: var(--ui-shop-rim);', radius]);
  rule('.treasury-bar', ['border-bottom-width: 2px;', 'border-bottom-color: var(--ui-shop-rim);']);
  rule('.treasury .treasury-title, .treasury .treasury-section', ['color: var(--ui-shop-title);']);
  rule('.treasury-wallet', ['background-color: var(--ui-shop-ribbon);', 'color: #fff5df;']);
  rule('.treasury-feature', [image('cover-' + theme.id), 'background-color: var(--ui-shop-cover);', 'border-color: var(--ui-shop-rim);', radius]);
  rule('.treasury .treasury-feature-title, .treasury .treasury-prize', ['color: #fff2cc;', '-unity-text-outline-width: 0.65px;', '-unity-text-outline-color: var(--ui-shop-ribbon);', 'text-shadow: 0 1px 1px var(--ui-shop-ribbon);']);
  rule('.treasury-rewards', ['background-color: rgba(15, 17, 29, 0.32);']);
  rule('.treasury-quantity', ['color: #fff9e8;']);
  rule('.treasury-deal', ['background-color: var(--ui-shop-ribbon);', 'color: #fff5df;', 'border-top-width: 1px;', 'border-top-color: var(--ui-shop-rim);']);
  rule('.treasury-pack, .ui-shop-offer', ['background-color: var(--ui-shop-stock);', 'border-color: var(--ui-shop-rim);', 'color: var(--ui-shop-ink);']);
  rule('.treasury .treasury-price', ['width: 103px;', 'min-height: 44px;', 'padding: 2px 4px ' + (2 + p.depth) + 'px 4px;', 'font-size: 19px;']);
  return rules.join('\n\n') + '\n';
}
