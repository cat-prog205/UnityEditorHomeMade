using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Core class that hooks into Unity's Hierarchy window and dispatches
    /// rendering to feature handlers based on GameObject naming conventions.
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyExtension
    {
        private static Dictionary<int, CachedItemData> _itemCache = new Dictionary<int, CachedItemData>();
        
        private struct CachedItemData
        {
            public HierarchyItemType Type;
            public string Name;
        }
        
        public enum HierarchyItemType
        {
            Normal,
            Header,
            Separator
        }

        static HierarchyExtension()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyItemGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        private static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            var settings = HierarchySettings.Instance;
            if (settings == null || !settings.enabled)
                return;

#pragma warning disable CS0618 // InstanceIDToObject still works, EntityIdToObject is Unity 6.1+
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
#pragma warning restore CS0618
            if (go == null)
                return;

            // Check if cache is stale (name changed)
            var itemData = GetItemData(go, settings);

            switch (itemData.Type)
            {
                case HierarchyItemType.Header:
                    HierarchyHeader.Draw(go, selectionRect, settings);
                    break;
                case HierarchyItemType.Separator:
                    HierarchySeparator.Draw(go, selectionRect, settings);
                    break;
                case HierarchyItemType.Normal:
                    // Draw icons for normal items - each feature is independent
                    HierarchyIcons.Draw(go, selectionRect, settings);
                    break;
            }

            // Tree lines are drawn for all items
            if (settings.showTreeLines)
                HierarchyTreeLines.Draw(go, selectionRect, settings);
        }

        private static CachedItemData GetItemData(GameObject go, HierarchySettings settings)
        {
            int instanceID = go.GetInstanceID();
            string currentName = go.name;
            
            // Check if cached and name hasn't changed
            if (_itemCache.TryGetValue(instanceID, out var cached))
            {
                if (cached.Name == currentName)
                    return cached;
            }

            // Recalculate and cache
            var data = new CachedItemData
            {
                Type = DetermineItemType(currentName, settings),
                Name = currentName
            };
            _itemCache[instanceID] = data;
            return data;
        }

        private static HierarchyItemType DetermineItemType(string name, HierarchySettings settings)
        {
            // Check separator first (longer prefix takes priority)
            if (!string.IsNullOrEmpty(settings.separatorPrefix) && name.StartsWith(settings.separatorPrefix))
                return HierarchyItemType.Separator;

            // Check header
            if (!string.IsNullOrEmpty(settings.headerPrefix) && name.StartsWith(settings.headerPrefix))
                return HierarchyItemType.Header;

            return HierarchyItemType.Normal;
        }

        public static string GetDisplayName(string name, string prefix)
        {
            if (string.IsNullOrEmpty(prefix) || !name.StartsWith(prefix))
                return name;
            
            return name.Substring(prefix.Length).Trim();
        }

        public static void ClearCache()
        {
            _itemCache.Clear();
        }
    }
}
