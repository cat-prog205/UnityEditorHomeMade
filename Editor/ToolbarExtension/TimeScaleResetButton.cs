using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    [InitializeOnLoad]
    static class TimeScaleResetButton
    {
        const string IconPath = "Assets/2DAbilitySystem/ThirdParty/UnityEditorHomeMade/Editor/ToolbarExtension/Icons/timescale.png";

        static TimeScaleResetButton()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath)
                       ?? EditorGUIUtility.IconContent("d_Refresh").image as Texture2D;
            if (!GUILayout.Button(new GUIContent(icon, "Reset Time Scale to 1"), EditorStyles.toolbarButton, GUILayout.Width(ToolbarExtender.ButtonWidth)))
                return;

            Time.timeScale = 1f;
        }
    }
}
