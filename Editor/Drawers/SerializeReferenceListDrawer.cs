using System;
using System.Collections;
using fefek5.Toys.Editor.Editors;
using fefek5.Toys.Editor.VisualElements;
using fefek5.Toys.Runtime.Attributes;
using UnityEditor;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SerializeReferenceListAttribute))]
    public class SerializeReferenceListDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!property.isArray || property.propertyType == SerializedPropertyType.String)
                return new HelpBox($"{nameof(SerializeReferenceListAttribute)} works only on a list or an array.",
                    HelpBoxMessageType.Error);

            return new SerializeReferenceListElement(property, GetElementType(property));
        }

        private Type GetElementType(SerializedProperty property)
        {
            // The live collection always has closed generic arguments, e.g. List<StatTransport<int>>.
            var collectionType = property.GetTarget<IList>()?.GetType() ?? fieldInfo.FieldType;

            return collectionType.IsArray ? collectionType.GetElementType() : collectionType.GetGenericArguments()[0];
        }
    }
}
