// SPDX-License-Identifier: MIT
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement] public partial class UiAlert : VisualElement
    {
        private string variant = "info";
        [UxmlAttribute] public string Variant
        {
            get => variant;
            set { RemoveFromClassList("ui-alert--" + variant); variant = value ?? "info"; AddToClassList("ui-alert--" + variant); }
        }
        public UiAlert() { AddToClassList("ui-alert"); }
        public UiAlert WithVariant(string value) { Variant = value; return this; }
    }
    [UxmlElement] public partial class UiAlertTitle : Label
    {
        public UiAlertTitle() { AddToClassList("ui-alert-title"); }
        public UiAlertTitle(string text) : this() { this.text = text; }
    }
    [UxmlElement] public partial class UiAlertDescription : Label
    {
        public UiAlertDescription() { AddToClassList("ui-alert-description"); }
        public UiAlertDescription(string text) : this() { this.text = text; }
    }
}
