// SPDX-License-Identifier: MIT
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement] public partial class UiToast : UiMotionElement
    {
        protected override string OpenClass => "ui-toast--open";
        private IVisualElementScheduledItem dismiss;
        [UxmlAttribute] public int DurationMs { get; set; } = 4000;
        public UiToast()
        {
            AddToClassList("ui-toast"); focusable = true;
            OnAfterOpen += ScheduleDismiss;
            OnBeforeClose += () => dismiss?.Pause();
            RegisterCallback<AttachToPanelEvent>(_ => { if (Open) ScheduleDismiss(); });
            RegisterCallback<DetachFromPanelEvent>(_ => dismiss?.Pause());
            RegisterCallback<PointerEnterEvent>(_ => dismiss?.Pause());
            RegisterCallback<PointerLeaveEvent>(_ => { if (Open) ScheduleDismiss(); });
            RegisterCallback<FocusInEvent>(_ => dismiss?.Pause());
            RegisterCallback<FocusOutEvent>(_ => { if (Open) ScheduleDismiss(); });
            RegisterCallback<NavigationSubmitEvent>(evt => { Hide(); evt.StopPropagation(); });
            RegisterCallback<NavigationCancelEvent>(evt => { Hide(); evt.StopPropagation(); });
        }
        private void ScheduleDismiss()
        {
            dismiss?.Pause();
            var focused = panel?.focusController?.focusedElement as VisualElement;
            if (DurationMs > 0 && focused != this && (focused == null || !Contains(focused)))
                dismiss = schedule.Execute(Hide).StartingIn(DurationMs);
        }
        public UiToast WithDuration(int milliseconds) { DurationMs = milliseconds; return this; }
    }
    [UxmlElement] public partial class UiToastTitle : Label
    {
        public UiToastTitle() { AddToClassList("ui-toast-title"); }
        public UiToastTitle(string text) : this() { this.text = text; }
    }
    [UxmlElement] public partial class UiToastDescription : Label
    {
        public UiToastDescription() { AddToClassList("ui-toast-description"); }
        public UiToastDescription(string text) : this() { this.text = text; }
    }
}
