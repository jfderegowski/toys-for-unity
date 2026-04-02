using System;

namespace fefek5.Toys.Runtime.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InspectorButtonAttribute : Attribute
    {
        public string Label { get; }

        public InspectorButtonAttribute(string label = null)
        {
            Label = label;
        }
    }
}