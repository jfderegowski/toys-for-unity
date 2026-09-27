using System;
using System.Collections.Generic;
using System.Linq;
using fefek5.Toys.Editor.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace fefek5.Toys.Editor.VisualElements
{
    /// <summary>
    /// A list of ScriptableObjects that are sub-assets of the asset holding the list, each a foldout with its inspector
    /// inside. The add button creates a sub-asset of the type picked from every concrete type assignable to
    /// <c>elementType</c>; the remove button takes the selected one out after asking and destroys it when it is a sub-asset. The type of an element never
    /// changes afterwards, because replacing the object would break every reference to it.
    /// </summary>
    public class SubAssetListElement : ListView
    {
        private const string ITEM_CLASS = "sub-asset-list__item";

        /// <summary>
        /// Text shown on the right of each header, e.g. a runtime value. Refreshed every 200 ms while set.
        /// </summary>
        public Func<Object, string> ItemInfo { get; set; }

        /// <summary>The elements of the list, as of the last refresh.</summary>
        public IReadOnlyList<Object> Items => _items;

        /// <summary>
        /// Raised after the list read the property again: after an add, a remove, a move, an undo or any change to the
        /// project, such as a rename.
        /// </summary>
        public event Action Reloaded;

        private readonly SerializedObject _serializedObject;
        private readonly string _propertyPath;
        private readonly Type _elementType;

        private readonly List<Object> _items = new();
        private readonly HashSet<Object> _expanded = new();
        private readonly Dictionary<Object, InspectorElement> _inspectors = new();

        /// <param name="property">A list or array of object references on an asset</param>
        /// <param name="elementType">Base type of the elements, a ScriptableObject</param>
        public SubAssetListElement(SerializedProperty property, Type elementType)
        {
            _serializedObject = property.serializedObject;
            _propertyPath = property.propertyPath;
            _elementType = elementType;

            itemsSource = _items;
            makeItem = MakeItem;
            bindItem = BindItem;

            headerTitle = property.displayName;
            showFoldoutHeader = true;
            showAddRemoveFooter = true;
            showBorder = true;
            reorderable = true;
            reorderMode = ListViewReorderMode.Animated;
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            selectionType = SelectionType.Single;
            overridingAddButtonBehavior = (_, button) => TypeDropdownField.ShowMenu(button.worldBound, GetTypes(), Add);
            onRemove = _ => Remove(selectedIndex);
            itemIndexChanged += Move;

            // Values shown by ItemInfo live outside the serialized data, so nothing else tells the headers they moved.
            schedule.Execute(UpdateInfo).Every(200);

            // A rename in the inspector of an element re-imports the asset, which is what tells the list.
            RegisterCallback<AttachToPanelEvent>(_ => {
                Undo.undoRedoPerformed += Reload;
                EditorApplication.projectChanged += Reload;
            });
            RegisterCallback<DetachFromPanelEvent>(_ => {
                Undo.undoRedoPerformed -= Reload;
                EditorApplication.projectChanged -= Reload;
            });

            Reload();
        }

        private SerializedProperty Property => _serializedObject.FindProperty(_propertyPath);

        private Object Owner => _serializedObject.targetObject;

        private IEnumerable<Type> GetTypes() =>
            TypeCache.GetTypesDerivedFrom(_elementType)
                .Prepend(_elementType)
                .Where(type => !type.IsAbstract && !type.ContainsGenericParameters &&
                               typeof(ScriptableObject).IsAssignableFrom(type));

        /// <summary>Reads the property again and redraws the list.</summary>
        public void Reload()
        {
            if (!Owner)
                return;

            _serializedObject.Update();

            var property = Property;

            _items.Clear();

            for (var i = 0; i < property.arraySize; i++)
                _items.Add(property.GetArrayElementAtIndex(i).objectReferenceValue);

            foreach (var item in _inspectors.Keys.Where(item => !_items.Contains(item)).ToList())
                _inspectors.Remove(item);

            _expanded.RemoveWhere(item => !_items.Contains(item));

            RefreshItems();

            Reloaded?.Invoke();
        }

        #region Items

        private VisualElement MakeItem()
        {
            var header = new VisualElement {
                style = { flexDirection = FlexDirection.Row, flexGrow = 1, justifyContent = Justify.FlexEnd }
            };

            header.Add(new Label { name = "type", style = { color = Color.gray, marginLeft = 8 } });
            header.Add(new Label {
                name = "info", style = { minWidth = 60, unityTextAlign = TextAnchor.MiddleRight, marginLeft = 8 }
            });

            var foldout = new FoldoutElement(string.Empty, header);
            foldout.AddToClassList(ITEM_CLASS);

            foldout.RegisterValueChangedCallback(evt => {
                // Foldouts inside the inspector of the element send their changes up through this one.
                if (evt.target != foldout || foldout.userData is not Object item || !item)
                    return;

                if (evt.newValue)
                    _expanded.Add(item);
                else
                    _expanded.Remove(item);

                FillContent(foldout, item);
            });

            return foldout;
        }

        private void BindItem(VisualElement element, int index)
        {
            var foldout = (FoldoutElement)element;
            var item = _items[index];

            foldout.userData = item;
            foldout.text = item ? item.name : "(missing)";
            foldout.Q<Label>("type").text = item ? item.GetType().GetDisplayName() : string.Empty;
            foldout.Q<Label>("info").text = GetInfo(item);
            foldout.SetValueWithoutNotify(item && _expanded.Contains(item));

            FillContent(foldout, item);
        }

        private void FillContent(FoldoutElement foldout, Object item)
        {
            foldout.Clear();

            if (!item || !foldout.value)
                return;

            // Kept per element, so scrolling and refreshing the list does not build the inspector again.
            if (!_inspectors.TryGetValue(item, out var inspector))
                _inspectors[item] = inspector = new InspectorElement(item);

            foldout.Add(inspector);
        }

        private void UpdateInfo()
        {
            if (ItemInfo == null)
                return;

            this.Query<FoldoutElement>(className: ITEM_CLASS)
                .ForEach(foldout => foldout.Q<Label>("info").text = GetInfo(foldout.userData as Object));
        }

        private string GetInfo(Object item) => item && ItemInfo != null ? ItemInfo(item) : string.Empty;

        #endregion

        #region Assets

        private void Add(Type type)
        {
            var item = ScriptableObject.CreateInstance(type);

            item.name = $"New {type.GetDisplayName()}";

            AssetDatabase.AddObjectToAsset(item, Owner);
            Undo.RegisterCreatedObjectUndo(item, $"Add {type.GetDisplayName()}");

            _serializedObject.Update();

            var property = Property;

            property.arraySize++;
            property.GetArrayElementAtIndex(property.arraySize - 1).objectReferenceValue = item;

            _serializedObject.ApplyModifiedProperties();
            Save(Owner);

            _expanded.Add(item);
            Reload();

            var index = _items.IndexOf(item);

            SetSelection(index);
            ScrollToItem(index);
        }

        private void Remove(int index)
        {
            if (index < 0 || index >= _items.Count)
                return;

            var item = _items[index];
            var itemName = item ? item.name : "(missing)";

            if (!EditorUtility.DisplayDialog($"Remove {_elementType.GetDisplayName()}",
                    $"Remove '{itemName}' from '{Owner.name}'?\n\nFields that reference it lose it. Undo brings it back.",
                    "Remove", "Cancel"))
                return;

            Undo.SetCurrentGroupName($"Remove {itemName}");

            var group = Undo.GetCurrentGroup();

            _serializedObject.Update();

            var property = Property;

            // Cleared first: deleting an element that still references an object has only cleared it in some versions
            // of Unity.
            property.GetArrayElementAtIndex(index).objectReferenceValue = null;
            property.DeleteArrayElementAtIndex(index);

            _serializedObject.ApplyModifiedProperties();

            // Only a sub-asset of the owner is destroyed; an asset of its own that was dragged in stays.
            if (item && AssetDatabase.IsSubAsset(item) && AssetDatabase.GetAssetPath(item) == AssetDatabase.GetAssetPath(Owner))
                Undo.DestroyObjectImmediate(item);

            Undo.CollapseUndoOperations(group);
            Save(Owner);

            ClearSelection();
            Reload();
        }

        /// <summary>The list has already moved the element in its own items; this moves it in the property as well.</summary>
        private void Move(int from, int to)
        {
            _serializedObject.Update();
            Property.MoveArrayElement(from, to);
            _serializedObject.ApplyModifiedProperties();

            Save(Owner);
        }

        /// <summary>
        /// Writes the asset and imports it again. The Project window lists sub-assets as of the last import, so without
        /// it a change shows only once the project is saved.
        /// </summary>
        private static void Save(Object asset)
        {
            AssetDatabase.SaveAssetIfDirty(asset);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
        }

        #endregion
    }
}
