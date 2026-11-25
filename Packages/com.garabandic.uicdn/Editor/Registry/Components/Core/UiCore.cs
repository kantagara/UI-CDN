// SPDX-License-Identifier: MIT
using System;
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    public static class UiBuilder
    {
        public static T With<T>(this T element, params VisualElement[] children) where T : VisualElement
        {
            foreach (var child in children) element.Add(child);
            return element;
        }

        public static T Class<T>(this T element, string className) where T : VisualElement
        {
            element.AddToClassList(className);
            return element;
        }
    }

    /// <summary>Override animation methods and call CompleteEnter/Exit when a tween finishes.</summary>
    [UxmlElement]
    public partial class UiMotionElement : VisualElement
    {
        private bool isOpen;
        private int revision;
        private IVisualElementScheduledItem completion;
        protected virtual string OpenClass => "ui-motion--open";
        public event Action OnBeforeOpen;
        public event Action OnAfterOpen;
        public event Action OnBeforeClose;
        public event Action OnAfterClose;
        // Match --ui-duration-panel if you customize the theme. Zero enables reduced motion.
        [UxmlAttribute] public int AnimationDurationMs { get; set; } = 180;
        [UxmlAttribute] public bool Open { get => isOpen; set => SetOpen(value); }

        public UiMotionElement()
        {
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                revision++;
                completion?.Pause();
                style.display = isOpen ? DisplayStyle.Flex : DisplayStyle.None;
                EnableInClassList(OpenClass, isOpen);
            });
            style.display = DisplayStyle.None;
        }

        public void Show() => SetOpen(true);
        public void Hide() => SetOpen(false);

        private void SetOpen(bool value)
        {
            if (isOpen == value) return;
            isOpen = value;
            var current = ++revision;
            completion?.Pause();
            if (value)
            {
                style.display = DisplayStyle.Flex;
                OnBeforeOpen?.Invoke();
                if (current != revision) return;
                if (panel == null) { AddToClassList(OpenClass); CompleteEnter(); return; }
                completion = schedule.Execute(() => { if (current == revision) PlayEnterAnimation(); }).StartingIn(16);
            }
            else
            {
                OnBeforeClose?.Invoke();
                if (current != revision) return;
                if (panel == null) { RemoveFromClassList(OpenClass); CompleteExit(); return; }
                PlayExitAnimation();
            }
        }

        protected virtual void PlayEnterAnimation()
        {
            AddToClassList(OpenClass);
            ScheduleCompletion(true);
        }

        protected virtual void PlayExitAnimation()
        {
            RemoveFromClassList(OpenClass);
            ScheduleCompletion(false);
        }

        private void ScheduleCompletion(bool entering)
        {
            var current = revision;
            completion = schedule.Execute(() =>
            {
                if (current != revision) return;
                if (entering) CompleteEnter(); else CompleteExit();
            }).StartingIn(Math.Max(0, AnimationDurationMs));
        }

        protected void CompleteEnter() { if (isOpen) OnAfterOpen?.Invoke(); }
        protected void CompleteExit()
        {
            if (isOpen) return;
            style.display = DisplayStyle.None;
            OnAfterClose?.Invoke();
        }
    }
}
