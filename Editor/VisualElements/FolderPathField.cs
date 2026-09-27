using System.IO;
using UnityEditor;

namespace fefek5.Toys.Editor.VisualElements
{
    /// <summary>A <see cref="PathField"/> for a folder.</summary>
    public class FolderPathField : PathField
    {
        public FolderPathField(string label = null) : base(label) { }

        protected override string Kind => "folder";

        protected override string Browse(string absolutePath) =>
            EditorUtility.OpenFolderPanel(PanelTitle, GetBrowseFolder(absolutePath), string.Empty);

        protected override bool Exists(string absolutePath) => Directory.Exists(absolutePath);
    }
}
