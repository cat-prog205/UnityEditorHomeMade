using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Discovers dashboard tools: concrete <see cref="EdWindowBase"/> windows and
    /// members marked with <see cref="ToolEntryAttribute"/>.
    /// </summary>
    public static class ToolCatalog
    {
        public readonly struct Entry
        {
            public readonly string Title;
            public readonly string Category;
            public readonly string Keywords;
            public readonly int Order;
            public readonly Type WindowType;
            public readonly string MenuPath;

            public Entry(string title, string category, string keywords, int order, Type windowType, string menuPath)
            {
                Title = title ?? string.Empty;
                Category = string.IsNullOrEmpty(category) ? "Tools" : category;
                Keywords = keywords ?? string.Empty;
                Order = order;
                WindowType = windowType;
                MenuPath = menuPath;
            }

            public bool CanOpen => WindowType != null || !string.IsNullOrEmpty(MenuPath);
        }

        static List<Entry> _entries;
        static readonly HashSet<string> Seen = new();

        public static IReadOnlyList<Entry> Entries
        {
            get
            {
                if (_entries == null)
                    Rebuild();
                return _entries;
            }
        }

        [InitializeOnLoadMethod]
        static void Init()
        {
            _entries = null;
        }

        public static void Rebuild()
        {
            _entries = new List<Entry>(32);
            Seen.Clear();

            var types = new HashSet<Type>();
            foreach (var type in TypeCache.GetTypesDerivedFrom<EdWindowBase>())
                types.Add(type);
            foreach (var type in TypeCache.GetTypesWithAttribute<ToolEntryAttribute>())
                types.Add(type);
            foreach (var method in TypeCache.GetMethodsWithAttribute<ToolEntryAttribute>())
            {
                var declaring = method.DeclaringType;
                if (declaring != null)
                    types.Add(declaring);
            }

            foreach (var type in types)
                CollectType(type);

            _entries.Sort(CompareEntries);
        }

        public static void Open(in Entry entry)
        {
            if (entry.WindowType != null)
            {
                var window = EditorWindow.GetWindow(entry.WindowType, false, entry.Title);
                window.Show();
                window.Focus();
                return;
            }

            if (!string.IsNullOrEmpty(entry.MenuPath))
                EditorApplication.ExecuteMenuItem(entry.MenuPath);
        }

        static void CollectType(Type type)
        {
            if (type == typeof(ToolDashboardWindow) || type == typeof(EdWindowBase))
                return;

            var classAttr = type.GetCustomAttribute<ToolEntryAttribute>(inherit: false);
            if (classAttr != null && classAttr.Hidden)
                return;

            var isWindow = typeof(EdWindowBase).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericType;
            if (isWindow)
            {
                Add(new Entry(
                    FirstNonEmpty(classAttr?.Title, NicifyTypeName(type)),
                    classAttr?.Category,
                    classAttr?.Keywords,
                    classAttr?.Order ?? 0,
                    type,
                    FindMenuPath(type)));
            }
            else if (classAttr != null)
            {
                var menuPath = FindMenuPath(type);
                if (!string.IsNullOrEmpty(menuPath))
                {
                    Add(new Entry(
                        FirstNonEmpty(classAttr.Title, NicifyTypeName(type)),
                        classAttr.Category,
                        classAttr.Keywords,
                        classAttr.Order,
                        typeof(EditorWindow).IsAssignableFrom(type) ? type : null,
                        menuPath));
                }
            }

            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            MethodInfo[] methods;
            try
            {
                methods = type.GetMethods(flags);
            }
            catch
            {
                return;
            }

            for (var m = 0; m < methods.Length; m++)
            {
                var method = methods[m];
                var methodAttr = method.GetCustomAttribute<ToolEntryAttribute>(inherit: false);
                if (methodAttr == null || methodAttr.Hidden)
                    continue;

                // Window types are already listed; skip their Open() if it also has [ToolEntry].
                if (isWindow)
                    continue;

                var menuPath = GetMenuPath(method);
                if (string.IsNullOrEmpty(menuPath))
                    continue;

                Add(new Entry(
                    FirstNonEmpty(methodAttr.Title, NicifyMethodName(method.Name)),
                    methodAttr.Category,
                    methodAttr.Keywords,
                    methodAttr.Order,
                    null,
                    menuPath));
            }
        }

        static void Add(Entry entry)
        {
            if (!entry.CanOpen)
                return;

            var key = entry.WindowType != null
                ? "T:" + entry.WindowType.FullName
                : "M:" + entry.MenuPath;
            if (!Seen.Add(key))
                return;

            _entries.Add(entry);
        }

        static string FindMenuPath(Type type)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            MethodInfo[] methods;
            try
            {
                methods = type.GetMethods(flags);
            }
            catch
            {
                return null;
            }

            for (var i = 0; i < methods.Length; i++)
            {
                var path = GetMenuPath(methods[i]);
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            return null;
        }

        static string GetMenuPath(MethodInfo method)
        {
            var items = method.GetCustomAttributes(typeof(MenuItem), inherit: false);
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] is not MenuItem menu)
                    continue;
                if (menu.validate)
                    continue;
                if (string.IsNullOrEmpty(menu.menuItem))
                    continue;
                if (menu.menuItem.StartsWith("CONTEXT/", StringComparison.Ordinal))
                    continue;
                return menu.menuItem;
            }

            return null;
        }

        static string NicifyTypeName(Type type)
        {
            var name = type.Name;
            if (name.EndsWith("Window", StringComparison.Ordinal))
                name = name.Substring(0, name.Length - 6);
            return ObjectNames.NicifyVariableName(name);
        }

        static string NicifyMethodName(string name) => ObjectNames.NicifyVariableName(name);

        static string FirstNonEmpty(string a, string b) => string.IsNullOrEmpty(a) ? b : a;

        static int CompareEntries(Entry a, Entry b)
        {
            var cat = string.Compare(a.Category, b.Category, StringComparison.OrdinalIgnoreCase);
            if (cat != 0)
                return cat;
            if (a.Order != b.Order)
                return a.Order.CompareTo(b.Order);
            return string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase);
        }
    }
}
