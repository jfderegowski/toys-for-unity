using System;
using UnityEngine;

namespace fefek5.Toys.Runtime.Types
{
    [Serializable]
    public struct InspectorButton
    {
        [SerializeField] private string methodName;

        public string MethodName => methodName;

        public InspectorButton(string methodName)
        {
            this.methodName = methodName;
        }
    }
}