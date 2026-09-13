// MIT License — https://github.com/marijnz/unity-toolbar-extender

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#if !UNITY_6000_3_OR_NEWER
using System.Reflection;
#endif

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Hooks the Unity main toolbar visual tree and hosts IMGUI docks for
    /// <see cref="ToolbarExtender"/>. Idempotent: will not insert duplicate docks.
    /// </summary>
    public static class ToolbarCallback
    {
        const string LeftDockName = "HomeMadeToolbarLeft66";
        const string RightDockName = "HomeMadeToolbarRight66";
        const int MaxSetupAttempts = 200;

        public static Action OnToolbarGUI;
        public static Action OnToolbarGUILeft;
        public static Action OnToolbarGUIRight;

        static int _setupAttempts;
        static EditorWindow _toolbarWindow;

        static Type ToolbarType => typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        static Type MainToolbarWindowType => typeof(Editor).Assembly.GetType("UnityEditor.MainToolbarWindow");

        static ToolbarCallback()
        {
            EditorApplication.update += Tick;
        }

        static void Tick()
        {
            if (_toolbarWindow != null)
                return;

            if (++_setupAttempts > MaxSetupAttempts)
            {
                EditorApplication.update -= Tick;
                return;
            }

            if (TryHook())
                _setupAttempts = 0;
        }

#if UNITY_6000_3_OR_NEWER
        static bool TryHook()
        {
            var windowType = MainToolbarWindowType;
            if (windowType == null)
                return true;

            var toolbars = Resources.FindObjectsOfTypeAll(windowType);
            if (toolbars.Length == 0)
                return false;

            if (toolbars[0] is not EditorWindow window)
                return false;

            var root = window.rootVisualElement;
            if (root == null)
                return false;

            if (root.Q(LeftDockName) != null && root.Q(RightDockName) != null)
            {
                ApplyPlayGap(root);
                _toolbarWindow = window;
                return true;
            }

            var middleContainer = root.Q(className: "unity-overlay-container__middle-container");
            var parent = middleContainer?.parent;
            if (parent == null)
                return false;

            var leftDock = CreateDock(LeftDockName, Justify.FlexEnd, paddingLeft: 8, paddingRight: 48);
            var rightDock = CreateDock(RightDockName, Justify.FlexStart, paddingLeft: 48, paddingRight: 8);

            var middleIndex = parent.IndexOf(middleContainer);
            parent.Insert(middleIndex, leftDock);
            parent.Insert(middleIndex + 2, rightDock);

            leftDock.Add(CreateImguiHost(() => OnToolbarGUILeft?.Invoke(), minWidth: 280f));
            rightDock.Add(CreateImguiHost(() => OnToolbarGUIRight?.Invoke(), minWidth: 240f));

            _toolbarWindow = window;
            return true;
        }

        static void ApplyPlayGap(VisualElement root)
        {
            var left = root.Q(LeftDockName);
            var right = root.Q(RightDockName);
            if (left != null)
            {
                left.style.paddingLeft = 8;
                left.style.paddingRight = 48;
                left.style.marginRight = 8;
            }

            if (right != null)
            {
                right.style.paddingLeft = 48;
                right.style.paddingRight = 8;
                right.style.marginLeft = 8;
            }
        }

        static VisualElement CreateDock(string name, Justify justify, float paddingLeft, float paddingRight)
        {
            return new VisualElement
            {
                name = name,
                pickingMode = PickingMode.Ignore,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 0,
                    flexDirection = FlexDirection.Row,
                    flexBasis = 0,
                    justifyContent = justify,
                    alignItems = Align.Center,
                    paddingLeft = paddingLeft,
                    paddingRight = paddingRight,
                    overflow = Overflow.Visible,
                }
            };
        }

        static IMGUIContainer CreateImguiHost(Action onGui, float minWidth)
        {
            return new IMGUIContainer(onGui)
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    flexGrow = 0,
                    flexShrink = 0,
                    minWidth = minWidth,
                    height = 24,
                    overflow = Overflow.Visible,
                }
            };
        }
#else
        static ScriptableObject _legacyToolbar;
        static Type _guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
#if UNITY_2020_1_OR_NEWER
        static PropertyInfo _windowBackend = _guiViewType?.GetProperty("windowBackend",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static PropertyInfo _viewVisualTree = typeof(Editor).Assembly.GetType("UnityEditor.IWindowBackend")
            ?.GetProperty("visualTree", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#else
        static PropertyInfo _viewVisualTree = _guiViewType?.GetProperty("visualTree",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#endif
        static FieldInfo _imguiContainerOnGui = typeof(IMGUIContainer).GetField("m_OnGUIHandler",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        static bool TryHook()
        {
            if (ToolbarType == null)
                return true;

            if (_legacyToolbar == null)
            {
                var toolbars = Resources.FindObjectsOfTypeAll(ToolbarType);
                _legacyToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                if (_legacyToolbar == null)
                    return false;
            }

#if UNITY_2021_1_OR_NEWER
            var rootField = _legacyToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            var mRoot = rootField?.GetValue(_legacyToolbar) as VisualElement;
            if (mRoot == null)
                return false;

            RegisterZone("ToolbarZoneLeftAlign", () => OnToolbarGUILeft?.Invoke());
            RegisterZone("ToolbarZoneRightAlign", () => OnToolbarGUIRight?.Invoke());

            void RegisterZone(string rootName, Action cb)
            {
                var zone = mRoot.Q(rootName);
                if (zone == null || zone.Q(rootName + "-HomeMade") != null)
                    return;

                var parent = new VisualElement { name = rootName + "-HomeMade", style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
                var container = new IMGUIContainer { style = { flexGrow = 1 } };
                container.onGUIHandler += () => cb?.Invoke();
                parent.Add(container);
                zone.Add(parent);
            }
#else
            object visualTreeOwner = _legacyToolbar;
#if UNITY_2020_1_OR_NEWER
            visualTreeOwner = _windowBackend.GetValue(_legacyToolbar);
#endif
            var visualTree = (VisualElement)_viewVisualTree.GetValue(visualTreeOwner, null);
            var container = (IMGUIContainer)visualTree[0];
            var handler = (Action)_imguiContainerOnGui.GetValue(container);
            handler -= DrawLegacy;
            handler += DrawLegacy;
            _imguiContainerOnGui.SetValue(container, handler);
#endif
            return true;
        }

        static void DrawLegacy() => OnToolbarGUI?.Invoke();
#endif
    }
}
