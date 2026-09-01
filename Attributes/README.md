# Custom Attributes

Custom PropertyAttributes to enhance Unity Inspector experience without third-party dependencies.

## Quick Reference

| Attribute               | Purpose                              |
| ----------------------- | ------------------------------------ |
| `[Button]`              | Turn methods into clickable buttons  |
| `[ReadOnly]`            | Display field without allowing edits |
| `[Title]`               | Add section headers                  |
| `[Required]`            | Show error when reference is null    |
| `[InfoBox]`             | Display info/warning/error messages  |
| `[ShowIf]` / `[HideIf]` | Conditionally show/hide fields       |
| `[BoxGroup]`            | Group fields in a labeled box        |

---

## Usage Guide

### [Button]

Turns a method into a clickable button in the Inspector. Supports parameters.

```csharp
[Button]
private void TestMethod() { }

[Button("Custom Label")]
private void MethodWithParams(string text, int count) { }
```

### [ReadOnly]

Displays the field value but prevents editing.

```csharp
[ReadOnly] public float currentHealth = 100f;
```

### [Title]

Adds a bold header before the field.

```csharp
[Title("Player Settings")]
public float speed = 5f;
```

### [Required]

Shows an error box when a reference field is null.

```csharp
[Required("Prefab is required!")]
public GameObject playerPrefab;
```

### [InfoBox]

Displays an information, warning, or error box above the field.

```csharp
[InfoBox("Adjust carefully.", InfoBoxType.Info)]
public float value;

[InfoBox("This affects performance!", InfoBoxType.Warning)]
public int poolSize;
```

### [ShowIf] / [HideIf]

Conditionally shows or hides a field based on a boolean field.

```csharp
public bool useAdvanced;

[ShowIf("useAdvanced")]
public float advancedValue;

[HideIf("useAdvanced")]
public string basicMode;
```

### [BoxGroup]

Groups multiple fields into a labeled box.

```csharp
[BoxGroup("Movement")]
public float speed;

[BoxGroup("Movement")]
public float jumpForce;
```

---

## Table View Attributes

Render `List<T>` or `T[]` fields as spreadsheet-style tables in the Inspector. Located in `Attributes/TableView/`.

| Attribute          | Purpose                                          |
| ------------------ | ------------------------------------------------ |
| `[TableView]`      | Mark a list/array field to render as a table     |
| `[TableColumn]`    | Per-column metadata (header, width, order, etc.) |
| `TableAlignment`   | Enum: Auto, Left, Center, Right                  |
| `ITableValidatable`| Interface for row-level validation highlights    |

### [TableView]

```csharp
[TableView(MaxVisibleRows = 15, RowHeight = 22f)]
public List<EnemyData> enemies;
```

Properties: `ReadOnly`, `MaxVisibleRows` (default 20), `RowHeight` (default 20), `NumberAlignment`, `EnumAlignment`, `StringAlignment`, `BoolAlignment`.

### [TableColumn]

```csharp
[System.Serializable]
public class EnemyData
{
    [TableColumn("Name", Order = 0)] public string name;
    [TableColumn("HP", Width = 60, Order = 1)] public int hp;
    [TableColumn("Type", Order = 2)] public EnemyType type;
}
```

Properties: `Header`, `Width` (0 = auto), `Order`, `ReadOnly`, `Tooltip`, `Alignment`.

### ITableValidatable

```csharp
public class EnemyData : ITableValidatable
{
    public List<ValidationResult> Validate()
    {
        var results = new List<ValidationResult>();
        if (hp <= 0)
            results.Add(new ValidationResult(ValidationSeverity.Error, "HP must be > 0", nameof(hp)));
        return results;
    }
}
```

Features: CSV import/export, right-click context menu (insert, duplicate, remove), row selection, hover highlighting.

---

## Folder Structure

```
Attributes/
├── Inspector/    # PropertyAttributes for Inspector fields
└── TableView/    # Table rendering system for BlueprintBase lists
```

## Technical Notes

- Inspector attributes inherit from `UnityEngine.PropertyAttribute`
- PropertyDrawers are located in `Assets/Scripts/Editor/Drawers/`
- BoxGroup is handled by the global `ButtonEditor` custom editor
- TableView editor scripts are in `Assets/Scripts/Editor/` (TableViewEditor, TableRenderer, TableCsvUtility)
- TableView only applies to `BlueprintBase` subclasses via `[CustomEditor(typeof(BlueprintBase), true)]`
