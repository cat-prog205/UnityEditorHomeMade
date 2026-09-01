using System;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Groups multiple fields within a named box in the Inspector.
    /// This attribute is handled by the custom ButtonEditor, not a PropertyDrawer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class BoxGroupAttribute : Attribute
    {
        public string GroupName { get; }

        public BoxGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}
