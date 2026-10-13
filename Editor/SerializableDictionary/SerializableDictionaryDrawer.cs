using fefek5.Toys.Editor.VisualElements;
using fefek5.Toys.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) =>
            new SerializableDictionaryElement(property);
    }
}
