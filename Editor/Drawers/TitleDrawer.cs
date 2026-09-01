using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    [CustomPropertyDrawer(typeof(TitleAttribute))]
    public class TitleDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var attr = (TitleAttribute)attribute;
            
            position.y += 8;
            position.height = EditorGUIUtility.singleLineHeight;
            
            var style = attr.Bold ? EditorStyles.boldLabel : EditorStyles.label;
            EditorGUI.LabelField(position, attr.Label, style);
        }

        public override float GetHeight()
        {
            return EditorGUIUtility.singleLineHeight + 12;
        }
    }
}
