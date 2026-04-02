using System.Reflection;
using fefek5.Toys.Editor.VisualElements;
using fefek5.Toys.Runtime.Attributes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.Editors
{
    [CustomEditor(typeof(MonoBehaviour), true), CanEditMultipleObjects]
    public class MonoBehaviourEditro : UnityEditor.Editor
    {
        private const BindingFlags METHOD_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                  BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var type = target.GetType();
            var methods = type.GetMethods(METHOD_FLAGS);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<InspectorButtonAttribute>();
                if (attr == null) continue;
                if (method.GetParameters().Length != 0) continue;

                var label = attr.Label ?? ObjectNames.NicifyVariableName(method.Name);

                root.Add(new InspectorButton(target, method.Name, label));
            }

            return root;
        }
    }
}