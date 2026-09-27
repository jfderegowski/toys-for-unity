using System;
using System.IO;
using fefek5.Toys.Runtime.Types;
using UnityEngine;

namespace fefek5.Toys.Runtime.Extensions
{
    public static class PathRootExtensions
    {
        private static string _projectFolder;

        /// <summary>The root folder, absolute, with forward slashes.</summary>
        /// <param name="root">The root</param>
        /// <param name="customFolder">Folder of <see cref="PathRoot.Custom"/>, absolute or from the project folder</param>
        // Read on use, not in a static constructor: Unity does not allow its paths during deserialization.
        public static string GetFolder(this PathRoot root, string customFolder = null) => root switch {
            PathRoot.PersistentData => Normalize(Application.persistentDataPath),
            PathRoot.StreamingAssets => Normalize(Application.streamingAssetsPath),
            PathRoot.Custom when !string.IsNullOrEmpty(customFolder) => PathRoot.Project.ToAbsolute(customFolder),
            _ => _projectFolder ??= Normalize(Path.GetDirectoryName(Application.dataPath)),
        };

        /// <summary>A path from the root, or an absolute one, as absolute. Empty stays empty.</summary>
        /// <inheritdoc cref="GetFolder" path="/param"/>
        public static string ToAbsolute(this PathRoot root, string path, string customFolder = null)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            return Normalize(Path.GetFullPath(Path.IsPathRooted(path)
                ? path
                : Path.Combine(root.GetFolder(customFolder), path)));
        }

        /// <summary>An absolute path from the root, e.g. "Assets/Textures". A path outside the root stays absolute.</summary>
        /// <inheritdoc cref="GetFolder" path="/param"/>
        public static string ToRelative(this PathRoot root, string absolutePath, string customFolder = null)
        {
            var path = Normalize(absolutePath);
            var folder = root.GetFolder(customFolder);

            if (string.Equals(path, folder, StringComparison.OrdinalIgnoreCase))
                return ".";

            return path.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase) ? path[(folder.Length + 1)..] : path;
        }

        /// <summary>Forward slashes, no trailing slash.</summary>
        public static string Normalize(string path) => path.Replace('\\', '/').TrimEnd('/');
    }
}
