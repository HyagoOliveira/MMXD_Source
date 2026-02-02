using UnityEngine;

public class PixelCamera : MonoBehaviour
{
	[Range(0f, 1f)]
	public float pixelSize;

	private string m_pixelSizePropertyName = "_PixelSize";

	private int m_pixelSizeID;

	private Material m_material;

	private void Awake()
	{
		InitPropertyIDs();
	}

	private void InitPropertyIDs()
	{
		if (m_material == null)
		{
			m_material = new Material(Shader.Find("Orange/PixelShader"));
		}
		m_pixelSizeID = Shader.PropertyToID(m_pixelSizePropertyName);
	}

	private void OnValidate()
	{
		if (m_material == null)
		{
			m_material = new Material(Shader.Find("Orange/PixelShader"));
		}
		m_material.SetFloat(m_pixelSizeID, pixelSize);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, m_material);
	}
}
