#if UNITY_2019_3_OR_NEWER
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace MackySoft.SerializeReferenceExtensions.Editor
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        private struct TypePopupCache
        {
            public AdvancedTypePopup     TypePopup { get; }
            public AdvancedDropdownState State     { get; }

            public TypePopupCache(AdvancedTypePopup typePopup, AdvancedDropdownState state)
            {
                this.TypePopup = typePopup;
                this.State     = state;
            }
        }

        private const           int        k_MaxTypePopupLineCount      = 13;
        private static readonly Type       k_UnityObjectType            = typeof(UnityEngine.Object);
        private static readonly GUIContent k_NullDisplayName            = new(TypeMenuUtility.k_NullDisplayName);
        private static readonly GUIContent k_IsNotManagedReferenceLabel = new("The property type is not manage reference.");

        private readonly Dictionary<string, TypePopupCache> m_TypePopups     = new();
        private readonly Dictionary<string, GUIContent>     m_TypeNameCaches = new();

        private SerializedProperty m_TargetProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                // Draw the subclass selector popup.
                var popupPosition = EditorGUI.IndentedRect(position);
                //popupPosition.width -= string.IsNullOrEmpty(label.text) ? 0f : EditorGUIUtility.labelWidth;
                //popupPosition.x += string.IsNullOrEmpty(label.text) ? 0f : EditorGUIUtility.labelWidth;
                popupPosition.width  -= EditorGUIUtility.labelWidth;
                popupPosition.x      += EditorGUIUtility.labelWidth;
                popupPosition.height =  EditorGUIUtility.singleLineHeight;

                if (EditorGUI.DropdownButton(popupPosition, this.GetTypeName(property), FocusType.Keyboard))
                {
                    var popup = this.GetTypePopup(property);
                    this.m_TargetProperty = property;
                    popup.TypePopup.Show(popupPosition);
                }

                // Draw the managed reference property.
                EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                EditorGUI.LabelField(position, label, k_IsNotManagedReferenceLabel);
            }

            EditorGUI.EndProperty();
        }

        private TypePopupCache GetTypePopup(SerializedProperty property)
        {
            // Cache this string. This property internally call Assembly.GetName, which result in a large allocation.
            var managedReferenceFieldTypename = property.managedReferenceFieldTypename;

            if (!this.m_TypePopups.TryGetValue(managedReferenceFieldTypename, out var result))
            {
                var state = new AdvancedDropdownState();

                var baseType = ManagedReferenceUtility.GetType(managedReferenceFieldTypename);
                var popup = new AdvancedTypePopup(
                    TypeCache.GetTypesDerivedFrom(baseType).Append(baseType).Where(p =>
                        (p.IsPublic || p.IsNestedPublic) &&
                        !p.IsAbstract &&
                        !p.IsGenericType &&
                        !k_UnityObjectType.IsAssignableFrom(p) &&
                        Attribute.IsDefined(p, typeof(SerializableAttribute))
                    ),
                    k_MaxTypePopupLineCount,
                    state
                );
                popup.OnItemSelected += item =>
                {
                    var type = item.Type;
                    var obj  = this.m_TargetProperty.SetManagedReference(type);
                    this.m_TargetProperty.isExpanded = obj != null;
                    this.m_TargetProperty.serializedObject.ApplyModifiedProperties();
                    this.m_TargetProperty.serializedObject.Update();
                };

                result = new TypePopupCache(popup, state);
                this.m_TypePopups.Add(managedReferenceFieldTypename, result);
            }

            return result;
        }

        private GUIContent GetTypeName(SerializedProperty property)
        {
            // Cache this string.
            var managedReferenceFullTypename = property.managedReferenceFullTypename;

            if (string.IsNullOrEmpty(managedReferenceFullTypename)) return k_NullDisplayName;
            if (this.m_TypeNameCaches.TryGetValue(managedReferenceFullTypename, out var cachedTypeName)) return cachedTypeName;

            var    type     = ManagedReferenceUtility.GetType(managedReferenceFullTypename);
            string typeName = null;

            var typeMenu = TypeMenuUtility.GetAttribute(type);
            if (typeMenu != null)
            {
                typeName = typeMenu.GetTypeNameWithoutPath();
                if (!string.IsNullOrWhiteSpace(typeName)) typeName = ObjectNames.NicifyVariableName(typeName);
            }

            if (string.IsNullOrWhiteSpace(typeName)) typeName = ObjectNames.NicifyVariableName(type.Name);

            var result = new GUIContent(typeName);
            this.m_TypeNameCaches.Add(managedReferenceFullTypename, result);

            return result;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return EditorGUI.GetPropertyHeight(property, true); }
    }
}
#endif
