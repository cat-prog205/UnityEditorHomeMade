using UnityEngine;
using UnityEditor;

namespace UnityEditorHomeMade
{

    [CustomPropertyDrawer(typeof(UnderlineTitleAttribute))]
    public class UnderlineTitleDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var attributeAsUnderlineTitle = this.attribute as UnderlineTitleAttribute;

            position        =  EditorGUI.IndentedRect(position);
            position.height =  EditorGUIUtility.singleLineHeight;
            position.y      += attributeAsUnderlineTitle.Space;

            GUI.Label(position, attributeAsUnderlineTitle.Title, EditorStyles.boldLabel);

            position.y      += EditorGUIUtility.singleLineHeight;
            position.height =  1f;
            EditorGUI.DrawRect(position, Color.gray);
        }

        public override float GetHeight()
        {
            var attributeAsUnderlineTitle = this.attribute as UnderlineTitleAttribute;

            return attributeAsUnderlineTitle.Space + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
        }
    }
}