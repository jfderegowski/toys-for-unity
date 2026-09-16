using System;
using UnityEngine;

namespace fefek5.Toys.Runtime.Attributes
{
    /// <summary>
    /// Draws a [SerializeReference] list or array whose add button lists every concrete type assignable to the
    /// element type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializeReferenceListAttribute : PropertyAttribute
    {
        // Without applyToCollection Unity hands the drawer every element instead of the collection itself.
        public SerializeReferenceListAttribute() : base(applyToCollection: true) { }
    }
}
