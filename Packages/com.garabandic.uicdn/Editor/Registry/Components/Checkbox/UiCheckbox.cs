// SPDX-License-Identifier: MIT
using System;
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement] public partial class UiCheckbox : Toggle
    {
        public event Action Cancelled;
        public UiCheckbox()
        {
            AddToClassList("ui-checkbox"); focusable = true;
            RegisterCallback<NavigationCancelEvent>(evt => { if (Cancelled == null) return; Cancelled.Invoke(); evt.StopPropagation(); });
        }
        public UiCheckbox(string text, bool value = false) : this() { this.text = text; SetValueWithoutNotify(value); }
        public UiCheckbox OnChanged(Action<bool> callback) { this.RegisterValueChangedCallback(evt => callback(evt.newValue)); return this; }
        public UiCheckbox WithValue(bool value) { this.value = value; return this; }
    }
}
