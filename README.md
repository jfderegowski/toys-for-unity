# Toys for Unity

Collection of useful toys for Unity: extension methods, inspector attributes and
UI Toolkit elements.

Requires Unity 6000.0 or newer.

## Installation

In Unity: **Window → Package Manager → + → Install package from git URL**, then
paste:

```
https://github.com/jfderegowski/toys-for-unity.git#v0.0.1
```

Or add it directly to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.fefek5.toys-for-unity": "https://github.com/jfderegowski/toys-for-unity.git#v0.0.1"
  }
}
```

Always pin a tag with `#vX.Y.Z`. Without it the Package Manager resolves whatever
`main` points at, which changes under you.

## Updating

The Package Manager does not check for updates to packages installed from a git
URL — it records the resolved commit hash in `Packages/packages-lock.json` and
reuses it from then on. To move to a newer version, bump the tag in
`manifest.json`:

```diff
- "com.fefek5.toys-for-unity": "https://github.com/jfderegowski/toys-for-unity.git#v0.0.1"
+ "com.fefek5.toys-for-unity": "https://github.com/jfderegowski/toys-for-unity.git#v0.1.0"
```

See [CHANGELOG.md](CHANGELOG.md) for what changed between versions.

## Contents

| Area | What is in it |
| --- | --- |
| `Runtime/Extensions` | Extension methods for common Unity and BCL types, including UI Toolkit's `VisualElement` and `UQueryBuilder`. |
| `Runtime/Attributes` | `InspectorButtonAttribute`, `SerializeReferenceListAttribute`. |
| `Runtime/HasValue` | `HasValue<T>` optional-value type. |
| `Editor/Drawers` | Property drawers backing the runtime attributes. |
| `Editor/VisualElements` | `Button`, `FoldoutElement`, `InspectorButtonElement`, `SerializeReferenceListElement`. |
| `Editor/Icons` | `EditorIconsDatabase` for built-in editor icon lookup. |

## License

[LGPL-3.0-only](LICENSE.md)
