using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using fefek5.Toys.Runtime.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    public class InspectorButtonElement : VisualElement
    {
        private object[] _parameterValues;
        private Type[] _parameterTypes;

        private readonly object _target;
        private readonly SerializedProperty _property;

        private string _methodName;
        private string _buttonLabel;
        private MethodInfo _method;

        private const BindingFlags METHOD_FLAGS =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private const BindingFlags FIELD_FLAGS =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public InspectorButtonElement(object target, string methodName)
            : this(target, methodName, ObjectNames.NicifyVariableName(methodName)) { }

        public InspectorButtonElement(object target, string methodName, string buttonLabel)
            : this(target, methodName, buttonLabel, null) { }

        public InspectorButtonElement(object target, string methodName, string buttonLabel, SerializedProperty property)
        {
            _target = target;
            _property = property;
            Build(methodName, buttonLabel);
        }

        private void Build(string methodName, string buttonLabel)
        {
            Clear();
            _parameterValues = null;
            _parameterTypes = null;
            style.flexDirection = FlexDirection.Column;

            _methodName = methodName;
            _buttonLabel = buttonLabel;
            _method = !methodName.IsBlank()
                ? _target?.GetType().GetMethod(methodName, METHOD_FLAGS)
                : null;

            var button = new Button(CallMethod) {
                text = buttonLabel
            };

            if (_method == null)
            {
                BuildMethodMissingUI(button, methodName);
                return;
            }

            Add(button);
            DrawParameters();
        }

        private void BuildMethodMissingUI(Button button, string methodName)
        {
            var row = new VisualElement {
                style = { flexDirection = FlexDirection.Row }
            };

            button.style.flexGrow = 1;
            button.SetEnabled(false);
            button.tooltip = $"Method '{methodName}' not found on {_target?.GetType().Name ?? "null target"}";
            row.Add(button);

            var refreshIcon = EditorGUIUtility.IconContent("Refresh").image
                              ?? EditorGUIUtility.IconContent("d_Refresh").image;

            var refreshButton = new Button(ResetToScriptDefault) {
                tooltip = "Reset to script default (re-reads field initializer)",
                style = {
                    width = 20,
                    height = 20,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    marginLeft = 2,
                }
            };

            if (refreshIcon != null)
                refreshButton.iconImage = Background.FromTexture2D((Texture2D)refreshIcon);
            else refreshButton.text = "↻";

            row.Add(refreshButton);
            Add(row);
        }

        private void CallMethod()
        {
            if (_method == null) return;

            _method.Invoke(_target, GetParameters());

            if (_target is UnityEngine.Object targetObject)
                EditorUtility.SetDirty(targetObject);
        }

        private object[] GetParameters() => _parameterValues ?? Array.Empty<object>();

        private void ResetToScriptDefault()
        {
            if (_property == null)
            {
                Debug.LogWarning("InspectorButtonElement: no SerializedProperty — cannot reset.");
                return;
            }

            var defaults = TryGetScriptDefaults(_property);

            if (defaults == null)
            {
                Debug.LogWarning(
                    $"InspectorButtonElement: failed to read script default for '{_property.propertyPath}'. " +
                    "Make sure field has an initializer like `new InspectorButton(nameof(Method))`.");
                return;
            }

            var serializedObject = _property.serializedObject;
            serializedObject.Update();

            var methodNameProp = _property.FindPropertyRelative("_methodName");
            var buttonLabelProp = _property.FindPropertyRelative("_buttonLabel");

            if (methodNameProp != null) methodNameProp.stringValue = defaults.Value.methodName ?? string.Empty;
            if (buttonLabelProp != null) buttonLabelProp.stringValue = defaults.Value.buttonLabel ?? string.Empty;

            serializedObject.ApplyModifiedProperties();

            if (serializedObject.targetObject != null)
                EditorUtility.SetDirty(serializedObject.targetObject);

            var nextLabel = !string.IsNullOrEmpty(defaults.Value.buttonLabel)
                ? defaults.Value.buttonLabel
                : ObjectNames.NicifyVariableName(defaults.Value.methodName ?? string.Empty);

            Build(defaults.Value.methodName, nextLabel);
        }

        private static (string methodName, string buttonLabel)? TryGetScriptDefaults(SerializedProperty property)
        {
            var ownerType = GetOwnerType(property);
            var fieldName = GetLastFieldName(property);

            if (ownerType == null || string.IsNullOrEmpty(fieldName))
                return null;

            object ownerInstance = null;
            GameObject tempGameObject = null;

            try
            {
                if (typeof(ScriptableObject).IsAssignableFrom(ownerType))
                {
                    ownerInstance = ScriptableObject.CreateInstance(ownerType);
                }
                else if (typeof(Component).IsAssignableFrom(ownerType))
                {
                    tempGameObject = EditorUtility.CreateGameObjectWithHideFlags(
                        "__InspectorButtonDefaults__", HideFlags.HideAndDontSave);
                    ownerInstance = tempGameObject.AddComponent(ownerType);
                }
                else
                {
                    ownerInstance = Activator.CreateInstance(ownerType);
                }

                if (ownerInstance == null) return null;

                var field = GetFieldOnHierarchy(ownerType, fieldName);
                if (field == null) return null;

                var defaultButton = field.GetValue(ownerInstance);
                if (defaultButton == null) return null;

                var bType = field.FieldType;
                var methodNameField = bType.GetField("_methodName", FIELD_FLAGS);
                var buttonLabelField = bType.GetField("_buttonLabel", FIELD_FLAGS);

                return (
                    methodNameField?.GetValue(defaultButton) as string,
                    buttonLabelField?.GetValue(defaultButton) as string
                );
            }
            catch (Exception e)
            {
                Debug.LogWarning($"InspectorButtonElement: defaults read failed — {e.Message}");
                return null;
            }
            finally
            {
                if (tempGameObject != null)
                    UnityEngine.Object.DestroyImmediate(tempGameObject);
                else if (ownerInstance is UnityEngine.Object uo)
                    UnityEngine.Object.DestroyImmediate(uo);
            }
        }

        private static Type GetOwnerType(SerializedProperty property)
        {
            var path = property.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            var type = property.serializedObject.targetObject.GetType();

            // Walk all segments EXCEPT the last (which is the InspectorButton field itself)
            for (var i = 0; i < elements.Length - 1; i++)
            {
                var element = elements[i];
                var isIndexed = element.Contains("[");
                var fieldName = isIndexed
                    ? element[..element.IndexOf("[", StringComparison.Ordinal)]
                    : element;

                var field = GetFieldOnHierarchy(type, fieldName);
                if (field == null) return null;

                type = field.FieldType;

                if (isIndexed)
                {
                    if (type.IsArray) type = type.GetElementType();
                    else if (type.IsGenericType) type = type.GetGenericArguments()[0];
                }
            }

            return type;
        }

        private static string GetLastFieldName(SerializedProperty property)
        {
            var path = property.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            var last = elements[^1];
            if (last.Contains("["))
                last = last[..last.IndexOf("[", StringComparison.Ordinal)];
            return last;
        }

        private static FieldInfo GetFieldOnHierarchy(Type type, string name)
        {
            while (type != null)
            {
                var field = type.GetField(name, FIELD_FLAGS);
                if (field != null) return field;
                type = type.BaseType;
            }
            return null;
        }

        private void DrawParameters()
        {
            var parameters = _method.GetParameters();
            if (parameters.Length <= 0) return;

            if (!parameters.All(parameter => IsUnitySerializable(parameter.ParameterType))) return;

            var root = new VisualElement();
            var values = new object[parameters.Length];
            var types = new Type[parameters.Length];

            // _parameterValues musi istnieć zanim odpalą się callbacki pól
            _parameterValues = values;
            _parameterTypes = types;

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var type = parameter.ParameterType;
                var label = ObjectNames.NicifyVariableName(parameter.Name);

                values[i] = GetDefaultValue(type);
                types[i] = type;

                var index = i; // capture dla closure
                var field = BuildField(type, label, values[i], value => _parameterValues[index] = value);

                if (field != null) root.Add(field);
            }

            Add(root);
        }

        private static object GetDefaultValue(Type type) =>
            type.IsValueType ? Activator.CreateInstance(type) : null;

        // Buduje pole edycyjne UI Toolkit dla danego typu parametru.
        // onChange zapisuje aktualną wartość bezpośrednio do tablicy parametrów.
        private static VisualElement BuildField(Type type, string label, object initial, Action<object> onChange)
        {
            if (type == typeof(int))
            {
                var field = new IntegerField(label) { value = (int)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(long))
            {
                var field = new LongField(label) { value = (long)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(float))
            {
                var field = new FloatField(label) { value = (float)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(double))
            {
                var field = new DoubleField(label) { value = (double)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(bool))
            {
                var field = new Toggle(label) { value = (bool)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(string))
            {
                var field = new TextField(label) { value = (string)initial ?? string.Empty };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(char))
            {
                var field = new TextField(label, 1, false, false, '\0')
                    { value = initial?.ToString() ?? string.Empty };
                field.RegisterValueChangedCallback(evt =>
                    onChange(evt.newValue.Length > 0 ? evt.newValue[0] : '\0'));
                return field;
            }

            if (type == typeof(byte))
            {
                var field = new IntegerField(label) { value = (byte)initial };
                field.RegisterValueChangedCallback(evt =>
                    onChange((byte)Mathf.Clamp(evt.newValue, byte.MinValue, byte.MaxValue)));
                return field;
            }

            if (type.IsEnum)
            {
                var field = new EnumField(label, (Enum)initial);
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(Vector2))
            {
                var field = new Vector2Field(label) { value = (Vector2)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(Vector3))
            {
                var field = new Vector3Field(label) { value = (Vector3)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(Vector4))
            {
                var field = new Vector4Field(label) { value = (Vector4)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(Vector2Int))
            {
                var field = new Vector2IntField(label) { value = (Vector2Int)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(Vector3Int))
            {
                var field = new Vector3IntField(label) { value = (Vector3Int)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(Rect))
            {
                var field = new RectField(label) { value = (Rect)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(RectInt))
            {
                var field = new RectIntField(label) { value = (RectInt)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(Bounds))
            {
                var field = new BoundsField(label) { value = (Bounds)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(BoundsInt))
            {
                var field = new BoundsIntField(label) { value = (BoundsInt)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(Color))
            {
                var field = new ColorField(label) { value = (Color)initial };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(Color32))
            {
                var field = new ColorField(label) { value = (Color32)initial };
                field.RegisterValueChangedCallback(evt => onChange((Color32)evt.newValue));
                return field;
            }

            if (type == typeof(AnimationCurve))
            {
                var field = new CurveField(label) { value = (AnimationCurve)initial ?? new AnimationCurve() };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(Gradient))
            {
                var field = new GradientField(label) { value = (Gradient)initial ?? new Gradient() };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            if (type == typeof(Quaternion))
            {
                var quaternion = (Quaternion)initial;
                var field = new Vector4Field(label)
                    { value = new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w) };
                field.RegisterValueChangedCallback(evt =>
                {
                    var vector = evt.newValue;
                    onChange(new Quaternion(vector.x, vector.y, vector.z, vector.w));
                });
                return field;
            }

            if (type == typeof(LayerMask))
            {
                var field = new LayerMaskField(label, ((LayerMask)initial).value);
                field.RegisterValueChangedCallback(evt => onChange((LayerMask)evt.newValue));
                return field;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                var field = new ObjectField(label)
                {
                    objectType = type,
                    allowSceneObjects = !typeof(ScriptableObject).IsAssignableFrom(type),
                    value = (UnityEngine.Object)initial
                };
                field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
                return field;
            }

            // Niewspierany typ (np. custom [Serializable]) — brak pola, przekaże default.
            return null;
        }

        private static bool IsUnitySerializable(Type type)
        {
            if (type == typeof(int)
                || type == typeof(long)
                || type == typeof(float) || type == typeof(double)
                || type == typeof(bool) || type == typeof(string)
                || type == typeof(char) || type == typeof(byte))
                return true;

            if (type.IsEnum)
                return true;

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                return true;

            // Unity built-in serializable structs
            if (type == typeof(UnityEngine.Vector2)
                || type == typeof(UnityEngine.Vector3)
                || type == typeof(UnityEngine.Vector4)
                || type == typeof(UnityEngine.Vector2Int)
                || type == typeof(UnityEngine.Vector3Int)
                || type == typeof(UnityEngine.Rect)
                || type == typeof(UnityEngine.RectInt)
                || type == typeof(UnityEngine.Bounds)
                || type == typeof(UnityEngine.BoundsInt)
                || type == typeof(UnityEngine.Color)
                || type == typeof(UnityEngine.Color32)
                || type == typeof(UnityEngine.AnimationCurve)
                || type == typeof(UnityEngine.Gradient)
                || type == typeof(UnityEngine.Quaternion)
                || type == typeof(UnityEngine.LayerMask))
                return true;

            // Custom [Serializable] class/struct
            if (type.IsValueType || type.IsClass)
                return Attribute.IsDefined(type, typeof(SerializableAttribute));

            return false;
        }
    }
}
