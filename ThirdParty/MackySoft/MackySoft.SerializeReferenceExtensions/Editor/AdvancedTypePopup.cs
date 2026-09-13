#if UNITY_2019_3_OR_NEWER
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace MackySoft.SerializeReferenceExtensions.Editor
{
    public class AdvancedTypePopupItem : AdvancedDropdownItem
    {
        public Type Type { get; }

        public AdvancedTypePopupItem(Type type, string name) : base(name) { this.Type = type; }
    }

    /// <summary>
    /// A type popup with a fuzzy finder.
    /// </summary>
    public class AdvancedTypePopup : AdvancedDropdown
    {
        private const int kMaxNamespaceNestCount = 16;

        public static void AddTo(AdvancedDropdownItem root, IEnumerable<Type> types)
        {
            var itemCount = 0;

            // Add null item.
            var nullItem = new AdvancedTypePopupItem(null, TypeMenuUtility.k_NullDisplayName)
            {
                id = itemCount++
            };
            root.AddChild(nullItem);

            var typeArray = types.OrderByType().ToArray();

            // Single namespace if the root has one namespace and the nest is unbranched.
            var isSingleNamespace = true;
            var namespaces        = new string[kMaxNamespaceNestCount];
            foreach (var type in typeArray)
            {
                var splittedTypePath = TypeMenuUtility.GetSplittedTypePath(type);

                if (splittedTypePath.Length <= 1) continue;
                for (var k = 0; splittedTypePath.Length - 1 > k; k++)
                {
                    var ns = namespaces[k];
                    if (ns == null)
                    {
                        namespaces[k] = splittedTypePath[k];
                    }
                    else if (ns != splittedTypePath[k])
                    {
                        isSingleNamespace = false;

                        break;
                    }
                }
            }

            // Add type items.
            foreach (var type in typeArray)
            {
                var splittedTypePath = TypeMenuUtility.GetSplittedTypePath(type);

                if (splittedTypePath.Length == 0) continue;

                var parent = root;

                // Add namespace items.
                if (!isSingleNamespace)
                    for (var k = 0; splittedTypePath.Length - 1 > k; k++)
                    {
                        var foundItem = GetItem(parent, splittedTypePath[k]);
                        if (foundItem != null)
                        {
                            parent = foundItem;
                        }
                        else
                        {
                            var newItem = new AdvancedDropdownItem(splittedTypePath[k])
                            {
                                id = itemCount++
                            };
                            parent.AddChild(newItem);
                            parent = newItem;
                        }
                    }

                // Add type item.
                var item = new AdvancedTypePopupItem(type, ObjectNames.NicifyVariableName(splittedTypePath[splittedTypePath.Length - 1]))
                {
                    id = itemCount++
                };
                parent.AddChild(item);
            }
        }

        private static AdvancedDropdownItem GetItem(AdvancedDropdownItem parent, string name)
        {
            foreach (var item in parent.childList)
                if (item.name == name)
                    return item;

            return null;
        }

        private static readonly float k_HeaderHeight = EditorGUIUtility.singleLineHeight * 2f;

        private Type[] m_Types;

        public event Action<AdvancedTypePopupItem> OnItemSelected;

        public AdvancedTypePopup(IEnumerable<Type> types, int maxLineCount, AdvancedDropdownState state) : base(state)
        {
            this.SetTypes(types);
            this.minimumSize = new Vector2(this.minimumSize.x, EditorGUIUtility.singleLineHeight * maxLineCount + k_HeaderHeight);
        }

        public void SetTypes(IEnumerable<Type> types) { this.m_Types = types.ToArray(); }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Select Type");
            AddTo(root, this.m_Types);

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            if (item is AdvancedTypePopupItem typePopupItem) this.OnItemSelected?.Invoke(typePopupItem);
        }
    }
}
#endif
