#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditorHomeMade
{
#if !TESTING_INSPECTOR_HAS_INSPECTOR_GADGETS
    [CustomPropertyDrawer(typeof(Object), true)]
#endif
    public sealed class QuickReferenceDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 18f;
        private const float ButtonSpacing = 2f;

        private static GUIContent _searchIcon;
        private static GUIStyle _buttonStyle;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, false);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var referenceType = GetReferenceType();
            if (referenceType == null || referenceType == typeof(Object))
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var showButton = property.objectReferenceValue == null || Event.current.alt;
            var fieldPosition = position;

            if (showButton)
            {
                fieldPosition.width -= ButtonWidth + ButtonSpacing;

                var buttonPosition = new Rect(
                    fieldPosition.xMax + ButtonSpacing,
                    position.y,
                    ButtonWidth,
                    EditorGUIUtility.singleLineHeight);

                if (GUI.Button(buttonPosition, GetSearchIcon(referenceType, property), GetButtonStyle()))
                {
                    AssignBestReference(property, referenceType);
                    GUI.changed = true;
                }
            }

            EditorGUI.PropertyField(fieldPosition, property, label);
        }

        private Type GetReferenceType()
        {
            if (fieldInfo == null)
                return null;

            var type = fieldInfo.FieldType;
            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];

            return type;
        }

        private static GUIContent GetSearchIcon(Type referenceType, SerializedProperty property)
        {
            _searchIcon ??= new GUIContent(EditorGUIUtility.IconContent("ViewToolZoom").image);

            var searchesComponents = typeof(Component).IsAssignableFrom(referenceType) &&
                property.serializedObject.targetObject is Component;

            _searchIcon.tooltip = searchesComponents
                ? "Click to find an appropriate component on this object, its parents, its children, or in the scene.\n\nCtrl + Click searches prefab assets in the project."
                : "Click to find an appropriate asset in the project.";

            return _searchIcon;
        }

        private static GUIStyle GetButtonStyle()
        {
            return _buttonStyle ??= new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(0, 0, 0, 0),
            };
        }

        private static void AssignBestReference(SerializedProperty property, Type referenceType)
        {
            Object result;
            var targetComponent = property.serializedObject.targetObject as Component;

            if (targetComponent != null && typeof(Component).IsAssignableFrom(referenceType) && !Event.current.control)
                result = FindComponent(targetComponent.gameObject, referenceType, property.displayName);
            else
                result = FindAsset(referenceType, property.displayName);

            if (result == null)
                return;

            property.objectReferenceValue = result;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static Component FindComponent(GameObject owner, Type componentType, string nameHint)
        {
            var component = owner.GetComponent(componentType);
            if (component != null)
                return component;

            component = FindBestMatch(owner.GetComponentsInParent(componentType, true), nameHint) as Component;
            if (component != null)
                return component;

            component = FindBestMatch(owner.GetComponentsInChildren(componentType, true), nameHint) as Component;
            if (component != null)
                return component;

            var sceneComponents = Resources.FindObjectsOfTypeAll(componentType);
            return FindBestMatch(sceneComponents, nameHint, IsSceneObject) as Component;
        }

        private static bool IsSceneObject(Object candidate)
        {
            if (candidate is not Component component)
                return false;

            return component.gameObject.scene.IsValid() && !EditorUtility.IsPersistent(component);
        }

        private static Object FindAsset(Type referenceType, string nameHint)
        {
            var isComponent = typeof(Component).IsAssignableFrom(referenceType);
            var filter = isComponent ? "t:Prefab" : $"t:{referenceType.Name}";
            var guids = AssetDatabase.FindAssets(filter);

            Object bestAsset = null;
            var bestScore = int.MaxValue;

            try
            {
                for (var index = 0; index < guids.Length; index++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[index]);
                    var progress = guids.Length == 0 ? 1f : index / (float)guids.Length;

                    if (EditorUtility.DisplayCancelableProgressBar(
                        $"Find {referenceType.Name}",
                        path,
                        progress))
                    {
                        return null;
                    }

                    Object candidate;
                    if (isComponent)
                    {
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        candidate = prefab != null ? prefab.GetComponentInChildren(referenceType, true) : null;
                    }
                    else
                    {
                        candidate = AssetDatabase.LoadAssetAtPath(path, referenceType);
                    }

                    UpdateBestMatch(candidate, nameHint, ref bestAsset, ref bestScore);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return bestAsset;
        }

        private static Object FindBestMatch(Object[] candidates, string nameHint, Func<Object, bool> predicate = null)
        {
            Object bestMatch = null;
            var bestScore = int.MaxValue;

            foreach (var candidate in candidates)
            {
                if (predicate != null && !predicate(candidate))
                    continue;

                UpdateBestMatch(candidate, nameHint, ref bestMatch, ref bestScore);
            }

            return bestMatch;
        }

        private static void UpdateBestMatch(Object candidate, string nameHint, ref Object bestMatch, ref int bestScore)
        {
            if (candidate == null)
                return;

            var score = CalculateDistance(Normalize(nameHint), Normalize(candidate.name));
            if (score >= bestScore)
                return;

            bestMatch = candidate;
            bestScore = score;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace(" ", string.Empty).ToLowerInvariant();
        }

        private static int CalculateDistance(string first, string second)
        {
            if (first.Length == 0)
                return second.Length;

            if (second.Length == 0)
                return first.Length;

            var previous = new int[second.Length + 1];
            var current = new int[second.Length + 1];

            for (var column = 0; column <= second.Length; column++)
                previous[column] = column;

            for (var row = 1; row <= first.Length; row++)
            {
                current[0] = row;

                for (var column = 1; column <= second.Length; column++)
                {
                    var replacementCost = first[row - 1] == second[column - 1] ? 0 : 1;
                    current[column] = Math.Min(
                        Math.Min(current[column - 1] + 1, previous[column] + 1),
                        previous[column - 1] + replacementCost);
                }

                var swap = previous;
                previous = current;
                current = swap;
            }

            return previous[second.Length];
        }
    }
}

#endif
