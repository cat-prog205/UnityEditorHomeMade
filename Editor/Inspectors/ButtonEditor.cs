using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonEditor : UnityEditor.Editor
    {
        private Dictionary<string, object[]> parameterCache = new();
        private Dictionary<string, bool> foldoutStates = new();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Collect BoxGroup fields
            var boxGroups = new Dictionary<string, List<SerializedProperty>>();
            var normalProperties = new List<SerializedProperty>();
            
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            
            while (iterator.NextVisible(false))
            {
                var field = target.GetType().GetField(iterator.name, 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (field != null)
                {
                    var boxGroup = field.GetCustomAttribute<BoxGroupAttribute>();
                    if (boxGroup != null)
                    {
                        if (!boxGroups.ContainsKey(boxGroup.GroupName))
                            boxGroups[boxGroup.GroupName] = new List<SerializedProperty>();
                        boxGroups[boxGroup.GroupName].Add(iterator.Copy());
                        continue;
                    }
                }
                normalProperties.Add(iterator.Copy());
            }
            
            // Draw normal properties
            foreach (var prop in normalProperties)
            {
                EditorGUILayout.PropertyField(prop, true);
            }
            
            // Draw BoxGroups
            foreach (var group in boxGroups)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(group.Key, EditorStyles.boldLabel);
                
                foreach (var prop in group.Value)
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Draw Button methods
            DrawButtonMethods();
        }

        private void DrawButtonMethods()
        {
            var methods = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<ButtonAttribute>() != null);

            if (!methods.Any()) return;

            EditorGUILayout.Space();

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<ButtonAttribute>();
                var label = string.IsNullOrEmpty(attr.Label) ? ObjectNames.NicifyVariableName(method.Name) : attr.Label;
                var parameters = method.GetParameters();

                string key = $"{target.GetEntityId()}_{method.Name}_{string.Join("_", parameters.Select(p => p.ParameterType.Name))}";

                if (!parameterCache.ContainsKey(key) || parameterCache[key].Length != parameters.Length)
                {
                    parameterCache[key] = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        parameterCache[key][i] = GetDefaultValue(parameters[i].ParameterType);
                    }
                }

                if (parameters.Length > 0)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    if (!foldoutStates.ContainsKey(key)) foldoutStates[key] = false;
                    
                    Rect headerRect = EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                    
                    GUILayout.FlexibleSpace();
                    Rect foldoutRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.foldout, GUILayout.Width(13));
                    foldoutRect.y += 1;
                    foldoutRect.x += 3;

                    foldoutStates[key] = EditorGUI.Foldout(foldoutRect, foldoutStates[key], GUIContent.none, true);
                    
                    EditorGUILayout.EndHorizontal();

                    if (foldoutStates[key])
                    {
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            string niceName = ObjectNames.NicifyVariableName(parameters[i].Name);
                            parameterCache[key][i] = DrawParameterField(
                                niceName,
                                parameters[i].ParameterType,
                                parameterCache[key][i]
                            );
                        }
                        EditorGUI.indentLevel--;

                        if (GUILayout.Button("Invoke"))
                        {
                            foreach (var t in targets)
                                method.Invoke(t, parameterCache[key]);
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    if (GUILayout.Button(label))
                    {
                        foreach (var t in targets)
                            method.Invoke(t, null);
                    }
                }
            }
        }

        private object DrawParameterField(string name, Type type, object value)
        {
            if (type == typeof(string))
                return EditorGUILayout.TextField(name, (string)value ?? "");
            if (type == typeof(int))
                return EditorGUILayout.IntField(name, (int)value);
            if (type == typeof(float))
                return EditorGUILayout.FloatField(name, (float)value);
            if (type == typeof(bool))
                return EditorGUILayout.Toggle(name, (bool)value);
            if (type == typeof(Vector2))
                return EditorGUILayout.Vector2Field(name, (Vector2)value);
            if (type == typeof(Vector3))
                return EditorGUILayout.Vector3Field(name, (Vector3)value);
            if (type == typeof(Color))
                return EditorGUILayout.ColorField(name, (Color)value);
            if (type.IsEnum)
                return EditorGUILayout.EnumPopup(name, (Enum)value);
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                return EditorGUILayout.ObjectField(name, (UnityEngine.Object)value, type, true);

            EditorGUILayout.LabelField(name, $"[Unsupported: {type.Name}]");
            return value;
        }

        private object GetDefaultValue(Type type)
        {
            if (type == typeof(string)) return "";
            if (type == typeof(Color)) return Color.white;
            if (type.IsValueType) return Activator.CreateInstance(type);
            return null;
        }
    }
}
