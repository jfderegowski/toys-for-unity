using System.IO;
using System.Linq;
using fefek5.Toys.Runtime.Extensions;
using fefek5.Toys.Runtime.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    /// <summary>
    /// A text field for a path on disk with a browse button, which also takes a path dropped from the Project window
    /// or the file explorer. Subclasses decide what the path points at: how it is picked, when it exists and what can
    /// be dropped. A path that does not exist is shown in yellow.
    /// </summary>
    public abstract class PathField : TextField
    {
        private static readonly Color MISSING_COLOR = new(1f, 0.76f, 0.03f);

        /// <summary>
        /// Store the path from <see cref="Root"/>, e.g. "Assets/Textures". A path outside it stays absolute.
        /// </summary>
        public bool Relative { get; set; } = true;

        /// <summary>The folder a relative path starts from, and the panel opens in when there is no path yet.</summary>
        public PathRoot Root
        {
            get => _root;
            set
            {
                _root = value;
                SetValueWithoutNotify(this.value);
            }
        }

        /// <summary>The folder of <see cref="PathRoot.Custom"/>, absolute or from the project folder.</summary>
        public string CustomRoot
        {
            get => _customRoot;
            set
            {
                _customRoot = value;
                SetValueWithoutNotify(this.value);
            }
        }

        /// <summary>The path as absolute, whichever way it is stored.</summary>
        public string AbsolutePath => ToAbsolute(value);

        private readonly VisualElement _input;

        private PathRoot _root;
        private string _customRoot;

        protected PathField(string label) : base(label)
        {
            AddToClassList(alignedFieldUssClassName);

            _input = this.Q(className: inputUssClassName);

            var browse = new Button(OnBrowse) {
                tooltip = "Browse",
                style = { width = 22, marginLeft = 2, marginRight = 0, paddingLeft = 0, paddingRight = 0 }
            };

            // An Image of its own size: iconImage draws the icon at its texture size, which is far taller than a row.
            if (EditorGUIUtility.IconContent("Folder Icon").image is Texture2D icon)
                browse.Add(new Image {
                    image = icon,
                    pickingMode = PickingMode.Ignore,
                    style = { width = 14, height = 14, alignSelf = Align.Center }
                });
            else
                browse.text = "...";

            hierarchy.Add(browse);

            // Trickle down, so the drop reaches this field before the text input takes it as text.
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated, TrickleDown.TrickleDown);
            RegisterCallback<DragPerformEvent>(OnDragPerform, TrickleDown.TrickleDown);
        }

        /// <summary>What the path points at, for the panel title and the tooltip of a missing path, e.g. "folder".</summary>
        protected abstract string Kind { get; }

        /// <summary>Title of the panel: the label, or what the path points at.</summary>
        protected string PanelTitle => string.IsNullOrEmpty(label) ? $"Select {Kind}" : label;

        /// <summary>Opens the panel for picking the path.</summary>
        /// <param name="absolutePath">Current path, absolute, or empty</param>
        /// <returns>The picked path, absolute, or empty when cancelled</returns>
        protected abstract string Browse(string absolutePath);

        /// <param name="absolutePath">Never empty</param>
        protected abstract bool Exists(string absolutePath);

        /// <summary>Whether a dragged path can be dropped on the field. By default one that exists.</summary>
        protected virtual bool CanDrop(string absolutePath) => Exists(absolutePath);

        // Every change goes through here, typed, picked or from the bound property, and so does a change of the root.
        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);

            // TextField's constructor sets the value too, before this one and its subclasses have set their fields.
            if (_input == null)
                return;

            var missing = !string.IsNullOrEmpty(newValue) && !Exists(ToAbsolute(newValue));

            _input.style.color = missing ? MISSING_COLOR : StyleKeyword.Null;
            tooltip = missing ? $"No {Kind} at {ToAbsolute(newValue)}" : null;
        }

        private void OnBrowse()
        {
            var picked = Browse(AbsolutePath);

            if (!string.IsNullOrEmpty(picked))
                value = ToStored(picked);
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (GetDropped() == null)
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            evt.StopPropagation();
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            var dropped = GetDropped();

            if (dropped == null)
                return;

            DragAndDrop.AcceptDrag();
            value = ToStored(dropped);
            evt.StopPropagation();
        }

        // The Project window drags paths from the project folder, whatever the root.
        private string GetDropped() =>
            DragAndDrop.paths.Select(path => PathRoot.Project.ToAbsolute(path)).FirstOrDefault(CanDrop);

        /// <summary>
        /// The folder the panel opens in: the path itself when it is a folder, else the nearest folder above it that
        /// exists, else the root folder.
        /// </summary>
        protected string GetBrowseFolder(string absolutePath)
        {
            for (var folder = absolutePath; !string.IsNullOrEmpty(folder); folder = Path.GetDirectoryName(folder))
                if (Directory.Exists(folder))
                    return folder;

            return Root.GetFolder(CustomRoot);
        }

        private string ToAbsolute(string path) => Root.ToAbsolute(path, CustomRoot);

        /// <summary>An absolute path as it is stored: from the root when <see cref="Relative"/> and inside it.</summary>
        private string ToStored(string absolutePath) =>
            Relative ? Root.ToRelative(absolutePath, CustomRoot) : PathRootExtensions.Normalize(absolutePath);
    }
}
