// SPDX-License-Identifier: MIT
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement]
    public partial class UiDialog : UiMotionElement
    {
        protected override string OpenClass => "ui-dialog--open";
        private VisualElement previousFocus;
        private VisualElement panelRoot;
        [UxmlAttribute] public bool CloseOnOverlay { get; set; } = true;
        [UxmlAttribute] public bool CloseOnCancel { get; set; } = true;

        public UiDialog()
        {
            AddToClassList("ui-dialog");
            focusable = true;
            OnBeforeOpen += () => { previousFocus = panel?.focusController?.focusedElement as VisualElement; };
            OnAfterOpen += FocusFirst;
            OnAfterClose += () => { if (previousFocus?.panel != null) previousFocus.Focus(); };
            RegisterCallback<ClickEvent>(evt =>
            {
                if (CloseOnOverlay && (evt.target == this || evt.target is UiDialogOverlay)) Hide();
            });
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                panelRoot = panel.visualTree;
                panelRoot.RegisterCallback<FocusInEvent>(GuardFocus, TrickleDown.TrickleDown);
                panelRoot.RegisterCallback<KeyDownEvent>(HandleKey, TrickleDown.TrickleDown);
                panelRoot.RegisterCallback<NavigationCancelEvent>(HandleCancel, TrickleDown.TrickleDown);
                if (Open) schedule.Execute(FocusFirst);
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                if (panelRoot == null) return;
                panelRoot.UnregisterCallback<FocusInEvent>(GuardFocus, TrickleDown.TrickleDown);
                panelRoot.UnregisterCallback<KeyDownEvent>(HandleKey, TrickleDown.TrickleDown);
                panelRoot.UnregisterCallback<NavigationCancelEvent>(HandleCancel, TrickleDown.TrickleDown);
                panelRoot = null;
            });
        }

        private bool IsTop => Open && panelRoot != null && panelRoot.Query<UiDialog>().ToList().LastOrDefault(d => d.Open) == this;
        private VisualElement[] FocusTargets() => this.Query<VisualElement>().ToList()
            .Where(e => e != this && e.focusable && e.enabledInHierarchy && e.tabIndex >= 0 && e.resolvedStyle.display != DisplayStyle.None && e.resolvedStyle.visibility == Visibility.Visible && AncestorsVisible(e)).ToArray();
        private bool AncestorsVisible(VisualElement e)
        {
            for (var p = e.parent; p != null && p != this; p = p.parent)
                if (p.resolvedStyle.display == DisplayStyle.None || p.resolvedStyle.visibility == Visibility.Hidden) return false;
            return true;
        }
        private void FocusFirst() { if (Open) (FocusTargets().FirstOrDefault() ?? this).Focus(); }
        private void GuardFocus(FocusInEvent evt) { if (IsTop && evt.target is VisualElement target && target != this && !Contains(target)) FocusFirst(); }
        private void HandleCancel(NavigationCancelEvent evt) { if (IsTop && CloseOnCancel) { Hide(); evt.StopPropagation(); } }
        private void HandleKey(KeyDownEvent evt)
        {
            if (!IsTop) return;
            if (evt.keyCode == KeyCode.Escape && CloseOnCancel) { Hide(); evt.StopPropagation(); return; }
            if (evt.keyCode != KeyCode.Tab) return;
            var targets = FocusTargets();
            var index = System.Array.IndexOf(targets, panel.focusController.focusedElement as VisualElement);
            if (targets.Length > 0) targets[(index + (evt.shiftKey ? targets.Length - 1 : 1) + targets.Length) % targets.Length].Focus();
            else Focus();
            focusController.IgnoreEvent(evt);
            evt.StopPropagation();
        }
        public UiDialog WithOpen(bool value) { Open = value; return this; }
    }
    [UxmlElement] public partial class UiDialogOverlay : VisualElement { public UiDialogOverlay() { AddToClassList("ui-dialog-overlay"); } }
    [UxmlElement] public partial class UiDialogContent : VisualElement { public UiDialogContent() { AddToClassList("ui-dialog-content"); } }
    [UxmlElement] public partial class UiDialogHeader : VisualElement { public UiDialogHeader() { AddToClassList("ui-dialog-header"); } }
    [UxmlElement] public partial class UiDialogTitle : Label
    {
        public UiDialogTitle() { AddToClassList("ui-dialog-title"); }
        public UiDialogTitle(string text) : this() { this.text = text; }
    }
    [UxmlElement] public partial class UiDialogDescription : Label
    {
        public UiDialogDescription() { AddToClassList("ui-dialog-description"); }
        public UiDialogDescription(string text) : this() { this.text = text; }
    }
    [UxmlElement] public partial class UiDialogFooter : VisualElement { public UiDialogFooter() { AddToClassList("ui-dialog-footer"); } }
}
