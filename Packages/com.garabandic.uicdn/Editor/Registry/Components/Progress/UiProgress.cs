// SPDX-License-Identifier: MIT
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    // ProgressBar's inherited migration alias collides when the preview and owned type coexist.
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false)]
    [UxmlElement] public partial class UiProgress : ProgressBar
    {
        public UiProgress() { AddToClassList("ui-progress"); lowValue = 0; highValue = 100; }
        public UiProgress(float value, string title = "") : this() { this.value = UnityEngine.Mathf.Clamp(value, lowValue, highValue); this.title = title; }
        public UiProgress WithValue(float value) { this.value = UnityEngine.Mathf.Clamp(value, lowValue, highValue); return this; }
    }
}
