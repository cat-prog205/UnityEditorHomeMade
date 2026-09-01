using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorHomeMade;

namespace UnityEditorHomeMade.Samples
{
    /// <summary>
    /// Living showcase of every UnityEditorHomeMade control. Open via Tools > HomeMade > UI Demo.
    /// Copy patterns from here when building new tool windows.
    /// </summary>
    [ToolEntry("UI Demo", category: "HomeMade", keywords: "sample buttons search section")]
    public sealed class HomeMadeUIDemoWindow : EdWindowBase
    {
        [SerializeField] EdSearchField _search = new();
        [SerializeField] List<string> _items = new();

        [MenuItem("Tools/HomeMade/UI Demo")]
        static void Open()
        {
            var window = GetWindow<HomeMadeUIDemoWindow>("HomeMade UI");
            window.minSize = new Vector2(420f, 420f);
            window.Show();
        }

        void OnEnable()
        {
            if (_items.Count == 0)
                RebuildItems();
        }

        protected override void OnToolbarGUI()
        {
            if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(60f)))
            {
                RebuildItems();
                SetStatus($"Reloaded {_items.Count} items.");
            }

            GUILayout.FlexibleSpace();
            _search.OnToolbarGUI(220f);
        }

        protected override void OnBodyGUI()
        {
            using (var section = new EdSection("Semantic Buttons", "demo.buttons"))
            {
                if (section.Expanded)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (EdGUI.ConfirmButton("Confirm", GUILayout.Width(90f)))
                            SetStatus("Confirmed.", MessageType.Info);

                        if (EdGUI.WarningButton("Overwrite", GUILayout.Width(90f)))
                            SetStatus("Overwrite pressed.", MessageType.Warning);

                        if (EdGUI.DangerButton("Delete", GUILayout.Width(90f)))
                            SetStatus("Deleted (single item, undoable - no dialog needed).", MessageType.Warning);
                    }

                    EditorGUILayout.Space(2f);

                    if (EdGUI.DangerButtonWithConfirm(
                            "Clear All (asks first)",
                            "This clears every item and cannot be undone. Continue?",
                            "Clear All Items",
                            GUILayout.Width(160f)))
                    {
                        _items.Clear();
                        IsDirty = true;
                        SetStatus("All items cleared.", MessageType.Warning);
                    }
                }
            }

            using (var section = new EdSection("Items", "demo.items", rightText: $"{_items.Count} items"))
            {
                if (section.Expanded)
                    DrawItems();
            }

            using (var section = new EdSection("Misc", "demo.misc", defaultExpanded: false))
            {
                if (section.Expanded)
                {
                    EdGUI.Header("Header helper");
                    EditorGUILayout.LabelField("A separator line follows.");
                    EdGUI.HorizontalLine();
                    EditorGUILayout.LabelField("Sections remember expansion across editor restarts.");
                }
            }
        }

        protected override void OnFooterGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (EdGUI.ConfirmButton("Apply", GUILayout.Width(90f)))
                {
                    IsDirty = false;
                    SetStatus("Applied. Dirty flag cleared - note the tab title.");
                }

                if (EdGUI.DangerButton("Close", GUILayout.Width(90f)))
                    Close();
            }

            EditorGUILayout.Space(2f);
        }

        void DrawItems()
        {
            var shown = 0;
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                if (!_search.Matches(_items[i]))
                    continue;
                shown++;

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(_items[i]);
                    if (EdGUI.XButton($"Remove {_items[i]}"))
                    {
                        _items.RemoveAt(i);
                        IsDirty = true;
                    }
                }
            }

            if (shown == 0)
            {
                EditorGUILayout.LabelField(
                    _search.HasQuery ? $"No items match \"{_search.Query}\"." : "List is empty. Press Reload.",
                    EdStyles.CenteredMiniLabel);
            }
        }

        void RebuildItems()
        {
            _items.Clear();
            string[] kinds = { "Sword", "Shield", "Potion", "Ring", "Helmet", "Scroll", "Gem", "Boots" };
            for (var i = 0; i < 100; i++)
                _items.Add($"{kinds[i % kinds.Length]}_{i:D3}");
            IsDirty = false;
        }
    }
}
