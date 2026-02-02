using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextSpacing : BaseMeshEffect
{
	public float _textSpacing = 1f;

	public OrangeText RefOrangeText;

	public override void ModifyMesh(VertexHelper vh)
	{
		if (!IsActive() || vh.currentVertCount == 0)
		{
			return;
		}
		List<UIVertex> list = new List<UIVertex>();
		vh.GetUIVertexStream(list);
		int currentIndexCount = vh.currentIndexCount;
		if (RefOrangeText == null)
		{
			RefOrangeText = GetComponent<OrangeText>();
		}
		bool flag = false;
		if (RefOrangeText.alignment == TextAnchor.MiddleRight)
		{
			flag = true;
		}
		if (flag)
		{
			int num = 1;
			for (int num2 = currentIndexCount - 6 - 1; num2 >= 0; num2--)
			{
				UIVertex uIVertex = list[num2];
				uIVertex.position += new Vector3((0f - _textSpacing) * (float)num, 0f, 0f);
				list[num2] = uIVertex;
				if (num2 % 6 <= 2)
				{
					vh.SetUIVertex(uIVertex, num2 / 6 * 4 + num2 % 6);
				}
				if (num2 % 6 == 4)
				{
					vh.SetUIVertex(uIVertex, num2 / 6 * 4 + num2 % 6 - 1);
				}
				if (num2 % 6 == 0)
				{
					num++;
				}
			}
			return;
		}
		for (int i = 6; i < currentIndexCount; i++)
		{
			UIVertex uIVertex = list[i];
			uIVertex.position += new Vector3(_textSpacing * (float)(i / 6), 0f, 0f);
			list[i] = uIVertex;
			if (i % 6 <= 2)
			{
				vh.SetUIVertex(uIVertex, i / 6 * 4 + i % 6);
			}
			if (i % 6 == 4)
			{
				vh.SetUIVertex(uIVertex, i / 6 * 4 + i % 6 - 1);
			}
		}
	}
}
