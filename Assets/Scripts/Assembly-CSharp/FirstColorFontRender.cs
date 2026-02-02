using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FirstColorFontRender : BaseMeshEffect
{
	public Color m_ColorFirst = Color.white;

	public Color m_ColorOther = Color.white;

	public float m_fFirstFontScale = 1.2f;

	private static readonly Vector2[] Position = new Vector2[4]
	{
		Vector2.up,
		Vector2.one,
		Vector2.right,
		Vector2.zero
	};

	public override void ModifyMesh(VertexHelper vh)
	{
		if (vh.currentVertCount <= 0)
		{
			return;
		}
		UIVertex vertex = default(UIVertex);
		Vector3[] array = new Vector3[4];
		for (int i = 0; i < vh.currentVertCount && i < 4; i++)
		{
			vh.PopulateUIVertex(ref vertex, i);
			array[i] = vertex.position;
		}
		float num = array[1].x - array[0].x;
		float num2 = num * m_fFirstFontScale;
		array[1].x = array[0].x + num2;
		array[2].x = array[0].x + num2;
		float num3 = (array[0].y - array[3].y) * m_fFirstFontScale;
		array[0].y = array[3].y + num3;
		array[1].y = array[3].y + num3;
		float num4 = num2 - num;
		for (int j = 0; j < vh.currentVertCount; j++)
		{
			vh.PopulateUIVertex(ref vertex, j);
			if (j < 4)
			{
				vertex.position = array[j];
				vertex.color = m_ColorFirst;
			}
			else
			{
				vertex.position.Set(vertex.position.x + num4, vertex.position.y, vertex.position.z);
				vertex.color = m_ColorOther;
			}
			vh.SetUIVertex(vertex, j);
		}
	}
}
