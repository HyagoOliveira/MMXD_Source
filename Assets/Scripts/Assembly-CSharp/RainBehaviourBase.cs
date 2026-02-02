using UnityEngine;

public abstract class RainBehaviourBase : MonoBehaviour
{
	public int Depth;

	[HideInInspector]
	public float Alpha;

	[HideInInspector]
	public RainDropTools.RainDropShaderType ShaderType;

	[HideInInspector]
	public bool VRMode;

	[HideInInspector]
	public float Distance;

	[HideInInspector]
	public Vector3 GForceVector;

	public virtual bool IsPlaying
	{
		get
		{
			return false;
		}
	}

	public virtual bool IsEnabled
	{
		get
		{
			return false;
		}
	}

	public virtual int CurrentDrawCall
	{
		get
		{
			return 0;
		}
	}

	public virtual int MaxDrawCall
	{
		get
		{
			return 0;
		}
	}

	public virtual void Refresh()
	{
	}

	public virtual void StartRain()
	{
	}

	public virtual void StopRain()
	{
	}

	public virtual void StopRainImmidiate()
	{
	}

	public virtual void ApplyFinalDepth(int depth)
	{
	}

	public virtual void ApplyGlobalWind(Vector2 globalWind)
	{
	}

	public virtual void Awake()
	{
	}

	public virtual void Update()
	{
	}
}
