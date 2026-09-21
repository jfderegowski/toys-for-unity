using System;
using UnityEngine;

namespace fefek5.Toys.Runtime.Attributes
{
    /// <summary>
    /// Draws a [SerializeReference] field with a dropdown of every concrete type assignable to the field type. Picking
    /// a type creates a new instance of it in place of the current value. On a list or an array it draws each element.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SelectTypeAttribute : PropertyAttribute { }
}
