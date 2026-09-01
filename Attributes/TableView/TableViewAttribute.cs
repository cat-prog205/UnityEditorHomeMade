using System;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Marks a List or Array field to be rendered as a table in the Inspector.
    /// Place on fields of type List&lt;T&gt; or T[] in ScriptableObjects.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TableViewAttribute : Attribute
    {
        /// <summary>If true, the table is read-only (no editing cells).</summary>
        public bool ReadOnly { get; set; } = false;        /// <summary>Maximum number of rows visible before scrolling. Default: 20.</summary>
        public int MaxVisibleRows { get; set; } = 20;


        /// <summary>Height of each data row in pixels.</summary>
        public float RowHeight { get; set; } = 20f;

        /// <summary>Default alignment for int/float fields. Default: Center.</summary>
        public TableAlignment NumberAlignment { get; set; } = TableAlignment.Center;

        /// <summary>Default alignment for enum fields. Default: Center.</summary>
        public TableAlignment EnumAlignment { get; set; } = TableAlignment.Center;

        /// <summary>Default alignment for string fields. Default: Left.</summary>
        public TableAlignment StringAlignment { get; set; } = TableAlignment.Left;

        /// <summary>Default alignment for bool fields. Default: Center.</summary>
        public TableAlignment BoolAlignment { get; set; } = TableAlignment.Center;
    }
}
