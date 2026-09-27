using fefek5.Toys.Editor.VisualElements;
using fefek5.Toys.Runtime.Types;
using UnityEditor;
using FilePathAttribute = fefek5.Toys.Runtime.Attributes.FilePathAttribute;

namespace fefek5.Toys.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(FilePathAttribute), true)]
    [CustomPropertyDrawer(typeof(FilePath))]
    public class FilePathDrawer : PathDrawer
    {
        // A FilePath without the attribute accepts any file.
        protected override PathField CreateField(string label) =>
            new FilePathField(label, (attribute as FilePathAttribute)?.Extensions);
    }
}
