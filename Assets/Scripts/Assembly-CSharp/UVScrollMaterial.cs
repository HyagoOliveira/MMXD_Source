using UnityEngine;

public class UVScrollMaterial : MonoBehaviour
{
	public Camera Camera;

	public bool FollowCamera = true;

	public Vector3 min;

	public Vector3 max;

	[Tooltip("Intensity of rain (0-1)")]
	[Range(0f, 1f)]
	public float Intensity;

	public float WidthMultiplier = 1.5f;

	private float cameraMultiplier = 1f;

	private Bounds visibleBounds;

	private float yOffset;

	private float visibleWorldWidth;

	[SerializeField]
	private Renderer[] renderers;

	[SerializeField]
	private Texture MainTex;

	[SerializeField]
	private Texture NoiseTex;

	[SerializeField]
	private Color[] Color;

	[Range(-1f, 1f)]
	[SerializeField]
	private float[] MoveSpeedX;

	[Range(-1f, 1f)]
	[SerializeField]
	private float[] MoveSpeedY;

	[Range(0f, 10f)]
	[SerializeField]
	private float[] FactorTime;

	[Range(0f, 1f)]
	[SerializeField]
	private float[] factor;

	private bool isCustomRender;

	private MaterialPropertyBlock mpb;

	public bool IsRenderersExist()
	{
		if (renderers != null)
		{
			return renderers.Length != 0;
		}
		return false;
	}

	protected void Awake()
	{
		isCustomRender = !IsRenderersExist();
		if (!isCustomRender)
		{
			mpb = new MaterialPropertyBlock();
			UpdateProperty();
		}
	}

	public void UpdateProperty()
	{
		renderers[0].GetPropertyBlock(mpb);
		if (null != MainTex)
		{
			mpb.SetTexture(Shader.PropertyToID("_MainTex"), MainTex);
		}
		if (null != NoiseTex)
		{
			mpb.SetTexture(Shader.PropertyToID("_Noise"), NoiseTex);
		}
		UpdatePropertyBlock();
	}

	private void Start()
	{
		if (Camera == null)
		{
			Camera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
		}
		WeatherSystem component = base.transform.parent.GetComponent<WeatherSystem>();
		if (component.isStartActive)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.WEATHER_SYSTEM_CTRL, true, component.StartActiveLivel, component.isStartActiveFade);
		}
		else
		{
			base.transform.gameObject.SetActive(false);
		}
	}

	public void EffectSubjoin(int IntensityLevel, bool isFade)
	{
		base.transform.gameObject.SetActive(true);
	}

	public void EffectLessen(int IntensityLevel, bool isFade)
	{
		base.transform.gameObject.SetActive(false);
	}

	private void TransformRenderer(Renderer p)
	{
		if (p == null)
		{
			return;
		}
		if (FollowCamera)
		{
			if (p.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.FxLayer)
			{
				p.gameObject.transform.position = new Vector3(Camera.transform.position.x, visibleBounds.max.y + yOffset + 3f, p.gameObject.transform.position.z);
			}
			else
			{
				p.gameObject.transform.position = new Vector3(Camera.transform.position.x, visibleBounds.max.y + yOffset, p.gameObject.transform.position.z);
			}
		}
		else
		{
			p.gameObject.transform.position = new Vector3(p.gameObject.transform.position.x, visibleBounds.max.y + yOffset, p.gameObject.transform.position.z);
		}
		if (p.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.FxLayer)
		{
			p.gameObject.transform.localScale = new Vector3(visibleWorldWidth * WidthMultiplier + 0.5f, 1f, 2f);
		}
		else
		{
			p.gameObject.transform.localScale = new Vector3(visibleWorldWidth * WidthMultiplier, 1f, 1f);
		}
	}

	private void Update()
	{
		if (base.transform.parent != null)
		{
			base.transform.parent = null;
		}
		cameraMultiplier = Camera.orthographicSize * 0.25f;
		visibleBounds.min = min;
		visibleBounds.max = max;
		visibleWorldWidth = visibleBounds.size.x;
		yOffset = visibleBounds.max.y - visibleBounds.min.y;
		for (int i = 0; i < renderers.Length; i++)
		{
			TransformRenderer(renderers[i]);
		}
	}

	private void UpdatePropertyBlock()
	{
		int num = 0;
		Renderer[] array = renderers;
		foreach (Renderer obj in array)
		{
			mpb.SetColor(Shader.PropertyToID("_Color"), Color[num]);
			mpb.SetFloat(Shader.PropertyToID("_SpeedX"), MoveSpeedX[num]);
			mpb.SetFloat(Shader.PropertyToID("_SpeedY"), MoveSpeedY[num]);
			mpb.SetFloat(Shader.PropertyToID("_distortFactorTime"), FactorTime[num]);
			mpb.SetFloat(Shader.PropertyToID("_distortFactor"), factor[num]);
			obj.SetPropertyBlock(mpb);
			num++;
		}
	}
}
