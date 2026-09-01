using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Base class for tool windows enforcing the standard three-zone layout:
    ///
    ///   [ Toolbar   ]  - fixed top, never scrolls          -> override OnToolbarGUI
    ///   [ Body      ]  - auto-scrolls when content grows   -> override OnBodyGUI (required)
    ///   [ StatusBar ]  - shown via SetStatus, dismissible
    ///   [ Footer    ]  - fixed bottom, never scrolls       -> override OnFooterGUI
    ///
    /// Concrete subclasses are listed automatically in Tools &gt; HomeMade &gt; Dashboard
    /// (Ctrl+Shift+O). Add <see cref="ToolEntryAttribute"/> on the class to set title,
    /// category, or <c>Hidden = true</c>.
    ///
    /// Do NOT declare OnGUI in subclasses; it would replace this layout entirely.
    /// Set <see cref="IsDirty"/> when the user has unsaved edits: the tab title gets a "*"
    /// suffix automatically.
    /// Use SetStatus instead of Debug.Log for operation results so the user sees feedback
    /// inside the window.
    /// </summary>
    public abstract class EdWindowBase : EditorWindow
    {
        [SerializeField] Vector2 _bodyScroll;

        string _statusMessage = string.Empty;
        MessageType _statusType = MessageType.Info;
        string _baseTitle;
        bool _isDirty;

        /// <summary>Override and return false for windows that need no top toolbar.</summary>
        protected virtual bool ShowToolbar => true;

        /// <summary>Unsaved-changes flag; adds/removes a "*" suffix on the tab title.</summary>
        protected bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty == value)
                    return;
                _isDirty = value;
                UpdateTitleSuffix();
            }
        }

        void OnGUI()
        {
            if (ShowToolbar)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                    OnToolbarGUI();
            }

            _bodyScroll = EditorGUILayout.BeginScrollView(_bodyScroll, GUILayout.ExpandHeight(true));
            OnBodyGUI();
            EditorGUILayout.EndScrollView();

            DrawStatusBar();
            OnFooterGUI();
        }

        /// <summary>Top toolbar content. Use EditorStyles.toolbarButton for buttons here.</summary>
        protected virtual void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();
        }

        /// <summary>Main window content. Already wrapped in a scroll view.</summary>
        protected abstract void OnBodyGUI();

        /// <summary>Fixed bottom area for primary actions (Confirm/Close). Drawn below the status bar.</summary>
        protected virtual void OnFooterGUI()
        {
        }

        /// <summary>Shows a dismissible message in the status bar. Use instead of Debug.Log.</summary>
        protected void SetStatus(string message, MessageType type = MessageType.Info)
        {
            _statusMessage = message ?? string.Empty;
            _statusType = type;
            Repaint();
        }

        protected void ClearStatus()
        {
            _statusMessage = string.Empty;
        }

        void DrawStatusBar()
        {
            if (string.IsNullOrEmpty(_statusMessage))
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.HelpBox(_statusMessage, _statusType);
                if (GUILayout.Button(EdGUI.TempContent("\u00d7", "Dismiss"), GUILayout.Width(22f), GUILayout.ExpandHeight(true)))
                    ClearStatus();
            }
        }

        void UpdateTitleSuffix()
        {
            _baseTitle ??= titleContent.text.TrimEnd(' ', '*');
            var title = _isDirty ? _baseTitle + " *" : _baseTitle;
            titleContent = new GUIContent(title, titleContent.image, titleContent.tooltip);
        }
    }
}
