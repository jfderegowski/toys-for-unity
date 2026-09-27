using fefek5.Toys.Editor.VisualElements;
using fefek5.Toys.Runtime.Attributes;
using fefek5.Toys.Runtime.Extensions;
using fefek5.Toys.Runtime.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.Drawers
{
    /// <summary>
    /// Base of the drawers for a path: a string with a <see cref="PathAttribute"/> as one row, or a FolderPath or
    /// FilePath, with or without one, as a foldout. Each subclass creates its own <see cref="PathField"/>.
    /// </summary>
    public abstract class PathDrawer : PropertyDrawer
    {
        // The fields of FolderPath and FilePath.
        private const string PATH_PROPERTY = "_path";
        private const string ROOT_PROPERTY = "_root";
        private const string CUSTOM_ROOT_PROPERTY = "_customRoot";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var pathAttribute = attribute as PathAttribute;

            if (property.propertyType == SerializedPropertyType.String)
            {
                var field = CreateField(property.displayName);

                field.Relative = pathAttribute?.Relative ?? true;
                field.Root = pathAttribute?.Root ?? PathRoot.Project;
                field.BindProperty(property);

                return field;
            }

            var path = property.FindPropertyRelative(PATH_PROPERTY);
            var root = property.FindPropertyRelative(ROOT_PROPERTY);
            var customRoot = property.FindPropertyRelative(CUSTOM_ROOT_PROPERTY);

            if (path == null || root == null || customRoot == null)
                return new HelpBox($"{GetType().Name} works only on a string, FolderPath or FilePath field.",
                    HelpBoxMessageType.Error);

            var pathField = CreateField("Path");

            pathField.Relative = pathAttribute?.Relative ?? true;

            return CreateFoldout(property, pathField, path, root, customRoot);
        }

        protected abstract PathField CreateField(string label);

        /// <summary>
        /// A foldout with the full path greyed out in its header, and inside it the root, the folder of
        /// <see cref="PathRoot.Custom"/> while that is picked, and the path.
        /// </summary>
        private static VisualElement CreateFoldout(SerializedProperty property, PathField pathField,
            SerializedProperty path, SerializedProperty root, SerializedProperty customRoot)
        {
            var fullPath = new Label {
                style = {
                    flexGrow = 1,
                    flexShrink = 1,
                    marginLeft = 8,
                    color = Color.gray,
                    unityTextAlign = TextAnchor.MiddleRight,
                    overflow = Overflow.Hidden,
                    textOverflow = TextOverflow.Ellipsis,
                    whiteSpace = WhiteSpace.NoWrap,
                }
            };

            var foldout = new FoldoutElement(property.displayName, fullPath);

            // Bound to the property itself, the foldout remembers whether it is open.
            foldout.BindProperty(property);

            var rootField = new EnumField("Root", PathRoot.Project) { tooltip = "The folder a relative path starts from" };
            rootField.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            rootField.BindProperty(root);

            var customRootField = new FolderPathField("Root Folder");
            customRootField.BindProperty(customRoot);

            pathField.BindProperty(path);

            foldout.Add(rootField);
            foldout.Add(customRootField);
            foldout.Add(pathField);

            Sync();
            foldout.TrackPropertyValue(path, _ => Sync());
            foldout.TrackPropertyValue(root, _ => Sync());
            foldout.TrackPropertyValue(customRoot, _ => Sync());

            return foldout;

            void Sync()
            {
                var value = (PathRoot)root.enumValueIndex;

                pathField.Root = value;
                pathField.CustomRoot = customRoot.stringValue;
                customRootField.style.display = value == PathRoot.Custom ? DisplayStyle.Flex : DisplayStyle.None;

                // Read from the property: the field may not have its bound value yet.
                fullPath.text = string.IsNullOrEmpty(path.stringValue)
                    ? "None"
                    : value.ToAbsolute(path.stringValue, customRoot.stringValue);
                fullPath.tooltip = fullPath.text;
            }
        }
    }
}
