using fefek5.Toys.Editor.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    public class FoldoutElement : Foldout
    {
        private const float ICON_SIZE = 16;

        public VisualElement Header { get; private set; }

        /// <summary>
        /// Icon before the label; hidden when the foldout is made without one.
        /// </summary>
        public Image Icon { get; }

        /// <summary>
        /// The clickable row of the foldout, with the arrow, the icon, the label and the header.
        /// </summary>
        protected VisualElement Input => hierarchy[0].hierarchy[0];
        
        public FoldoutElement(SerializedProperty property, VisualElement header =  null, Texture icon = null)
            : this(property.displayName, header, icon)
        {
            this.BindProperty(property);
            
            foreach (var child in property.GetVisibleChildren()) 
                Add(new PropertyField(child));
        }

        public FoldoutElement(string label, VisualElement header =  null, Texture icon = null)
        {
            text = label;

            Icon = new Image {
                image = icon,
                style = {
                    width = ICON_SIZE,
                    height = ICON_SIZE,
                    flexShrink = 0,
                    marginRight = 5,
                    display = icon ? DisplayStyle.Flex : DisplayStyle.None
                }
            };

            // After the arrow, before the label.
            Input.Insert(1, Icon);

            if (header == null) return;
            
            Header = header;
            
            Input.Add(header);
        }
    }
}
