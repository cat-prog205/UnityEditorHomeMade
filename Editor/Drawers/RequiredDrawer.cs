using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (RequiredAttribute)attribute;
            bool isNull = property.objectReferenceValue == null;
            
            if (isNull)
            {
                string msg = string.IsNullOrEmpty(attr.Message) ? $"{property.displayName} is required!" : attr.Message;
                Rect helpBox = new Rect(position.x, position.y, position.width, 30);
                EditorGUI.HelpBox(helpBox, msg, MessageType.Error);
                position.y += 32;
            }
            
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property, label, true);
            if (property.objectReferenceValue == null)
                height += 34;
            return height;
        }
    }
}
