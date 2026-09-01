using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Standardized IMGUI controls. Button colors follow <see cref="EdTheme"/> semantics:
    /// use <see cref="DangerButton(string, GUILayoutOption[])"/> for destructive/close actions,
    /// <see cref="ConfirmButton(string, GUILayoutOption[])"/> for save/apply/add actions.
    /// </summary>
    public static class EdGUI
    {
        // ---------------------------------------------------------------- Buttons

        /// <summary>Red button for destructive or exit actions (Delete, Close, Clear).</summary>
        public static bool DangerButton(string label, params GUILayoutOption[] options)
            => ColoredButton(TempContent(label), EdTheme.Danger, options);

        public static bool DangerButton(GUIContent content, params GUILayoutOption[] options)
            => ColoredButton(content, EdTheme.Danger, options);

        /// <summary>Green button for positive actions (Confirm, Save, Add, Apply).</summary>
        public static bool ConfirmButton(string label, params GUILayoutOption[] options)
            => ColoredButton(TempContent(label), EdTheme.Confirm, options);

        public static bool ConfirmButton(GUIContent content, params GUILayoutOption[] options)
            => ColoredButton(content, EdTheme.Confirm, options);

        /// <summary>Yellow button for risky-but-recoverable actions (Force Rebuild, Overwrite).</summary>
        public static bool WarningButton(string label, params GUILayoutOption[] options)
            => ColoredButton(TempContent(label), EdTheme.Warning, options);

        public static bool WarningButton(GUIContent content, params GUILayoutOption[] options)
            => ColoredButton(content, EdTheme.Warning, options);

        /// <summary>
        /// Red button that opens a confirmation dialog. Returns true only when the user
        /// confirmed. Use for destructive actions that cannot be undone.
        /// </summary>
        public static bool DangerButtonWithConfirm(
            string label,
            string dialogMessage,
            string dialogTitle = "Are you sure?",
            params GUILayoutOption[] options)
        {
            if (!DangerButton(label, options))
                return false;
            return EditorUtility.DisplayDialog(dialogTitle, dialogMessage, "Yes", "Cancel");
        }

        /// <summary>Small fixed-width red "X" button for inline row deletion. Always 24px wide.</summary>
        public static bool XButton(string tooltip = "Remove", float width = 24f)
        {
            using (new BackgroundColorScope(EdTheme.Danger))
                return GUILayout.Button(TempContent("\u00d7", tooltip), GUILayout.Width(width));
        }

        /// <summary>Rect-based red close button, for absolute-positioned layouts (e.g. top-right corner).</summary>
        public static bool CloseButton(Rect rect, string tooltip = "Close")
        {
            using (new BackgroundColorScope(EdTheme.Danger))
                return GUI.Button(rect, TempContent("\u00d7", tooltip));
        }

        static bool ColoredButton(GUIContent content, Color color, params GUILayoutOption[] options)
        {
            using (new BackgroundColorScope(color))
                return GUILayout.Button(content, options);
        }

        // ---------------------------------------------------------------- Misc controls

        /// <summary>Bold section-less header label.</summary>
        public static void Header(string title)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        /// <summary>Thin horizontal separator line.</summary>
        public static void HorizontalLine(float height = 1f, float verticalMargin = 4f)
        {
            GUILayout.Space(verticalMargin);
            var rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, EdTheme.Separator);
            GUILayout.Space(verticalMargin);
        }

        static readonly GUIContent SharedContent = new();

        /// <summary>Reusable GUIContent to avoid per-frame allocations in OnGUI.</summary>
        public static GUIContent TempContent(string text, string tooltip = null)
        {
            SharedContent.text = text;
            SharedContent.tooltip = tooltip;
            SharedContent.image = null;
            return SharedContent;
        }

        // ---------------------------------------------------------------- Scopes

        /// <summary>Sets GUI.backgroundColor for a block and always restores the previous value.</summary>
        public readonly struct BackgroundColorScope : IDisposable
        {
            readonly Color _previous;

            public BackgroundColorScope(Color color)
            {
                _previous = GUI.backgroundColor;
                GUI.backgroundColor = color;
            }

            public void Dispose() => GUI.backgroundColor = _previous;
        }

        /// <summary>Sets GUI.contentColor for a block and always restores the previous value.</summary>
        public readonly struct ContentColorScope : IDisposable
        {
            readonly Color _previous;

            public ContentColorScope(Color color)
            {
                _previous = GUI.contentColor;
                GUI.contentColor = color;
            }

            public void Dispose() => GUI.contentColor = _previous;
        }
    }
}
