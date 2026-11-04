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
        private readonly Func<SerializedProperty, VisualElement> _createContent;

        private string _label;
        private DropdownField _dropdown;

        /// <summary>The name before the dropdown; at the start the display name of the property.</summary>
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                _dropdown.label = value;
            }
        }

        /// <param name="property">The [SerializeReference] property</param>
        /// <param name="fieldType">The type every type to pick from is assignable to</param>
        /// <param name="createContent">
        /// Builds what the foldout shows for the current value, again after each pick, so it has to handle every type
        /// and null; by default the foldout shows the fields of the value.
        /// </param>
        public SelectTypeElement(SerializedProperty property, Type fieldType,
            Func<SerializedProperty, VisualElement> createContent = null)
        {
            _property = property;
            _types = fieldType.GetAssignableTypes().ToList();
            _createContent = createContent;
            _label = property.displayName;

            Add(CreateFoldout());
        }

        private FoldoutElement CreateFoldout()
        {
            // The label is the dropdown's, so Unity lines the dropdown up with the other fields of the inspector.
            var dropdown = new TypeDropdownField(_types, _property.managedReferenceValue?.GetType(), OnPicked) {
                label = _label,
                style = { flexGrow = 1 }
            };
            _dropdown = dropdown;
            dropdown.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            ResetFoldoutStyles(dropdown);

            // The dropdown sits in the foldout's toggle, which would expand or collapse on the same click or key press.
            dropdown.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
            dropdown.RegisterCallback<NavigationSubmitEvent>(evt => evt.StopPropagation());

            FoldoutElement foldout;

            if (_createContent == null)
            {
                foldout = new FoldoutElement(_property, dropdown) { text = string.Empty };
            }
            else
            {
                foldout = new FoldoutElement(string.Empty, dropdown);
                foldout.BindProperty(_property);
                foldout.Add(_createContent(_property));
            }

            // The name is the dropdown's label now, and a click on it still has to fold.
            dropdown.labelElement.RegisterCallback<ClickEvent>(_ => foldout.value = !foldout.value);

            return foldout;
        }

        /// <summary>
        /// The foldout's toggle styles every text inside it and an inspector field reaches past its parent on the
        /// right, so without this the dropdown in the header looks different from one in a field below it.
        /// </summary>
        private static void ResetFoldoutStyles(DropdownField dropdown)
        {
            // Past the toggle the dropdown gets cut off.
            dropdown.style.marginRight = 0;

            dropdown.labelElement.style.marginLeft = 0;
            dropdown.labelElement.style.paddingTop = 2;

            var text = dropdown.Q(className: DropdownField.textUssClassName);
            text.style.marginLeft = 0;
            text.style.paddingTop = 0;
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
