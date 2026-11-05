using fefek5.Toys.Editor.VisualElements;
using fefek5.Toys.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        // Titled with the label of its PropertyField, which is the name of the property unless the field sets another.
        public override VisualElement CreatePropertyGUI(SerializedProperty property) =>
            new SerializableDictionaryElement(property) { headerTitle = preferredLabel };
    }
}
