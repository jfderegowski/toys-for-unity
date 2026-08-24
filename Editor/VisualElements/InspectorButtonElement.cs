using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using fefek5.Toys.Runtime.Extensions;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    public class InspectorButtonElement : VisualElement
    {
        private object[] _parameterValues;

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
            style.marginLeft = 3;
            style.marginTop = 1;
            style.marginRight = -2;
            style.marginBottom = 1;

            _target = target;
            _property = property;
            Build(methodName, buttonLabel);
        }

        private void Build(string methodName, string buttonLabel)
        {
            Clear();
            _parameterValues = null;
            style.flexDirection = FlexDirection.Column;

            _methodName = methodName;
            _buttonLabel = buttonLabel;
            _method = !methodName.IsBlank()
                ? _target?.GetType().GetMethod(methodName, METHOD_FLAGS)
                : null;

            var button = new Button(CallMethod) {
                text = buttonLabel,
                style = {
                    marginLeft = 0,
                    marginTop = 0,
                    marginRight = 0,
                    marginBottom = 0,
                }
            };

            if (_method == null)
            {
                BuildMethodMissingUI(button, methodName);
                return;
            }

            var parameters = CreateParametersElement();

            if (parameters == null)
            {
                Add(button);
                return;
            }

            button.style.borderLeftWidth = 0;
            button.style.marginBottom = 0;
            button.style.paddingRight = 28;

            // Header: [toggle][button] w jednym rzędzie, parametry pod spodem.
            var toggle = new Button {
                style = {
                    width = 22,
                    marginLeft = 0,
                    marginTop = 0,
                    marginRight = 0,
                    marginBottom = 0,
                }
            };
            var icon = ApplyDropdownIcon(toggle);

            button.style.flexGrow = 1;
            button.style.marginLeft = 0;

            var header = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            header.Add(toggle);
            header.Add(button);

            // Stan rozwinięcia pamiętany w sesji (static dict — czyści się przy
            // domain reload, bez zapisu do plików).
            var key = ExpandKey(methodName);
            var expanded = _expandedState.TryGetValue(key, out var saved) && saved;

            parameters.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;

            void SetExpanded(bool value)
            {
                expanded = value;
                parameters.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                _expandedState[key] = value;
                UpdateCorners(toggle, button, parameters, icon, value);
            }

            // Stary handler (re-Build na tym samym elemencie) odpinamy, żeby nie dublować.
            if (_applyExpand != null) _setAllExpanded -= _applyExpand;
            _applyExpand = SetExpanded;
            _setAllExpanded += _applyExpand;
            RegisterCallback<DetachFromPanelEvent>(_ => {
                if (_applyExpand != null) _setAllExpanded -= _applyExpand;
            });

            // ClickEvent zamiast clicked — daje dostęp do altKey.
            toggle.RegisterCallback<ClickEvent>(evt => {
                var value = parameters.style.display == DisplayStyle.None;
                if (evt.altKey) _setAllExpanded?.Invoke(value); // Alt = wszystkie
                else SetExpanded(value);
            });

            Add(header);
            Add(parameters);

            UpdateCorners(toggle, button, parameters, icon, expanded);
        }

        // Stan rozwinięcia per przycisk, trzymany tylko w pamięci (sesja edytora).
        private static readonly Dictionary<string, bool> _expandedState = new();

        // Wszystkie żywe przyciski — Alt+klik toggla działa na każdy.
        private static event Action<bool> _setAllExpanded;
        private Action<bool> _applyExpand;

        private string ExpandKey(string methodName) =>
            $"{_target?.GetType().FullName}:{_property?.propertyPath ?? methodName}";

        private const int CORNER_RADIUS = 3; // jak podstawowy button Unity

        // [toggle][button] w rzędzie, parametry pod spodem.
        // Toggle trzyma lewe rogi, button prawe; wewnętrzne (stykające się) rogi = 0.
        // Rozwinięte: dolne rogi headera płaskie (łączy się z parametrami), parametry
        // dostają zaokrąglony dół. Schowane: header zaokrąglony też na dole.
        private static void UpdateCorners(
            Button toggle, Button button, VisualElement parameters, VisualElement icon, bool expanded)
        {
            var bottom = expanded ? 0 : CORNER_RADIUS;

            // Obracamy samą ikonkę (nie cały button, bo rozjeżdża layout).
            // Schowane: w prawo (▸). Rozwinięte: w dół (▾).
            icon.style.rotate = new StyleRotate(
                new Rotate(new Angle(expanded ? 0 : -90, AngleUnit.Degree)));

            toggle.style.borderTopLeftRadius = CORNER_RADIUS;
            toggle.style.borderTopRightRadius = 0;
            toggle.style.borderBottomLeftRadius = bottom;
            toggle.style.borderBottomRightRadius = 0;

            button.style.borderTopLeftRadius = 0;
            button.style.borderTopRightRadius = CORNER_RADIUS;
            button.style.borderBottomLeftRadius = 0;
            button.style.borderBottomRightRadius = bottom;

            parameters.style.borderBottomLeftRadius = CORNER_RADIUS;
            parameters.style.borderBottomRightRadius = CORNER_RADIUS;
        }

        private const string DROPDOWN_ICON_NAME = "DropdownArrow";

        // Zwraca element ikonki (do obracania) — osobny child, nie iconImage buttona,
        // żeby obrót nie ruszał layoutu toggla. Obrót animowany przez transition.
        private static VisualElement ApplyDropdownIcon(Button toggle)
        {
            var texture = LoadIcon(DROPDOWN_ICON_NAME);

            Color tint = new Color32(0x68, 0x68, 0x68, 0xFF);

            VisualElement icon;

            if (texture != null)
            {
                icon = new Image {
                    image = texture,
                    tintColor = tint,
                    style = { width = 12, height = 12 }
                };
            }
            else
            {
                icon = new Label("▾") {
                    style = { unityTextAlign = TextAnchor.MiddleCenter, color = tint }
                };
            }

            icon.style.alignSelf = Align.Center;
            // Ikonka nie może łapać kliknięć — inaczej button toggla ich nie dostaje.
            icon.pickingMode = PickingMode.Ignore;

            // Płynna animacja obrotu.
            icon.style.transitionProperty = new List<StylePropertyName> { "rotate" };
            icon.style.transitionDuration = new List<TimeValue> { new(0.15f, TimeUnit.Second) };
            icon.style.transitionTimingFunction =
                new List<EasingFunction> { new(EasingMode.EaseOutCubic) };

            toggle.Add(icon);
            return icon;
        }

        private static Texture2D LoadIcon(string name)
        {
            var guids = AssetDatabase.FindAssets($"{name} t:Texture2D");
            if (guids.Length <= 0) return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
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

        private VisualElement CreateParametersElement()
        {
            var parameters = _method.GetParameters();
            if (parameters.Length <= 0) return null;

            if (!parameters.All(parameter => IsUnitySerializable(parameter.ParameterType))) return null;

            var defaultBackgroundStyleColor = style.backgroundColor;
            var defaultBackgroundColor = defaultBackgroundStyleColor.value;

            var root = new VisualElement() {
                style = {
                    backgroundColor = new StyleColor(Color.Lerp(defaultBackgroundColor, Color.black, 0.1f)),
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    // marginRight = 3,
                    // marginLeft = 3,
                    // marginBottom = 1,
                    paddingLeft = 6,
                    paddingTop = 6,
                    paddingRight = 8,
                    paddingBottom = 6,
                }
            };
            var values = new object[parameters.Length];

            // _parameterValues musi istnieć zanim odpalą się callbacki pól
            _parameterValues = values;

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var type = parameter.ParameterType;
                var label = ObjectNames.NicifyVariableName(parameter.Name);

                values[i] = GetDefaultValue(type);

                var index = i; // capture dla closure
                var field = BuildField(type, label, values[i], value => _parameterValues[index] = value);

                if (field != null) root.Add(field);
            }

            return root;
        }

        private static object GetDefaultValue(Type type) =>
            type.IsValueType ? Activator.CreateInstance(type) : null;

        // Buduje pole edycyjne UI Toolkit dla danego typu parametru.
        // onChange zapisuje aktualną wartość bezpośrednio do tablicy parametrów.
        private static VisualElement BuildField(Type type, string label, object initial, Action<object> onChange)
        {
            // Typy mapujące się 1:1 na BaseField<T> — wartość bez konwersji.
            if (type == typeof(int)) return Field<IntegerField, int>(label, (int)initial, onChange);
            if (type == typeof(long)) return Field<LongField, long>(label, (long)initial, onChange);
            if (type == typeof(float)) return Field<FloatField, float>(label, (float)initial, onChange);
            if (type == typeof(double)) return Field<DoubleField, double>(label, (double)initial, onChange);
            if (type == typeof(bool)) return Field<Toggle, bool>(label, (bool)initial, onChange);
            if (type == typeof(string)) return Field<TextField, string>(label, (string)initial ?? string.Empty, onChange);
            if (type == typeof(Vector2)) return Field<Vector2Field, Vector2>(label, (Vector2)initial, onChange);
            if (type == typeof(Vector3)) return Field<Vector3Field, Vector3>(label, (Vector3)initial, onChange);
            if (type == typeof(Vector4)) return Field<Vector4Field, Vector4>(label, (Vector4)initial, onChange);
            if (type == typeof(Vector2Int)) return Field<Vector2IntField, Vector2Int>(label, (Vector2Int)initial, onChange);
            if (type == typeof(Vector3Int)) return Field<Vector3IntField, Vector3Int>(label, (Vector3Int)initial, onChange);
            if (type == typeof(Rect)) return Field<RectField, Rect>(label, (Rect)initial, onChange);
            if (type == typeof(RectInt)) return Field<RectIntField, RectInt>(label, (RectInt)initial, onChange);
            if (type == typeof(Bounds)) return Field<BoundsField, Bounds>(label, (Bounds)initial, onChange);
            if (type == typeof(BoundsInt)) return Field<BoundsIntField, BoundsInt>(label, (BoundsInt)initial, onChange);
            if (type == typeof(Color)) return Field<ColorField, Color>(label, (Color)initial, onChange);
            if (type == typeof(AnimationCurve)) return Field<CurveField, AnimationCurve>(label, (AnimationCurve)initial ?? new AnimationCurve(), onChange);
            if (type == typeof(Gradient)) return Field<GradientField, Gradient>(label, (Gradient)initial ?? new Gradient(), onChange);

            // Typy wymagające konwersji wartości lub specjalnego konstruktora.
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

            if (type == typeof(Color32))
            {
                var field = new ColorField(label) { value = (Color32)initial };
                field.RegisterValueChangedCallback(evt => onChange((Color32)evt.newValue));
                return field;
            }

            if (type == typeof(Quaternion))
            {
                var quaternion = (Quaternion)initial;
                var field = new Vector4Field(label)
                    { value = new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w) };
                field.RegisterValueChangedCallback(evt => {
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
                var field = new ObjectField(label) {
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

        // Tworzy proste BaseField<TValue>, ustawia etykietę/wartość i podpina callback.
        private static VisualElement Field<TField, TValue>(string label, TValue initial, Action<object> onChange)
            where TField : BaseField<TValue>, new()
        {
            var field = new TField { label = label, value = initial };
            field.RegisterValueChangedCallback(evt => onChange(evt.newValue));
            return field;
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