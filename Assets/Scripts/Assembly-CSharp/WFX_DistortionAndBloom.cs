using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("KriptoFX/WFX_BloomAndDistortion")]
[ImageEffectAllowedInSceneView]
public class WFX_DistortionAndBloom : MonoBehaviour
{
	[Range(0.05f, 1f)]
	[Tooltip("Camera render texture resolution")]
	public float RenderTextureResolutoinFactor = 0.25f;

	public bool UseBloom = true;

	[Range(0.1f, 3f)]
	[Tooltip("Filters out pixels under this level of brightness.")]
	public float Threshold = 2f;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Makes transition between under/over-threshold gradual.")]
	public float SoftKnee;

	[Range(1f, 7f)]
	[Tooltip("Changes extent of veiling effects in A screen resolution-independent fashion.")]
	public float Radius = 7f;

	[Tooltip("Blend factor of the result image.")]
	public float Intensity = 1f;

	[Tooltip("Controls filter quality and buffer resolution.")]
	public bool HighQuality;

	[Tooltip("Reduces flashing noise with an additional filter.")]
	public bool AntiFlicker;

	private const string shaderName = "Hidden/KriptoFX/PostEffects/WFX_Bloom";

	private const string shaderAdditiveName = "Hidden/KriptoFX/PostEffects/WFX_BloomAdditive";

	private RenderTexture source;

	private RenderTexture destination;

	private int previuosFrameWidth;

	private int previuosFrameHeight;

	private float previousScale;

	private Camera _cameraInstance;

	private Material m_Material;

	private Material m_MaterialAdditive;

	private const int kMaxIterations = 16;

	private readonly RenderTexture[] m_blurBuffer1 = new RenderTexture[16];

	private readonly RenderTexture[] m_blurBuffer2 = new RenderTexture[16];

	public Material mat
	{
		get
		{
			if (m_Material == null)
			{
				m_Material = CheckShaderAndCreateMaterial(Shader.Find("Hidden/KriptoFX/PostEffects/WFX_Bloom"));
			}
			return m_Material;
		}
	}

	public Material matAdditive
	{
		get
		{
			if (m_MaterialAdditive == null)
			{
				m_MaterialAdditive = CheckShaderAndCreateMaterial(Shader.Find("Hidden/KriptoFX/PostEffects/WFX_BloomAdditive"));
				m_MaterialAdditive.renderQueue = 3900;
			}
			return m_MaterialAdditive;
		}
	}

	public static Material CheckShaderAndCreateMaterial(Shader s)
	{
		if (s == null || !s.isSupported)
		{
			return null;
		}
		return new Material(s)
		{
			hideFlags = HideFlags.DontSave
		};
	}

	private void OnDisable()
	{
		if (m_Material != null)
		{
			Object.DestroyImmediate(m_Material);
		}
		m_Material = null;
		if (m_MaterialAdditive != null)
		{
			Object.DestroyImmediate(m_MaterialAdditive);
		}
		m_MaterialAdditive = null;
		if (_cameraInstance != null)
		{
			_cameraInstance.gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		if (_cameraInstance != null)
		{
			Object.DestroyImmediate(_cameraInstance.gameObject);
		}
	}

	private void OnGUI()
	{
		if (Event.current.type.Equals(EventType.Repaint) && UseBloom)
		{
			Graphics.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), destination, matAdditive);
		}
	}

	private void Start()
	{
		InitializeRenderTarget();
	}

	private void LateUpdate()
	{
		if (previuosFrameWidth != Screen.width || previuosFrameHeight != Screen.height || Mathf.Abs(previousScale - RenderTextureResolutoinFactor) > 0.01f)
		{
			InitializeRenderTarget();
			previuosFrameWidth = Screen.width;
			previuosFrameHeight = Screen.height;
			previousScale = RenderTextureResolutoinFactor;
		}
		Shader.EnableKeyword("DISTORT_OFF");
		UpdateCameraCopy();
		if (UseBloom)
		{
			UpdateBloom();
		}
		Shader.SetGlobalTexture("_GrabTextureMobile", source);
		Shader.SetGlobalFloat("_GrabTextureMobileScale", RenderTextureResolutoinFactor);
		Shader.DisableKeyword("DISTORT_OFF");
	}

	private void InitializeRenderTarget()
	{
		int num = (int)((float)Screen.width * RenderTextureResolutoinFactor);
		int num2 = (int)((float)Screen.height * RenderTextureResolutoinFactor);
		source = new RenderTexture(num, num2, 16, RenderTextureFormat.DefaultHDR);
		if (UseBloom)
		{
			destination = new RenderTexture(((double)RenderTextureResolutoinFactor > 0.99) ? num : (num / 2), ((double)RenderTextureResolutoinFactor > 0.99) ? num2 : (num2 / 2), 0, RenderTextureFormat.ARGB32);
		}
	}

	private void UpdateBloom()
	{
		bool isMobilePlatform = Application.isMobilePlatform;
		int num = source.width;
		int num2 = source.height;
		if (!HighQuality)
		{
			num /= 2;
			num2 /= 2;
		}
		RenderTextureFormat format = (isMobilePlatform ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
		float num3 = Mathf.Log(num2, 2f) + Radius - 8f;
		int num4 = (int)num3;
		int num5 = Mathf.Clamp(num4, 1, 16);
		float num6 = Mathf.GammaToLinearSpace(Threshold);
		mat.SetFloat("_Threshold", num6);
		float num7 = num6 * SoftKnee + 1E-05f;
		Vector3 vector = new Vector3(num6 - num7, num7 * 2f, 0.25f / num7);
		mat.SetVector("_Curve", vector);
		bool flag = !HighQuality && AntiFlicker;
		mat.SetFloat("_PrefilterOffs", flag ? (-0.5f) : 0f);
		mat.SetFloat("_SampleScale", 0.5f + num3 - (float)num4);
		mat.SetFloat("_Intensity", Mathf.Max(0f, Intensity));
		RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, format);
		Graphics.Blit(source, temporary, mat, AntiFlicker ? 1 : 0);
		RenderTexture renderTexture = temporary;
		for (int i = 0; i < num5; i++)
		{
			m_blurBuffer1[i] = RenderTexture.GetTemporary(renderTexture.width / 2, renderTexture.height / 2, 0, format);
			Graphics.Blit(renderTexture, m_blurBuffer1[i], mat, (i == 0) ? (AntiFlicker ? 3 : 2) : 4);
			renderTexture = m_blurBuffer1[i];
		}
		for (int num8 = num5 - 2; num8 >= 0; num8--)
		{
			RenderTexture renderTexture2 = m_blurBuffer1[num8];
			mat.SetTexture("_BaseTex", renderTexture2);
			m_blurBuffer2[num8] = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, 0, format);
			Graphics.Blit(renderTexture, m_blurBuffer2[num8], mat, HighQuality ? 6 : 5);
			renderTexture = m_blurBuffer2[num8];
		}
		destination.DiscardContents();
		Graphics.Blit(renderTexture, destination, mat, HighQuality ? 8 : 7);
		for (int j = 0; j < 16; j++)
		{
			if (m_blurBuffer1[j] != null)
			{
				RenderTexture.ReleaseTemporary(m_blurBuffer1[j]);
			}
			if (m_blurBuffer2[j] != null)
			{
				RenderTexture.ReleaseTemporary(m_blurBuffer2[j]);
			}
			m_blurBuffer1[j] = null;
			m_blurBuffer2[j] = null;
		}
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void InitializeCameraCopy()
	{
		if (_cameraInstance != null)
		{
			_cameraInstance.gameObject.SetActive(true);
		}
		GameObject gameObject = GameObject.Find("RenderTextureCamera");
		if (gameObject == null)
		{
			GameObject gameObject2 = new GameObject("RenderTextureCamera");
			gameObject2.transform.parent = Camera.main.transform;
			_cameraInstance = gameObject2.AddComponent<Camera>();
			_cameraInstance.CopyFrom(Camera.main);
			_cameraInstance.clearFlags = Camera.main.clearFlags;
			_cameraInstance.depth--;
			_cameraInstance.allowHDR = true;
			_cameraInstance.targetTexture = source;
			Shader.SetGlobalTexture("_GrabTextureMobile", source);
			Shader.SetGlobalFloat("_GrabTextureMobileScale", RenderTextureResolutoinFactor);
			_cameraInstance.Render();
		}
		else
		{
			_cameraInstance = gameObject.GetComponent<Camera>();
		}
	}

	private void UpdateCameraCopy()
	{
		Camera current = Camera.current;
		if (current != null && current.name == "SceneCamera")
		{
			source.DiscardContents();
			current.targetTexture = source;
			current.Render();
			current.targetTexture = null;
			return;
		}
		current = Camera.main;
		bool allowHDR = current.allowHDR;
		source.DiscardContents();
		current.allowHDR = true;
		current.targetTexture = source;
		current.Render();
		current.allowHDR = allowHDR;
		current.targetTexture = null;
	}
}
