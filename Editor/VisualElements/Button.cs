using System;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    public class Button : UnityEngine.UIElements.Button
    {
        public Button() : base()
        {
            OverrideStyle();
        }

        public Button(Background iconImage, Action clickEvent = null) : base(iconImage, clickEvent)
        {
            OverrideStyle();
        }

        public Button(Action clickEvent) : base(clickEvent)
        {
            OverrideStyle();
        }

        private void OverrideStyle()
        {
            focusable = false;
            
            // style.marginLeft = 0;
            // style.marginTop = 0;
            // style.marginRight = 0;
            // style.marginBottom = 0;
            
            style.borderLeftWidth = 0;
            style.borderTopWidth = 0;
            style.borderRightWidth = 0;
            style.borderBottomWidth = 0;

            style.justifyContent = Justify.Center;
            
            style.alignItems = Align.Center;
        }
    }
}