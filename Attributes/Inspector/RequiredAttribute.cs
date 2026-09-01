using UnityEngine;

namespace UnityEditorHomeMade
{
    public class RequiredAttribute : PropertyAttribute
    {
        public string Message { get; }

        public RequiredAttribute(string message = null)
        {
            Message = message;
        }
    }
}
