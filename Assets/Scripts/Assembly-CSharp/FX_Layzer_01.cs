using UnityEngine;

[ExecuteInEditMode]
public class FX_Layzer_01 : MonoBehaviour
{
	public GameObject lineStart;

	public GameObject lineEnd;

	public Material lineMaterial;

	public Shader lineShader;

	public int uvTieX = 1;

	public int uvTieY = 1;

	public int timeSpeed = 10;

	private Vector2 uvSize;

	private int lastIndex = -1;

	private LineRenderer lineRenderer;

	private void Start()
	{
	}

	private void Update()
	{
		uvSize = new Vector2(1f / (float)uvTieX, 1f / (float)uvTieY);
		int num = (int)(Time.timeSinceLevelLoad * (float)timeSpeed) % (uvTieX * uvTieY);
		if (num != lastIndex)
		{
			int num2 = num % uvTieX;
			int num3 = num / uvTieX;
			Vector2 value = new Vector2((float)num2 * uvSize.x, 1f - uvSize.y - (float)num3 * uvSize.y);
			lineRenderer = GetComponent<LineRenderer>();
			lineRenderer.sharedMaterial.SetTextureOffset("_MainTex", value);
			lineRenderer.sharedMaterial.SetTextureScale("_MainTex", uvSize);
			lineRenderer.material = lineMaterial;
			lineMaterial.shader = lineShader;
			lineRenderer.SetPosition(0, lineStart.transform.localPosition);
			lineRenderer.SetPosition(1, lineEnd.transform.localPosition);
			lastIndex = num;
		}
	}
}
