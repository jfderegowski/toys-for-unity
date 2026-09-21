using System;
using System.Collections.Generic;
using System.Linq;
using fefek5.Toys.Editor.Extensions;
using fefek5.Toys.Editor.VisualElements;
using fefek5.Toys.Runtime.Attributes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SelectTypeAttribute))]
    public class SelectTypeDrawer : PropertyDrawer
    {
        private const string NONE_CHOICE = "None";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
                return new HelpBox($"{nameof(SelectTypeAttribute)} works only on a [SerializeReference] field.",
                    HelpBoxMessageType.Error);

            var fieldType = GetFieldType(property);

            if (fieldType == null)
                return new HelpBox($"{nameof(SelectTypeAttribute)} can not resolve {property.managedReferenceFieldTypename}.",
                    HelpBoxMessageType.Error);

            var types = fieldType.GetAssignableTypes().OrderBy(type => type.GetDisplayName()).ToList();

            var root = new VisualElement();
            root.Add(CreateFoldout(root, property, types));

            return root;
        }

        private static FoldoutElement CreateFoldout(VisualElement root, SerializedProperty property, List<Type> types)
        {
            var choices = types.Select(type => type.GetDisplayName()).Prepend(NONE_CHOICE).ToList();

            var dropdown = new DropdownField(choices, 0) {
                style = { flexGrow = 1 }
            };

            dropdown.SetValueWithoutNotify(property.managedReferenceValue?.GetType().GetDisplayName() ?? NONE_CHOICE);

            // The dropdown sits in the foldout's toggle, which would expand or collapse on the same click or key press.
            dropdown.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
            dropdown.RegisterCallback<NavigationSubmitEvent>(evt => evt.StopPropagation());

            dropdown.RegisterValueChangedCallback(_ => {
                var type = dropdown.index > 0 ? types[dropdown.index - 1] : null;

                property.serializedObject.Update();
                property.managedReferenceValue = type == null ? null : Activator.CreateInstance(type, true);
                property.isExpanded = type != null;
                property.serializedObject.ApplyModifiedProperties();

                // The fields of the old type are gone, so the foldout is built again for the new one.
                root.Clear();
                root.Add(CreateFoldout(root, property, types));
                root.Bind(property.serializedObject);
            });

            return new FoldoutElement(property, dropdown);
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
