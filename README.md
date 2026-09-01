# UnityEditorHomeMade

Reusable Unity Editor UI core (IMGUI). One consistent standard for every tool window:
three-zone layout with auto-scrolling body, semantic-colored buttons (danger = red,
confirm = green), collapsible sections that remember their state, and a toolbar search field.

Editor-only, zero dependencies on game code. Works on Unity 2021.3+.

## Install in another project

Currently this lives in `Assets/UnityEditorHomeMade` for easy iteration. To reuse elsewhere, pick one:

- **Copy folder**: copy this folder anywhere under the target project's `Assets/` (or into `Packages/` as an embedded package - `package.json` is already set up for that). Done.
- **Git URL**: push this folder to its own git repo, then in the target project's `Packages/manifest.json`:

```json
"com.unityeditorhomemade.core": "https://github.com/<you>/unity-editor-homemade.git"
```

- **Local path**: `"com.unityeditorhomemade.core": "file:../../shared/com.unityeditorhomemade.core"`

## Quick start

Open `Tools > HomeMade > UI Demo` to see everything live. A minimal window:

```csharp
using UnityEditor;
using UnityEngine;
using UnityEditorHomeMade;

public sealed class MyToolWindow : EdWindowBase
{
    [SerializeField] EdSearchField _search = new();

    [MenuItem("Tools/Game/My Tool")]
    static void Open()
    {
        var w = GetWindow<MyToolWindow>("My Tool");
        w.minSize = new Vector2(420, 360);
    }

    protected override void OnToolbarGUI()
    {
        if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(60)))
            Load();
        GUILayout.FlexibleSpace();
        _search.OnToolbarGUI(200f);   // mandatory for lists > ~50 items
    }

    protected override void OnBodyGUI()  // already inside a scroll view
    {
        using (var s = new EdSection("Entries", "mytool.entries", rightText: $"{_rows.Count} rows"))
        {
            if (!s.Expanded) return;
            foreach (var row in _rows)
            {
                if (!_search.Matches(row.Name)) continue;
                DrawRow(row);            // end rows with EdGUI.XButton() to delete
            }
        }
    }

    protected override void OnFooterGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (EdGUI.ConfirmButton("Save", GUILayout.Width(90)))
            {
                Save();
                IsDirty = false;
                SetStatus("Saved.");
            }
        }
    }
}
```

## API overview

| Type | Purpose |
|---|---|
| `EdWindowBase` | Base `EditorWindow`: toolbar / scrolling body / status bar / footer, `IsDirty` title flag, `SetStatus()` |
| `EdGUI` | `ConfirmButton` (green), `DangerButton` (red), `WarningButton`, `DangerButtonWithConfirm` (dialog), `XButton` (inline row delete), `CloseButton(Rect)`, `Header`, `HorizontalLine`, `BackgroundColorScope` |
| `EdSection` | Collapsible header bar ("folder"); whole header is clickable; state persists across restarts |
| `EdSearchField` | Toolbar search with clear button; `Matches(text)` for case-insensitive filtering |
| `EdTheme` | Semantic palette (Danger/Confirm/Warning), auto-tuned for dark & light skins |
| `EdStyles` | Lazily-created shared `GUIStyle`s (safe to touch only inside OnGUI) |
| `EdPersist` | Prefixed `EditorPrefs`/`SessionState` wrappers for tool state |

## The rules baked into this package

1. **Layout**: toolbar (fixed) → body (always in a scroll view) → status bar → footer (fixed). Primary actions live in toolbar/footer, never inside the scrolled body.
2. **Color = meaning**: red for destructive/close, green for confirm/save/add, yellow for risky. Never recolor buttons ad hoc.
3. **Irreversible destructive actions ask first** (`DangerButtonWithConfirm`). Single undoable deletes don't.
4. **Search is mandatory for big lists** (> ~50 items): `EdSearchField` in the toolbar, filter with `Matches`.
5. **Sections remember state**; don't nest deeper than 2 levels.
6. **Status bar over Debug.Log** for operation results (`SetStatus`).
7. **Dirty flag on unsaved edits** (`IsDirty` → `*` in tab title).
8. **No heavy work in OnGUI**: cache, recompute on button press / `EndChangeCheck` only.
9. Every icon-only button gets a tooltip; disabled beats hidden (`EditorGUI.DisabledScope` + tooltip).
10. Keep menu paths under one root, e.g. `Tools/Game/...`.
