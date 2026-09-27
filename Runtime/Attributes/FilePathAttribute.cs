using System;

namespace fefek5.Toys.Runtime.Attributes
{
    /// <summary>
    /// Draws a string field as a path to a file, picked with the file panel or dropped on the field. Named like
    /// <c>UnityEditor.FilePathAttribute</c>, so editor code using both namespaces needs an alias.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FilePathAttribute : PathAttribute
    {
        /// <summary>Extensions the file panel and dropping accept, e.g. "png" or ".png". Empty accepts any file.</summary>
        public string[] Extensions { get; }

        public FilePathAttribute(params string[] extensions)
        {
            Extensions = extensions;
        }
    }
}
