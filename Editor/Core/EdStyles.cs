using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Lazily-created GUIStyle cache. Styles must never be built in static constructors
    /// or field initializers: GUI.skin is only valid inside OnGUI, so every property here
    /// initializes on first access from an OnGUI call.
    /// </summary>
    public static class EdStyles
    {
        static GUIStyle _sectionBody;
        static GUIStyle _sectionHeaderLabel;
        static GUIStyle _sectionHeaderRight;
        static GUIStyle _toolbarSearchField;
        static GUIStyle _toolbarSearchCancel;
        static GUIStyle _centeredMiniLabel;

        /// <summary>Boxed body drawn under an expanded section header.</summary>
        public static GUIStyle SectionBody => _sectionBody ??= new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 8, 6, 8),
        };

        public static GUIStyle SectionHeaderLabel => _sectionHeaderLabel ??= new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleLeft,
        };

        public static GUIStyle SectionHeaderRight => _sectionHeaderRight ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight,
        };

        public static GUIStyle ToolbarSearchField => _toolbarSearchField ??=
            GUI.skin.FindStyle("ToolbarSearchTextField")
            ?? GUI.skin.FindStyle("ToolbarSeachTextField") // pre-2020 typo variant
            ?? EditorStyles.toolbarTextField; // last-resort fallback, always available

        /// <summary>May fall back to a plain toolbar button; check <see cref="ToolbarSearchCancelIsFallback"/>.</summary>
        public static GUIStyle ToolbarSearchCancel
        {
            get
            {
                if (_toolbarSearchCancel == null)
                {
                    _toolbarSearchCancel =
                        GUI.skin.FindStyle("ToolbarSearchCancelButton")
                        ?? GUI.skin.FindStyle("ToolbarSeachCancelButton");
                    if (_toolbarSearchCancel == null)
                    {
                        _toolbarSearchCancel = EditorStyles.toolbarButton;
                        ToolbarSearchCancelIsFallback = true;
                    }
                }

                return _toolbarSearchCancel;
            }
        }

        public static bool ToolbarSearchCancelIsFallback { get; private set; }

        public static GUIStyle CenteredMiniLabel => _centeredMiniLabel ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
        };
    }
}
