using System;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Declares metadata for a column when the parent list is rendered as a table.
    /// Place on serialized fields inside the list element type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TableColumnAttribute : Attribute
    {
        /// <summary>Column header text. Defaults to the field name if empty.</summary>
        public string Header { get; }

        /// <summary>Fixed column width in pixels. 0 = auto-size.</summary>
        public float Width { get; set; } = 0f;

        /// <summary>Column display order. Lower values appear first.</summary>
        public int Order { get; set; } = 100;

        /// <summary>If true, this column is read-only regardless of the table setting.</summary>
        public bool ReadOnly { get; set; } = false;

        /// <summary>Tooltip text shown when hovering the column header.</summary>
        public string Tooltip { get; set; } = "";

        /// <summary>
        /// Text alignment override for this column.
        /// Auto = inherit from TableView default for this field type.
        /// </summary>
        public TableAlignment Alignment { get; set; } = TableAlignment.Auto;

        public TableColumnAttribute(string header = "")
        {
            Header = header;
        }
    }
}
