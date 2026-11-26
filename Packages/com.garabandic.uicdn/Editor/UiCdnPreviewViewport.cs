using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor
{
    internal sealed class UiCdnPreviewViewport : VisualElement
    {
        private const int Bezel = 6;
        private static readonly List<string> Sizes = new List<string>
        {
            "Full width", "Phone portrait · 390 × 844", "Phone landscape · 844 × 390"
        };
        private readonly DropdownField picker;
        private readonly VisualElement stage = new VisualElement { name = "preview-stage" };
        private readonly VisualElement frame = new VisualElement { name = "phone-frame" };
        private readonly ScrollView screen = new ScrollView { name = "phone-screen",
            horizontalScrollerVisibility = ScrollerVisibility.Hidden, verticalScrollerVisibility = ScrollerVisibility.Hidden };
        private readonly VisualElement preview;
        private Vector2 size;

        internal UiCdnPreviewViewport(VisualElement preview, int initial, Action<int> selected)
        {
            this.preview = preview;
            picker = new DropdownField("Preview size", Sizes, Mathf.Clamp(initial, 0, Sizes.Count - 1)) { name = "preview-size" };
            picker.RegisterValueChangedCallback(_ => { ApplySize(); selected(picker.index); });
            Add(picker);
            Add(stage);
            frame.Add(screen);
            stage.RegisterCallback<GeometryChangedEvent>(_ => Fit());
            ApplySize();
        }

        private void ApplySize()
        {
            bool phone = picker.index > 0;
            preview.EnableInClassList("preview--phone", phone);
            stage.EnableInClassList("preview-stage--phone", phone);
            if (!phone)
            {
                if (frame.parent != null) frame.RemoveFromHierarchy();
                stage.Add(preview);
                stage.style.height = StyleKeyword.Null;
                preview.style.minHeight = StyleKeyword.Null;
                return;
            }

            size = picker.index == 1 ? new Vector2(390, 844) : new Vector2(844, 390);
            if (frame.parent == null)
            {
                screen.Add(preview);
                stage.Add(frame);
            }
            frame.style.width = size.x + Bezel * 2;
            frame.style.height = size.y + Bezel * 2;
            preview.style.minHeight = size.y;
            screen.scrollOffset = Vector2.zero;
            Fit();
        }

        internal void Fit()
        {
            if (picker.index == 0 || stage.contentRect.width <= 0) return;
            var detail = GetFirstAncestorOfType<ScrollView>();
            if (detail == null || !(detail.contentViewport.contentRect.height > 0)) return;
            float width = size.x + Bezel * 2, height = size.y + Bezel * 2;
            float availableHeight = Mathf.Max(400, detail.contentViewport.contentRect.height - 150);
            float scale = Mathf.Min(1, (stage.contentRect.width - 16) / width, availableHeight / height);
            if (!(scale > 0)) return;
            frame.style.scale = new Scale(new Vector3(scale, scale, 1));
            stage.style.height = Mathf.Ceil(height * scale) + 16;
        }
    }
}
