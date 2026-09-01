using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    [InitializeOnLoad]
    static class ToolbarClearPlayerPrefButton
    {
        const string IconPath = "Assets/2DAbilitySystem/ThirdParty/UnityEditorHomeMade/Editor/ToolbarExtension/Icons/trash.png";

        static ToolbarClearPlayerPrefButton()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath)
                       ?? EditorGUIUtility.IconContent("d_TreeEditor.Trash").image as Texture2D;
            if (!GUILayout.Button(new GUIContent(icon, "Clear PlayerPrefs"), EditorStyles.toolbarButton, GUILayout.Width(ToolbarExtender.ButtonWidth)))
                return;

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
