using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var attr = (InfoBoxAttribute)attribute;
            
            MessageType msgType = attr.Type switch
            {
                InfoBoxType.Warning => MessageType.Warning,
                InfoBoxType.Error => MessageType.Error,
                InfoBoxType.Info => MessageType.Info,
                _ => MessageType.None
            };
            
            position.height = GetHeight() - 4;
            EditorGUI.HelpBox(position, attr.Message, msgType);
        }

        public override float GetHeight()
        {
            var attr = (InfoBoxAttribute)attribute;
            float minHeight = 40;
            float textHeight = EditorStyles.helpBox.CalcHeight(new GUIContent(attr.Message), EditorGUIUtility.currentViewWidth - 50);
            return Mathf.Max(minHeight, textHeight + 8);
        }
    }
}
