using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    [InitializeOnLoad]
    static class TimeScaleSlider
    {
        const float Min = 0f;
        const float Max = 10f;

        static TimeScaleSlider()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            GUILayout.Label("Time", EditorStyles.miniLabel, GUILayout.Width(36f));
            GUILayout.Space(4f);
            var next = GUILayout.HorizontalSlider(Time.timeScale, Min, Max, GUILayout.Width(90f));
            if (!Mathf.Approximately(next, Time.timeScale))
                Time.timeScale = next;
            GUILayout.Space(4f);
            GUILayout.Label($"{Time.timeScale:0.00}x", EditorStyles.miniLabel, GUILayout.Width(44f));
        }
    }
}
