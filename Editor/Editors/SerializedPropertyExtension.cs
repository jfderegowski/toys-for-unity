using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace fefek5.Toys.Editor.Editors
{
    /// <summary>
    /// Reflection helpers that resolve the live object behind a <see cref="SerializedProperty"/>.
    /// <para>
    /// A <see cref="SerializedProperty"/> only exposes serialized values, and
    /// <see cref="SerializedObject.targetObject"/> is the <see cref="UnityEngine.Object"/> that owns the field,
    /// never the field itself. Anything serialized inline, a plain <see cref="SerializableAttribute"/> class or
    /// struct, has to be walked out of the property path by hand.
    /// </para>
    /// </summary>
    public static class SerializedPropertyExtension
    {
        private const BindingFlags FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        #region Target

        /// <summary>
        /// Resolve the object a property points at.
        /// </summary>
        /// <param name="property">The property to resolve</param>
        /// <typeparam name="T">Expected type of the value, a class, an interface it implements, or a struct</typeparam>
        /// <returns>The instance behind the property, or the default of <typeparamref name="T"/> when it can not
        /// be resolved or is of another type</returns>
        /// <example>
        /// <code>
        /// var saveVar = property.GetTarget&lt;SaveVar&gt;();
        /// </code>
        /// </example>
        public static T GetTarget<T>(this SerializedProperty property) =>
            property.TryGetTarget<T>(out var target) ? target : default;

        /// <summary>
        /// Resolve the object a property points at without throwing when the path leads nowhere.
        /// </summary>
        /// <param name="property">The property to resolve</param>
        /// <param name="target">The resolved instance, or the default of <typeparamref name="T"/> on failure</param>
        /// <typeparam name="T">Expected type of the value</typeparam>
        /// <returns>True when the path resolved to a <typeparamref name="T"/></returns>
        public static bool TryGetTarget<T>(this SerializedProperty property, out T target)
        {
            target = default;

            if (property == null) return false;

            var serializedObject = property.serializedObject;
            if (serializedObject == null) return false;

            if (GetValueAtPath(serializedObject.targetObject, property.propertyPath) is not T value) return false;

            target = value;
            return true;
        }

        /// <summary>
        /// Resolve the object a property points at on every edited object. Use it instead of
        /// <see cref="GetTarget{T}"/> when the drawer has to survive multi object editing.
        /// </summary>
        /// <param name="property">The property to resolve</param>
        /// <typeparam name="T">Expected type of the value</typeparam>
        /// <returns>One instance per target object, skipping the ones that can not be resolved</returns>
        public static IEnumerable<T> GetTargets<T>(this SerializedProperty property)
        {
            if (property == null) yield break;

            var serializedObject = property.serializedObject;
            if (serializedObject == null) yield break;

            var propertyPath = property.propertyPath;

            foreach (var targetObject in serializedObject.targetObjects)
            {
                if (GetValueAtPath(targetObject, propertyPath) is not T value) continue;

                yield return value;
            }
        }

        /// <summary>
        /// Resolve the object that owns a property, one step up the path. That is the nested class or the array
        /// or list element the property sits in, and the target object itself for a field declared straight on
        /// it or on one of its collections.
        /// </summary>
        /// <param name="property">The property whose owner is wanted</param>
        /// <typeparam name="T">Expected type of the owner</typeparam>
        /// <returns>The owner of the property, or the default of <typeparamref name="T"/> when it can not be
        /// resolved or is of another type</returns>
        public static T GetTargetParent<T>(this SerializedProperty property)
        {
            if (property == null) return default;

            var serializedObject = property.serializedObject;
            if (serializedObject == null) return default;

            // ".Array.data[i]" is one step of the path, not two, so it is collapsed before the last step is cut off.
            var propertyPath = property.propertyPath.Replace(".Array.data[", "[");
            var lastSeparator = propertyPath.LastIndexOf('.');

            var parent = lastSeparator < 0
                ? serializedObject.targetObject
                : GetValueAtPath(serializedObject.targetObject, propertyPath[..lastSeparator]);

            return parent is T value ? value : default;
        }

        #endregion

        #region Reflection

        /// <summary>
        /// Walk a property path down from an object, following nested fields, arrays and lists.
        /// </summary>
        /// <param name="root">Object the path starts from, normally the serialized target object</param>
        /// <param name="propertyPath">Path as reported by <see cref="SerializedProperty.propertyPath"/>, with or
        /// without the ".Array.data[" parts already collapsed</param>
        /// <returns>The instance behind the path, or null when a segment can not be resolved</returns>
        private static object GetValueAtPath(object root, string propertyPath)
        {
            if (root == null || string.IsNullOrEmpty(propertyPath)) return root;

            var current = root;
            var elements = propertyPath.Replace(".Array.data[", "[").Split('.');

            foreach (var element in elements)
            {
                if (current == null) return null;

                var indexStart = element.IndexOf('[');

                if (indexStart < 0)
                {
                    current = GetFieldValue(current, element);
                    continue;
                }

                var indexEnd = element.IndexOf(']', indexStart);
                if (indexEnd < 0) return null;
                if (!int.TryParse(element[(indexStart + 1)..indexEnd], out var index)) return null;

                current = GetElementAt(GetFieldValue(current, element[..indexStart]), index);
            }

            return current;
        }

        /// <summary>
        /// Read a field by name, walking up the inheritance chain because private fields of a base class are not
        /// visible on the derived type.
        /// </summary>
        /// <param name="instance">Object to read the field from</param>
        /// <param name="fieldName">Name of the field, backing fields of auto properties included</param>
        /// <returns>The value of the field, or null when no such field exists</returns>
        private static object GetFieldValue(object instance, string fieldName)
        {
            var type = instance.GetType();

            while (type != null)
            {
                var field = type.GetField(fieldName, FIELD_FLAGS);
                if (field != null) return field.GetValue(instance);

                type = type.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Read one element of an array or a list.
        /// </summary>
        /// <param name="collection">The collection to read from</param>
        /// <param name="index">Index of the element</param>
        /// <returns>The element, or null when the collection is not indexable or the index is out of range</returns>
        private static object GetElementAt(object collection, int index)
        {
            if (collection is not IList list) return null;

            return index >= 0 && index < list.Count ? list[index] : null;
        }

        #endregion
    }
}
