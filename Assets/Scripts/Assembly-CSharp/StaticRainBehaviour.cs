using UnityEngine;

[ExecuteInEditMode]
public class StaticRainBehaviour : RainBehaviourBase
{
	[SerializeField]
	public StaticRainVariables Variables;

	[HideInInspector]
	private StaticRainController rainController { get; set; }

	public override int CurrentDrawCall
	{
		get
		{
			if (!(rainController == null))
			{
				return 1;
			}
			return 0;
		}
	}

	public override int MaxDrawCall
	{
		get
		{
			return 1;
		}
	}

	public override bool IsPlaying
	{
		get
		{
			if (rainController == null)
			{
				return false;
			}
			return rainController.IsPlaying;
		}
	}

	public override bool IsEnabled
	{
		get
		{
			if (Alpha != 0f)
			{
				return CurrentDrawCall != 0;
			}
			return false;
		}
	}

	public override void Refresh()
	{
		if (rainController != null)
		{
			Object.DestroyImmediate(rainController.gameObject);
			rainController = null;
		}
		rainController = CreateController();
		rainController.Refresh();
		rainController.NoMoreRain = true;
	}

	public override void StartRain()
	{
		if (rainController == null)
		{
			rainController = CreateController();
			rainController.Refresh();
		}
		rainController.NoMoreRain = false;
		rainController.Play();
	}

	public override void StopRain()
	{
		if (!(rainController == null))
		{
			rainController.NoMoreRain = true;
		}
	}

	public override void StopRainImmidiate()
	{
		if (!(rainController == null))
		{
			Object.DestroyImmediate(rainController.gameObject);
			rainController = null;
			Refresh();
		}
	}

	public override void ApplyFinalDepth(int depth)
	{
		if (!(rainController == null))
		{
			rainController.RenderQueue = depth;
		}
	}

	public override void ApplyGlobalWind(Vector2 globalWind)
	{
		bool flag = rainController == null;
	}

	public override void Awake()
	{
		if (Application.isPlaying)
		{
			Refresh();
		}
	}

	private void Start()
	{
		if (Application.isPlaying && Variables.AutoStart)
		{
			StartRain();
		}
	}

	public override void Update()
	{
		InitParams();
		if (!(rainController == null))
		{
			rainController.ShaderType = ShaderType;
			rainController.Alpha = Alpha;
			rainController.VRMode = VRMode;
			rainController.UpdateController();
		}
	}

	private StaticRainController CreateController()
	{
		StaticRainController staticRainController = RainDropTools.CreateHiddenObject("Controller", base.transform).gameObject.AddComponent<StaticRainController>();
		staticRainController.Variables = Variables;
		staticRainController.Alpha = 0f;
		staticRainController.NoMoreRain = false;
		staticRainController.camera = GetComponentInParent<Camera>();
		return staticRainController;
	}

	public void InitParams()
	{
		StaticRainVariables variable = Variables;
	}
}
