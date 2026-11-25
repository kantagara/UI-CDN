// Selectors are scoped so the same screen can also be installed with another theme.
export function pixelFinishUss(scope) {
  const rules = [];
  function rule(selectors, properties) {
    const scoped = selectors.split(', ').map(s => scope + ' ' + s);
    rules.push(scoped.join(',\n') + ' { ' + properties + ' }');
  }
  const display = '-unity-font: var(--ui-display-font, url("Assets/Jersey10-Regular.ttf")); -unity-font-definition: var(--ui-display-font, url("Assets/Jersey10-Regular.ttf")); -unity-font-style: normal;';
  const image = name => 'background-image: url("Assets/Pixel/' + name + '.png");';
  rule('.ui-button, .ui-game-title, .ui-heading, .ui-card-title, .ui-dialog-title, .ui-toast-title, .ui-alert-title, .ui-badge, .treasury Label, .armory .armory-heading, .armory .armory-item-name, .armory .armory-value, .armory-filter, .gear-item .gear-caption', display);
  rule('.ui-card, .ui-dialog-content, .ui-toast, .treasury, .armory', 'border-radius: 0; border-width: 2px; border-color: #69617a; border-top-color: #afa0aa; border-bottom-color: #100f1e; background-color: #28263c;');
  rule('.ui-button', 'border-radius: 0; border-width: 2px; border-color: #62556b; min-height: 44px; font-size: 24px;');
  rule('.ui-button--primary, .ui-tabs-trigger--active', image('cap-pixel') + ' -unity-slice-left: 5; -unity-slice-right: 5; -unity-slice-top: 5; -unity-slice-bottom: 5; background-color: transparent; color: #251c29; border-color: transparent; padding: 1px 10px 3px 10px;');
  rule('.ui-button--secondary', 'background-color: #3c374d; color: #fff0cd;');
  rule('.ui-button--ghost', 'background-color: transparent; border-color: transparent;');
  rule('.ui-button--danger', 'background-color: #9a4354; color: #fff0cd;');
  rule('.ui-button:focus, .treasury .treasury-price--selected', 'border-width: 2px; border-color: #fff2bc;');
  rule('.ui-button:active', 'translate: 0 2px; -unity-background-image-tint-color: #e6c991;');
  rule('.treasury-bar, .treasury-wallet, .treasury-feature, .treasury-rewards, .treasury-deal, .treasury-pack, .armory-sheet, .armory-portrait, .gear-item, .gear-item .gear-equipped, .armory-filter', 'border-radius: 0;');
  rule('.treasury .treasury-title, .treasury .treasury-section', 'color: #ffe09d;');
  rule('.treasury-bar', 'border-bottom-width: 2px; border-bottom-color: #6b5b73;');
  rule('.treasury-feature, .armory-banner', image('forge') + ' -unity-background-scale-mode: scale-and-crop; border-color: #af8267;');
  rule('.treasury-feature', '-unity-background-image-tint-color: #a49ba6;');
  rule('.treasury .treasury-feature-title, .treasury .treasury-prize', 'color: #fff1c2; text-shadow: 2px 2px 0 #191725;');
  rule('.treasury-wallet, .treasury-deal, .treasury-rewards', 'background-color: #262237; color: #ffdfa0;');
  rule('.treasury-pack', 'background-color: #363047; color: #fff0cd; border-width: 2px; border-color: #665970; border-bottom-color: #191628;');
  rule('.treasury .treasury-price', 'width: 103px; min-height: 44px; padding: 1px 3px 3px 3px; font-size: 23px;');
  for (const name of ['chest','rocket','hammer','heart','coins','pouch']) rule('.treasury .art-' + name, image(name));
  rule('.treasury-wallet-coin, .ui-shop-coin', image('coins') + ' -unity-background-scale-mode: scale-to-fit;');
  rule('.armory .armory-heading', 'font-size: 48px; color: #fff1b3; text-shadow: 2px 2px 0 #241b31;');
  rule('.armory .armory-item-name', 'font-size: 30px; color: #ffdf96;');
  rule('.armory-region', 'color: #f5c489; text-shadow: 1px 1px 0 #241b31;');
  rule('.armory-filter', 'font-size: 21px; padding: 0 8px; background-color: #302b41; border-width: 2px; border-color: #675975; color: #efe3d7;');
  rule('.armory-filter--active', 'background-color: #e9bd72; border-color: #ffdf9c; color: #292438;');
  rule('.armory-count', 'color: #cec4cf;');
  rule('.gear-item, .armory-portrait', 'background-color: #1d1c31; border-color: #696279;');
  rule('.gear-item--rare', 'border-color: #529a9f;');
  rule('.gear-item--epic', 'border-color: #a375ae;');
  rule('.gear-item--selected', 'border-color: #f1bc69; background-color: #322b3c;');
  rule('.gear-item:focus, .armory-filter:focus', 'border-color: #fff1b3;');
  rule('.gear-item .gear-caption', 'font-size: 20px; color: #f4e9d5;');
  rule('.gear-item .gear-equipped', 'background-color: #95c7a8; color: #222d37;');
  rule('.armory-sheet', 'background-color: #342e43; border-color: #766479; border-width: 2px;');
  rule('.armory-rarity', 'color: #e4b678;');
  rule('.armory-baseline, .armory-stat-title', 'color: #e1d5d2;');
  rule('.armory-value', 'font-size: 22px; color: #fff2c7;');
  rule('.armory-stat', 'border-top-color: #5d5063;');
  rule('.armory-delta', 'color: #d1c5cd;');
  rule('.armory-delta--up', 'color: #90dda6;');
  rule('.armory-delta--down', 'color: #ffa09b;');
  rule('.armory .armory-equip', 'font-size: 25px;');
  for (const name of ['iron','dawn','cinder','oak','sun','moon'])
    rule('.gear-art-' + name, '--ui-gear-icon: url("Assets/Pixel/' + name + '.png");');
  return rules.join('\n') + '\n';
}
