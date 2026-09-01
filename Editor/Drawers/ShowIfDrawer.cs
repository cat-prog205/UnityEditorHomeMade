using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(HideIfAttribute), true)]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldShow(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (ShouldShow(property))
                return EditorGUI.GetPropertyHeight(property, label, true);
            return 0;
        }

        private bool ShouldShow(SerializedProperty property)
        {
            var attr = (ShowIfAttribute)attribute;
            
            // 1. Try finding as a serialized property (fast & handles nesting automatically)
            var conditionPath = GetConditionPath(property, attr.ConditionField);
            var conditionProperty = property.serializedObject.FindProperty(conditionPath);
            
            if (conditionProperty != null)
            {
                return CheckCondition(conditionProperty, attr.CompareValue, attr.Invert);
            }

            // 2. Fallback to reflection for non-serialized fields/properties
            return ShouldShowReflectionNative(property);
        }

        private string GetConditionPath(SerializedProperty property, string conditionField)
        {
            var path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot == -1) return conditionField;
            return path.Substring(0, lastDot + 1) + conditionField;
        }

        private bool CheckCondition(SerializedProperty prop, object compareValue, bool invert)
        {
            bool result = false;
            if (compareValue != null)
            {
                string valStr = GetPropertyValueAsString(prop);
                result = valStr.Equals(compareValue.ToString(), System.StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                if (prop.propertyType == SerializedPropertyType.Boolean)
                    result = prop.boolValue;
                else if (prop.propertyType == SerializedPropertyType.ObjectReference)
                    result = prop.objectReferenceValue != null;
            }
            return invert ? !result : result;
        }

        private string GetPropertyValueAsString(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Enum:
                    return prop.enumNames[prop.enumValueIndex];
                case SerializedPropertyType.Integer:
                    return prop.intValue.ToString();
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Boolean:
                    return prop.boolValue.ToString();
                default:
                    return "";
            }
        }

        private bool ShouldShowReflectionNative(SerializedProperty property)
        {
            var attr = (ShowIfAttribute)attribute;
            object parentObj = GetParentProxy(property);
            if (parentObj == null) return true;

            var type = parentObj.GetType();
            object value = null;
            
            var field = type.GetField(attr.ConditionField, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) value = field.GetValue(parentObj);
            else
            {
                var prop = type.GetProperty(attr.ConditionField, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                if (prop != null) value = prop.GetValue(parentObj);
                else return true;
            }

            bool result;
            if (attr.CompareValue != null)
                result = value != null && value.ToString().Equals(attr.CompareValue.ToString(), System.StringComparison.OrdinalIgnoreCase);
            else
            {
                if (value is bool b) result = b;
                else result = value != null;
            }
            return attr.Invert ? !result : result;
        }

        private object GetParentProxy(SerializedProperty prop)
        {
            var path = prop.propertyPath;
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');

            for (int i = 0; i < elements.Length - 1; i++)
            {
                var element = elements[i];
                if (element == "Array" && i + 1 < elements.Length && elements[i + 1].StartsWith("data["))
                {
                    var indexPart = elements[i + 1];
                    var index = int.Parse(indexPart.Substring(5, indexPart.Length - 6));
                    obj = GetValueAt(obj, index);
                    i++;
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }
            return obj;
        }

        private object GetValue(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();
            var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(source);

            var prop = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null) return prop.GetValue(source, null);

            return null;
        }

        private object GetValueAt(object source, int index)
        {
            if (source is System.Collections.IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();
                for (int i = 0; i <= index; i++)
                {
                    if (!enumerator.MoveNext()) return null;
                }
                return enumerator.Current;
            }
            return null;
        }
    }
}
