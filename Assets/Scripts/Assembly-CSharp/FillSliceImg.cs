using UnityEngine;
using UnityEngine.UI;

public class FillSliceImg : BaseMeshEffect
{
	[Range(0f, 1f)]
	public float fValue;

	public override void ModifyMesh(VertexHelper vh)
	{
		if (vh.currentVertCount < 36)
		{
			return;
		}
		if (fValue == 0f)
		{
			vh.Clear();
			return;
		}
		if (fValue > 1f)
		{
			fValue = 1f;
		}
		UIVertex[] array = new UIVertex[4];
		float num = 0f;
		for (int i = 12; i < 24; i += 4)
		{
			for (int j = 0; j < 4; j++)
			{
				vh.PopulateUIVertex(ref array[j], i + j);
			}
			num = array[3].position.x - array[0].position.x;
			array[2].position.x = array[0].position.x + num * fValue;
			array[3].position.x = array[0].position.x + num * fValue;
			for (int k = 0; k < 4; k++)
			{
				vh.SetUIVertex(array[k], i + k);
			}
		}
		for (int l = 24; l < 36; l += 4)
		{
			for (int m = 0; m < 4; m++)
			{
				vh.PopulateUIVertex(ref array[m], l + m);
				array[m].position.x -= num * (1f - fValue);
				vh.SetUIVertex(array[m], l + m);
			}
		}
	}

	public void SetFValue(float fv)
	{
		fValue = fv;
		base.graphic.SetVerticesDirty();
	}
}
