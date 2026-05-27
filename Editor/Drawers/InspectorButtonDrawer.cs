using System;
using System.Reflection;
using fefek5.Toys.Editor.VisualElements;
using fefek5.Toys.Runtime.Types;
using UnityEditor;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(InspectorButton))]
    public class InspectorButtonDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var methodName = property.FindPropertyRelative("_methodName").stringValue;
            var buttonLabel = property.FindPropertyRelative("_buttonLabel").stringValue;
            var target = GetTargetObject(property);

            if (string.IsNullOrEmpty(buttonLabel))
                buttonLabel = ObjectNames.NicifyVariableName(methodName);

            var isnpectorButton = new InspectorButtonElement(target, methodName, buttonLabel);

            return isnpectorButton;
        }
        
        private static object GetTargetObject(SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            var path = property.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');

            // Walk all segments EXCEPT the last one (which is the InspectorButton field itself)
            for (var i = 0; i < elements.Length - 1; i++)
            {
                var element = elements[i];
                
                if (element.Contains("["))
                {
                    var fieldName = element[..element.IndexOf("[", StringComparison.Ordinal)];
                    var index = int.Parse(element[element.IndexOf("[", StringComparison.Ordinal)..]
                        .Replace("[", "").Replace("]", ""));
                    
                    obj = GetFieldValue(obj, fieldName);
                    obj = ((System.Collections.IList)obj)[index];
                }
                else obj = GetFieldValue(obj, element);
            }

            return obj;
        }

        private static object GetFieldValue(object source, string name)
        {
            var type = source.GetType();
            
            while (type != null)
            {
                var field = type.GetField(
                    name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (field != null) 
                    return field.GetValue(source);
                
                type = type.BaseType;
            }
            
            return null;
        }

    }
}