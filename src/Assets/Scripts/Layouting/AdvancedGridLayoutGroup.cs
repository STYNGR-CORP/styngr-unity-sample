using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Layouting
{
    /// <summary>
    /// Represents the advanced version of the layouting for the <see cref="GridLayoutGroup"/>.
    /// </summary>
    public class AdvancedGridLayoutGroup : GridLayoutGroup
    {
        [SerializeField] protected int cellsPerLine = 1;

        /// <summary>
        /// Sets the horizontal layout size (width).
        /// </summary>
        public override void SetLayoutHorizontal()
        {
            float width = (GetComponent<RectTransform>()).rect.width;
            float useableWidth = width - padding.horizontal - (cellsPerLine - 1) * spacing.x;
            float cellWidth = useableWidth / cellsPerLine;
            cellSize = new Vector2(cellWidth, cellSize.y);
            base.SetLayoutHorizontal();
        }
    }
}
