// Pixel Forge artwork. Coordinates are native texels. Shapes use a fixed palette and hard edges.
const ink = '#151526', plum = '#33223d', violet = '#584263', slate = '#595e79';
const steel = '#94b6c2', frost = '#e1f2d5', gold = '#efb64f', cream = '#fff1a5';
const amber = '#bd713c', copper = '#84483e', scarlet = '#dc6253', jade = '#49b8a0';

function canvas(width, height) {
  const pixels = Array(width * height).fill(null);
  const dot = (x, y, c) => {
    if (x >= 0 && x < width && y >= 0 && y < height) pixels[y * width + x] = c;
  };
  const rect = (x, y, w, h, c) => {
    for (let row = y; row < y + h; row++) for (let col = x; col < x + w; col++) dot(col, row, c);
  };
  function polygon(points, c) {
    // Fill between the intersections of each pixel-centre scanline and the polygon.
    for (let y = 0; y < height; y++) {
      const crossings = [];
      for (let i = 0; i < points.length; i++) {
        const [x1, y1] = points[i], [x2, y2] = points[(i + 1) % points.length];
        if ((y1 > y + 0.5) !== (y2 > y + 0.5))
          crossings.push(x1 + (y + 0.5 - y1) * (x2 - x1) / (y2 - y1));
      }
      crossings.sort((a, b) => a - b);
      for (let i = 0; i < crossings.length; i += 2)
        for (let x = Math.ceil(crossings[i] - 0.5); x + 0.5 < crossings[i + 1]; x++) dot(x, y, c);
    }
  }
  return { width, height, pixels, rect, dot, polygon };
}

function sword(dawn) {
  const s = canvas(24,24), p = s.polygon, r = s.rect;
  p([[3,18],[5,18],[8,14],[5,12],[5,10],[9,11],[18,2],[23,1],[22,7],[13,16],[15,19],[13,20],[10,17],[6,21],[6,23],[2,23],[1,20]], ink);
  p([[9,12],[19,3],[22,2],[21,6],[12,15]], dawn ? gold : steel);
  p([[10,13],[20,3],[22,2],[12,14]], dawn ? cream : frost);
  p([[11,14],[21,4],[20,7],[13,15]], dawn ? amber : slate);
  p([[4,19],[8,15],[10,17],[6,21]], copper);
  r(5,18,2,1,amber); r(3,20,2,2,dawn ? gold : steel);
  p(dawn ? [[4,9],[7,10],[9,12],[12,13],[14,15],[16,15],[17,18],[14,18],[11,16],[8,15],[5,13]] :
    [[5,11],[8,12],[14,17],[14,19],[12,18],[6,13]], dawn ? gold : amber);
  if (dawn) { r(9,13,3,3,ink); r(9,13,2,2,jade); r(9,13,1,1,frost); r(5,10,2,1,cream); }
  return s;
}

function axe() {
  const s = canvas(24,24), p = s.polygon;
  p([[2,20],[14,6],[17,8],[6,23],[2,23]],ink);
  p([[3,21],[15,7],[16,8],[5,22]],amber);
  p([[5,3],[10,4],[13,6],[16,2],[19,4],[18,6],[21,8],[23,6],[23,13],[20,17],[15,18],[15,14],[12,11],[9,10],[6,12],[3,11],[6,8]],ink);
  p([[5,4],[10,5],[14,8],[17,5],[17,8],[20,10],[22,8],[21,13],[18,16],[16,16],[17,13],[13,10],[10,9],[6,11],[7,7]],scarlet);
  p([[16,15],[20,12],[22,9],[21,14],[18,16],[16,17]],frost);
  p([[7,5],[11,6],[13,8],[10,7],[7,7]],'#ffa375');
  s.rect(13,7,3,3,copper); s.rect(13,7,2,2,gold); return s;
}

function shield(kind) {
  const s = canvas(24,24), p = s.polygon, r = s.rect;
  const round = kind === 'oak';
  const colors = round ? [steel,slate,copper] : kind === 'sun' ? [gold,amber,'#186763'] : [steel,slate,'#57365c'];
  p(round ? [[7,2],[17,2],[22,7],[22,17],[17,22],[7,22],[2,17],[2,7]] :
    [[2,4],[8,4],[12,1],[16,4],[22,4],[21,13],[18,18],[12,23],[6,18],[3,13]], ink);
  p(round ? [[7,3],[17,3],[21,7],[21,17],[17,21],[7,21],[3,17],[3,7]] :
    [[3,5],[8,5],[12,2],[16,5],[21,5],[20,13],[17,18],[12,21],[7,18],[4,13]], colors[0]);
  p([[5,7],[9,7],[12,4],[16,7],[19,7],[18,13],[16,16],[12,19],[8,16],[6,13]], colors[2]);
  if (round) {
    p([[5,8],[8,5],[19,16],[16,19]],colors[1]);
    p([[5,15],[16,4],[19,7],[8,18]], colors[1]);
    r(8,9,7,6,ink); r(9,9,5,5,steel); r(9,9,3,2,frost);
  } else if (kind === 'sun') {
    r(11,7,2,10,gold); r(7,11,10,2,gold); r(9,9,6,6,gold); r(10,10,4,4,cream); r(8,8,1,1,gold); r(16,15,1,1,gold);
  } else {
    p([[13,7],[11,10],[12,12],[15,13],[17,11],[16,15],[12,17],[8,14],[8,10],[10,8]],frost);
    r(15,8,2,2,gold);
  }
  r(5,5,3,1,round ? frost : colors[0]); return s;
}

function forge() {
  const s = canvas(220,56), r=s.rect, p=s.polygon;
  r(0,0,220,56,'#28213b'); r(0,12,220,14,'#463048'); r(0,26,220,16,'#84505b'); r(0,42,220,14,'#c37b67');
  for (const [x,y] of [[11,9],[31,7],[61,13],[76,4],[89,9],[103,4],[126,7],[185,7],[212,13]]) r(x,y,1,1,'#e5ba99');
  r(107,7,1,5,cream); r(105,9,5,1,cream);
  p([[0,31],[13,26],[21,28],[33,22],[43,28],[57,19],[69,28],[88,18],[100,25],[117,15],[136,28],[151,18],[174,30],[198,20],[220,26],[220,56],[0,56]],'#58405a');
  p([[0,37],[22,31],[37,37],[60,29],[78,37],[100,29],[115,31],[135,24],[151,36],[168,30],[195,32],[211,26],[220,30],[220,56],[0,56]],'#3a344e');
  for (const [x,y,h] of [[2,29,15],[8,26,20],[16,32,11],[61,30,16],[69,26,21],[82,31,13],[96,27,14],[108,26,14],[205,27,20],[212,23,23]]) {
    p([[x,y+h],[x+3,y+h-7],[x+1,y+h-7],[x+4,y+5],[x+3,y+5],[x+5,y],[x+9,y+9],[x+7,y+9],[x+11,y+h],[x,y+h]],'#28283c');
  }
  p([[0,37],[17,36],[35,42],[60,39],[80,42],[104,35],[124,39],[143,35],[164,41],[193,37],[220,39],[220,56],[0,56]],ink);
  // Distant warm windows and their stepped roofs.
  for (const [x,y] of [[73,37],[92,34],[113,35]]) {
    r(x,y,9,7,'#302b3a'); p([[x-2,y+1],[x+4,y-4],[x+10,y+1]],ink); r(x+3,y+2,2,2,amber);
  }
  // The open smithy has a brick chimney, overlapping roof tiles and a lit hearth.
  r(174,5,8,20,ink); r(175,5,5,14,'#744551'); r(173,4,10,3,ink); r(174,4,8,1,'#c78266');
  for (const [x,y,w] of [[176,9,3],[178,12,3],[175,15,3]]) r(x,y,w,1,'#bd7664');
  p([[132,29],[145,11],[176,8],[199,26],[207,28],[204,32],[135,33]],ink);
  p([[135,28],[146,13],[176,10],[197,27],[162,27]],'#604255');
  p([[158,27],[176,10],[198,27]],'#af6660'); p([[163,27],[176,13],[193,27]],'#392c40');
  for (let i=0;i<4;i++) {
    const x=145-i*3, y=15+i*3;
    r(x,y,17,1,'#a66c6d'); r(x+18,y-1,8,1,'#845467');
  }
  r(135,30,67,3,ink); r(138,32,4,20,copper); r(139,33,1,18,amber);
  r(195,30,4,23,copper); r(195,31,1,20,'#cc8264'); r(141,33,53,17,'#3c2c3a');
  r(173,28,18,23,'#74505a'); r(175,33,15,1,copper);
  r(175,37,14,12,ink); r(177,35,10,13,ink); r(178,38,8,11,scarlet);
  p([[176,48],[178,44],[180,44],[181,39],[183,41],[183,36],[186,40],[186,45],[188,46],[188,49]],gold);
  r(181,43,3,5,cream); r(173,49,18,2,amber); r(132,52,73,2,'#946056');
  r(149,40,17,2,steel); r(147,41,19,2,slate); r(150,43,11,2,slate); r(154,45,6,2,slate);
  r(152,47,10,2,steel); r(151,49,12,3,copper); r(152,49,11,1,amber);
  for (const [x,y] of [[174,38],[172,35],[170,39],[187,33],[184,28]]) r(x,y,1,1,gold);
  for (const [x,y,w] of [[115,51,6],[127,54,8],[184,54,12],[203,51,8],[89,49,3]]) r(x,y,w,1,'#68505c');
  return s;
}

// Each glyph is drawn on its own transparent 16px inventory grid.
const lootPalette = { k:ink, b:copper, a:amber, g:gold, y:cream, r:scarlet, h:'#ffac80', t:jade, s:slate, w:frost, v:violet };
const loot = {
  chest: ['    kkkkkkkk','  kkggggggggkk',' kgyygggggggagk',' kgbbggbbbgabagk',' kgbbggbbbgabagk','kggggggggggggagk','kaaaaakyykaaaabk','kgbbbbkggkbbbgbk','kgbbbbakabbbbgbk',' kgggggggggggak','  kkkkkkkkkkkk'],
  heart: ['  kkk   kkk',' khhrk krhhk','khyhrrkrrhhhk','khhrrrrrrrhrk','krrrrrrrrrrrk',' krrrrrrrrrk','  krrrrrrbk','   krrrrbk','    krrbk','     kbk','      k'],
  hammer:['       kkkkk','      kswwwsk','     kswwwsssk','     ksswsssssk','     kkssssttsk','      kgkksssk','     kgak kkk','    kgak','   kgak','  kgak',' kgak','  kk'],
  rocket:['        kkk','       kwwrk','      kwrrrk','     ktwwrrk','    kttwwwwk','    kttwwwk','   kswwwwk','  krrsssk','  krkssk','   kkrrk','   kyyk','  kygk','  gg'],
  coins:['      kkkk','     kyyggk','     kgaabk','   kkkggggk','  kyyggkkk','  kgagbkyggk',' kggggggaabk','kyyggkyggggk','kgaabkgggabk',' kgggkggggk','  kkkkkkkk'],
  pouch:['    a   a','   kgggggk','    kgggk','    kyyyk','    kaagk','   kagaagk','  kggggaaak',' kggyggaaabk',' kggyyaaaabk',' kggggggaabk','  kggggaabk','   kkaabkk','     kkk'],
};
function glyph(rows) {
  const s=canvas(16,16), x=Math.floor((16-Math.max(...rows.map(r=>r.length)))/2), y=Math.floor((16-rows.length)/2);
  rows.forEach((row,dy)=>[...row].forEach((ch,dx)=>{if (ch !== ' ') s.dot(x+dx,y+dy,lootPalette[ch]);}));
  return s;
}

export function buildPixelArt(add) {
  function save(name, art) {
    const colors = new Map([[null,[0,0,0,0]]]);
    for (const hex of new Set(art.pixels)) {
      if (hex) colors.set(hex,[...hex.slice(1).match(/../g).map(n=>parseInt(n,16)),255]);
    }
    add('Pixel/'+name, art.width, art.height, (x,y)=>colors.get(art.pixels[Math.floor(y)*art.width+Math.floor(x)]), true);
  }
  save('forge',forge());
  for (const [name,art] of Object.entries({ iron:sword(false), dawn:sword(true), cinder:axe(), oak:shield('oak'), sun:shield('sun'), moon:shield('moon') })) save(name,art);
  for (const [name,rows] of Object.entries(loot)) save(name,glyph(rows));
  const cap=canvas(16,16), r=cap.rect;
  r(2,0,12,16,ink); r(0,2,16,12,ink); r(2,2,12,11,gold); r(4,1,8,1,cream); r(2,2,12,2,cream);
  r(1,4,2,8,amber); r(13,4,2,8,amber); r(2,12,12,2,amber); r(4,14,8,1,copper);
  save('cap-pixel',cap);
}
