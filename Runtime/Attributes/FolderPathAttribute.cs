using System;

namespace fefek5.Toys.Runtime.Attributes
{
    /// <summary>Draws a string field as a path to a folder, picked with the folder panel or dropped on the field.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FolderPathAttribute : PathAttribute { }
}
