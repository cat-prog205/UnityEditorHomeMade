using UnityEngine;

namespace UnityEditorHomeMade
{

    public class UnderlineTitleAttribute : PropertyAttribute
    {
        public string Title { get; private set; }
        public int    Space { get; private set; }

        public UnderlineTitleAttribute(string title, int space = 12)
        {
            this.Title = title;
            this.Space = space;
        }
    }
}