// SPDX-License-Identifier: MIT
using System;
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement] public partial class UiSlider : Slider
    {
        public event Action<float> Submitted;
        public event Action Cancelled;
        public UiSlider()
        {
            AddToClassList("ui-slider"); focusable = true;
            RegisterCallback<NavigationSubmitEvent>(evt => { if (Submitted == null) return; Submitted.Invoke(value); evt.StopPropagation(); });
            RegisterCallback<NavigationCancelEvent>(evt => { if (Cancelled == null) return; Cancelled.Invoke(); evt.StopPropagation(); });
        }
        public UiSlider(string label, float minimum, float maximum, float value = 0) : this()
        {
            this.label = label; lowValue = minimum; highValue = maximum; SetValueWithoutNotify(value);
        }
        public UiSlider OnChanged(Action<float> callback) { this.RegisterValueChangedCallback(evt => callback(evt.newValue)); return this; }
        public UiSlider WithValue(float value) { this.value = value; return this; }
    }
}
