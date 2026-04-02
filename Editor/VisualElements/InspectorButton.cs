using System;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace fefek5.Toys.Editor.VisualElements
{
    public class InspectorButton : Button
    {
        private readonly object _target;
        private readonly string _methodName;
        private readonly MethodInfo _method;
        private object[] _parameters;
        
        private const BindingFlags METHOD_FLAGS =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public InspectorButton(Object target, string methodName, string label = null)
        {
            text = label ?? ObjectNames.NicifyVariableName(methodName);

            var method = target.GetType().GetMethod(methodName, METHOD_FLAGS);

            clicked += () =>
            {
                if (method != null && method.GetParameters().Length == 0)
                {
                    method.Invoke(target, null);
                    EditorUtility.SetDirty(target);
                }
            };
        }

        public InspectorButton(object target, string methodName, Background iconImage)
            : this(target, methodName, iconImage, CreateClickEvent)
        {
            
        }

        public InspectorButton(object target, string methodName, Background iconImage, Action clickEvent) 
            : base(iconImage, clickEvent)
        {
            _target = target;
            _methodName = methodName;
            _method = target.GetType().GetMethod(methodName, METHOD_FLAGS);
        }

        private void CallMethod()
        {
            if (_method == null) return;
            
            _method.Invoke(_target, null);
        }

        private object[] GetParameters()
        {
            // TODO: Create some input visual elements for paramethers from method (use only types that unity can serialize)
            return null;
        }
    }
}