using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditorHomeMade
{
    [InitializeOnLoad]
    static class SceneSwitcherDropdown
    {
        const string IconPath = "Assets/UnityEditorHomeMade/Editor/ToolbarExtension/Icons/scene.png";

        static string[] _scenePaths;

        static SceneSwitcherDropdown()
        {
            RefreshSceneList();
            EditorApplication.projectChanged += RefreshSceneList;
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            var sceneName = Application.isPlaying
                ? SceneManager.GetActiveScene().name
                : EditorSceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName))
                sceneName = "Untitled";

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath)
                       ?? EditorGUIUtility.IconContent("d_SceneAsset Icon").image as Texture2D;
            var content = new GUIContent(sceneName, icon, "Switch Scene");

            if (!EditorGUILayout.DropdownButton(content, FocusType.Passive, EditorStyles.toolbarDropDown, GUILayout.Width(140f)))
                return;

            ShowMenu();
        }

        static void ShowMenu()
        {
            var menu = new GenericMenu();
            if (_scenePaths == null || _scenePaths.Length == 0)
            {
                menu.AddDisabledItem(new GUIContent("No Scenes Found"));
                menu.ShowAsContext();
                return;
            }

            foreach (var scenePath in _scenePaths.OrderBy(p => p.Replace('\\', '/')))
            {
                var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                var folder = GetRelativeFolderPath(Path.GetDirectoryName(scenePath));
                var menuPath = string.IsNullOrEmpty(folder) ? sceneName : $"{folder}/{sceneName}";
                var path = scenePath;
                menu.AddItem(new GUIContent(menuPath), false, () => SwitchScene(path));
            }

            menu.ShowAsContext();
        }

        static string GetRelativeFolderPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return string.Empty;

            var relative = fullPath.Replace('\\', '/');
            if (relative.StartsWith("Assets/"))
                return relative.Substring(7);
            return relative == "Assets" ? string.Empty : relative;
        }

        static void SwitchScene(string scenePath)
        {
            if (Application.isPlaying)
            {
                var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                if (Application.CanStreamedLevelBeLoaded(sceneName))
                    SceneManager.LoadScene(sceneName);
                else
                    Debug.LogError($"[HomeMade] Scene '{sceneName}' is not in Build Settings.");
                return;
            }

            if (!File.Exists(scenePath))
            {
                Debug.LogError($"[HomeMade] Scene not found: {scenePath}");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(scenePath);
        }

        static void RefreshSceneList()
        {
            _scenePaths = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories);
        }
    }
}
