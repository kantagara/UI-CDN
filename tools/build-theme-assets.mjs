// Deterministic, original 9-slice artwork. No external art or graphics dependencies.
import { deflateSync } from 'node:zlib';
import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { createHash } from 'node:crypto';
import { resolve } from 'node:path';
import { profiles, shopFinishes } from './game-finishes.mjs';
const output = resolve('Packages/com.garabandic.uicdn/Editor/Registry/Themes/Assets');
mkdirSync(output, { recursive: true });
const crc = bytes => {
  let value = 0xffffffff;
  for (const byte of bytes) { value ^= byte; for (let bit = 0; bit < 8; bit++) value = (value >>> 1) ^ (0xedb88320 & -(value & 1)); }
  return (value ^ 0xffffffff) >>> 0;
};
function chunk(type, data) {
  const label = Buffer.from(type), size = Buffer.alloc(4), checksum = Buffer.alloc(4);
  size.writeUInt32BE(data.length); checksum.writeUInt32BE(crc(Buffer.concat([label, data])));
  return Buffer.concat([size, label, data, checksum]);
}
const color = value => value.match(/[a-f\d]{2}/gi).map(x => parseInt(x, 16));
const mix = (a, b, t) => a.map((v, i) => v + (b[i] - v) * Math.max(0, Math.min(1, t)));
const roundDistance = (x, y, width, height, radius) => {
  const qx = Math.abs(x - width / 2) - width / 2 + radius;
  const qy = Math.abs(y - height / 2) - height / 2 + radius;
  return Math.hypot(Math.max(qx, 0), Math.max(qy, 0)) + Math.min(Math.max(qx, qy), 0) - radius;
};
function png(name, width, height, pixel) {
  const raw = Buffer.alloc(height * (width * 4 + 1));
  for (let y = 0; y < height; y++) for (let x = 0; x < width; x++) {
    const accumulated = [0, 0, 0, 0];
    for (const dy of [0.25, 0.75]) for (const dx of [0.25, 0.75]) {
      const sample = pixel(x + dx, y + dy);
      for (let c = 0; c < 3; c++) accumulated[c] += sample[c] * sample[3] / 255;
      accumulated[3] += sample[3];
    }
    const at = y * (width * 4 + 1) + x * 4 + 1;
    for (let c = 0; c < 3; c++) raw[at + c] = accumulated[3] ? Math.round(accumulated[c] * 255 / accumulated[3]) : 0;
    raw[at + 3] = Math.round(accumulated[3] / 4);
  }
  const header = Buffer.alloc(13); header.writeUInt32BE(width); header.writeUInt32BE(height, 4); header[8] = 8; header[9] = 6;
  writeFileSync(resolve(output, name + '.png'), Buffer.concat([Buffer.from([137,80,78,71,13,10,26,10]), chunk('IHDR', header), chunk('IDAT', deflateSync(raw)), chunk('IEND', Buffer.alloc(0))]));
}
function surface(name, top, bottom, trim, shadow) {
  const a = color(top), b = color(bottom), edge = color(trim), dark = color(shadow);
  png(name, 96, 96, (x, y) => {
    const distance = roundDistance(x, y, 96, 96, 17);
    if (distance > 0) return [0,0,0,0];
    const depth = -distance;
    let rgb;
    if (depth < 1.5) rgb = dark;
    else if (depth < 4.5) rgb = mix(edge, color('b77816'), y / 110);
    else if (depth < 6) rgb = dark;
    else {
      rgb = mix(a, b, (y - 6) / 83);
      if (depth < 8 && y < 46) rgb = mix(rgb, [255,255,255], 0.42);
      if (y < 43) rgb = mix(rgb, [255,255,255], 0.12 * (1 - (y - 6) / 38));
    }
    return [...rgb, 255];
  });
}
// A single sculpted cap: lit top lip, continuous face and recessed lower wall.
png('royal-action', 96, 96, (x, y) => {
  const depth = -roundDistance(x, y, 96, 96, 13);
  if (depth < 0) return [0,0,0,0];
  const face = -roundDistance(x - 3, y - 2, 90, 83, 10);
  let rgb = mix(color('538114'), color('305b0c'), y / 96);
  if (depth < 1.7) rgb = color('364609');
  else if (face > 0) {
    rgb = mix(color('a6e630'), color('60b710'), (y - 4) / 77);
    if (face < 2.5 && y < 45) rgb = color('dff886');
    if (face < 2 && y > 54) rgb = color('579c10');
  }
  return [...rgb,255];
});
surface('royal-secondary', '64b8fb', '2163b4', 'ffe382', '233556');
surface('royal-panel', '415880', '28395c', 'f8d98a', '242e48');
surface('royal-parchment', 'fff6da', 'e4d3a8', 'd4b66e', '54442d');
png('royal-coin', 80, 80, (x, y) => {
  const dx = (x - 40) / 0.86, dy = y - 40;
  const radius = Math.hypot(dx, dy);
  if (radius > 36) return [0,0,0,0];
  let rgb = radius > 33 ? color('805019') : radius > 28 ? mix(color('fff1a4'), color('d08b14'), y/80) : mix(color('ffe971'),color('efa60c'),y/80);
  if (radius < 25 && Math.abs(dx + dy * 0.45) < 3) rgb = mix(rgb,[255,255,224],0.45);
  if (Math.abs(dx) < 2.2 && Math.abs(dy) < 15 || Math.abs(dy - 10) < 2 && Math.abs(dx) < 8) rgb = color('c17c09');
  return [...rgb,255];
});
export const themeAssets = ['royal-action','royal-secondary','royal-panel','royal-parchment','royal-coin'].map(name => `Themes/Assets/${name}.png`);
themeAssets.push('Themes/Assets/Coiny-Regular.ttf', 'Themes/Assets/Nunito-SemiBold.ttf', 'Themes/Assets/Fredoka-SemiBold.ttf');
for (const id of ['chest', 'rocket', 'hammer', 'heart', 'coins', 'pouch']) themeAssets.push(`Themes/Assets/loot-${id}.png`);
// Painted enamel behind the loot. The broad alternating rays converge behind the chest.
png('treasury-enamel', 384, 192, (x, y) => {
  const angle = Math.atan2(y - 119, x - 86);
  const light = Math.max(0, 1 - Math.hypot((x - 85)/300, (y - 100)/220));
  const ray = Math.pow(0.5 + 0.5 * Math.cos(angle * 18), 1.6);
  return [...mix(color('7c193f'), color('ff6351'), light * 0.7 + ray * 0.15), 255];
});
themeAssets.push('Themes/Assets/treasury-enamel.png');

// Transparent artwork lets USS supply the action, hover, danger and card colors.
// Outer pixels carry the silhouette; a constant 20px slice protects the corners.
const empty = [0, 0, 0, 0];
const shade = alpha => [10, 16, 28, Math.round(alpha * 255)];
const shine = alpha => [255, 254, 247, Math.round(alpha * 255)];
const importSettings = readFileSync(resolve(output, 'royal-action.png.meta'), 'utf8');
function addArtwork(name, width, height, paint) {
  png(name, width, height, paint);
  const target = 'Themes/Assets/' + name + '.png';
  themeAssets.push(target);
  const meta = resolve(output, name + '.png.meta');
  if (!existsSync(meta)) {
    const guid = createHash('sha256').update('com.garabandic.uicdn/' + target).digest('hex').slice(0, 32);
    writeFileSync(meta, importSettings.replace(/^guid: [a-f0-9]+/m, 'guid: ' + guid));
  }
}
for (const [family, p] of Object.entries(profiles)) {
  function capPixel(x, y, depth) {
    const outside = -roundDistance(x, y, 64, 64, p.radius);
    if (outside < 0) return empty;
    if (outside < p.edge) return shade(0.65);
    const face = -roundDistance(x - p.edge, y - p.edge, 64 - 2 * p.edge, 64 - 2 * p.edge - depth, p.radius - p.edge);
    if (face < 0) return shade(0.36);
    if (face < 1.7) return y < 30 ? shine(p.gleam) : shade(0.16);
    // Keep the center quiet behind price lettering. Most of the depth is in the lip.
    const light = (0.52 - y / 64) * 0.23;
    return light > 0 ? shine(light) : shade(-light);
  }
  addArtwork('cap-' + family, 64, 64, (x, y) => capPixel(x, y, p.depth));
  addArtwork('cap-' + family + '-pressed', 64, 64, (x, y) => capPixel(x, y, 1));
  addArtwork('frame-' + family, 64, 64, (x, y) => {
    const edge = -roundDistance(x, y, 64, 64, p.radius - 1);
    if (edge < 0) return empty;
    if (edge < 1.4) return shade(0.28);
    if (edge < 3) return y < 36 ? shine(p.gleam) : shade(0.16);
    return shine(0.055 * Math.max(0, 1 - y / 42));
  });
}

// Low-contrast covers sit behind the editable title and the six shared illustrations.
// Anchor the lighting at the chest. Stagger the repeating marks between rows.
const cell = (position, width) => ((position % width) + width) % width - width / 2;
const coverPatterns = {
  'candy-pop': (x, y) => Math.abs(cell(x + 0.65 * y, 65)) < 9 ? 0.11 : 0,
  'casual-royal': (x, y) => Math.pow(0.5 + 0.5 * Math.cos(Math.atan2(y - 117, x - 76) * 16), 2) * 0.14,
  'toy-box': (x, y) => {
    const a = cell(x + Math.floor(y / 44) * 23, 54), b = cell(y, 44);
    const r = Math.hypot(a, b);
    return r < 8 ? 0.07 : r < 9.7 ? (b < 0 ? 0.18 : -0.08) : 0;
  },
  'jungle-quest': (x, y) => {
    const a = cell(x + 0.58 * y + Math.floor(y / 46) * 27, 59), b = cell(y, 46);
    return Math.abs(a) < 6 * Math.max(0, 1 - (b / 16) ** 2) ? (a < 0 ? 0.14 : 0.05) : 0;
  },
  'sunny-puzzle': (x, y) => (1 + Math.cos(14 * Math.atan2(y - 134, x - 71))) * 0.066,
  'mint-cloud': (x, y) => {
    const a = cell(x + Math.floor(y / 34) * 26, 52), b = cell(y, 34);
    return Math.abs(b - 4 - 0.018 * a * a) < 1.5 ? 0.15 : 0;
  },
  'neon-arcade': (x, y) => Math.min(Math.abs(cell(x + 1.35 * y, 52)), Math.abs(cell(x - 1.35 * y, 52))) < 1.1 ? 0.23 : 0,
  'paper-play': (x, y) => Math.hypot(cell(x + Math.floor(y / 8) * 4, 8), cell(y, 8)) < 1.1 ? -0.19 : 0,
  'dark': (x, y) => Math.min(Math.abs(cell(x, 34)), Math.abs(cell(y, 34))) < 0.65 ? 0.12 : 0,
  'studio-light': (x, y) => Math.abs(cell(x + 0.85 * y, 42)) < 1 ? 0.12 : 0,
};
for (const id of Object.keys(shopFinishes)) {
  addArtwork('cover-' + id, 384, 192, (x, y) => {
    const halo = Math.max(0, 1 - Math.hypot((x - 80) / 285, (y - 111) / 190));
    const light = -0.10 + halo * 0.25 + coverPatterns[id](x, y);
    return light > 0 ? shine(light) : shade(-light);
  });
}
