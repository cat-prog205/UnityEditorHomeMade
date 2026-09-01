using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Collapsible "folder" section with a clickable header bar. The whole header toggles
    /// expansion (not just the arrow), and the expanded state persists across editor
    /// restarts via <see cref="EdPersist"/>.
    ///
    /// Usage:
    /// <code>
    /// using (var section = new EdSection("Rewards", "mytool.rewards", rightText: $"{list.Count} items"))
    /// {
    ///     if (section.Expanded)
    ///         DrawRewards();
    /// }
    /// </code>
    /// </summary>
    public sealed class EdSection : IDisposable
    {
        const float HeaderHeight = 22f;
        const float ArrowSize = 14f;

        /// <summary>True when the section body should be drawn this frame.</summary>
        public bool Expanded { get; }

        public EdSection(string title, string persistKey = null, string rightText = null, bool defaultExpanded = true)
        {
            var key = "Section." + (string.IsNullOrEmpty(persistKey) ? title : persistKey);
            var expanded = EdPersist.GetBool(key, defaultExpanded);

            var rect = GUILayoutUtility.GetRect(0f, HeaderHeight, GUILayout.ExpandWidth(true));
            var e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                expanded = !expanded;
                EdPersist.SetBool(key, expanded);
                e.Use();
                GUI.changed = true;
            }

            if (e.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, EdTheme.SectionHeaderBg);

                var arrowRect = new Rect(rect.x + 4f, rect.y + (rect.height - ArrowSize) * 0.5f, ArrowSize, ArrowSize);
                EditorStyles.foldout.Draw(arrowRect, false, false, expanded, false);

                var labelRect = new Rect(rect.x + 22f, rect.y, rect.width - 22f, rect.height);
                EdStyles.SectionHeaderLabel.Draw(labelRect, EdGUI.TempContent(title), false, false, false, false);

                if (!string.IsNullOrEmpty(rightText))
                {
                    var rightRect = new Rect(rect.x, rect.y, rect.width - 8f, rect.height);
                    EdStyles.SectionHeaderRight.Draw(rightRect, EdGUI.TempContent(rightText), false, false, false, false);
                }
            }

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            Expanded = expanded;
            if (Expanded)
                EditorGUILayout.BeginVertical(EdStyles.SectionBody);
        }

        public void Dispose()
        {
            if (Expanded)
                EditorGUILayout.EndVertical();
            GUILayout.Space(3f);
        }
    }
}
