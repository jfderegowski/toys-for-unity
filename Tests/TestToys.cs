using System;
using fefek5.Toys.Runtime.Attributes;
using fefek5.Toys.Runtime.Types;
using UnityEngine;

namespace Tests
{
    // Manualny test InspectorButton. Pokrywa OBA mechanizmy rysowania:
    //  1. serializowane pole typu InspectorButton  -> InspectorButtonDrawer (ścieżka z
    //     SerializedProperty, czyli też refresh / reset-to-default przy braku metody),
    //  2. atrybut [InspectorButton] na metodzie     -> MonoBehaviourEditor.
    // Plus: panel parametrów dla szerokiego zestawu typów, metoda static, mutacja stanu
    // oraz zagnieżdżona [Serializable] klasa (test rozwiązywania owner-type).
    public class TestToys : MonoBehaviour
    {
        // --- Pola typu InspectorButton (ścieżka Drawer-a) ---------------------------

        // Bez etykiety: label brany z nazwy metody (NicifyVariableName).
        [SerializeField] private InspectorButton _noLabel = new(nameof(NoArgs));

        // Z własną etykietą.
        [SerializeField] private InspectorButton _withLabel = new(nameof(SingleArg), "Run Single Arg");

        // Brakująca metoda: rysuje stan błędu + przycisk refresh (reset-to-default).
        [SerializeField] private InspectorButton _missing = new("ThisMethodDoesNotExist");

        // Zwykłe pola obok, żeby sprawdzić rozmieszczenie buttonów między polami.
        [SerializeField] private string _someString;
        [SerializeField] private SomeStruct _nested;

        // --- Metody z atrybutem [InspectorButton] (ścieżka Editor-a) ----------------

        // Bez parametrów: goły button, bez panelu parametrów.
        [InspectorButton]
        private void NoArgs() => Debug.Log("NoArgs()");

        // Jeden parametr: minimalny panel parametrów.
        [InspectorButton("Single Arg")]
        private void SingleArg(int amount) => Debug.Log($"SingleArg({amount})");

        // Typy proste: pełny zestaw pól skalarnych.
        [InspectorButton]
        private void Primitives(int i, long l, float f, double d, bool b, string s, char c, byte by) =>
            Debug.Log($"Primitives(i={i}, l={l}, f={f}, d={d}, b={b}, s=\"{s}\", c='{c}', by={by})");

        // Enum: EnumField.
        [InspectorButton]
        private void EnumArg(KeyCode key) => Debug.Log($"EnumArg({key})");

        // Struktury Unity: wektory / kolor / maska.
        [InspectorButton]
        private void UnityStructs(Vector2 v2, Vector3 v3, Vector4 v4, Color color, LayerMask mask) =>
            Debug.Log($"UnityStructs(v2={v2}, v3={v3}, v4={v4}, color={color}, mask={mask.value})");

        // Geometria + typy referencyjne z domyślnym new (curve/gradient).
        [InspectorButton]
        private void Geometry(Rect rect, Bounds bounds, Quaternion rotation, AnimationCurve curve, Gradient gradient) =>
            Debug.Log($"Geometry(rect={rect}, bounds={bounds}, euler={rotation.eulerAngles}, " +
                      $"curveKeys={curve.length}, gradientStops={gradient.colorKeys.Length})");

        // Referencje do obiektów Unity: ObjectField (scena vs. asset).
        [InspectorButton]
        private void ObjectRef(GameObject go, Transform t, Material material) =>
            Debug.Log($"ObjectRef(go={go}, t={t}, material={material})");

        // Metoda static: METHOD_FLAGS obejmuje Static.
        [InspectorButton]
        private static void StaticArgs(float value) => Debug.Log($"StaticArgs({value})");

        // Efekt uboczny: mutuje komponent (oczekiwany SetDirty po wywołaniu).
        [InspectorButton("Rename GameObject")]
        private void MutateState(string newName)
        {
            gameObject.name = newName;
            Debug.Log($"MutateState — renamed to \"{newName}\"");
        }
    }

    // Zagnieżdżona [Serializable] klasa: testuje walk po owner-type (GetOwnerType)
    // oraz reset-to-default dla pola w typie innym niż sam MonoBehaviour.
    [Serializable]
    public class SomeStruct
    {
        [SerializeField] private string _before;
        [SerializeField] private InspectorButton _button = new(nameof(Nested), "Nested Button");
        [SerializeField] private string _after;

        [InspectorButton("Nested Attribute")]
        public void Nested(int number, ScriptableObject so) =>
            Debug.Log($"[SomeStruct][Nested]: {number}", so);
    }
}
