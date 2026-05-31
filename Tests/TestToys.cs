using System;
using fefek5.Toys.Runtime.Attributes;
using fefek5.Toys.Runtime.Types;
using UnityEngine;

namespace Tests
{
    public class TestToys : MonoBehaviour
    {
        [SerializeField] private InspectorButton _button = new(nameof(Method3), "___");
        [SerializeField] private InspectorButton _button5 = new(nameof(Method2), "___");
        [SerializeField] private InspectorButton _button3 = new(nameof(Method3), "___");

        [SerializeField] private SomeStruct SomeStruct;
        [SerializeField] private string _someString;

        [InspectorButton("[" + nameof(TestToys) + "]" + "[" + nameof(Method3) + "]")]
        public void Method3(int number, ScriptableObject so)
        {
            Debug.Log($"[{nameof(TestToys)}][{nameof(Method3)}]: {number}]", so);
        }

        [InspectorButton("[" + nameof(TestToys) + "]" + "[" + nameof(Method2) + "]")]
        public void Method2()
        {
            Debug.Log($"[{nameof(TestToys)}][{nameof(Method2)}]");
        }
    }

    [Serializable]
    public class SomeStruct
    {
        [SerializeField] private string _someString;
        [SerializeField] private InspectorButton _button = new(nameof(Method));
        [SerializeField] private string _someString2;
        [SerializeField] private string _someString3;

        [InspectorButton("[" + nameof(SomeStruct) + "]" + "[" + nameof(Method) + "]")]
        public void Method(int number, ScriptableObject so)
        {
            Debug.Log($"[{nameof(SomeStruct)}][{nameof(Method)}]: {number}]", so);
        }
        
        [InspectorButton("[" + nameof(SomeStruct) + "]" + "[" + nameof(Method2) + "]")]
        public void Method2()
        {
            Debug.Log($"[{nameof(SomeStruct)}][{nameof(Method2)}]");
        }
    }
}