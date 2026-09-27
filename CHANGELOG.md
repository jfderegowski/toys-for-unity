# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `SelectTypeElement`, the foldout with a type dropdown that `SelectTypeDrawer` draws, usable on
  its own in custom inspectors.
- `TypeDropdownField`, a dropdown of the given types that only reports the pick, and
  `TypeDropdownField.ShowMenu` with the same types as a menu for add buttons. `SelectTypeElement`
  and `SerializeReferenceListElement` use them.
- `SubAssetListElement`, a list of ScriptableObject sub-assets of the asset holding it, each a
  foldout with its inspector. The add button creates a sub-asset of the picked type, remove asks
  first, reordering is saved, and `ItemInfo` shows extra text in each header.
- `FolderPathAttribute` and `FilePathAttribute` (both deriving from `PathAttribute`) for string
  fields: a text field with a browse button that also takes a path dropped from the Project
  window or the file explorer, and shows a missing path in yellow. Paths inside the project are
  stored from the project folder unless `Relative = false`, or from another folder with
  `Root = PathRoot.PersistentData`, `StreamingAssets` or `Custom`; `FilePathAttribute` takes extensions.
- `PathField`, the abstract element behind them, with `FolderPathField` and `FilePathField`,
  usable on their own in custom inspectors and windows.
- `FolderPath` and `FilePath` structs holding a path and the `PathRoot` it starts from, set in the
  constructor (`new FilePath("Stats.json", PathRoot.PersistentData)`, or a custom folder) and
  drawn as a foldout with the full path in its header and the root and path inside. They have `FullPath`, `Exists`, `Name` and implicit
  conversions to and from `string`; the attributes are only needed to limit file extensions.
  `PathRootExtensions` resolves a plain string the same way.

## [0.1.0] - 2026-09-21

### Added

- `SelectTypeAttribute` with its drawer: a foldout for a `[SerializeReference]` field with a
  dropdown of every concrete type assignable to it, which creates the picked type in place of
  the current value. On a list or an array it draws each element.
- `TypeExtensions.GetAssignableTypes` and `TypeExtensions.GetDisplayName`, shared by
  `SelectTypeDrawer` and `SerializeReferenceListElement`.
- `ConvertToColor` and `ConvertToColorHex` string extensions, which pick a color from the
  string's hash code.

## [0.0.1] - 2026-09-20

### Added

- Extension methods for `Action`, `AsyncOperation`, `Camera`, `Color`, `Enumerable`,
  `Enumerator`, `FileInfo`, `GameObject`, `LayerMask`, `List`, `Mathf`, numbers,
  `Quaternion`, `Renderer`, `string`, `Task`, `Transform`, `Vector2`, `Vector3`
  and vector conversions.
- UI Toolkit extensions: `VisualElementExtensions` and `UQueryBuilderExtensions`.
- `InspectorButtonAttribute` with its drawer and `InspectorButtonElement`, for
  invoking methods straight from the inspector.
- `SerializeReferenceListAttribute` with its drawer and
  `SerializeReferenceListElement`.
- `HasValue<T>` optional-value type with a custom property drawer.
- Editor visual elements: `Button`, `FoldoutElement`.
- `MonoBehaviourEditor` and serialized property extensions.
- `EditorIconsDatabase` for built-in editor icon lookup.

[Unreleased]: https://github.com/jfderegowski/toys-for-unity/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/jfderegowski/toys-for-unity/compare/v0.0.1...v0.1.0
[0.0.1]: https://github.com/jfderegowski/toys-for-unity/releases/tag/v0.0.1
