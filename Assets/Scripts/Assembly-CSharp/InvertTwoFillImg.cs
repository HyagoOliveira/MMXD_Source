using UnityEngine;
using UnityEngine.UI;

public class InvertTwoFillImg : BaseMeshEffect
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
		UIVertex[] array = new UIVertex[72];
		float num = 0f;
		float num2 = 0f;
		vh.PopulateUIVertex(ref array[0], 0);
		vh.PopulateUIVertex(ref array[3], 35);
		float num3 = array[3].position.x - array[0].position.x;
		num2 = array[0].position.x - num3;
		float num4 = fValue * num3 * 2f;
		int num5 = 0;
		for (int i = 0; i < 36; i++)
		{
			vh.PopulateUIVertex(ref array[i], i);
		}
		for (int j = 36; j < 72; j++)
		{
			vh.PopulateUIVertex(ref array[j], j - 36);
		}
		float num6 = 0f;
		int num7 = 72;
		vh.Clear();
		while (num7 > 36)
		{
			if (array[num7 - 1].position.x - array[num7 - 4].position.x > 0f)
			{
				num6 += array[num7 - 1].position.x - array[num7 - 4].position.x;
				float num8 = 0f;
				if (num6 > num4)
				{
					num8 = (num6 - num4) / (array[num7 - 1].position.x - array[num7 - 4].position.x);
				}
				Vector2 vector = array[num7 - 1].uv0 - array[num7 - 4].uv0;
				vector *= num8;
				for (int num9 = num7; num9 > num7 - 12; num9 -= 4)
				{
					num = array[num9 - 1].position.x - array[num9 - 4].position.x;
					num *= 1f - num8;
					Vector3 position = array[num9 - 4].position;
					array[num9 - 4].position = new Vector3(num2 + num, position.y, position.z);
					position = array[num9 - 3].position;
					array[num9 - 3].position = new Vector3(num2 + num, position.y, position.z);
					position = array[num9 - 2].position;
					array[num9 - 2].position = new Vector3(num2, position.y, position.z);
					position = array[num9 - 1].position;
					array[num9 - 1].position = new Vector3(num2, position.y, position.z);
					float y = array[num9 - 3].uv0.y;
					float y2 = array[num9 - 4].uv0.y;
					array[num9 - 4].uv0 = new Vector2(array[num9 - 4].uv0.x + vector.x, y);
					array[num9 - 3].uv0 = new Vector2(array[num9 - 3].uv0.x + vector.x, y2);
					array[num9 - 2].uv0 = new Vector2(array[num9 - 2].uv0.x, y2);
					array[num9 - 1].uv0 = new Vector2(array[num9 - 1].uv0.x, y);
					num5 = vh.currentVertCount;
					vh.AddVert(array[num9 - 4]);
					vh.AddVert(array[num9 - 3]);
					vh.AddVert(array[num9 - 2]);
					vh.AddVert(array[num9 - 1]);
					vh.AddTriangle(num5, num5 + 1, num5 + 2);
					vh.AddTriangle(num5 + 2, num5 + 3, num5);
				}
				num2 += num;
				if (num6 > num4)
				{
					return;
				}
			}
			num7 -= 12;
		}
		for (num7 = 0; num7 < 36; num7 += 12)
		{
			if (array[num7 + 3].position.x - array[num7].position.x > 0f)
			{
				num6 += array[num7 + 3].position.x - array[num7].position.x;
				float num10 = 1f;
				if (num6 > num4)
				{
					num10 = 1f - (num6 - num4) / (array[num7 + 3].position.x - array[num7].position.x);
				}
				Vector2 vector2 = array[num7 + 3].uv0 - array[num7].uv0;
				vector2 *= num10;
				for (int k = num7; k < num7 + 12; k += 4)
				{
					num = array[k + 3].position.x - array[k].position.x;
					num *= num10;
					num2 = array[k].position.x + num;
					Vector3 position = array[k + 2].position;
					array[k + 2].position = new Vector3(num2, position.y, position.z);
					position = array[k + 3].position;
					array[k + 3].position = new Vector3(num2, position.y, position.z);
					array[k + 2].uv0 = new Vector2(array[k].uv0.x + vector2.x, array[k + 2].uv0.y);
					array[k + 3].uv0 = new Vector2(array[k].uv0.x + vector2.x, array[k + 3].uv0.y);
					num5 = vh.currentVertCount;
					vh.AddVert(array[k]);
					vh.AddVert(array[k + 1]);
					vh.AddVert(array[k + 2]);
					vh.AddVert(array[k + 3]);
					vh.AddTriangle(num5, num5 + 1, num5 + 2);
					vh.AddTriangle(num5 + 2, num5 + 3, num5);
				}
			}
			if (num6 > num4)
			{
				break;
			}
		}
	}

	public void SetFValue(float fv)
	{
		fValue = fv;
		base.graphic.SetVerticesDirty();
	}
}
