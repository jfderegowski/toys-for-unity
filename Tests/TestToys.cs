using System;
using fefek5.Toys.Runtime.Attributes;
using fefek5.Toys.Runtime.Types;
using UnityEngine;

namespace Tests
{
    public class TestToys : MonoBehaviour
    {
        [SerializeField] private InspectorButton _button = new(nameof(Method));

        [SerializeField] private SomeStruct SomeStruct;

        [InspectorButton("[" + nameof(TestToys) + "]" + "[" + nameof(Method) + "]")]
        public void Method(int number, ScriptableObject so)
        {
            Debug.Log($"[{nameof(TestToys)}][{nameof(Method)}]: {number}]");
        }
    }

    [Serializable]
    public class SomeStruct
    {
        [SerializeField] private InspectorButton _button = new(nameof(Method));
        
        public void Method(int number, ScriptableObject so)
        {
            Debug.Log($"[{nameof(SomeStruct)}][{nameof(Method)}]: {number}]");
        }
    }
}