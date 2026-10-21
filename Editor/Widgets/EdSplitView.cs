using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    public enum EdSplitMode
    {
        Auto,
        SideBySide,
        Stacked,
    }

    /// <summary>
    /// Two independent scroll panes with a draggable splitter ("phone split screen").
    /// Use it for content the user must compare; tabs hide one side.
    /// Keep it as a [SerializeField] on the window so ratio and mode survive reloads.
    /// <code>
    /// [SerializeField] EdSplitView _split = new();
    /// protected override void OnToolbarGUI() { _split.OnToolbarGUI(); ... }
    /// protected override void OnBodyGUI() { _split.Draw(this, DrawLeft, DrawRight); }
    /// </code>
    /// </summary>
    [Serializable]
    public sealed class EdSplitView
    {
        const float SplitterSize = 5f;
        const float MinPane = 120f;

        [SerializeField] EdSplitMode _mode;
        [SerializeField] float _ratio = 0.5f;
        [SerializeField] float _sideBySideMinWidth = 700f;

        Vector2 _firstScroll;
        Vector2 _secondScroll;

        public EdSplitMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        /// <summary>Auto mode goes side by side from this window width, stacked below it.</summary>
        public float SideBySideMinWidth
        {
            get => _sideBySideMinWidth;
            set => _sideBySideMinWidth = value;
        }

        /// <summary>Mode popup for the window toolbar.</summary>
        public void OnToolbarGUI(float width = 85f)
        {
            _mode = (EdSplitMode)EditorGUILayout.EnumPopup(_mode, EditorStyles.toolbarPopup, GUILayout.Width(width));
        }

        /// <summary>
        /// Draws both panes filling an <see cref="EdWindowBase"/> body (sized from the window so the
        /// base scroll view never scrolls). Pass null for a side to give the other the whole body.
        /// </summary>
        public void Draw(EditorWindow window, Action first, Action second)
        {
            if (first == null || second == null)
            {
                Action only = first ?? second;
                only?.Invoke();
                return;
            }

            float width = window.position.width - 6f;
            float height = window.position.height - EditorStyles.toolbar.fixedHeight - 6f;
            bool sideBySide = _mode == EdSplitMode.SideBySide || (_mode == EdSplitMode.Auto && width >= _sideBySideMinWidth);
            float total = (sideBySide ? width : height) - SplitterSize;
            float firstSize = Mathf.Clamp(total * _ratio, MinPane, Mathf.Max(MinPane, total - MinPane));

            if (sideBySide)
            {
                using (new EditorGUILayout.HorizontalScope(GUILayout.Height(height)))
                {
                    Pane(ref _firstScroll, first, GUILayout.Width(firstSize), GUILayout.Height(height));
                    Splitter(GUILayoutUtility.GetRect(SplitterSize, height, GUILayout.Width(SplitterSize)), true, total, window);
                    Pane(ref _secondScroll, second, GUILayout.Width(total - firstSize), GUILayout.Height(height));
                }
            }
            else
            {
                Pane(ref _firstScroll, first, GUILayout.Width(width), GUILayout.Height(firstSize));
                Splitter(GUILayoutUtility.GetRect(width, SplitterSize, GUILayout.Height(SplitterSize)), false, total, window);
                Pane(ref _secondScroll, second, GUILayout.Width(width), GUILayout.Height(total - firstSize));
            }
        }

        static void Pane(ref Vector2 scroll, Action draw, params GUILayoutOption[] size)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll, size);
            draw();
            EditorGUILayout.EndScrollView();
        }

        // hotControl keeps the drag alive when the mouse leaves the thin bar.
        void Splitter(Rect rect, bool vertical, float total, EditorWindow window)
        {
            Event e = Event.current;
            int id = GUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.AddCursorRect(rect, vertical ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical);

            switch (e.GetTypeForControl(id))
            {
                case EventType.Repaint:
                    EditorGUI.DrawRect(rect, EdTheme.Separator);
                    break;

                case EventType.MouseDown when e.button == 0 && rect.Contains(e.mousePosition):
                    GUIUtility.hotControl = id;
                    e.Use();
                    break;

                case EventType.MouseDrag when GUIUtility.hotControl == id:
                    float delta = vertical ? e.delta.x : e.delta.y;
                    _ratio = Mathf.Clamp01(_ratio + delta / Mathf.Max(1f, total));
                    e.Use();
                    window.Repaint();
                    break;

                case EventType.MouseUp when GUIUtility.hotControl == id:
                    GUIUtility.hotControl = 0;
                    e.Use();
                    break;
            }
        }
    }
}
