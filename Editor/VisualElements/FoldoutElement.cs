using fefek5.Toys.Editor.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    public class FoldoutElement : Foldout
    {
        public VisualElement Header { get; private set; }
        
        public FoldoutElement(SerializedProperty property, VisualElement header =  null)
            : this(property.displayName, header)
        {
            this.BindProperty(property);
            
            foreach (var child in property.GetVisibleChildren()) 
                Add(new PropertyField(child));
        }

        public FoldoutElement(string label, VisualElement header =  null)
        {
            text = label;

            if (header == null) return;
            
            Header = header;
            
            hierarchy[0].hierarchy[0].Add(header);
        }
    }
}
