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

        /// <summary>
        /// Background of the inspector and the other editor windows.
        /// </summary>
        public static Color InspectorBackground =>
            EditorGUIUtility.isProSkin ? DarkInspectorBackground : LightInspectorBackground;
    }
}
