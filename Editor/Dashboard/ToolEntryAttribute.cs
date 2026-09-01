using System;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Registers a tool on the HomeMade Dashboard (Tools &gt; HomeMade &gt; Dashboard).
    /// <para>
    /// On an <see cref="EdWindowBase"/> subclass the attribute is optional: the window
    /// is listed automatically. Use this to set title, category, keywords, or hide it.
    /// </para>
    /// <para>
    /// On a static method (with <c>[MenuItem]</c>) this is required for the action to appear.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ToolEntryAttribute : Attribute
    {
        public string Title { get; }
        public string Category { get; set; }
        public string Keywords { get; set; }
        public bool Hidden { get; set; }
        public int Order { get; set; }

        public ToolEntryAttribute(string title = null, string category = null, string keywords = null)
        {
            Title = title;
            Category = category;
            Keywords = keywords;
        }
    }
}
