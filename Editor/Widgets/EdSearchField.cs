using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Toolbar search field with a clear (x) button. Mark the owning field [SerializeField]
    /// on an EditorWindow so the query survives domain reloads.
    ///
    /// Rule of thumb: any list over ~50 items must be searchable. Draw this in the toolbar
    /// and filter rows with <see cref="Matches(string)"/>.
    ///
    /// Usage:
    /// <code>
    /// [SerializeField] EdSearchField _search = new();
    ///
    /// // in toolbar:
    /// _search.OnToolbarGUI(200f);
    ///
    /// // when drawing rows:
    /// if (!_search.Matches(row.Name)) continue;
    /// </code>
    /// </summary>
    [Serializable]
    public sealed class EdSearchField
    {
        [SerializeField] string _query = string.Empty;

        public string Query
        {
            get => _query;
            set => _query = value ?? string.Empty;
        }

        public bool HasQuery => !string.IsNullOrEmpty(_query);

        /// <summary>Draws the field (toolbar style). Returns true if the query changed this frame.</summary>
        public bool OnToolbarGUI(float width = 0f)
        {
            EditorGUI.BeginChangeCheck();
            _query = width > 0f
                ? GUILayout.TextField(_query, EdStyles.ToolbarSearchField, GUILayout.Width(width))
                : GUILayout.TextField(_query, EdStyles.ToolbarSearchField, GUILayout.MinWidth(80f), GUILayout.ExpandWidth(true));
            var changed = EditorGUI.EndChangeCheck();

            if (HasQuery)
            {
                var label = EdStyles.ToolbarSearchCancelIsFallback ? "\u00d7" : string.Empty;
                if (GUILayout.Button(label, EdStyles.ToolbarSearchCancel))
                {
                    Clear();
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>Case-insensitive contains. An empty query matches everything.</summary>
        public bool Matches(string text)
        {
            if (!HasQuery)
                return true;
            return !string.IsNullOrEmpty(text)
                   && text.IndexOf(_query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>True if any of the texts match the query.</summary>
        public bool MatchesAny(params string[] texts)
        {
            if (!HasQuery)
                return true;
            if (texts == null)
                return false;
            for (var i = 0; i < texts.Length; i++)
            {
                if (Matches(texts[i]))
                    return true;
            }

            return false;
        }

        public void Clear()
        {
            _query = string.Empty;
            GUI.FocusControl(null);
        }
    }
}
