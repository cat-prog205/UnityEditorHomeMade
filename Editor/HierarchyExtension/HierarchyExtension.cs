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
        private static Dictionary<EntityId, CachedItemData> _itemCache = new Dictionary<EntityId, CachedItemData>();
        
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
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= OnHierarchyItemGUI;
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyItemGUI;
        }

        private static void OnHierarchyItemGUI(EntityId entityId, Rect selectionRect)
        {
            var settings = HierarchySettings.Instance;
            if (settings == null || !settings.enabled)
                return;

            var go = EditorUtility.EntityIdToObject(entityId) as GameObject;
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
            var entityId = go.GetEntityId();
            string currentName = go.name;
            
            // Check if cached and name hasn't changed
            if (_itemCache.TryGetValue(entityId, out var cached))
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
            _itemCache[entityId] = data;
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
