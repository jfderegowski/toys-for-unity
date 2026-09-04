using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    public class FoldoutCustomHeaderElement : Foldout
    {
        public VisualElement Header { get; private set; }

        public FoldoutCustomHeaderElement(string label, VisualElement header)
        {
            text = label;

            Header = header;
            
            hierarchy[0].hierarchy[0].Add(header);
        }
    }
}
