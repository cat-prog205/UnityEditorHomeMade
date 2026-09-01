using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Plays the first enabled scene in Build Settings without leaving the scene
    /// currently open in the editor. Stop Play returns to that design scene.
    /// </summary>
    [InitializeOnLoad]
    static class ToolbarPlayFirstBuildScene
    {
        static ToolbarPlayFirstBuildScene()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnToolbarGUI()
        {
            var icon = EditorGUIUtility.IconContent(EditorApplication.isPlaying ? "d_PauseButton" : "d_PlayButton");
            var tooltip = EditorApplication.isPlaying
                ? "Stop Play Mode"
                : "Play first scene in Build Settings";
            var content = new GUIContent(icon.image, tooltip);

            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying))
            {
                if (!GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(ToolbarExtender.ButtonWidth)))
                    return;
            }

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            PlayFirstBuildScene();
        }

        static void PlayFirstBuildScene()
        {
            var path = GetFirstEnabledBuildScenePath();
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog(
                    "Play First Build Scene",
                    "Build Settings has no enabled scene. Add a scene in File > Build Settings.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (sceneAsset == null)
            {
                EditorUtility.DisplayDialog(
                    "Play First Build Scene",
                    $"Could not load build scene:\n{path}",
                    "OK");
                return;
            }

            EditorSceneManager.playModeStartScene = sceneAsset;
            EditorApplication.isPlaying = true;
        }

        static string GetFirstEnabledBuildScenePath()
        {
            var scenes = EditorBuildSettings.scenes;
            for (var i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled && !string.IsNullOrEmpty(scenes[i].path))
                    return scenes[i].path;
            }

            return null;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                EditorSceneManager.playModeStartScene = null;
        }
    }
}
