using System;
using System.IO;
using fefek5.Toys.Runtime.Extensions;
using UnityEngine;

namespace fefek5.Toys.Runtime.Types
{
    /// <summary>
    /// A path to a file, drawn with a browse button and a dropdown of the folder it starts from. Stored from that
    /// folder when inside it, e.g. "Assets/Icons/Coin.png" from <see cref="PathRoot.Project"/>; absolute otherwise.
    /// <c>[FilePath("png")]</c> on the field limits the extensions.
    /// </summary>
    [Serializable]
    public struct FilePath : IEquatable<FilePath>
    {
        [SerializeField] private string _path;
        [SerializeField] private PathRoot _root;
        [SerializeField] private string _customRoot;

        /// <param name="path">From <paramref name="root"/>, or absolute</param>
        /// <param name="root">The folder a relative path starts from</param>
        public FilePath(string path, PathRoot root = PathRoot.Project)
        {
            _path = path;
            _root = root;
            _customRoot = null;
        }

        /// <param name="path">From <paramref name="customRoot"/>, or absolute</param>
        /// <param name="customRoot">The folder a relative path starts from, absolute or from the project folder</param>
        public FilePath(string path, string customRoot)
        {
            _path = path;
            _root = PathRoot.Custom;
            _customRoot = customRoot;
        }

        /// <summary>The path as stored.</summary>
        public string Value => _path ?? string.Empty;

        /// <summary>The folder a relative path starts from.</summary>
        public PathRoot Root => _root;

        /// <summary>The folder of <see cref="PathRoot.Custom"/>, absolute or from the project folder.</summary>
        public string CustomRoot => _customRoot ?? string.Empty;

        /// <summary>The path as absolute, whichever way it is stored.</summary>
        public string FullPath => _root.ToAbsolute(_path, _customRoot);

        public bool IsEmpty => string.IsNullOrEmpty(_path);

        public bool Exists => !IsEmpty && File.Exists(FullPath);

        /// <summary>Name of the file with its extension.</summary>
        public string Name => Path.GetFileName(Value);

        public string NameWithoutExtension => Path.GetFileNameWithoutExtension(Value);

        /// <summary>Extension with the dot, e.g. ".png", or empty.</summary>
        public string Extension => Path.GetExtension(Value);

        /// <summary>The folder holding the file, from the same root.</summary>
        public FolderPath Folder
        {
            get
            {
                if (IsEmpty)
                    return default;

                var folder = PathRootExtensions.Normalize(Path.GetDirectoryName(Value) ?? string.Empty);

                return _root == PathRoot.Custom ? new FolderPath(folder, _customRoot) : new FolderPath(folder, _root);
            }
        }

        public bool Equals(FilePath other) =>
            Value == other.Value && _root == other._root && CustomRoot == other.CustomRoot;

        public override bool Equals(object obj) => obj is FilePath other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Value, _root, CustomRoot);

        public override string ToString() => Value;

        public static bool operator ==(FilePath left, FilePath right) => left.Equals(right);

        public static bool operator !=(FilePath left, FilePath right) => !left.Equals(right);

        public static implicit operator string(FilePath path) => path.Value;

        /// <summary>A path from the project folder, or absolute.</summary>
        public static implicit operator FilePath(string path) => new(path);
    }
}
