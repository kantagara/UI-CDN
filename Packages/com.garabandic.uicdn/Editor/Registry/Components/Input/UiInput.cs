// SPDX-License-Identifier: MIT
using System;
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement] public partial class UiInput : TextField
    {
        private bool invalid;
        [UxmlAttribute] public bool Invalid { get => invalid; set { invalid = value; EnableInClassList("ui-input--invalid", value); } }
        public event Action<string> Submitted;
        public event Action Cancelled;
        public UiInput()
        {
            AddToClassList("ui-input"); focusable = true;
            RegisterCallback<NavigationSubmitEvent>(evt => { if (Submitted == null) return; Submitted.Invoke(value); evt.StopPropagation(); });
            RegisterCallback<NavigationCancelEvent>(evt => { if (Cancelled == null) return; Cancelled.Invoke(); evt.StopPropagation(); });
        }
        public UiInput(string label, string value = "") : this() { this.label = label; SetValueWithoutNotify(value); }
        public UiInput OnChanged(Action<string> callback) { this.RegisterValueChangedCallback(evt => callback(evt.newValue)); return this; }
        public UiInput WithValue(string value) { this.value = value; return this; }
    }
}
