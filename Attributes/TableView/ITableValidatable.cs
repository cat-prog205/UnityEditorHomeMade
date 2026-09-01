using System.Collections.Generic;

namespace UnityEditorHomeMade
{
    /// <summary>Severity level for validation results.</summary>
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    /// <summary>Describes a single validation issue on a data row.</summary>
    [System.Serializable]
    public struct ValidationResult
    {
        public ValidationSeverity Severity;
        public string Message;
        /// <summary>Optional: the field name this issue relates to. Empty = whole row.</summary>
        public string FieldName;

        public ValidationResult(ValidationSeverity severity, string message, string fieldName = "")
        {
            Severity = severity;
            Message = message;
            FieldName = fieldName;
        }
    }

    /// <summary>
    /// Implement on data classes (list element types) to enable inline validation
    /// in the Table View. The renderer will call Validate() on each row and
    /// highlight cells/rows based on the results.
    /// </summary>
    public interface ITableValidatable
    {
        List<ValidationResult> Validate();
    }
}
