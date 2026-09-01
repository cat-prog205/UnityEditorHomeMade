using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Semantic color palette for editor tools. Colors carry meaning, not decoration:
    /// Danger = destructive/close, Confirm = create/save/apply, Warning = risky but recoverable.
    /// Values are tuned separately for dark (Pro) and light skins so contrast stays readable.
    /// </summary>
    public static class EdTheme
    {
        public static bool DarkSkin => EditorGUIUtility.isProSkin;

        /// <summary>Destructive or exit actions: Delete, Close, X, Clear All.</summary>
        public static Color Danger => DarkSkin
            ? new Color(0.95f, 0.36f, 0.34f)
            : new Color(0.85f, 0.30f, 0.26f);

        /// <summary>Positive actions: Confirm, Save, Add, Apply.</summary>
        public static Color Confirm => DarkSkin
            ? new Color(0.38f, 0.82f, 0.50f)
            : new Color(0.32f, 0.70f, 0.40f);

        /// <summary>Risky-but-recoverable actions: Force Rebuild, Overwrite.</summary>
        public static Color Warning => DarkSkin
            ? new Color(0.98f, 0.76f, 0.30f)
            : new Color(0.92f, 0.64f, 0.14f);

        /// <summary>Background of collapsible section headers.</summary>
        public static Color SectionHeaderBg => DarkSkin
            ? new Color(0.28f, 0.28f, 0.28f)
            : new Color(0.68f, 0.68f, 0.68f);

        /// <summary>Thin separator lines.</summary>
        public static Color Separator => DarkSkin
            ? new Color(0.13f, 0.13f, 0.13f)
            : new Color(0.55f, 0.55f, 0.55f);
    }
}
