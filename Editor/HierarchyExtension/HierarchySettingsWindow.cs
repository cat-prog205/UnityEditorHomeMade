using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Settings window for configuring Hierarchy Extension features.
    /// </summary>
    [ToolEntry("Hierarchy Settings", category: "Hierarchy", keywords: "header separator tree icons")]
    public class HierarchySettingsWindow : EditorWindow
    {
        private HierarchySettings _settings;
        private Vector2 _scrollPosition;
        private SerializedObject _serializedSettings;

        [MenuItem("GameObject/HomeMade/Hierarchy Settings", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<HierarchySettingsWindow>("Hierarchy Settings");
            window.minSize = new Vector2(350, 400);
            window.Show();
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            _settings = HierarchySettings.Instance;
            if (_settings != null)
            {
                _serializedSettings = new SerializedObject(_settings);
            }
        }

        private void OnGUI()
        {
            if (_settings == null)
            {
                EditorGUILayout.HelpBox("Settings not found. Click to create.", MessageType.Warning);
                if (GUILayout.Button("Create Settings"))
                {
                    LoadSettings();
                }
                return;
            }

            _serializedSettings?.Update();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            DrawHeader();
            EditorGUILayout.Space(10);
            
            DrawGeneralSection();
            EditorGUILayout.Space(10);
            
            DrawPrefixesSection();
            EditorGUILayout.Space(10);
            
            DrawHeaderStyleSection();
            EditorGUILayout.Space(10);
            
            DrawSeparatorStyleSection();
            EditorGUILayout.Space(10);
            
            DrawFeaturesSection();
            EditorGUILayout.Space(10);
            
            DrawActionsSection();
            
            EditorGUILayout.EndScrollView();

            _serializedSettings?.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("Hierarchy Extension", titleStyle, GUILayout.Height(30));
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("HomeMade", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawGeneralSection()
        {
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            EditorGUI.BeginChangeCheck();
            var enabled = EditorGUILayout.Toggle("Enabled", _settings.enabled);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_settings, "Toggle Hierarchy Extension");
                _settings.enabled = enabled;
                EditorUtility.SetDirty(_settings);
                HierarchyExtension.ClearCache();
                EditorApplication.RepaintHierarchyWindow();
            }
            
            EditorGUI.indentLevel--;
        }

        private void DrawPrefixesSection()
        {
            EditorGUILayout.LabelField("Prefixes", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            DrawStringProperty("headerPrefix", "Header Prefix");
            DrawStringProperty("separatorPrefix", "Separator Prefix");
            
            EditorGUI.indentLevel--;
        }

        private void DrawHeaderStyleSection()
        {
            EditorGUILayout.LabelField("Header Style", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            DrawColorProperty("headerBackgroundColor", "Background Color");
            DrawColorProperty("headerTextColor", "Text Color");
            DrawEnumProperty("headerFontStyle", "Font Style");
            DrawEnumProperty("headerTextAlignment", "Text Alignment");
            
            // Preview
            EditorGUILayout.Space(5);
            var previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(24));
            previewRect.x += EditorGUI.indentLevel * 15;
            previewRect.width -= EditorGUI.indentLevel * 15;
            
            EditorGUI.DrawRect(previewRect, _settings.headerBackgroundColor);
            var style = new GUIStyle(EditorStyles.label)
            {
                fontStyle = _settings.headerFontStyle,
                alignment = _settings.headerTextAlignment
            };
            style.normal.textColor = _settings.headerTextColor;
            EditorGUI.LabelField(previewRect, "PREVIEW HEADER", style);
            
            EditorGUI.indentLevel--;
        }

        private void DrawSeparatorStyleSection()
        {
            EditorGUILayout.LabelField("Separator Style", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            DrawColorProperty("separatorColor", "Line Color");
            DrawColorProperty("separatorTextColor", "Text Color");
            DrawEnumProperty("separatorFontStyle", "Font Style");
            DrawFloatProperty("separatorHeight", "Line Height");
            DrawBoolProperty("separatorShowLabel", "Show Label");
            
            // Preview
            EditorGUILayout.Space(5);
            var previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(24));
            previewRect.x += EditorGUI.indentLevel * 15;
            previewRect.width -= EditorGUI.indentLevel * 15;
            
            var bgColor = EditorGUIUtility.isProSkin 
                ? new Color(0.22f, 0.22f, 0.22f, 1f) 
                : new Color(0.76f, 0.76f, 0.76f, 1f);
            EditorGUI.DrawRect(previewRect, bgColor);
            
            float lineY = previewRect.y + previewRect.height / 2f - 1f;
            var leftLine = new Rect(previewRect.x, lineY, previewRect.width * 0.3f, _settings.separatorHeight);
            var rightLine = new Rect(previewRect.x + previewRect.width * 0.7f, lineY, previewRect.width * 0.3f, _settings.separatorHeight);
            EditorGUI.DrawRect(leftLine, _settings.separatorColor);
            EditorGUI.DrawRect(rightLine, _settings.separatorColor);
            
            var sepStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = _settings.separatorFontStyle,
                alignment = TextAnchor.MiddleCenter
            };
            sepStyle.normal.textColor = _settings.separatorTextColor;
            EditorGUI.LabelField(previewRect, "PREVIEW", sepStyle);
            
            EditorGUI.indentLevel--;
        }

        private void DrawFeaturesSection()
        {
            EditorGUILayout.LabelField("Tree Lines", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            DrawBoolProperty("showTreeLines", "Show Tree Lines");
            if (_settings.showTreeLines)
            {
                EditorGUI.indentLevel++;
                DrawColorProperty("treeLineColor", "Line Color");
                DrawFloatProperty("treeLineWidth", "Line Width");
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Icons", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            DrawBoolProperty("showComponentIcons", "Show Component Icons");
            DrawBoolProperty("showTagLayer", "Show Tag/Layer");
            DrawBoolProperty("showToggleIcon", "Show Toggle Icon");
            DrawFloatProperty("iconsRightPadding", "Right Padding");
            
            EditorGUI.indentLevel--;
        }

        private void DrawActionsSection()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset to Defaults"))
            {
                if (EditorUtility.DisplayDialog("Reset Settings", 
                    "Are you sure you want to reset all settings to defaults?", "Reset", "Cancel"))
                {
                    Undo.RecordObject(_settings, "Reset Hierarchy Settings");
                    _settings.ResetToDefaults();
                    HierarchyExtension.ClearCache();
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
            
            if (GUILayout.Button("Refresh Hierarchy"))
            {
                HierarchyExtension.ClearCache();
                EditorApplication.RepaintHierarchyWindow();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Select Settings Asset"))
            {
                Selection.activeObject = _settings;
                EditorGUIUtility.PingObject(_settings);
            }
        }

        private void DrawBoolProperty(string propertyName, string label)
        {
            var prop = _serializedSettings.FindProperty(propertyName);
            if (prop != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(prop, new GUIContent(label));
                if (EditorGUI.EndChangeCheck())
                {
                    _serializedSettings.ApplyModifiedProperties();
                    HierarchyExtension.ClearCache();
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }

        private void DrawColorProperty(string propertyName, string label)
        {
            var prop = _serializedSettings.FindProperty(propertyName);
            if (prop != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(prop, new GUIContent(label));
                if (EditorGUI.EndChangeCheck())
                {
                    _serializedSettings.ApplyModifiedProperties();
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }

        private void DrawStringProperty(string propertyName, string label)
        {
            var prop = _serializedSettings.FindProperty(propertyName);
            if (prop != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(prop, new GUIContent(label));
                if (EditorGUI.EndChangeCheck())
                {
                    _serializedSettings.ApplyModifiedProperties();
                    HierarchyExtension.ClearCache();
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }

        private void DrawFloatProperty(string propertyName, string label)
        {
            var prop = _serializedSettings.FindProperty(propertyName);
            if (prop != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(prop, new GUIContent(label));
                if (EditorGUI.EndChangeCheck())
                {
                    _serializedSettings.ApplyModifiedProperties();
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }

        private void DrawEnumProperty(string propertyName, string label)
        {
            var prop = _serializedSettings.FindProperty(propertyName);
            if (prop != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(prop, new GUIContent(label));
                if (EditorGUI.EndChangeCheck())
                {
                    _serializedSettings.ApplyModifiedProperties();
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }
    }
}
