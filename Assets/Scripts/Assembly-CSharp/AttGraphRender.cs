using UnityEngine;
using UnityEngine.UI;

public class AttGraphRender : BaseMeshEffect
{
	public bool bAutoUpdate;

	public Color32 DrawBgColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public Color32 DrawFgColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public float fMaxAttr = 100f;

	public float fRadiusSize = 10f;

	public float[] fArrts;

	public Vector3[] vPoss;

	private float minx;

	private float maxx;

	private float miny;

	private float maxy;

	public void UpdateMesh()
	{
		if (vPoss == null || vPoss.Length != 10)
		{
			vPoss = new Vector3[10];
		}
		for (int i = 0; i < 5; i++)
		{
			vPoss[i] = Quaternion.Euler(0f, 0f, 72 * i) * new Vector3(0f, fRadiusSize);
			vPoss[i + 5] = Quaternion.Euler(0f, 0f, 72 * i) * new Vector3(0f, fArrts[i] / fMaxAttr * fRadiusSize);
			if (minx > vPoss[i].x)
			{
				minx = vPoss[i].x;
			}
			if (maxx < vPoss[i].x)
			{
				maxx = vPoss[i].x;
			}
			if (miny > vPoss[i].y)
			{
				miny = vPoss[i].y;
			}
			if (maxy < vPoss[i].y)
			{
				maxy = vPoss[i].y;
			}
		}
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (fArrts == null || fArrts.Length != 5)
		{
			fArrts = new float[5];
			for (int i = 0; i < 5; i++)
			{
				fArrts[i] = 10f;
			}
		}
		if (vPoss == null || vPoss.Length != 10)
		{
			UpdateMesh();
		}
		vh.Clear();
		if (bAutoUpdate)
		{
			UpdateMesh();
		}
		float num = maxx - minx;
		float num2 = maxy - miny;
		for (int j = 0; j < 5; j++)
		{
			vh.AddVert(vPoss[j], DrawBgColor, new Vector2((vPoss[j].x - minx) / num, (vPoss[j].y - miny) / num2));
		}
		for (int k = 5; k < 10; k++)
		{
			vh.AddVert(vPoss[k], DrawFgColor, new Vector2((vPoss[k].x - minx) / num, (vPoss[k].y - miny) / num2));
		}
		vh.AddTriangle(0, 1, 2);
		vh.AddTriangle(2, 3, 0);
		vh.AddTriangle(3, 4, 0);
		vh.AddTriangle(5, 6, 7);
		vh.AddTriangle(7, 8, 5);
		vh.AddTriangle(8, 9, 5);
	}
}
