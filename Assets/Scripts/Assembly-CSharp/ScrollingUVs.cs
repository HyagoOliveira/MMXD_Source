using UnityEngine;

public class ScrollingUVs : MonoBehaviour
{
	public Vector2 uvAnimationRate = new Vector2(1f, 0f);

	[SerializeField]
	private SkinnedMeshRenderer m_render;

	private Vector2 uvOffset = Vector2.zero;

	private int textureID;

	private float val = 10f;

	private void Awake()
	{
		textureID = Shader.PropertyToID("_MainTex");
	}

	private void Start()
	{
		if (m_render == null)
		{
			m_render = GetComponent<SkinnedMeshRenderer>();
		}
	}

	private void LateUpdate()
	{
		val += Time.deltaTime;
		val = Mathf.Repeat(val, 100f);
		uvOffset = uvAnimationRate * val;
		m_render.material.SetTextureOffset(textureID, uvOffset);
	}
}
