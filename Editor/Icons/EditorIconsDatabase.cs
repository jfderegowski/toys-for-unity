using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace fefek5.Toys.Editor.Icons
{
    /// <summary>
    /// Baza ikonek edytorowych. Po utworzeniu assetu sama skanuje swój folder i podfoldery —
    /// nie trzeba przeciągać żadnych referencji.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(EditorIconsDatabase), menuName = "Toys/Editor Icons Database")]
    public class EditorIconsDatabase : ScriptableObject
    {
        #region Types

        [Serializable]
        private struct Icon : IEquatable<Icon>
        {
            /// <summary>Ścieżka względna do folderu bazy, bez rozszerzenia (np. "Arrows/DropdownArrow").</summary>
            public string path;

            public Texture2D texture;

            /// <summary>Null gdy plik nie jest zaimportowany jako Sprite.</summary>
            public Sprite sprite;

            public bool Equals(Icon other) =>
                path == other.path && texture == other.texture && sprite == other.sprite;

            public override bool Equals(object obj) => obj is Icon other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(path, texture, sprite);
        }

        #endregion

        #region Fields

        [SerializeField] private List<Icon> _icons = new();

        /// <summary>Klucz (ścieżka względna oraz sama nazwa pliku) -> ikonka. Budowana w pamięci, nieserializowana.</summary>
        private Dictionary<string, Icon> _map;

        private static EditorIconsDatabase[] _databases;
        private static bool _rescanScheduled;

        private static readonly string[] IMAGE_EXTENSIONS =
            { ".png", ".jpg", ".jpeg", ".tga", ".psd", ".gif", ".bmp", ".exr", ".tif", ".tiff" };

        #endregion

        #region Public API

        /// <summary>
        /// Zwraca ikonkę po nazwie pliku ("DropdownArrow") albo po ścieżce względnej do folderu bazy
        /// ("Arrows/DropdownArrow"), w obu przypadkach bez rozszerzenia.
        /// </summary>
        /// <typeparam name="T">Texture2D, Texture albo Sprite.</typeparam>
        /// <exception cref="KeyNotFoundException">Nie ma takiej ikonki w żadnej bazie.</exception>
        /// <exception cref="InvalidOperationException">Plik jest, ale nie jest zaimportowany jako Sprite.</exception>
        /// <exception cref="NotSupportedException">Nieobsługiwany typ T.</exception>
        public static T GetIcon<T>(string nameOrPath) where T : Object
        {
            if (!TryGetEntry(nameOrPath, out var icon))
                throw new KeyNotFoundException($"[{nameof(EditorIconsDatabase)}] There is no icon " +
                                               $"'{nameOrPath}' in any database ({DatabasesInfo()}).");

            if (typeof(T) == typeof(Sprite))
            {
                if (!icon.sprite)
                    throw new InvalidOperationException(
                        $"[{nameof(EditorIconsDatabase)}] Icon '{nameOrPath}' " +
                        $"({AssetDatabase.GetAssetPath(icon.texture)}) is not imported as a Sprite - " +
                        $"set Texture Type to Sprite in the importer.");

                return icon.sprite as T;
            }

            if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Texture) || typeof(T) == typeof(Object))
                return icon.texture as T;

            throw new NotSupportedException($"[{nameof(EditorIconsDatabase)}] Icons can be read as Texture2D, " +
                                            $"Texture or Sprite, not as {typeof(T).Name}.");
        }

        /// <summary>
        /// Wersja <see cref="GetIcon{T}"/> bez wyjątków — false gdy ikonki nie ma, gdy nie jest Sprite'em
        /// albo gdy typ T jest nieobsługiwany.
        /// </summary>
        public static bool TryGetIcon<T>(string nameOrPath, out T icon) where T : Object
        {
            icon = null;

            if (!TryGetEntry(nameOrPath, out var entry)) return false;

            if (typeof(T) == typeof(Sprite))
                icon = entry.sprite as T;
            else if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Texture) || typeof(T) == typeof(Object))
                icon = entry.texture as T;

            return (Object)icon != null;
        }

        /// <summary>Przeskanowanie folderu bazy. Wołane samo po reloadzie domeny i po zmianach w folderze.</summary>
        [ContextMenu("Rescan")]
        public void Rescan()
        {
            _map = null;

            var assetPath = AssetDatabase.GetAssetPath(this);

            // Instancja nie zapisana jako asset nie ma swojego folderu, więc nie ma czego skanować.
            if (string.IsNullOrEmpty(assetPath)) return;

            var folder = ToUnityPath(Path.GetDirectoryName(assetPath));

            var icons = AssetDatabase.FindAssets("t:Texture2D", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(path => CreateIcon(path, folder))
                .Where(icon => icon.texture)
                .ToList();

            // Asset brudzimy tylko gdy lista faktycznie się zmieniła — inaczej robiłby się szum w gicie.
            if (icons.SequenceEqual(_icons)) return;

            _icons = icons;
            EditorUtility.SetDirty(this);
        }

        #endregion

        #region Scanning

        private static Icon CreateIcon(string assetPath, string folder)
        {
            var relative = assetPath[(folder.Length + 1)..];
            var extension = Path.GetExtension(relative);

            return new Icon {
                path = relative[..^extension.Length],
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath),
                sprite = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().FirstOrDefault()
            };
        }

        [InitializeOnLoadMethod]
        private static void OnDomainReload() => ScheduleRescan();

        private static void ScheduleRescan()
        {
            if (_rescanScheduled) return;

            _rescanScheduled = true;

            // delayCall, żeby nie skanować w trakcie importu ani przed gotowością AssetDatabase.
            EditorApplication.delayCall += () => {
                _rescanScheduled = false;
                RescanAll();
            };
        }

        private static void RescanAll()
        {
            _databases = null;

            foreach (var database in Databases)
            {
                database.Rescan();
                AssetDatabase.SaveAssetIfDirty(database);
            }
        }

        private class Postprocessor : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
                string[] movedAssets, string[] movedFromAssetPaths)
            {
                // Tani filtr po rozszerzeniu — .asset bo mogła powstać nowa baza.
                var relevant = importedAssets.Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths)
                    .Any(IsRelevant);

                if (!relevant) return;

                _databases = null;
                ScheduleRescan();
            }

            private static bool IsRelevant(string path) =>
                path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) ||
                IMAGE_EXTENSIONS.Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Lookup

        private static EditorIconsDatabase[] Databases => _databases ??= AssetDatabase
            .FindAssets($"t:{nameof(EditorIconsDatabase)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(AssetDatabase.LoadAssetAtPath<EditorIconsDatabase>)
            .Where(database => database)
            .ToArray();

        /// <summary>Bazy odpytywane po kolei wg ścieżki assetu — wygrywa pierwsze trafienie.</summary>
        private static bool TryGetEntry(string nameOrPath, out Icon icon)
        {
            var key = ToUnityPath(nameOrPath).Trim('/');

            foreach (var database in Databases)
                if (database.Map.TryGetValue(key, out icon))
                    return true;

            icon = default;
            return false;
        }

        private Dictionary<string, Icon> Map
        {
            get
            {
                if (_map != null) return _map;

                _map = new Dictionary<string, Icon>(_icons.Count * 2, StringComparer.Ordinal);

                foreach (var icon in _icons)
                    if (icon.texture)
                        _map[icon.path] = icon;

                // Nazwa pliku jako skrót — tylko gdy jednoznaczna, inaczej zostaje sama ścieżka.
                foreach (var group in _icons.Where(icon => icon.texture).GroupBy(icon => Path.GetFileName(icon.path)))
                {
                    if (_map.ContainsKey(group.Key)) continue;

                    var duplicates = group.ToArray();

                    if (duplicates.Length > 1)
                    {
                        Debug.LogWarning($"[{nameof(EditorIconsDatabase)}] Icon name '{group.Key}' is not unique in " +
                                         $"{AssetDatabase.GetAssetPath(this)} - use the relative path instead. " +
                                         $"Found: {string.Join(", ", duplicates.Select(icon => icon.path))}.", this);
                        continue;
                    }

                    _map[group.Key] = duplicates[0];
                }

                return _map;
            }
        }

        private static string DatabasesInfo() => Databases.Length == 0
            ? "no database asset in the project"
            : string.Join(", ", Databases.Select(AssetDatabase.GetAssetPath));

        private static string ToUnityPath(string path) => path?.Replace('\\', '/') ?? string.Empty;

        #endregion
    }
}
