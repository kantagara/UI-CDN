// SPDX-License-Identifier: MIT
using System;
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement] public partial class UiSwitch : Toggle
    {
        public event Action Cancelled;
        public UiSwitch()
        {
            AddToClassList("ui-switch"); focusable = true;
            var thumb = new VisualElement { pickingMode = PickingMode.Ignore };
            thumb.AddToClassList("ui-switch-thumb");
            this.Q(className: "unity-toggle__checkmark").Add(thumb);
            RegisterCallback<NavigationCancelEvent>(evt => { if (Cancelled == null) return; Cancelled.Invoke(); evt.StopPropagation(); });
        }
        public UiSwitch(string text, bool value = false) : this() { this.text = text; SetValueWithoutNotify(value); }
        public UiSwitch OnChanged(Action<bool> callback) { this.RegisterValueChangedCallback(evt => callback(evt.newValue)); return this; }
        public UiSwitch WithValue(bool value) { this.value = value; return this; }
    }
}
