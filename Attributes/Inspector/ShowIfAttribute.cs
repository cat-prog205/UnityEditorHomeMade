using UnityEngine;

namespace UnityEditorHomeMade
{
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionField { get; }
        public object CompareValue { get; }
        public bool Invert { get; }

        public ShowIfAttribute(string conditionField)
            : this(conditionField, null, false)
        {
        }

        public ShowIfAttribute(string conditionField, object compareValue, bool invert = false)
        {
            ConditionField = conditionField;
            CompareValue = compareValue;
            Invert = invert;
        }
    }

    public class HideIfAttribute : ShowIfAttribute
    {
        public HideIfAttribute(string conditionField) : base(conditionField, null, true) { }

        public HideIfAttribute(string conditionField, object compareValue) : base(conditionField, compareValue, true) { }
    }
}
