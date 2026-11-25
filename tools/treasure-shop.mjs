// The registry previews the very same document shipped to the game.
const reward = (id, quantity, title) => `<ui:VisualElement class="treasury-reward" tooltip="${title}">
      <ui:VisualElement class="treasury-reward-art art-${id}" />
      <ui:Label class="treasury-quantity" text="${quantity}" />
    </ui:VisualElement>`;
const pack = (id, count, price, art) => `<ui:VisualElement class="treasury-pack">
    <ui:VisualElement class="treasury-money art-${art}" />
    <ui:Label class="treasury-count" text="${count}" />
    <c:UiButton name="${id}" text="${price}" class="treasury-price ui-game-action" />
  </ui:VisualElement>`;
export const shopMarkup = `<c:UiTreasureShop>
  <ui:VisualElement class="treasury-bar">
    <ui:Label text="Shop" class="treasury-title" />
    <ui:VisualElement class="treasury-wallet"><ui:VisualElement class="treasury-wallet-coin" /><ui:Label text="2,040" /></ui:VisualElement>
  </ui:VisualElement>
  <ui:VisualElement class="treasury-feature">
    <ui:Label text="King’s Treasury" class="treasury-feature-title" />
    <ui:VisualElement class="treasury-loot">
      <ui:VisualElement class="treasury-jackpot"><ui:VisualElement class="treasury-chest art-chest" /><ui:Label text="25,000" class="treasury-prize" /></ui:VisualElement>
      <ui:VisualElement class="treasury-rewards">
        ${reward('rocket', '×12', 'Rockets')}${reward('hammer', '×5', 'Hammers')}${reward('heart', '2h', 'Unlimited lives')}
      </ui:VisualElement>
    </ui:VisualElement>
    <ui:VisualElement class="treasury-deal"><ui:Label text="Treasure bundle" /><c:UiButton name="treasure-bundle" text="$12.99" class="treasury-price ui-game-action" /></ui:VisualElement>
  </ui:VisualElement>
  <ui:Label class="treasury-section" text="Coin packs" />
  ${pack('pocket', '1,000', '$1.99', 'coins')}
  ${pack('pouch', '5,000', '$4.99', 'pouch')}
  ${pack('chest', '10,000', '$8.99', 'chest')}
</c:UiTreasureShop>`;
