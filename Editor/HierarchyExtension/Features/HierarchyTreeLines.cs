using UnityEditor;
using UnityEngine;

namespace UnityEditorHomeMade
{
    /// <summary>
    /// Draws tree lines connecting parent-child relationships in the Hierarchy.
    /// Lines are aligned with the parent's foldout arrow icons.
    /// </summary>
    public static class HierarchyTreeLines
    {
        // Unity's indent per level in hierarchy
        private const float IndentPerLevel = 14f;

        public static void Draw(GameObject go, Rect rect, HierarchySettings settings)
        {
            var transform = go.transform;
            if (transform.parent == null)
                return;

            var lineColor = settings.treeLineColor;
            var lineWidth = settings.treeLineWidth;
            int depth = GetDepth(transform);
            
            if (depth <= 0)
                return;

            // Draw vertical lines for each ancestor level (including current parent)
            DrawAncestorLines(transform, rect, lineColor, lineWidth, depth);
            
            // Draw the branch (L-shape or T-shape) for current item
            DrawBranch(transform, rect, lineColor, lineWidth, depth);
        }

        private static void DrawAncestorLines(Transform transform, Rect rect, Color lineColor, float lineWidth, int depth)
        {
            // Start from the direct parent and go up
            var current = transform;
            int level = depth;
            
            while (current.parent != null && level > 0)
            {
                var parent = current.parent;
                
                // Check if CURRENT item (not parent) has siblings below it
                // This determines if we need a continuing vertical line at this parent's level
                if (HasSiblingsBelow(current))
                {
                    // Calculate X position for the parent's foldout arrow
                    float lineX = rect.x - (IndentPerLevel * (depth - level + 2)) + (IndentPerLevel / 2f) - 2f;
                    
                    // Draw full-height vertical line
                    var verticalRect = new Rect(lineX, rect.y, lineWidth, rect.height);
                    EditorGUI.DrawRect(verticalRect, lineColor);
                }
                
                current = parent;
                level--;
            }
        }

        private static void DrawBranch(Transform transform, Rect rect, Color lineColor, float lineWidth, int depth)
        {
            // Line X at the PARENT's foldout arrow center
            float lineX = rect.x - (IndentPerLevel * 2) + (IndentPerLevel / 2f) - 2f;
            float midY = rect.y + rect.height / 2f;
            
            bool isLastChild = !HasSiblingsBelow(transform);
            
            if (isLastChild)
            {
                // L-shape: vertical from top to middle only
                var verticalRect = new Rect(lineX, rect.y, lineWidth, rect.height / 2f);
                EditorGUI.DrawRect(verticalRect, lineColor);
            }
            else
            {
                // T-shape: full vertical line (connects to next sibling)
                var verticalRect = new Rect(lineX, rect.y, lineWidth, rect.height);
                EditorGUI.DrawRect(verticalRect, lineColor);
            }
            
            // Horizontal branch - stop near the child's arrow
            float branchLength = IndentPerLevel - 4f;
            var horizontalRect = new Rect(lineX, midY - (lineWidth / 2f), branchLength, lineWidth);
            EditorGUI.DrawRect(horizontalRect, lineColor);
        }

        private static bool HasSiblingsBelow(Transform transform)
        {
            if (transform.parent == null)
                return false;
            
            int siblingIndex = transform.GetSiblingIndex();
            int siblingCount = transform.parent.childCount;
            
            return siblingIndex < siblingCount - 1;
        }

        private static int GetDepth(Transform transform)
        {
            int depth = 0;
            var parent = transform.parent;
            while (parent != null)
            {
                depth++;
                parent = parent.parent;
            }
            return depth;
        }
    }
}
