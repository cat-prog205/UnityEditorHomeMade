using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Renders Separator items in the Hierarchy as horizontal divider lines.
    /// Separators use the "---" prefix by default.
    /// </summary>
    public static class HierarchySeparator
    {
        private static GUIStyle _labelStyle;

        public static void Draw(GameObject go, Rect rect, HierarchySettings settings)
        {
            // Get display name without prefix (may be empty or contain label)
            var displayName = HierarchyExtension.GetDisplayName(go.name, settings.separatorPrefix);
            
            // Draw background to cover default rendering
            var bgColor = EditorGUIUtility.isProSkin 
                ? new Color(0.22f, 0.22f, 0.22f, 1f) 
                : new Color(0.76f, 0.76f, 0.76f, 1f);
            EditorGUI.DrawRect(rect, bgColor);
            
            // Calculate line position
            float lineY = rect.y + (rect.height / 2f) - (settings.separatorHeight / 2f);
            
            if (string.IsNullOrEmpty(displayName) || !settings.separatorShowLabel)
            {
                // Draw full-width line
                var lineRect = new Rect(rect.x, lineY, rect.width, settings.separatorHeight);
                EditorGUI.DrawRect(lineRect, settings.separatorColor);
            }
            else
            {
                // Draw line with label in center
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle(EditorStyles.label);
                    _labelStyle.alignment = TextAnchor.MiddleCenter;
                }
                _labelStyle.fontStyle = settings.separatorFontStyle;
                _labelStyle.normal.textColor = settings.separatorTextColor;
                
                // Calculate label width
                var labelContent = new GUIContent(displayName);
                float labelWidth = _labelStyle.CalcSize(labelContent).x + 16;
                float labelX = rect.x + (rect.width - labelWidth) / 2f;
                
                // Draw left line
                var leftLineRect = new Rect(rect.x, lineY, labelX - rect.x - 8, settings.separatorHeight);
                EditorGUI.DrawRect(leftLineRect, settings.separatorColor);
                
                // Draw right line
                var rightLineRect = new Rect(labelX + labelWidth + 8, lineY, 
                    rect.x + rect.width - (labelX + labelWidth + 8), settings.separatorHeight);
                EditorGUI.DrawRect(rightLineRect, settings.separatorColor);
                
                // Draw label
                var labelRect = new Rect(labelX, rect.y, labelWidth, rect.height);
                EditorGUI.LabelField(labelRect, displayName, _labelStyle);
            }
        }
    }
}
