using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Settings for Hierarchy Extension features.
    /// Stored as ScriptableObject in project for team sharing.
    /// </summary>
    [CreateAssetMenu(fileName = "HierarchySettings", menuName = "HomeMade/Hierarchy Settings")]
    public class HierarchySettings : ScriptableObject
    {
        private static HierarchySettings _instance;
        private const string DefaultPath = "Assets/2DAbilitySystem/ThirdParty/UnityEditorHomeMade/Editor/HierarchyExtension/HierarchySettings.asset";

        public static HierarchySettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<HierarchySettings>(DefaultPath);
                    if (_instance == null)
                    {
                        // Try to find any instance in project
                        var guids = AssetDatabase.FindAssets("t:HierarchySettings");
                        if (guids.Length > 0)
                        {
                            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                            _instance = AssetDatabase.LoadAssetAtPath<HierarchySettings>(path);
                        }
                    }
                    if (_instance == null)
                    {
                        // Create default instance
                        _instance = CreateDefaultSettings();
                    }
                }
                return _instance;
            }
        }

        [Header("General")]
        public bool enabled = true;
        
        [Header("Prefixes")]
        [Tooltip("Prefix for Header objects (e.g., -Header Name)")]
        public string headerPrefix = "-";
        
        [Tooltip("Prefix for Separator objects (e.g., ---)")]
        public string separatorPrefix = "---";

        [Header("Header Style")]
        public Color headerBackgroundColor = new Color(0.298f, 0.298f, 0.298f, 1f); // #4C4C4C
        public Color headerTextColor = Color.white; // #FFFFFF
        public FontStyle headerFontStyle = FontStyle.Bold;
        public TextAnchor headerTextAlignment = TextAnchor.MiddleCenter;

        [Header("Separator Style")]
        public Color separatorColor = new Color(0.298f, 0.298f, 0.298f, 1f); // #4C4C4C
        public Color separatorTextColor = Color.white;
        public FontStyle separatorFontStyle = FontStyle.Normal;
        [Range(1f, 5f)]
        public float separatorHeight = 2f;
        public bool separatorShowLabel = true;

        [Header("Tree Lines")]
        public bool showTreeLines = true;
        public Color treeLineColor = new Color(0.298f, 0.298f, 0.298f, 1f); // #4C4C4C
        [Range(1f, 5f)]
        public float treeLineWidth = 2f;
        
        [Header("Icons")]
        public bool showComponentIcons = false;
        public bool showTagLayer = false;
        public bool showToggleIcon = false;
        public float iconsRightPadding = 20f;

        private static HierarchySettings CreateDefaultSettings()
        {
            var settings = CreateInstance<HierarchySettings>();
            AssetDatabase.CreateAsset(settings, DefaultPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[HomeMade] Created HierarchySettings at {DefaultPath}");
            return settings;
        }

        public void ResetToDefaults()
        {
            enabled = true;
            headerPrefix = "-";
            separatorPrefix = "---";
            
            headerBackgroundColor = new Color(0.298f, 0.298f, 0.298f, 1f);
            headerTextColor = Color.white;
            headerFontStyle = FontStyle.Bold;
            headerTextAlignment = TextAnchor.MiddleCenter;
            
            separatorColor = new Color(0.298f, 0.298f, 0.298f, 1f);
            separatorTextColor = Color.white;
            separatorFontStyle = FontStyle.Bold;
            separatorHeight = 2f;
            separatorShowLabel = true;
            
            showTreeLines = true;
            treeLineColor = new Color(0.298f, 0.298f, 0.298f, 1f);
            treeLineWidth = 1f;
            
            showComponentIcons = true;
            showTagLayer = false;
            showToggleIcon = true;
            iconsRightPadding = 32f;
            
            EditorUtility.SetDirty(this);
        }
    }
}
