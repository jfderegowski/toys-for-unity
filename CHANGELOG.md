# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
