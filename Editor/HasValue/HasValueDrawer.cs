using fefek5.Toys.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor
{
    [CustomPropertyDrawer(typeof(HasValue<>))]
    public class HasValueDrawer : PropertyDrawer
    {
        private const string HAS_VALUE_PROP_NAME = "hasValue";
        private const string VALUE_PROP_NAME = "value";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create the root VisualElement
            var root = new VisualElement();

            // Get properties
            var hasValueProp = property.FindPropertyRelative(HAS_VALUE_PROP_NAME);
            var valueProp = property.FindPropertyRelative(VALUE_PROP_NAME);

            var hasValue = hasValueProp.boolValue;
            var isValuePropExpandable = valueProp != null &&
                                        (valueProp.hasVisibleChildren
                                         || valueProp.propertyType == SerializedPropertyType.Generic
                                         || valueProp.isArray);
            
            // Define colors
            var defaultBackgroundStyleColor = root.style.backgroundColor;
            var defaultBackgroundColor = defaultBackgroundStyleColor.value;

            // Create PropertyField for hasValue to mimic standard bool rendering
            var hasValueField = new PropertyField(hasValueProp, $"Has {property.displayName}") {
                name = "has-value-field",
                style = {
                    backgroundColor = hasValue
                        ? new StyleColor(Color.Lerp(defaultBackgroundColor, Color.black, 0.1f))
                        : defaultBackgroundStyleColor,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                }
            };
            
            // Create container for value property
            var valueContainer = new VisualElement {
                name = "value-container",
                style = {
                    paddingTop = 4,
                    paddingRight = 4,
                    paddingLeft = isValuePropExpandable ? 18 : 4,
                    paddingBottom = 4,
                    backgroundColor = new StyleColor(Color.Lerp(defaultBackgroundColor, Color.black, 0.05f)),
                    borderTopWidth = 1,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                }
            };

            // Create PropertyField for value
            var valueField = new PropertyField(valueProp, property.displayName);

            // Add elements to the hierarchy
            root.Add(hasValueField);
            valueContainer.Add(valueField);
            root.Add(valueContainer);

            // Bind the hasValue property
            hasValueField.BindProperty(hasValueProp);

            // Initial visibility
            UpdateUI(null);

            // Register callback for hasValue changes
            hasValueField.RegisterValueChangeCallback(UpdateUI);

            // Ensure the value field is properly bound
            valueField.BindProperty(valueProp);

            return root;

            
            void UpdateUI(SerializedPropertyChangeEvent evt)
            {
                var boolValue = hasValueProp.boolValue;
                
                // Update visibility of the value field based on hasValue
                valueContainer.style.display = boolValue ? DisplayStyle.Flex : DisplayStyle.None;
                
                // Update background color of the hasValue field
                hasValueField.style.backgroundColor = boolValue
                    ? new StyleColor(Color.Lerp(defaultBackgroundColor, Color.black, 0.1f))
                    : defaultBackgroundStyleColor;
            }
        }
    }
}
