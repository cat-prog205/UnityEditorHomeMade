using UnityEngine;

namespace UnityEditorHomeMade
{
    public enum InfoBoxType { None, Info, Warning, Error }

    public class InfoBoxAttribute : PropertyAttribute
    {
        public string Message { get; }
        public InfoBoxType Type { get; }

        public InfoBoxAttribute(string message, InfoBoxType type = InfoBoxType.Info)
        {
            Message = message;
            Type = type;
        }
    }
}
