using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace fefek5.Toys.Editor.VisualElements
{
    /// <summary>A <see cref="PathField"/> for a file, optionally of the given extensions.</summary>
    public class FilePathField : PathField
    {
        /// <summary>Extensions without the dot, e.g. "png". Empty accepts any file.</summary>
        public string[] Extensions { get; }

        /// <param name="label">Label of the field</param>
        /// <param name="extensions">Extensions the panel and dropping accept, e.g. "png" or ".png"</param>
        public FilePathField(string label = null, params string[] extensions) : base(label)
        {
            Extensions = (extensions ?? Array.Empty<string>()).Select(extension => extension.TrimStart('.')).ToArray();
        }

        protected override string Kind => Extensions.Length > 0 ? $"{string.Join("/", Extensions)} file" : "file";

        protected override string Browse(string absolutePath)
        {
            var folder = GetBrowseFolder(absolutePath);

            return Extensions.Length > 0
                ? EditorUtility.OpenFilePanelWithFilters(PanelTitle, folder,
                    new[] { string.Join(", ", Extensions), string.Join(",", Extensions) })
                : EditorUtility.OpenFilePanel(PanelTitle, folder, string.Empty);
        }

        protected override bool Exists(string absolutePath) => File.Exists(absolutePath);

        protected override bool CanDrop(string absolutePath) => Exists(absolutePath) && HasExtension(absolutePath);

        private bool HasExtension(string path) =>
            Extensions.Length == 0 ||
            Extensions.Contains(Path.GetExtension(path).TrimStart('.'), StringComparer.OrdinalIgnoreCase);
    }
}