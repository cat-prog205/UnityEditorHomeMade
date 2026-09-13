using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace UnityEditorHomeMade
{
    [InitializeOnLoad]
    static class ToolbarRecompileButton
    {
        const string IconPath = "Assets/UnityEditorHomeMade/Editor/ToolbarExtension/Icons/reset.png";

        static ToolbarRecompileButton()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath)
                       ?? EditorGUIUtility.IconContent("d_Refresh").image as Texture2D;
            if (!GUILayout.Button(new GUIContent(icon, "Recompile Scripts"), EditorStyles.toolbarButton, GUILayout.Width(ToolbarExtender.ButtonWidth)))
                return;

            CompilationPipeline.RequestScriptCompilation();
        }
    }
}
