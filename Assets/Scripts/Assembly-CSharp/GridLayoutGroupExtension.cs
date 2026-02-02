using UnityEngine;
using UnityEngine.UI;

public static class GridLayoutGroupExtension
{
	public static void SetPadding(this GridLayoutGroup gridLayoutGroup, RectOffset rectOffset)
	{
		gridLayoutGroup.padding.left = rectOffset.left;
		gridLayoutGroup.padding.right = rectOffset.right;
		gridLayoutGroup.padding.top = rectOffset.top;
		gridLayoutGroup.padding.bottom = rectOffset.bottom;
	}

	public static void UpdateValue(this GridLayoutGroup gridLayoutGroup, Vector2 cellSize, Vector2 spacing, int constraintCount)
	{
		gridLayoutGroup.cellSize = cellSize;
		gridLayoutGroup.spacing = spacing;
		gridLayoutGroup.constraintCount = constraintCount;
	}
}
