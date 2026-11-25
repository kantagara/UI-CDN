// SPDX-License-Identifier: MIT
using System;
using System.Linq;
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement]
    public partial class UiTabs : VisualElement
    {
        private string selected = "";
        [UxmlAttribute] public string Value { get => selected; set => Select(value); }
        public event Action<string> ValueChanged;
        public UiTabs()
        {
            AddToClassList("ui-tabs");
            RegisterCallback<AttachToPanelEvent>(_ => Refresh());
            RegisterCallback<NavigationMoveEvent>(evt =>
            {
                if (!(evt.target is UiTabsTrigger trigger) || trigger.GetFirstAncestorOfType<UiTabs>() != this) return;
                int delta;
                if (evt.direction == NavigationMoveEvent.Direction.Left) delta = -1;
                else if (evt.direction == NavigationMoveEvent.Direction.Right) delta = 1;
                else return;
                var triggers = Triggers().Where(t => t.enabledInHierarchy).ToArray();
                if (triggers.Length == 0) return;
                var index = Array.IndexOf(triggers, trigger);
                var next = triggers[(index + delta + triggers.Length) % triggers.Length];
                Select(next.Value);
                next.Focus();
                focusController.IgnoreEvent(evt);
                evt.StopPropagation();
            });
        }
        private UiTabsTrigger[] Triggers() => this.Query<UiTabsTrigger>().ToList().Where(t => t.GetFirstAncestorOfType<UiTabs>() == this).ToArray();
        public void Refresh()
        {
            var triggers = Triggers();
            // UXML attributes are assigned before child elements have been deserialized.
            if (triggers.Length == 0) return;
            if (!triggers.Any(t => t.Value == selected && t.enabledSelf)) selected = triggers.FirstOrDefault(t => t.enabledSelf)?.Value ?? "";
            foreach (var trigger in triggers)
            {
                var active = trigger.Value == selected;
                trigger.EnableInClassList("ui-tabs-trigger--active", active);
                trigger.tabIndex = active ? 0 : -1;
            }
            foreach (var content in this.Query<UiTabsContent>().ToList().Where(t => t.GetFirstAncestorOfType<UiTabs>() == this))
            {
                var active = content.Value == selected;
                content.EnableInClassList("ui-tabs-content--active", active);
                content.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        public void Select(string value)
        {
            value = value ?? "";
            if (selected == value) { Refresh(); return; }
            var triggers = Triggers();
            if (triggers.Length > 0 && !triggers.Any(t => t.Value == value && t.enabledSelf)) return;
            selected = value;
            Refresh();
            ValueChanged?.Invoke(selected);
        }
        public UiTabs WithValue(string value) { Value = value; return this; }
    }
    [UxmlElement] public partial class UiTabsList : VisualElement { public UiTabsList() { AddToClassList("ui-tabs-list"); } }
    [UxmlElement] public partial class UiTabsTrigger : UiButton
    {
        [UxmlAttribute] public string Value { get; set; } = "";
        public UiTabsTrigger() { AddToClassList("ui-tabs-trigger"); Variant = "ghost"; clicked += () => GetFirstAncestorOfType<UiTabs>()?.Select(Value); }
        public UiTabsTrigger(string value, string text) : this() { Value = value; this.text = text; }
    }
    [UxmlElement] public partial class UiTabsContent : VisualElement
    {
        [UxmlAttribute] public string Value { get; set; } = "";
        public UiTabsContent() { AddToClassList("ui-tabs-content"); }
        public UiTabsContent(string value) : this() { Value = value; }
    }
}
