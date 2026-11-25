// SPDX-License-Identifier: MIT
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement] public partial class UiCard : VisualElement
    {
        public UiCard() { AddToClassList("ui-card"); }
    }
    [UxmlElement] public partial class UiCardHeader : VisualElement
    {
        public UiCardHeader() { AddToClassList("ui-card-header"); }
    }
    [UxmlElement] public partial class UiCardTitle : Label
    {
        public UiCardTitle() { AddToClassList("ui-card-title"); }
        public UiCardTitle(string text) : this() { this.text = text; }
    }
    [UxmlElement] public partial class UiCardDescription : Label
    {
        public UiCardDescription() { AddToClassList("ui-card-description"); }
        public UiCardDescription(string text) : this() { this.text = text; }
    }
    [UxmlElement] public partial class UiCardContent : VisualElement
    {
        public UiCardContent() { AddToClassList("ui-card-content"); }
    }
    [UxmlElement] public partial class UiCardFooter : VisualElement
    {
        public UiCardFooter() { AddToClassList("ui-card-footer"); }
    }
}
