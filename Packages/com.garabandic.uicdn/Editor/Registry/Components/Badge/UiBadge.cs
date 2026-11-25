// SPDX-License-Identifier: MIT
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement] public partial class UiBadge : Label
    {
        private string variant = "default";
        [UxmlAttribute] public string Variant
        {
            get => variant;
            set { RemoveFromClassList("ui-badge--" + variant); variant = value ?? "default"; AddToClassList("ui-badge--" + variant); }
        }
        public UiBadge() { AddToClassList("ui-badge"); }
        public UiBadge(string text) : this() { this.text = text; }
        public UiBadge WithVariant(string value) { Variant = value; return this; }
    }
}
