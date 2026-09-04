using System.Collections.Generic;
using UnityEditor;

namespace fefek5.Toys.Editor.Extensions
{
    public static class SerializedProppertyExtensions
    {
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty property)
        {
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(iterator, end))
            {
                yield return iterator.Copy();
                enterChildren = false;
            }
        }
    }
}