using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Renders Header items in the Hierarchy with a prominent background.
    /// Headers use the "-" prefix by default (e.g., "-UI Elements").
    /// </summary>
    public static class HierarchyHeader
    {
        private static GUIStyle _labelStyle;

        public static void Draw(GameObject go, Rect rect, HierarchySettings settings)
        {
            // Get display name without prefix
            var displayName = HierarchyExtension.GetDisplayName(go.name, settings.headerPrefix);
            
            // Draw background
            EditorGUI.DrawRect(rect, settings.headerBackgroundColor);
            
            // Setup label style
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(EditorStyles.label);
            }
            
            _labelStyle.fontStyle = settings.headerFontStyle;
            _labelStyle.alignment = settings.headerTextAlignment;
            _labelStyle.normal.textColor = settings.headerTextColor;
            
            // Offset rect slightly for better text positioning
            var textRect = new Rect(rect.x + 2, rect.y, rect.width - 4, rect.height);
            
            // Draw label
            EditorGUI.LabelField(textRect, displayName.ToUpper(), _labelStyle);
        }
    }
}
