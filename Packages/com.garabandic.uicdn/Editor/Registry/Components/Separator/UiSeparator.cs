// SPDX-License-Identifier: MIT
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement] public partial class UiSeparator : VisualElement
    {
        private bool vertical;
        [UxmlAttribute] public bool Vertical { get => vertical; set { vertical = value; EnableInClassList("ui-separator--vertical", value); } }
        public UiSeparator() { AddToClassList("ui-separator"); pickingMode = PickingMode.Ignore; }
        public UiSeparator(bool vertical) : this() { Vertical = vertical; }
        public UiSeparator WithVertical(bool value = true) { Vertical = value; return this; }
    }
}
