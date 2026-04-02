using fefek5.Toys.Runtime.Types;
using UnityEditor;
using UnityEngine.UIElements;
using VisualElements_InspectorButton = fefek5.Toys.Editor.VisualElements.InspectorButton;

namespace fefek5.Toys.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(InspectorButton))]
    public class InspectorButtonDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var methodName = property.FindPropertyRelative("methodName").stringValue;
            var target = property.serializedObject.targetObject;

            return new VisualElements_InspectorButton(target, methodName);
        }
    }
}