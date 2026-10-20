using UnityEditor;
using UnityEngine;

namespace fefek5.Toys.Editor
{
    /// <summary>
    /// Colors of the editor that follow its theme.
    /// </summary>
    public static class EditorColors
    {
        private static readonly Color DarkInspectorBackground = new Color32(56, 56, 56, 255);
        private static readonly Color LightInspectorBackground = new Color32(200, 200, 200, 255);

        private static readonly Color DarkFoldoutArrow = new Color32(180, 180, 180, 99);
        private static readonly Color LightFoldoutArrow = new Color32(0, 0, 0, 99);

        /// <summary>
        /// Background of the inspector and the other editor windows.
        /// </summary>
        public static Color InspectorBackground =>
            EditorGUIUtility.isProSkin ? DarkInspectorBackground : LightInspectorBackground;

        /// <summary>
        /// The arrow of a foldout, e.g. of a struct field: the color of the IN_foldout icons, as a tint for a white
        /// arrow.
        /// </summary>
        public static Color FoldoutArrow =>
            EditorGUIUtility.isProSkin ? DarkFoldoutArrow : LightFoldoutArrow;
    }
}
