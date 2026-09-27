using System;
using System.IO;
using fefek5.Toys.Runtime.Extensions;
using UnityEngine;

namespace fefek5.Toys.Runtime.Types
{
    /// <summary>
    /// A path to a folder, drawn with a browse button and a dropdown of the folder it starts from. Stored from that
    /// folder when inside it, e.g. "Assets/Textures" from <see cref="PathRoot.Project"/>; absolute otherwise.
    /// </summary>
    [Serializable]
    public struct FolderPath : IEquatable<FolderPath>
    {
        [SerializeField] private string _path;
        [SerializeField] private PathRoot _root;
        [SerializeField] private string _customRoot;

        /// <param name="path">From <paramref name="root"/>, or absolute</param>
        /// <param name="root">The folder a relative path starts from</param>
        public FolderPath(string path, PathRoot root = PathRoot.Project)
        {
            _path = path;
            _root = root;
            _customRoot = null;
        }

        /// <param name="path">From <paramref name="customRoot"/>, or absolute</param>
        /// <param name="customRoot">The folder a relative path starts from, absolute or from the project folder</param>
        public FolderPath(string path, string customRoot)
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

        public bool Exists => !IsEmpty && Directory.Exists(FullPath);

        /// <summary>Name of the folder itself.</summary>
        public string Name => Path.GetFileName(FullPath);

        public bool Equals(FolderPath other) =>
            Value == other.Value && _root == other._root && CustomRoot == other.CustomRoot;

        public override bool Equals(object obj) => obj is FolderPath other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Value, _root, CustomRoot);

        public override string ToString() => Value;

        public static bool operator ==(FolderPath left, FolderPath right) => left.Equals(right);

        public static bool operator !=(FolderPath left, FolderPath right) => !left.Equals(right);

        public static implicit operator string(FolderPath path) => path.Value;

        /// <summary>A path from the project folder, or absolute.</summary>
        public static implicit operator FolderPath(string path) => new(path);
    }
}
