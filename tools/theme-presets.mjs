import { gameFinishUss } from './game-finishes.mjs';
// Theme data is shared by the registry generator; each preset produces an editable standalone USS.
export const themes = [
  { id: 'royal-puzzle', name: 'Royal Puzzle', family: 'royal', description: 'A focused casual/puzzle direction: gold-trimmed controls, parchment offers and a deep royal canvas.', colors: ['#1d2451','#343b75','#454d88','#fff5d9','#d0cce7','#bea056','#83c92b','#a8df42','#ffffff','#e76d64','#83c92b'] },
  { id: 'dark', name: 'Studio Dark', family: 'minimal', description: 'Sharp dark surfaces and restrained blue accents.', colors: ['#0f1116','#171a21','#232832','#edf0f6','#99a3b6','#333b49','#8bb3ff','#a8c7ff','#0c172b','#ff8994','#78ddaf'] },
  { id: 'casual-royal', name: 'Casual Royal', family: 'chunky', description: 'Reference-inspired sky blue, lime actions, thick ink outlines and bold game panels.', colors: ['#29a7f2','#0868c9','#167cde','#ffffff','#d5edff','#10223b','#87df32','#9be64b','#193b23','#ff626e','#80ed08'] },
  { id: 'candy-pop', name: 'Candy Pop', family: 'chunky', description: 'Bubblegum pink, violet panels and lemon candy buttons.', colors: ['#ffe0f0','#a04dda','#b662e7','#ffffff','#f4ddff','#49145c','#ffe13d','#fff16b','#4a2451','#ff626e','#8be951'] },
  { id: 'toy-box', name: 'Toy Box', family: 'chunky', description: 'Primary red, blue and yellow with playful toy-like depth.', colors: ['#fff0c2','#2189e8','#409df0','#ffffff','#e7f5ff','#172f4f','#ffcc28','#ffdd65','#17314e','#ff655e','#78d94a'] },
  { id: 'jungle-quest', name: 'Jungle Quest', family: 'chunky', description: 'Leaf green panels, warm gold actions and dark forest outlines.', colors: ['#bce781','#287647','#368951','#ffffff','#d9f5c5','#173a29','#ffce35','#ffe36b','#24402b','#ff8070','#b4f249'] },
  { id: 'sunny-puzzle', name: 'Sunny Puzzle', family: 'soft', description: 'Cream canvas, rounded white cards and warm orange actions.', colors: ['#fff2d9','#fffdf5','#fff0d3','#4d3426','#80604c','#e5c9a7','#ffba62','#ffce8e','#52311d','#dc5067','#58ad78'] },
  { id: 'mint-cloud', name: 'Mint Cloud', family: 'soft', description: 'Calm aqua panels and generous rounded shapes for cozy casual games.', colors: ['#dff7f1','#f6fffc','#c8eee4','#204d49','#4f7570','#9fcdc1','#8dd9c2','#b0ead9','#204d49','#cd526b','#3f956c'] },
  { id: 'neon-arcade', name: 'Neon Arcade', family: 'neon', description: 'Midnight violet, electric cyan and luminous magenta borders.', colors: ['#100e26','#201a3c','#302551','#f4edff','#bab0d4','#a476e5','#69f1ed','#a6fffb','#162536','#ff73be','#92f5ac'] },
  { id: 'paper-play', name: 'Paper Play', family: 'paper', description: 'Warm paper, dark pencil outlines and an editorial flat look.', colors: ['#f3eadd','#fffaf0','#e8dfcf','#292b31','#67635d','#363940','#f0c85e','#ffdb7b','#292b31','#cb5360','#689d72'] },
  { id: 'studio-light', name: 'Studio Light', family: 'minimal', description: 'Clean light surfaces for minimal menus and utility screens.', colors: ['#eef2f8','#ffffff','#e4ebf5','#202f49','#586984','#c0ccdd','#315ed1','#4474ed','#ffffff','#c83c56','#258361'] },
];

export function themeUss(theme) {
  const selector = `.ui-theme-${theme.id}`;
  const keys = ['background','surface','surface-hover','foreground','muted','border','primary','primary-hover','primary-foreground','danger','success'];
  const chunky = theme.family === 'chunky', soft = theme.family === 'soft', paper = theme.family === 'paper';
  const radius = chunky ? [10,16,22] : soft ? [12,22,28] : paper ? [0,2,3] : theme.family === 'neon' ? [2,4,8] : [3,6,10];
  let css = `/* ${theme.name} | ${theme.family} | SPDX-License-Identifier: MIT */\n${selector} {\n`;
  css += keys.map((key, i) => `    --ui-${key}: ${theme.colors[i]};`).join('\n');
  css += `\n    --ui-radius-sm: ${radius[0]}px;\n    --ui-radius-md: ${radius[1]}px;\n    --ui-radius-lg: ${radius[2]}px;\n    --ui-font-sm: ${chunky ? 14 : 12}px;\n    --ui-font-md: ${chunky ? 16 : 14}px;\n    --ui-font-lg: ${chunky ? 26 : soft ? 24 : 20}px;\n    --ui-control-height: ${chunky ? 52 : soft ? 44 : 36}px;\n    color: var(--ui-foreground);\n    background-color: var(--ui-background);\n    font-size: var(--ui-font-md);\n}\n`;
  if (theme.family !== 'royal') return css + gameFinishUss(theme);
  {
    const sliced = (asset) => `background-image: url("Assets/${asset}.png"); background-color: transparent; -unity-background-image-tint-color: white; -unity-slice-left: 20; -unity-slice-right: 20; -unity-slice-top: 20; -unity-slice-bottom: 20; border-width: 0;`;
    css += `${selector} .ui-button { ${sliced('royal-action')} min-height: 52px; padding: 12px 22px; color: #ffffff; font-size: 18px; -unity-font-style: bold; -unity-text-outline-width: 0.7px; -unity-text-outline-color: #31571b; }\n`;
    css += `${selector} .ui-button:hover { background-color: transparent; -unity-background-image-tint-color: #fff6db; }\n`;
    css += `${selector} .ui-button:focus { border-width: 2px; border-color: #fff0aa; border-radius: 12px; }\n`;
    css += `${selector} .ui-button--secondary, ${selector} .ui-tabs-trigger { ${sliced('royal-secondary')} -unity-text-outline-color: #244469; }\n`;
    css += `${selector} .ui-button--ghost { background-image: none; background-color: transparent; font-size: 15px; -unity-text-outline-width: 0; }\n`;
    css += `${selector} .ui-button--danger { ${sliced('royal-secondary')} -unity-background-image-tint-color: #ff9fa2; }\n`;
    css += `${selector} .ui-card, ${selector} .ui-dialog-content, ${selector} .ui-toast { ${sliced('royal-panel')} border-radius: 16px; }\n`;
    css += `${selector} .ui-tabs-trigger--active { ${sliced('royal-action')} }\n`;
    css += `${selector} .ui-badge { color: #fff0ad; background-color: #252b59; border-width: 1px; border-color: #bea056; }\n`;
    css += `${selector} .ui-shop-offer { ${sliced('royal-parchment')} color: #393452; }\n`;
    css += `${selector} .ui-shop-coin { background-image: url("Assets/royal-coin.png"); }\n`;
    css += `${selector} .ui-shop-counter { ${sliced('royal-parchment')} color: #393452; }\n`;
    css += `${selector} .ui-shop-amount { color: #393452; }\n`;
    css += `${selector} .ui-game-title { color: #ffdf89; -unity-text-outline-width: 0.6px; -unity-text-outline-color: #151d40; }\n`;
    css += `${selector} .ui-shop .ui-game-action, ${selector} .ui-shop .ui-game-nav > .ui-button { min-height: 44px; padding: 8px 14px; font-size: 16px; }\n`;
    const lettering = '-unity-font-definition: var(--ui-display-font, url("Assets/Coiny-Regular.ttf")); -unity-font: var(--ui-display-font, url("Assets/Coiny-Regular.ttf")); -unity-font-style: normal;';
    css += `${selector} .ui-shop Label, ${selector} .ui-shop .ui-button { ${lettering} }\n`;
    css += `${selector} .ui-button, ${selector} .ui-card-title, ${selector} .ui-game-title, ${selector} .ui-shop-amount, ${selector} .ui-shop-supply-count, ${selector} .ui-shop-counter { ${lettering} }\n`;
    css += `${selector} .ui-button, ${selector} .ui-shop .ui-game-action { font-size: 22px; -unity-text-outline-width: 0.45px; -unity-text-outline-color: #32580c; text-shadow: 0 1px 1px #3c6612; padding-bottom: 10px; }\n`;
    css += `${selector} .ui-button:active, ${selector} .ui-button:active:focus { translate: 0 2px; scale: 0.98 0.94; -unity-background-image-tint-color: #d8edaa; }\n`;
    css += `${selector} .ui-shop .ui-game-title { font-size: 32px; -unity-text-outline-width: 0.5px; text-shadow: 0 1px 1px #101a38; }\n`;
    css += `${selector} .ui-shop .ui-shop-amount { font-size: 27px; color: #304461; }\n`;
    css += `${selector} .ui-shop .ui-badge { background-color: #ffda80; color: #402e19; border-width: 0; border-radius: 4px; font-size: 12px; letter-spacing: 0.5px; padding: 3px 8px; }\n`;
    css += `${selector} .ui-shop .ui-shop-counter { background-image: none; background-color: #303e68; border-width: 0; border-radius: 9px; color: #ffdf89; font-size: 16px; }\n`;
    css += `${selector} .ui-shop .ui-game-nav { background-color: #28345c; border-radius: 10px; border-width: 0; padding: 4px; }\n`;
    css += `${selector} .ui-shop .ui-game-nav > .ui-button { background-image: none; background-color: transparent; border-width: 0; border-radius: 7px; color: #cad5ea; font-size: 16px; -unity-text-outline-width: 0; text-shadow: 0 0 0 transparent; }\n`;
    css += `${selector} .ui-shop .ui-game-nav > .ui-button--primary { background-color: #476797; color: #ffffff; }\n`;
    css += `${selector} .ui-shop .ui-game-nav > .ui-button:focus { border-width: 1px; border-color: #e6f1ff; }\n`;
    css += `${selector} .ui-shop .ui-shop-section { color: #d9e3f2; font-size: 14px; letter-spacing: 0.5px; }\n`;
    css += `${selector} .ui-shop .ui-shop-supply-label { color: #dee6f2; font-size: 13px; }\n`;
    css += `${selector} .treasury { background-color: #15325a; border-width: 2px; border-color: #305383; }\n`;
    css += `${selector} .treasury Label, ${selector} .treasury .treasury-price { ${lettering} }\n`;
    css += `${selector} .treasury .treasury-title, ${selector} .treasury .treasury-feature-title, ${selector} .treasury .treasury-prize { color: #ffe480; -unity-text-outline-width: 1.4px; -unity-text-outline-color: #66411f; text-shadow: 0 2px 1px #462c27; }\n`;
    css += `${selector} .treasury-bar { border-bottom-width: 2px; border-bottom-color: #4587bc; }\n`;
    css += `${selector} .treasury-wallet { background-color: #082341; color: #fff0bb; }\n`;
    css += `${selector} .treasury-feature { background-image: url("Assets/treasury-enamel.png"); border-color: #ffcd62; border-bottom-color: #c78124; }\n`;
    css += `${selector} .treasury-deal { background-color: #801938; color: #fff0ab; border-top-width: 2px; border-top-color: #ffad4b; }\n`;
    css += `${selector} .treasury-quantity { color: #fff5ce; -unity-text-outline-width: 1px; -unity-text-outline-color: #602340; }\n`;
    css += `${selector} .treasury-section { color: #ffe7a2; text-shadow: 0 1px 1px #082442; }\n`;
    css += `${selector} .treasury-pack { ${sliced('royal-parchment')} color: #274260; }\n`;
    css += `${selector} .treasury .treasury-price { width: 103px; min-height: 43px; font-size: 19px; padding: 2px 4px 7px 4px; -unity-text-outline-width: 0.85px; -unity-text-outline-color: #365515; text-shadow: 0 1px 1px #365515; }\n`;
  }
  // Accessible focus must remain visible even when a family overrides normal borders.
  css += `${selector} .ui-button:focus { border-color: var(--ui-foreground); }\n`;
  css += `${selector} .ui-button--ghost:focus { border-width: 2px; }\n`;
  return css;
}
