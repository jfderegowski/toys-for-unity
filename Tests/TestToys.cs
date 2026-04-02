using fefek5.Toys.Runtime.Attributes;
using fefek5.Toys.Runtime.Types;
using UnityEngine;

namespace Tests
{
    public class TestToys : MonoBehaviour
    {
        [SerializeField] private InspectorButton _button = new InspectorButton(nameof(TestFromStruct));

        [InspectorButton("Test From Struct")]
        public void TestFromStruct()
        {
            Debug.Log("TestFromStruct");
        }

        [InspectorButton("Test From Attribute")]
        public void TestFromAttribute()
        {
            Debug.Log("TestFromAttribute");
        }
    }
}