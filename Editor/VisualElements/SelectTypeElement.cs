using System;
using System.Collections.Generic;
using System.Linq;
using fefek5.Toys.Editor.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    /// <summary>
    /// A foldout for a [SerializeReference] property with a <see cref="TypeDropdownField"/> in its header of every
    /// concrete type assignable to <c>fieldType</c>. Picking a type creates a new instance of it in place of the
    /// current value.
    /// </summary>
    public class SelectTypeElement : VisualElement
    {
        private readonly SerializedProperty _property;
        private readonly List<Type> _types;

        public SelectTypeElement(SerializedProperty property, Type fieldType)
        {
            _property = property;
            _types = fieldType.GetAssignableTypes().ToList();

            Add(CreateFoldout());
        }

        private FoldoutElement CreateFoldout()
        {
            var dropdown = new TypeDropdownField(_types, _property.managedReferenceValue?.GetType(), OnPicked) {
                style = { flexGrow = 1 }
            };

            // The dropdown sits in the foldout's toggle, which would expand or collapse on the same click or key press.
            dropdown.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
            dropdown.RegisterCallback<NavigationSubmitEvent>(evt => evt.StopPropagation());

            return new FoldoutElement(_property, dropdown);
        }

        private void OnPicked(Type type)
        {
            _property.serializedObject.Update();
            _property.managedReferenceValue = type == null ? null : Activator.CreateInstance(type, true);
            _property.isExpanded = type != null;
            _property.serializedObject.ApplyModifiedProperties();

            // The fields of the old type are gone, so the foldout is built again for the new one.
            Clear();
            Add(CreateFoldout());
            this.Bind(_property.serializedObject);
        }
    }
}
