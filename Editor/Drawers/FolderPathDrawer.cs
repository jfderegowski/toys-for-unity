using fefek5.Toys.Editor.VisualElements;
using fefek5.Toys.Runtime.Attributes;
using fefek5.Toys.Runtime.Types;
using UnityEditor;

namespace fefek5.Toys.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(FolderPathAttribute), true)]
    [CustomPropertyDrawer(typeof(FolderPath))]
    public class FolderPathDrawer : PathDrawer
    {
        protected override PathField CreateField(string label) => new FolderPathField(label);
    }
}
