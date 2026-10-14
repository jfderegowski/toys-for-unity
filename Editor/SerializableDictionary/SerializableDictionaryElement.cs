using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor
{
    /// <summary>
    /// The entries of a SerializableDictionary as a reorderable list with a key and its value on each row. An entry the
    /// dictionary leaves out — a null key or one already used above — gets a warning icon after its key, with the reason
    /// in a tooltip.
    /// The add button gives the new entry an unused key where the key type allows it.
    /// </summary>
    public class SerializableDictionaryElement : ListView
    {
        private const string ENTRIES_PROP_NAME = "entries";
        private const string KEY_PROP_NAME = "key";
        private const string VALUE_PROP_NAME = "value";

        private readonly SerializedProperty _entries;

        // Rows on screen by the index they show, so marking entries does not rebind their fields mid-edit.
        private readonly Dictionary<VisualElement, int> _boundRows = new();
        private readonly Dictionary<int, string> _problems = new();

        /// <param name="property">The SerializableDictionary property.</param>
        public SerializableDictionaryElement(SerializedProperty property)
        {
            _entries = property.FindPropertyRelative(ENTRIES_PROP_NAME);

            bindingPath = _entries.propertyPath;
            headerTitle = property.displayName;
            showFoldoutHeader = true;
            showBoundCollectionSize = true;
            showAddRemoveFooter = true;
            showBorder = true;
            reorderable = true;
            reorderMode = ListViewReorderMode.Animated;
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

            makeItem = MakeRow;
            bindItem = BindRow;
            unbindItem = UnbindRow;
            overridingAddButtonBehavior = (_, _) => AddEntry();

            FindProblems();
            this.TrackPropertyValue(_entries, _ => FindProblems());
        }

        #region Rows

        // The fields live as long as the row and are only rebound. A field replaced instead stays bound to the
        // index it showed, and after a reorder writes its old value into whichever entry took that index.
        private static VisualElement MakeRow()
        {
            var row = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    paddingLeft = 4,
                    paddingTop = 1,
                    paddingBottom = 1,
                }
            };

            var keyCell = new VisualElement {
                name = "key-cell",
                style = { flexDirection = FlexDirection.Row, flexBasis = Length.Percent(40), minWidth = 0, marginRight = 6 }
            };

            // Sits at the right edge of the key column; the key field gives up the room when it shows.
            var warning = new Image {
                name = "warning",
                image = EditorGUIUtility.IconContent("console.warnicon.sml").image,
                style = {
                    width = 16, height = 16, flexShrink = 0, marginLeft = 4, alignSelf = Align.Center,
                    display = DisplayStyle.None,
                }
            };

            keyCell.Add(MakeField("key-field"));
            keyCell.Add(warning);
            row.Add(keyCell);
            row.Add(MakeField("value-field"));

            return row;
        }

        private static PropertyField MakeField(string name) => new() {
            name = name,
            style = { flexGrow = 1, flexBasis = 0, minWidth = 0 }
        };

        private void BindRow(VisualElement row, int index)
        {
            var entry = _entries.GetArrayElementAtIndex(index);
            var key = entry.FindPropertyRelative(KEY_PROP_NAME);
            var value = entry.FindPropertyRelative(VALUE_PROP_NAME);

            // Inspector labels line up at a fraction of the inspector's width, which a foldout in the value column
            // starts past, leaving its fields no room for labels. A row with a foldout stacks the value under the key.
            var stacked = HasFoldout(key) || HasFoldout(value);
            var keyCell = row.Q("key-cell");
            var valueField = row.Q<PropertyField>("value-field");

            row.style.flexDirection = stacked ? FlexDirection.Column : FlexDirection.Row;
            keyCell.style.flexBasis = stacked ? StyleKeyword.Auto : Length.Percent(40);
            keyCell.style.width = stacked && !HasFoldout(key) ? Length.Percent(40) : StyleKeyword.Auto;
            keyCell.style.marginRight = stacked ? 0 : 6;
            valueField.style.flexBasis = stacked ? StyleKeyword.Auto : 0;

            BindField(row.Q<PropertyField>("key-field"), key, "Key");
            BindField(valueField, value, "Value");

            _boundRows[row] = index;
            ShowProblem(row, index);
        }

        private void UnbindRow(VisualElement row, int index)
        {
            row.Q<PropertyField>("key-field").Unbind();
            row.Q<PropertyField>("value-field").Unbind();

            _boundRows.Remove(row);
        }

        private static void BindField(PropertyField field, SerializedProperty property, string foldoutLabel)
        {
            // Plain fields go without a label, the column says what they are; a foldout needs text to click on.
            var hasFoldout = HasFoldout(property);

            field.label = hasFoldout ? foldoutLabel : string.Empty;
            // The foldout arrow hangs left of its field, over whatever is before it.
            field.style.paddingLeft = hasFoldout ? 14 : 0;
            field.BindProperty(property);
        }

        private static bool HasFoldout(SerializedProperty property) =>
            property.propertyType is SerializedPropertyType.Generic or SerializedPropertyType.ManagedReference;

        #endregion

        #region Problems

        private void FindProblems()
        {
            _problems.Clear();

            // Boxed values compare the way the runtime dictionary does: structs by fields, objects by Equals.
            var keys = new HashSet<object>();

            for (var i = 0; i < _entries.arraySize; i++)
            {
                var keyProperty = _entries.GetArrayElementAtIndex(i).FindPropertyRelative(KEY_PROP_NAME);

                // A reference to an asset that is gone reads as null here, but the dictionary still holds it.
                if (keyProperty.propertyType == SerializedPropertyType.ObjectReference &&
                    !keyProperty.objectReferenceValue && keyProperty.objectReferenceInstanceIDValue != 0)
                {
                    _problems[i] = "The key object is missing. The dictionary still holds this entry, but nothing can find it.";
                    continue;
                }

                var key = keyProperty.boxedValue;

                if (key is null)
                    _problems[i] = "The key is empty, so the dictionary leaves this entry out.";
                else if (!keys.Add(key))
                    _problems[i] = "The key is already used above, so the dictionary leaves this entry out.";
            }

            foreach (var (row, index) in _boundRows)
                ShowProblem(row, index);
        }

        private void ShowProblem(VisualElement row, int index)
        {
            var hasProblem = _problems.TryGetValue(index, out var problem);

            row.tooltip = problem;
            row.Q("warning").style.display = hasProblem ? DisplayStyle.Flex : DisplayStyle.None;
        }

        #endregion

        #region Adding

        private void AddEntry()
        {
            var serializedObject = _entries.serializedObject;
            serializedObject.Update();

            // Unity fills a new element with a copy of the last one, so only the key needs to change. The first one
            // it zeroes, skipping field initializers, so a struct or class key or value is built the way code would.
            var index = _entries.arraySize++;
            var entry = _entries.GetArrayElementAtIndex(index);

            if (index == 0)
            {
                SetConstructedValue(entry.FindPropertyRelative(KEY_PROP_NAME));
                SetConstructedValue(entry.FindPropertyRelative(VALUE_PROP_NAME));
            }

            var otherKeys = Enumerable.Range(0, index)
                .Select(i => _entries.GetArrayElementAtIndex(i).FindPropertyRelative(KEY_PROP_NAME))
                .ToList();

            SetUnusedKey(entry.FindPropertyRelative(KEY_PROP_NAME), otherKeys);

            serializedObject.ApplyModifiedProperties();
        }

        private static void SetConstructedValue(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.Generic || property.isArray)
                return;

            try
            {
                property.boxedValue = Activator.CreateInstance(property.boxedValue.GetType(), true);
            }
            catch (MissingMethodException)
            {
                // No parameterless constructor: the zeroed value stays.
            }
        }

        /// <summary>
        /// Gives the entry the add button just made a key none of <paramref name="otherKeys"/> has. The entry is a copy
        /// of the last one, or the constructed default when it is the first. Override it for a key type this cannot
        /// pick for, such as a struct whose valid keys come from elsewhere, and draw the dictionary with the subclass.
        /// </summary>
        /// <param name="key">The key property of the new entry.</param>
        /// <param name="otherKeys">The key properties of the entries above it, empty for the first entry.</param>
        protected virtual void SetUnusedKey(SerializedProperty key, IReadOnlyList<SerializedProperty> otherKeys)
        {
            if (otherKeys.Count == 0)
                return;

            switch (key.propertyType)
            {
                case SerializedPropertyType.Integer:
                    key.longValue = otherKeys.Max(other => other.longValue) + 1;
                    break;
                case SerializedPropertyType.Float:
                    key.doubleValue = System.Math.Floor(otherKeys.Max(other => other.doubleValue)) + 1;
                    break;
                case SerializedPropertyType.String:
                    key.stringValue = ObjectNames.GetUniqueName(
                        otherKeys.Select(other => other.stringValue).ToArray(),
                        string.IsNullOrEmpty(key.stringValue) ? "Key" : key.stringValue);
                    break;
                case SerializedPropertyType.Enum:
                    var used = otherKeys.Select(other => other.enumValueIndex).ToHashSet();
                    var unused = Enumerable.Range(0, key.enumNames.Length).Where(i => !used.Contains(i)).ToList();
                    if (unused.Count > 0)
                        key.enumValueIndex = unused[0];
                    break;
                case SerializedPropertyType.Boolean:
                    key.boolValue = !key.boolValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    // An empty field asks for an object instead of repeating the last one.
                    key.objectReferenceValue = null;
                    break;
            }
        }

        #endregion
    }
}
