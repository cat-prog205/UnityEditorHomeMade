using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Searchable launcher for HomeMade / project tools.
    /// Concrete <see cref="EdWindowBase"/> windows appear automatically;
    /// add <see cref="ToolEntryAttribute"/> for titles, categories, or non-window actions.
    /// </summary>
    public sealed class ToolDashboardWindow : EdWindowBase
    {
        const string SearchControlName = "UEHM.Dashboard.Search";

        [SerializeField] EdSearchField _search = new();
        [SerializeField] int _selected;

        readonly List<int> _filtered = new();
        bool _focusSearch = true;

        [MenuItem("Tools/HomeMade/Dashboard %#o", false, -100)]
        public static void Open()
        {
            var window = GetWindow<ToolDashboardWindow>(utility: true, title: "Tool Dashboard");
            window.minSize = new Vector2(380f, 420f);
            window.Show();
            window.Focus();
        }

        void OnEnable()
        {
            _focusSearch = true;
            ToolCatalog.Rebuild();
        }

        protected override void OnToolbarGUI()
        {
            GUILayout.Label("Tools", EditorStyles.miniLabel, GUILayout.Width(40f));
            GUI.SetNextControlName(SearchControlName);
            if (_search.OnToolbarGUI())
                _selected = 0;

            if (_focusSearch && UnityEngine.Event.current.type == UnityEngine.EventType.Repaint)
            {
                EditorGUI.FocusTextInControl(SearchControlName);
                _focusSearch = false;
            }
        }

        protected override void OnBodyGUI()
        {
            HandleKeyboard();
            RebuildFiltered();

            if (_filtered.Count == 0)
            {
                EditorGUILayout.LabelField(
                    _search.HasQuery ? $"No tools match \"{_search.Query}\"." : "No tools registered.",
                    EdStyles.CenteredMiniLabel);
                return;
            }

            string currentCategory = null;
            for (var i = 0; i < _filtered.Count; i++)
            {
                var entry = ToolCatalog.Entries[_filtered[i]];
                if (entry.Category != currentCategory)
                {
                    currentCategory = entry.Category;
                    EdGUI.Header(currentCategory);
                }

                DrawRow(i, entry);
            }
        }

        protected override void OnFooterGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    $"{_filtered.Count} / {ToolCatalog.Entries.Count}   Enter to open   Esc to close",
                    EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                if (EdGUI.DangerButton("Close", GUILayout.Width(90f)))
                    Close();
            }

            EditorGUILayout.Space(2f);
        }

        void DrawRow(int filteredIndex, ToolCatalog.Entry entry)
        {
            var selected = filteredIndex == _selected;

            using (new EditorGUILayout.HorizontalScope())
            {
                var prev = GUI.backgroundColor;
                if (selected)
                    GUI.backgroundColor = new Color(0.45f, 0.7f, 1f, 1f);

                var pressed = GUILayout.Button(entry.Title, EditorStyles.miniButton);
                GUI.backgroundColor = prev;

                if (!string.IsNullOrEmpty(entry.MenuPath))
                    GUILayout.Label(ShortMenu(entry.MenuPath), EditorStyles.miniLabel, GUILayout.Width(140f));

                if (pressed)
                    OpenFiltered(filteredIndex);
            }
        }

        void HandleKeyboard()
        {
            var e = UnityEngine.Event.current;
            if (e.type != UnityEngine.EventType.KeyDown)
                return;

            if (e.keyCode == KeyCode.Escape)
            {
                e.Use();
                Close();
                return;
            }

            if (_filtered.Count == 0)
                return;

            if (e.keyCode == KeyCode.DownArrow)
            {
                _selected = Mathf.Min(_selected + 1, _filtered.Count - 1);
                e.Use();
                Repaint();
            }
            else if (e.keyCode == KeyCode.UpArrow)
            {
                _selected = Mathf.Max(_selected - 1, 0);
                e.Use();
                Repaint();
            }
            else if (e.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
            {
                OpenFiltered(_selected);
                e.Use();
            }
        }

        void RebuildFiltered()
        {
            _filtered.Clear();
            var all = ToolCatalog.Entries;
            for (var i = 0; i < all.Count; i++)
            {
                var entry = all[i];
                if (_search.MatchesAny(entry.Title, entry.Category, entry.Keywords, entry.MenuPath))
                    _filtered.Add(i);
            }

            if (_selected >= _filtered.Count)
                _selected = Mathf.Max(0, _filtered.Count - 1);
        }

        void OpenFiltered(int filteredIndex)
        {
            if (filteredIndex < 0 || filteredIndex >= _filtered.Count)
                return;

            ToolCatalog.Open(ToolCatalog.Entries[_filtered[filteredIndex]]);
            Close();
        }

        static string ShortMenu(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            var slash = path.LastIndexOf('/');
            return slash >= 0 ? path.Substring(slash + 1) : path;
        }
    }
}
