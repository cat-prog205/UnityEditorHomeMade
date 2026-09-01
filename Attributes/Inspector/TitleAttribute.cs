using UnityEngine;

namespace UnityEditorHomeMade
{
    public class TitleAttribute : PropertyAttribute
    {
        public string Label { get; }
        public bool Bold { get; }

        public TitleAttribute(string label, bool bold = true)
        {
            Label = label;
            Bold = bold;
        }
    }
}
