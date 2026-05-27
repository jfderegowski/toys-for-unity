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
        private PropertyField[] _parameters;

        private readonly object _target;
        private readonly string _methodName;
        private readonly MethodInfo _method;
        private readonly Button _button;

        private const BindingFlags METHOD_FLAGS =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public InspectorButtonElement(object target, string methodName)
            : this(target, methodName, ObjectNames.NicifyVariableName(methodName)) { }

        public InspectorButtonElement(object target, string methodName, string buttonLabel)
        {
            _target = target;
            _methodName = methodName;
            _method = !methodName.IsBlank()
                ? target?.GetType().GetMethod(methodName, METHOD_FLAGS)
                : null;

            _button = new Button(CallMethod) {
                text = buttonLabel
            };

            Add(_button);

            if (_method == null)
            {
                var refreshButton = new Button();
                
                Add(refreshButton);
                
                _button.SetEnabled(false);
                _button.tooltip = $"Method '{methodName}' not found on {target?.GetType().Name ?? "null target"}";
                return;
            }

            DrawParameters();

            // text = label ?? ObjectNames.NicifyVariableName(methodName);
            _button.clicked += CallMethod;
        }

        private void CallMethod()
        {
            if (_method == null) return;

            _method.Invoke(_target, GetParameters());

            if (_target is UnityEngine.Object targetObject)
                EditorUtility.SetDirty(targetObject);
        }

        private object[] GetParameters() => _parameters.Select(field => field.dataSource).ToArray();

        // 1. Stwórz listę/słownik w klasie, aby utrzymać obiekty przy życiu
        private List<SerializedObject> _serializedParameterHolders = new();

        private void DrawParameters()
        {
            var parameters = _method.GetParameters();
            if (parameters.Length <= 0) return;

            var serializableParameters = parameters
                .Where(parameter => IsUnitySerializable(parameter.ParameterType)).ToArray();

            if (serializableParameters.Length != parameters.Length) return;

            var root = new VisualElement();
            _serializedParameterHolders.Clear(); // Czyścimy poprzednie referencje

            var propertyFields = new PropertyField[serializableParameters.Length];

            for (var i = 0; i < serializableParameters.Length; i++)
            {
                var parameter = serializableParameters[i];

                // Tworzymy holder
                var (holder, fieldName) = CreateParameterHolder(parameter.ParameterType);

                // Tworzymy SerializedObject i ZAPISUJEMY GO w polu klasy
                var serializedHolder = new SerializedObject(holder);
                _serializedParameterHolders.Add(serializedHolder);

                var serializedParam = serializedHolder.FindProperty(fieldName);

                // Tworzymy pole
                var propertyField = new PropertyField(serializedParam, ObjectNames.NicifyVariableName(parameter.Name));

                // KLUCZOWY MOMENT: Jawne powiązanie
                propertyField.BindProperty(serializedParam);

                root.Add(propertyField);
                propertyFields[i] = propertyField;
            }

            Add(root);
            _parameters = propertyFields;
        }

        // private void DrawParameters()
        // {
        //     var parameters = _method.GetParameters();
        //     
        //     if (parameters.Length <= 0) return;
        //     
        //     var serializableParameters = parameters
        //         .Where(parameter => IsUnitySerializable(parameter.ParameterType)).ToArray();
        //
        //     if (serializableParameters.Length != parameters.Length)
        //         return;
        //     
        //     var root = new VisualElement();
        //     
        //     var propertyFields = new PropertyField[serializableParameters.Length];
        //
        //     for (var i = 0; i < serializableParameters.Length; i++)
        //     {
        //         var parameter = serializableParameters[i];
        //         var (holder, fieldName) = CreateParameterHolder(parameter.ParameterType);
        //         var serializedHolder = new SerializedObject(holder);
        //         var serializedParam = serializedHolder.FindProperty(fieldName);
        //         var propertyField = new PropertyField(serializedParam, ObjectNames.NicifyVariableName(parameter.Name));
        //         
        //         root.Add(propertyField);
        //
        //         propertyFields[i] = propertyField;
        //     }
        //     
        //     Add(root);
        //     
        //     _parameters = propertyFields;
        // }

        private static readonly Dictionary<Type, string> TypeToFieldName = new()
        {
            { typeof(int), "intValue" },
            { typeof(long), "longValue" },
            { typeof(float), "floatValue" },
            { typeof(double), "doubleValue" },
            { typeof(bool), "boolValue" },
            { typeof(string), "stringValue" },
            { typeof(char), "charValue" },
            { typeof(byte), "byteValue" },
            { typeof(Vector2), "vector2Value" },
            { typeof(Vector3), "vector3Value" },
            { typeof(Vector4), "vector4Value" },
            { typeof(Vector2Int), "vector2IntValue" },
            { typeof(Vector3Int), "vector3IntValue" },
            { typeof(Rect), "rectValue" },
            { typeof(RectInt), "rectIntValue" },
            { typeof(Bounds), "boundsValue" },
            { typeof(BoundsInt), "boundsIntValue" },
            { typeof(Color), "colorValue" },
            { typeof(Color32), "color32Value" },
            { typeof(AnimationCurve), "animationCurveValue" },
            { typeof(Gradient), "gradientValue" },
            { typeof(Quaternion), "quaternionValue" },
            { typeof(LayerMask), "layerMaskValue" },
        };

        private static (ScriptableObject holder, string fieldName) CreateParameterHolder(Type parameterType)
        {
            var holder = ScriptableObject.CreateInstance<ParameterHolder>();

            if (TypeToFieldName.TryGetValue(parameterType, out var fieldName))
                return (holder, fieldName);

            if (parameterType.IsEnum)
                return (holder, "intValue");

            if (typeof(UnityEngine.Object).IsAssignableFrom(parameterType))
                return (holder, "objectValue");

            return (holder, null);
        }

        private class ParameterHolder : ScriptableObject
        {
            public int intValue;
            public long longValue;
            public float floatValue;
            public double doubleValue;
            public bool boolValue;
            public string stringValue;
            public char charValue;
            public byte byteValue;
            public Vector2 vector2Value;
            public Vector3 vector3Value;
            public Vector4 vector4Value;
            public Vector2Int vector2IntValue;
            public Vector3Int vector3IntValue;
            public Rect rectValue;
            public RectInt rectIntValue;
            public Bounds boundsValue;
            public BoundsInt boundsIntValue;
            public Color colorValue;
            public Color32 color32Value;
            public AnimationCurve animationCurveValue;
            public Gradient gradientValue;
            public Quaternion quaternionValue;
            public LayerMask layerMaskValue;
            public UnityEngine.Object objectValue;
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