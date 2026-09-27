using fefek5.Toys.Runtime.Types;
using UnityEngine;

namespace fefek5.Toys.Runtime.Attributes
{
    /// <summary>
    /// Base of the attributes that draw a string field as a path on disk with a browse button. Each subclass says
    /// what the path points at.
    /// </summary>
    public abstract class PathAttribute : PropertyAttribute
    {
        /// <summary>
        /// Store the path from <see cref="Root"/>, e.g. "Assets/Textures". A path outside it stays absolute.
        /// </summary>
        public bool Relative { get; set; } = true;

        /// <summary>
        /// The folder a relative path starts from, on a string field. A FolderPath or FilePath holds its own root,
        /// set where it is declared, e.g. <c>new FilePath("Stats.json", PathRoot.PersistentData)</c>.
        /// </summary>
        public PathRoot Root { get; set; } = PathRoot.Project;
    }
}
