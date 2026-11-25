// SPDX-License-Identifier: MIT
using System;
using System.Linq;
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement] public partial class UiAccordion : VisualElement
    {
        [UxmlAttribute] public bool Multiple { get; set; }
        public UiAccordion() { AddToClassList("ui-accordion"); }
        internal void Expand(UiAccordionItem item)
        {
            if (Multiple) return;
            foreach (var other in this.Query<UiAccordionItem>().ToList().Where(i => i.GetFirstAncestorOfType<UiAccordion>() == this && i != item)) other.Open = false;
        }
        public UiAccordion AllowMultiple(bool value = true) { Multiple = value; return this; }
    }
    [UxmlElement] public partial class UiAccordionItem : VisualElement
    {
        private bool open;
        [UxmlAttribute] public bool Open { get => open; set { open = value; Refresh(); } }
        public event Action<bool> ValueChanged;
        public UiAccordionItem()
        {
            AddToClassList("ui-accordion-item");
            RegisterCallback<AttachToPanelEvent>(_ => Refresh());
        }
        public UiAccordionItem WithOpen(bool value) { Open = value; return this; }
        public void Toggle() { Open = !Open; ValueChanged?.Invoke(Open); }
        public void Refresh()
        {
            EnableInClassList("ui-accordion-item--open", open);
            foreach (var content in this.Query<UiAccordionContent>().ToList().Where(c => c.GetFirstAncestorOfType<UiAccordionItem>() == this)) content.Open = open;
            if (open) GetFirstAncestorOfType<UiAccordion>()?.Expand(this);
        }
    }
    [UxmlElement] public partial class UiAccordionTrigger : UiButton
    {
        public UiAccordionTrigger()
        {
            AddToClassList("ui-accordion-trigger"); Variant = "ghost";
            clicked += () => GetFirstAncestorOfType<UiAccordionItem>()?.Toggle();
            Cancelled += () => { var item = GetFirstAncestorOfType<UiAccordionItem>(); if (item != null) item.Open = false; };
        }
        public UiAccordionTrigger(string text) : this() { this.text = text; }
    }
    [UxmlElement] public partial class UiAccordionContent : UiMotionElement
    {
        protected override string OpenClass => "ui-accordion-content--open";
        public UiAccordionContent() { AddToClassList("ui-accordion-content"); }
    }
}
