using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Draws component icons and indicators on the right side of Hierarchy items.
    /// Shows icons for main components, tag/layer info, and toggle visibility.
    /// </summary>
    public static class HierarchyIcons
    {
        // Component types to show icons for (ordered by priority)
        private static readonly Type[] ImportantComponents = new Type[]
        {
            typeof(Camera),
            typeof(Light),
            typeof(AudioSource),
            typeof(AudioListener),
            typeof(Canvas),
            typeof(ParticleSystem),
            typeof(Animator),
            typeof(Animation),
            typeof(Rigidbody),
            typeof(Rigidbody2D),
            typeof(Collider),
            typeof(Collider2D),
        };

        private static Dictionary<Type, Texture2D> _iconCache = new Dictionary<Type, Texture2D>();
        private static Texture2D _visibleIcon;
        private static Texture2D _hiddenIcon;
        
        private const float IconSize = 16f;
        private const float IconSpacing = 2f;

        public static void Draw(GameObject go, Rect rect, HierarchySettings settings)
        {
            float currentX = rect.xMax - settings.iconsRightPadding;

            // Draw visibility toggle
            if (settings.showToggleIcon)
            {
                currentX -= IconSize;
                
                var visibilityRect = new Rect(currentX, rect.y + (rect.height - IconSize) / 2f, IconSize, IconSize);
                
                var icon = go.activeSelf ? GetVisibleIcon() : GetHiddenIcon();
                if (icon != null)
                {
                    var color = GUI.color;
                    GUI.color = go.activeSelf ? new Color(1f, 1f, 1f, 0.7f) : new Color(1f, 1f, 1f, 0.3f);
                    
                    if (GUI.Button(visibilityRect, icon, GUIStyle.none))
                    {
                        Undo.RecordObject(go, "Toggle Active");
                        go.SetActive(!go.activeSelf);
                    }
                    
                    GUI.color = color;
                }
                
                currentX -= IconSpacing;
            }

            // Draw tag/layer indicators
            if (settings.showTagLayer)
            {
                string info = "";
                
                if (go.tag != "Untagged")
                    info = go.tag;
                
                if (go.layer != 0)
                {
                    string layerName = LayerMask.LayerToName(go.layer);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        info = string.IsNullOrEmpty(info) ? $"[{layerName}]" : $"{info} [{layerName}]";
                    }
                }

                if (!string.IsNullOrEmpty(info))
                {
                    var style = EditorStyles.miniLabel;
                    var content = new GUIContent(info);
                    float width = style.CalcSize(content).x;
                    
                    currentX -= width;
                    var labelRect = new Rect(currentX, rect.y, width, rect.height);
                    
                    var oldColor = GUI.color;
                    GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    EditorGUI.LabelField(labelRect, info, style);
                    GUI.color = oldColor;
                    
                    currentX -= IconSpacing * 2;
                }
            }

            // Draw component icons
            if (settings.showComponentIcons)
            {
                int maxIcons = 3;
                int iconCount = 0;
                
                foreach (var componentType in ImportantComponents)
                {
                    if (iconCount >= maxIcons)
                        break;

                    if (go.GetComponent(componentType) != null)
                    {
                        var icon = GetComponentIcon(componentType);
                        if (icon != null)
                        {
                            currentX -= IconSize;
                            var iconRect = new Rect(currentX, rect.y + (rect.height - IconSize) / 2f, IconSize, IconSize);
                            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                            currentX -= IconSpacing;
                            iconCount++;
                        }
                    }
                }
            }
        }

        private static Texture2D GetComponentIcon(Type type)
        {
            if (_iconCache.TryGetValue(type, out var cached))
                return cached;

            var icon = EditorGUIUtility.ObjectContent(null, type).image as Texture2D;
            _iconCache[type] = icon;
            return icon;
        }

        private static Texture2D GetVisibleIcon()
        {
            if (_visibleIcon == null)
                _visibleIcon = EditorGUIUtility.IconContent("scenevis_visible_hover").image as Texture2D;
            return _visibleIcon;
        }

        private static Texture2D GetHiddenIcon()
        {
            if (_hiddenIcon == null)
                _hiddenIcon = EditorGUIUtility.IconContent("scenevis_hidden_hover").image as Texture2D;
            return _hiddenIcon;
        }
    }
}
