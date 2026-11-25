using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Garabandic.UiCdn.Editor.Preview
{
    [UxmlElement]
    public partial class UiTreasureShop : VisualElement
    {
        public event Action<string> OfferSelected;
        private readonly Dictionary<Button, Action> handlers = new Dictionary<Button, Action>();

        public UiTreasureShop()
        {
            AddToClassList("treasury");
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                foreach (var button in this.Query<Button>(className: "treasury-price").ToList())
                {
                    Action handler = () => SelectOffer(button.name);
                    handlers.Add(button, handler);
                    button.clicked += handler;
                }
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                foreach (var pair in handlers) pair.Key.clicked -= pair.Value;
                handlers.Clear();
            });
        }

        // Game code can subscribe here to open its own confirmation dialog.
        public UiTreasureShop SelectOffer(string id)
        {
            var button = this.Q<Button>(id, "treasury-price");
            if (button == null) return this;
            foreach (var offer in this.Query<Button>(className: "treasury-price").ToList())
                offer.EnableInClassList("treasury-price--selected", offer == button);
            OfferSelected?.Invoke(id);
            return this;
        }
    }
}
