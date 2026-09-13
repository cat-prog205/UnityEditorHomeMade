// MIT License — https://github.com/marijnz/unity-toolbar-extender
// IMGUI docks on the Unity main toolbar. Do not also register the same
// controls via [MainToolbarElement] — that API hides items until the user
// enables them, and both systems fight over MainToolbarWindow.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    [InitializeOnLoad]
    public static class ToolbarExtender
    {
        public static readonly List<Action> LeftToolbarGUI = new();
        public static readonly List<Action> RightToolbarGUI = new();

        public const float ButtonWidth = 32f;
        public const float ItemGap = 6f;
        public const float PlayButtonGap = 56f;

        static int _toolCount;
        static GUIStyle _commandStyle;

        static ToolbarExtender()
        {
            CacheToolCount();
            ToolbarCallback.OnToolbarGUI = OnLegacyToolbarGUI;
            ToolbarCallback.OnToolbarGUILeft = DrawLeft;
            ToolbarCallback.OnToolbarGUIRight = DrawRight;
        }

        static void CacheToolCount()
        {
            var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null)
                return;

            var toolIcons = toolbarType.GetField(
#if UNITY_2019_1_OR_NEWER
                "k_ToolCount",
#else
                "s_ShownToolIcons",
#endif
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            if (toolIcons == null)
            {
                _toolCount = 8;
                return;
            }

#if UNITY_2019_3_OR_NEWER
            _toolCount = (int)toolIcons.GetValue(null);
#elif UNITY_2019_1_OR_NEWER
            _toolCount = (int)toolIcons.GetValue(null);
#else
            _toolCount = ((Array)toolIcons.GetValue(null)).Length;
#endif
        }

        static void DrawLeft()
        {
            GUILayout.BeginHorizontal();
            Invoke(LeftToolbarGUI);
            GUILayout.Space(PlayButtonGap);
            GUILayout.EndHorizontal();
        }

        static void DrawRight()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(PlayButtonGap);
            Invoke(RightToolbarGUI);
            GUILayout.EndHorizontal();
        }

        static void Invoke(List<Action> handlers)
        {
            for (var i = 0; i < handlers.Count; i++)
            {
                if (i > 0)
                    GUILayout.Space(ItemGap);
                handlers[i]?.Invoke();
            }
        }

        // Pre-2021 toolbar: one IMGUI overlay covering the whole bar.
        static void OnLegacyToolbarGUI()
        {
            _commandStyle ??= new GUIStyle("CommandLeft");

            const float space = 8f;
            const float playPauseStopWidth = 140f;
            const float dropdownWidth = 80f;
            var screenWidth = EditorGUIUtility.currentViewWidth;
            var playButtonsPosition = Mathf.RoundToInt((screenWidth - playPauseStopWidth) / 2f);

            var leftRect = new Rect(0f, 4f, screenWidth, 22f);
            leftRect.xMin += space + ButtonWidth * _toolCount + space + 128f;
            leftRect.xMax = playButtonsPosition - space;

            var rightRect = new Rect(0f, 4f, screenWidth, 22f);
            rightRect.xMin = playButtonsPosition + _commandStyle.fixedWidth * 3f + space;
            rightRect.xMax = screenWidth - space - dropdownWidth * 3f - ButtonWidth - 78f - space * 5f;

            if (leftRect.width > 0f)
            {
                GUILayout.BeginArea(leftRect);
                DrawLeft();
                GUILayout.EndArea();
            }

            if (rightRect.width > 0f)
            {
                GUILayout.BeginArea(rightRect);
                DrawRight();
                GUILayout.EndArea();
            }
        }
    }
}
