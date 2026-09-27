using System;
using fefek5.Toys.Editor.VisualElements;
using fefek5.Toys.Runtime.Attributes;
using UnityEditor;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SelectTypeAttribute))]
    public class SelectTypeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
                return new HelpBox($"{nameof(SelectTypeAttribute)} works only on a [SerializeReference] field.",
                    HelpBoxMessageType.Error);

            var fieldType = GetFieldType(property);

            if (fieldType == null)
                return new HelpBox($"{nameof(SelectTypeAttribute)} can not resolve {property.managedReferenceFieldTypename}.",
                    HelpBoxMessageType.Error);

            return new SelectTypeElement(property, fieldType);
        }

        private static Type GetFieldType(SerializedProperty property)
        {
            // "Assembly Namespace.Type" with generic arguments already closed, for list and array elements too.
            var typename = property.managedReferenceFieldTypename;
            var separator = typename.IndexOf(' ');

            return separator < 0 ? null : Type.GetType($"{typename[(separator + 1)..]}, {typename[..separator]}");
        }
    }
}
