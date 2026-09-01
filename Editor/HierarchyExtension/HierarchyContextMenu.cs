using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Adds context menu items to quickly create Header and Separator objects.
    /// </summary>
    public static class HierarchyContextMenu
    {
        private const int Priority = 0;

        [MenuItem("GameObject/HomeMade/Header", false, Priority)]
        private static void CreateHeader()
        {
            var settings = HierarchySettings.Instance;
            CreateHierarchyObject($"{settings.headerPrefix}New Header");
        }

        [MenuItem("GameObject/HomeMade/Separator", false, Priority + 1)]
        private static void CreateSeparator()
        {
            var settings = HierarchySettings.Instance;
            CreateHierarchyObject($"{settings.separatorPrefix}");
        }

        [MenuItem("GameObject/HomeMade/Separator with Label", false, Priority + 2)]
        private static void CreateSeparatorWithLabel()
        {
            var settings = HierarchySettings.Instance;
            CreateHierarchyObject($"{settings.separatorPrefix} Section");
        }

        private static void CreateHierarchyObject(string name)
        {
            var go = new GameObject(name);
            
            // Parent to selected object if any
            if (Selection.activeGameObject != null)
            {
                go.transform.SetParent(Selection.activeGameObject.transform);
            }
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            
            // Select the new object
            Selection.activeGameObject = go;
            
            // Focus on object for rename
            EditorApplication.delayCall += () =>
            {
                EditorWindow.focusedWindow?.SendEvent(
                    EditorGUIUtility.CommandEvent("Rename")
                );
            };
        }

        // Context menu when right-clicking on existing objects
        [MenuItem("GameObject/HomeMade/Convert to Header", true)]
        private static bool ValidateConvertToHeader()
        {
            return Selection.activeGameObject != null;
        }

        [MenuItem("GameObject/HomeMade/Convert to Header", false, Priority + 10)]
        private static void ConvertToHeader()
        {
            var go = Selection.activeGameObject;
            if (go == null) return;

            var settings = HierarchySettings.Instance;
            string newName = go.name;
            
            // Remove existing prefixes
            newName = RemoveExistingPrefixes(newName, settings);
            
            Undo.RecordObject(go, "Convert to Header");
            go.name = $"{settings.headerPrefix}{newName}";
            
            HierarchyExtension.ClearCache();
        }

        [MenuItem("GameObject/HomeMade/Remove Hierarchy Styling", true)]
        private static bool ValidateRemoveStyling()
        {
            return Selection.activeGameObject != null;
        }

        [MenuItem("GameObject/HomeMade/Remove Hierarchy Styling", false, Priority + 11)]
        private static void RemoveStyling()
        {
            var go = Selection.activeGameObject;
            if (go == null) return;

            var settings = HierarchySettings.Instance;
            
            Undo.RecordObject(go, "Remove Hierarchy Styling");
            go.name = RemoveExistingPrefixes(go.name, settings);
            
            HierarchyExtension.ClearCache();
        }

        private static string RemoveExistingPrefixes(string name, HierarchySettings settings)
        {
            // Order matters - check longer prefixes first
            if (name.StartsWith(settings.separatorPrefix))
                return name.Substring(settings.separatorPrefix.Length).Trim();
            if (name.StartsWith(settings.headerPrefix))
                return name.Substring(settings.headerPrefix.Length).Trim();
            
            return name;
        }
    }
}
