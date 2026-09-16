using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private const BindingFlags CONSTRUCTOR_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

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

            foreach (var type in GetAssignableTypes(_elementType).OrderBy(GetMenuName))
                menu.AddItem(new GUIContent(GetMenuName(type)), false, () => AddElement(type));

            if (menu.GetItemCount() == 0)
                menu.AddDisabledItem(new GUIContent($"No types assignable to {GetMenuName(_elementType)}"));

            menu.DropDown(position);
        }

        private void AddElement(Type type)
        {
            _property.serializedObject.Update();

            var index = _property.arraySize++;
            _property.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(type, true);

            _property.serializedObject.ApplyModifiedProperties();
        }

        private static IEnumerable<Type> GetAssignableTypes(Type elementType)
        {
            // Searching by the generic definition also finds generic types like Foo<T> : StatTransport<T>.
            var searchType = elementType.IsGenericType ? elementType.GetGenericTypeDefinition() : elementType;

            return TypeCache.GetTypesDerivedFrom(searchType)
                .Prepend(elementType)
                .Select(type => type.IsGenericTypeDefinition ? CloseGeneric(type, elementType) : type)
                .Where(type => type is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false } &&
                               elementType.IsAssignableFrom(type) &&
                               !typeof(UnityEngine.Object).IsAssignableFrom(type) &&
                               type.GetConstructor(CONSTRUCTOR_FLAGS, null, Type.EmptyTypes, null) != null)
                .Distinct();
        }

        private static Type CloseGeneric(Type definition, Type elementType)
        {
            var arguments = elementType.GetGenericArguments();
            if (definition.GetGenericArguments().Length != arguments.Length) return null;

            try
            {
                return definition.MakeGenericType(arguments);
            }
            catch (ArgumentException)
            {
                // Generic constraints not satisfied.
                return null;
            }
        }

        private static string GetMenuName(Type type)
        {
            var name = type.Name;
            var arityIndex = name.IndexOf('`');

            return ObjectNames.NicifyVariableName(arityIndex < 0 ? name : name[..arityIndex]);
        }
    }
}
