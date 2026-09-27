using System;
using fefek5.Toys.Editor.Extensions;
using UnityEditor;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    /// <summary>
    /// Unity's default list for a [SerializeReference] list or array, whose add button shows every concrete type
    /// assignable to the element type.
    /// </summary>
    public class SerializeReferenceListElement : ListView
    {
        private readonly SerializedProperty _property;
        private readonly Type _elementType;

        public SerializeReferenceListElement(SerializedProperty property, Type elementType)
        {
            _property = property;
            _elementType = elementType;

            // Same setup as the list Unity draws in the default inspector, only the add button is replaced.
            bindingPath = property.propertyPath;
            headerTitle = property.displayName;
            showFoldoutHeader = true;
            showBoundCollectionSize = true;
            showAddRemoveFooter = true;
            showBorder = true;
            reorderable = true;
            reorderMode = ListViewReorderMode.Animated;
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            overridingAddButtonBehavior = (_, button) =>
                TypeDropdownField.ShowMenu(button.worldBound, _elementType.GetAssignableTypes(), AddElement);
        }

        private void AddElement(Type type)
        {
            _property.serializedObject.Update();

            var index = _property.arraySize++;
            _property.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(type, true);

            _property.serializedObject.ApplyModifiedProperties();
        }
    }
}
