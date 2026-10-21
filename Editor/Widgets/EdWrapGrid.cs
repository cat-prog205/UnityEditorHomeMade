using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Selection grid (filters, categories) that wraps into rows to fit its width and never
    /// overflows a narrow pane. Columns come from the width measured at the last Repaint, so
    /// Layout and Repaint agree on the height. Keep one instance per grid as a window field.
    /// </summary>
    public sealed class EdWrapGrid
    {
        float _width = 300f;

        public int Draw(int selected, string[] labels, float minButtonWidth = 80f, GUIStyle style = null)
        {
            style ??= EditorStyles.miniButton;
            int columns = Mathf.Clamp(Mathf.FloorToInt(_width / minButtonWidth), 1, Mathf.Max(1, labels.Length));
            int rows = (labels.Length + columns - 1) / columns;
            Rect rect = GUILayoutUtility.GetRect(0f, rows * (EditorGUIUtility.singleLineHeight + 2f), GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint && rect.width > 1f && !Mathf.Approximately(rect.width, _width))
            {
                _width = rect.width;
                HandleUtility.Repaint();
            }

            return GUI.SelectionGrid(rect, selected, labels, columns, style);
        }
    }
}
