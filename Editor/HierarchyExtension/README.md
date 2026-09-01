# Hierarchy Extension

A Unity Hierarchy window enhancement toolkit with visual organization features.

## Features

### Headers
Create prominent headers with `-` prefix:
```
-UI Elements     → Displays as header with colored background
-Managers        → Centered, bold, uppercase text
```

### Separators
Create divider lines with `---` prefix:
```
---             → Horizontal line
--- Section     → Line with centered bold label
```

### Visual Enhancements
- **Tree Lines**: Visual parent-child connections (aligned with foldout arrows)
- **Component Icons**: Camera, Light, AudioSource icons on the right
- **Visibility Toggle**: Click to toggle active/inactive
- **Tag/Layer Indicators**: Show tag and layer info

## Usage

### Quick Create
**GameObject → Afterhours →**
- Header
- Separator / Separator with Label

### Convert Existing
Right-click object → **Afterhours →**
- Convert to Header
- Remove Hierarchy Styling

### Settings
Menu: **Afterhours → Hierarchy Settings**

Customize: Prefixes, Colors, Font styles, Feature toggles

## Installation
Auto-enabled on import. Settings asset created at:
`Assets/Settings/HierarchySettings.asset`
