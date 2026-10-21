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
| `EdSplitView` | Two scroll panes + draggable splitter, Auto / SideBySide / Stacked (phone-style split); for content the user compares |
| `EdWrapGrid` | Selection grid that wraps into rows to fit a narrow pane |
| `EdGUI.Row` / `TakeRight` / `ClippedFoldout` / `ClippedLabel` | Fixed one-line rows: buttons cut from the right, title clipped with full text as tooltip |
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
11. **Narrow-friendly**: a tool must stay usable at ~300px. Text left, buttons right; never put word-wrapped labels inside a horizontal group (rows overlap) — use `EdGUI.Row` + `TakeRight` + clipped text with tooltip. Wrap text only in vertical flow (`EdStyles.Wrap*`). Button grids use `EdWrapGrid`.
12. **Compare side by side, not in tabs** (`EdSplitView`). Global actions (pause, refresh) are small toolbar items, not tabs.
13. **No fixed width on text buttons** in the toolbar (labels get clipped); fixed widths only for icons/symbols.
14. **Show the common case without clicking**: key details inline under a row, full detail on selection/foldout; filters such as "only modified".
15. **Never mutate a list while drawing it**: queue the action and run it after the draw, then `Repaint()`.
