// SPDX-License-Identifier: MIT
using System;
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement]
    public partial class UiButton : Button
    {
        private string variant = "primary";
        [UxmlAttribute] public string Variant
        {
            get => variant;
            set { RemoveFromClassList("ui-button--" + variant); variant = value ?? "primary"; AddToClassList("ui-button--" + variant); }
        }
        public event Action Cancelled;
        public UiButton()
        {
            AddToClassList("ui-button");
            AddToClassList("ui-button--primary");
            focusable = true;
            // Button's Clickable already handles NavigationSubmitEvent and pointer clicks.
            RegisterCallback<NavigationCancelEvent>(evt => { if (Cancelled == null) return; Cancelled.Invoke(); evt.StopPropagation(); });
        }
        public UiButton(string text, Action onClick = null) : this() { this.text = text; if (onClick != null) clicked += onClick; }
        public UiButton WithVariant(string value) { Variant = value; return this; }
        public UiButton OnClick(Action callback) { clicked += callback; return this; }
    }
}
