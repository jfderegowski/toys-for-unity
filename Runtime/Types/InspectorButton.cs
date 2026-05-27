using System;
using UnityEngine;

namespace fefek5.Toys.Runtime.Types
{
    [Serializable]
    public struct InspectorButton
    {
        public string MethodName => _methodName;

        [SerializeField] private string _methodName;
        [SerializeField] private string _buttonLabel;

        public InspectorButton(string methodName, string buttonLabel = null)
        {
            _methodName = methodName;
            _buttonLabel = buttonLabel;
        }
    }
}