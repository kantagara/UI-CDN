// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    public enum GearSlot { Weapon, Shield }
    public enum GearRarity { Common, Rare, Epic }

    [UxmlElement]
    public partial class UiGearItem : Button
    {
        private static readonly CustomStyleProperty<Texture2D> GearIconStyle = new CustomStyleProperty<Texture2D>("--ui-gear-icon");
        private Texture2D baseIcon;
        private Texture2D themedIcon;
        public Texture2D DisplayedIcon => themedIcon != null ? themedIcon : baseIcon;
        private readonly VisualElement picture = new VisualElement().Class("gear-icon");
        private readonly Label caption = new Label().Class("gear-caption");
        private readonly Label mark = new Label("WORN").Class("gear-equipped");
        [UxmlAttribute("equipment-slot")] public GearSlot Slot { get; set; }
        [UxmlAttribute] public GearRarity Rarity { get; set; }
        [UxmlAttribute] public int Attack { get; set; }
        [UxmlAttribute] public int Guard { get; set; }
        [UxmlAttribute] public bool InitiallyEquipped { get; set; }
        [UxmlAttribute] public string ItemName { get => caption.text; set => caption.text = value; }
        [UxmlAttribute] public Texture2D Icon
        {
            get => baseIcon;
            set { baseIcon = value; RefreshIcon(); }
        }
        public UiGearItem()
        {
            AddToClassList("gear-item");
            focusable = true;
            mark.tooltip = "Equipped";
            Add(picture); Add(caption); Add(mark);
            RegisterCallback<CustomStyleResolvedEvent>(evt =>
            {
                evt.customStyle.TryGetValue(GearIconStyle, out Texture2D texture);
                if (texture == themedIcon) return;
                themedIcon = texture;
                RefreshIcon();
            });
            clicked += () => this.GetFirstAncestorOfType<UiArmory>()?.Inspect(this);
        }
        private void RefreshIcon()
        {
            picture.style.backgroundImage = DisplayedIcon == null ? new StyleBackground(StyleKeyword.None) : new StyleBackground(DisplayedIcon);
            var armory = this.GetFirstAncestorOfType<UiArmory>();
            if (armory != null && armory.Selected == this) armory.RefreshPortrait();
        }
        public UiGearItem(string title, GearSlot slot, int attack, int guard) : this()
        {
            ItemName = title; Slot = slot; Attack = attack; Guard = guard;
        }
        internal void Present(bool selected, bool equipped)
        {
            EnableInClassList("gear-item--selected", selected);
            EnableInClassList("gear-item--rare", Rarity == GearRarity.Rare);
            EnableInClassList("gear-item--epic", Rarity == GearRarity.Epic);
            mark.style.display = equipped ? DisplayStyle.Flex : DisplayStyle.None;
            tooltip = $"{ItemName} · {Rarity}" + (equipped ? " · Equipped" : "");
        }
    }

    [UxmlElement]
    public partial class UiArmory : VisualElement
    {
        private readonly VisualElement inventory = new VisualElement().Class("armory-inventory");
        private readonly Dictionary<GearSlot, UiGearItem> loadout = new Dictionary<GearSlot, UiGearItem>();
        private readonly Dictionary<GearSlot, Button> filters = new Dictionary<GearSlot, Button>();
        private readonly Label heading = new Label("Armory").Class("armory-heading");
        private readonly Label region = new Label().Class("armory-region");
        private readonly Label count = new Label().Class("armory-count");
        private readonly Label itemName = new Label().Class("armory-item-name");
        private readonly Label rarity = new Label().Class("armory-rarity");
        private readonly Label baseline = new Label().Class("armory-baseline");
        private readonly VisualElement hero = new VisualElement().Class("armory-portrait");
        private readonly Label attack = new Label(), guard = new Label();
        private readonly Label attackDelta = new Label(), guardDelta = new Label();
        private readonly UiButton equip = new UiButton().Class("armory-equip");
        private GearSlot slot;
        private IEnumerable<UiGearItem> Items => inventory.Children().OfType<UiGearItem>();
        public override VisualElement contentContainer => inventory;
        [UxmlAttribute] public string Title { get => heading.text; set => heading.text = value; }
        [UxmlAttribute] public string Location { get => region.text; set => region.text = value; }
        [UxmlAttribute] public string InitialItem { get; set; }
        public UiGearItem Selected { get; private set; }
        public event Action<UiGearItem> Equipped;

        public UiArmory()
        {
            AddToClassList("armory");
            Location = "SUNFALL FORGE";
            hierarchy.Add(new VisualElement().Class("armory-banner").With(region, heading));
            var bar = new VisualElement().Class("armory-bar");
            foreach (GearSlot type in Enum.GetValues(typeof(GearSlot)))
            {
                var tab = new Button(() => ShowSlot(type)) { text = type == GearSlot.Weapon ? "Weapons" : "Shields", name = "slot-" + type };
                filters[type] = tab.Class("armory-filter");
                bar.Add(tab);
            }
            bar.Add(count);
            hierarchy.Add(bar);
            hierarchy.Add(inventory);
            var copy = new VisualElement().Class("armory-copy").With(rarity, itemName, baseline);
            var sheet = new VisualElement().Class("armory-sheet");
            sheet.Add(new VisualElement().Class("armory-inspect").With(hero, copy));
            sheet.Add(Stat("Attack", attack, attackDelta));
            sheet.Add(Stat("Guard", guard, guardDelta));
            hierarchy.Add(sheet);
            equip.name = "equip";
            equip.clicked += EquipSelected;
            hierarchy.Add(equip);
            RegisterCallback<AttachToPanelEvent>(_ => schedule.Execute(Refresh));
            RegisterCallback<NavigationCancelEvent>(evt =>
            {
                if (panel?.focusController.focusedElement == filters[slot]) return;
                ShowSlot(slot); filters[slot].Focus(); evt.StopPropagation();
            });
        }

        private static VisualElement Stat(string title, Label value, Label change)
        {
            return new VisualElement().Class("armory-stat").With(new Label(title).Class("armory-stat-title"),
                value.Class("armory-value"), change.Class("armory-delta"));
        }

        public UiArmory WithItems(params UiGearItem[] items)
        {
            foreach (var item in items) Add(item);
            Refresh();
            return this;
        }

        public void Refresh()
        {
            var items = Items.ToList();
            foreach (var type in filters.Keys)
            {
                var current = GetEquipped(type);
                if (current?.Slot == type && items.Contains(current)) continue;
                loadout[type] = items.FirstOrDefault(i => i.Slot == type && i.InitiallyEquipped);
            }
            if (!items.Contains(Selected))
                Selected = items.FirstOrDefault(i => !string.IsNullOrEmpty(InitialItem) && i.name == InitialItem) ?? items.FirstOrDefault();
            ShowSlot(Selected?.Slot ?? slot, Selected);
        }

        public UiArmory ShowSlot(GearSlot type) => ShowSlot(type, null);
        private UiArmory ShowSlot(GearSlot type, UiGearItem preferred)
        {
            slot = type;
            var equipped = GetEquipped(type);
            var available = Items.Where(i => i.Slot == type).ToList();
            Selected = preferred ?? equipped ?? available.FirstOrDefault();
            foreach (var card in Items)
            {
                card.style.display = card.Slot == type ? DisplayStyle.Flex : DisplayStyle.None;
                card.Present(card == Selected, card == GetEquipped(card.Slot));
            }
            foreach (var tab in filters) tab.Value.EnableInClassList("armory-filter--active", tab.Key == type);
            count.text = available.Count + " ITEMS";
            ShowComparison(Selected, equipped);
            return this;
        }

        public UiArmory Inspect(UiGearItem item)
        {
            if (item != null && inventory.Children().Contains(item)) ShowSlot(item.Slot, item);
            return this;
        }

        public void EquipSelected()
        {
            var item = Selected;
            if (item == null || GetEquipped(slot) == item) return;
            loadout[slot] = item;
            ShowSlot(slot, item);
            if (panel != null) item.Focus();
            Equipped?.Invoke(item);
        }

        public UiGearItem GetEquipped(GearSlot type) => loadout.TryGetValue(type, out var item) ? item : null;

        private void ShowComparison(UiGearItem item, UiGearItem current)
        {
            RefreshPortrait();
            rarity.text = item == null ? "" : (item.Rarity + "  /  " + item.Slot).ToUpperInvariant();
            itemName.text = item?.ItemName ?? "Empty slot";
            baseline.text = current == null ? "Slot is empty" : "Equipped: " + current.ItemName;
            UpdateStat(attack, attackDelta, item?.Attack ?? 0, current?.Attack ?? 0);
            UpdateStat(guard, guardDelta, item?.Guard ?? 0, current?.Guard ?? 0);
            equip.text = item == null ? "No item" : current == item ? "Equipped" : "Equip " + item.ItemName;
            equip.SetEnabled(item != null && item != current);
        }

        internal void RefreshPortrait()
        {
            var texture = Selected?.DisplayedIcon;
            hero.style.backgroundImage = texture == null ? new StyleBackground(StyleKeyword.None) : new StyleBackground(texture);
        }

        private static void UpdateStat(Label value, Label delta, int next, int before)
        {
            value.text = next == before ? next.ToString() : $"{before} → {next}";
            int difference = next - before;
            delta.text = difference > 0 ? "+" + difference : difference == 0 ? "—" : difference.ToString();
            delta.EnableInClassList("armory-delta--up", difference > 0);
            delta.EnableInClassList("armory-delta--down", difference < 0);
        }
    }
}
