import { gameFinishUss } from './game-finishes.mjs';
// Presets with a palette share generated USS. The two illustrated presets own their USS directly.
export const themes = [
  { id: 'pixel-forge', name: 'Pixel Forge', family: 'pixel', description: 'A dusk-lit 16-bit forge with pixel sprites, stepped brass controls and Jersey 10 lettering.' },
  { id: 'royal-puzzle', name: 'Royal Puzzle', family: 'royal', description: 'A focused casual/puzzle direction: gold-trimmed controls, parchment offers and a deep royal canvas.' },
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

export function paletteUss(theme) {
  const selector = `.ui-theme-${theme.id}`;
  const keys = ['background','surface','surface-hover','foreground','muted','border','primary','primary-hover','primary-foreground','danger','success'];
  const chunky = theme.family === 'chunky', soft = theme.family === 'soft', paper = theme.family === 'paper';
  const radius = chunky ? [10,16,22] : soft ? [12,22,28] : paper ? [0,2,3] : theme.family === 'neon' ? [2,4,8] : [3,6,10];
  let css = `/* ${theme.name} | ${theme.family} | SPDX-License-Identifier: MIT */\n${selector} {\n`;
  css += keys.map((key, i) => `    --ui-${key}: ${theme.colors[i]};`).join('\n');
  css += `\n    --ui-radius-sm: ${radius[0]}px;\n    --ui-radius-md: ${radius[1]}px;\n    --ui-radius-lg: ${radius[2]}px;\n    --ui-font-sm: ${chunky ? 14 : 12}px;\n    --ui-font-md: ${chunky ? 16 : 14}px;\n    --ui-font-lg: ${chunky ? 26 : soft ? 24 : 20}px;\n    --ui-control-height: ${chunky ? 52 : soft ? 44 : 36}px;\n    color: var(--ui-foreground);\n    background-color: var(--ui-background);\n    font-size: var(--ui-font-md);\n}\n`;
  return css + gameFinishUss(theme);
}
