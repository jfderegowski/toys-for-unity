using System;
using System.Linq;
using fefek5.Toys.Editor.Extensions;
using UnityEditor;
using UnityEngine;
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
            overridingAddButtonBehavior = (_, button) => ShowTypesMenu(button.worldBound);
        }

        private void ShowTypesMenu(Rect position)
        {
            var menu = new GenericMenu();

            foreach (var type in _elementType.GetAssignableTypes().OrderBy(type => type.GetDisplayName()))
                menu.AddItem(new GUIContent(type.GetDisplayName()), false, () => AddElement(type));

            if (menu.GetItemCount() == 0)
                menu.AddDisabledItem(new GUIContent($"No types assignable to {_elementType.GetDisplayName()}"));

            menu.DropDown(position);
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
