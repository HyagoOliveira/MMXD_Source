using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasRenderer))]
[AddComponentMenu("UI/UISkewImage (UI)", 99)]
[ExecuteInEditMode]
public class UISkewImage : Image
{
	[SerializeField]
	private Vector2 offsetLeftButtom = Vector2.zero;

	[SerializeField]
	private Vector2 offsetRightButtom = Vector2.zero;

	[SerializeField]
	private Vector2 offsetLeftTop = Vector2.zero;

	[SerializeField]
	private Vector2 offsetRightTop = Vector2.zero;

	public Vector2 OffsetLeftButtom
	{
		get
		{
			return offsetLeftButtom;
		}
		set
		{
			offsetLeftButtom = value;
			SetAllDirty();
		}
	}

	public Vector2 OffsetRightButtom
	{
		get
		{
			return offsetRightButtom;
		}
		set
		{
			offsetRightButtom = value;
			SetAllDirty();
		}
	}

	public Vector2 OffsetLeftTop
	{
		get
		{
			return offsetLeftTop;
		}
		set
		{
			offsetLeftTop = value;
			SetAllDirty();
		}
	}

	public Vector2 OffsetRightTop
	{
		get
		{
			return offsetRightTop;
		}
		set
		{
			offsetRightTop = value;
			SetAllDirty();
		}
	}

	private Vector2 GetOffsetVector(int i)
	{
		switch (i)
		{
		case 0:
			return offsetLeftButtom;
		case 1:
			return offsetLeftTop;
		case 2:
			return offsetRightTop;
		default:
			return offsetRightButtom;
		}
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		base.OnPopulateMesh(toFill);
		int currentVertCount = toFill.currentVertCount;
		for (int i = 0; i < currentVertCount; i++)
		{
			UIVertex vertex = default(UIVertex);
			toFill.PopulateUIVertex(ref vertex, i);
			Vector2 offsetVector = GetOffsetVector(i);
			vertex.position += new Vector3(offsetVector.x, offsetVector.y, 0f);
			toFill.SetUIVertex(vertex, i);
		}
	}
}
