using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    public class FoldoutCustomHeaderElement : Foldout
    {
        public VisualElement HeaderParent { get; private set; }
        public VisualElement Header { get; private set; }

        public FoldoutCustomHeaderElement(VisualElement header)
        {
            var headerParent = new PropertyField() {
                style = {
                    position = Position.Absolute,
                    left = 0,
                    right = 0,
                    top = 0,
                    height = 18,
                }
            };

            headerParent.Add(header);
            
            hierarchy.Insert(1, headerParent);

            HeaderParent = headerParent;
            Header = header;
        }
    }
}
