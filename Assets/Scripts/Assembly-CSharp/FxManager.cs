#define RELEASE
using System.Collections.Generic;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class FxManager : MonoBehaviourSingleton<FxManager>
{
	private const string assetFolder = "prefab/fx/";

	public List<FxBase> listActiveFxBase = new List<FxBase>();

	public List<ParticleSystem> listActiveParticleSystem = new List<ParticleSystem>();

	private bool bLastLock;

	public void PreloadFx(string p_fxName, int p_count = 1, Callback p_callback = null)
	{
		if (p_fxName == string.Empty)
		{
			return;
		}
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<FxBase>("prefab/fx/" + p_fxName, p_fxName, p_count, delegate
		{
			if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(p_fxName))
			{
				Callback callback = p_callback;
				if (callback != null)
				{
					callback();
				}
			}
			else
			{
				Debug.LogError("Failed to PreloadFX : " + p_fxName + ", are you missing Asset or FxBase Component?");
			}
		});
	}

	public void PreloadFx(string p_fxBundleName, string p_fxName, int p_count = 1, Callback p_callback = null)
	{
		if (p_fxName == string.Empty)
		{
			return;
		}
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<FxBase>(p_fxBundleName, p_fxName, p_count, delegate
		{
			if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(p_fxName))
			{
				Callback callback = p_callback;
				if (callback != null)
				{
					callback();
				}
			}
			else
			{
				Debug.LogError("Failed to PreloadFX : " + p_fxName + ", are you missing Asset or FxBase Component?");
			}
		});
	}

	public void RegisterFxBase(FxBase tFxBase)
	{
		while (listActiveFxBase.Contains(tFxBase))
		{
			listActiveFxBase.Remove(tFxBase);
		}
		listActiveFxBase.Add(tFxBase);
	}

	public void RegisterFxBase(ParticleSystem tParticleSystem)
	{
		while (listActiveParticleSystem.Contains(tParticleSystem))
		{
			listActiveParticleSystem.Remove(tParticleSystem);
		}
		listActiveParticleSystem.Add(tParticleSystem);
	}

	public void UnRegisterFxBase(FxBase tFxBase)
	{
		if (listActiveFxBase.Contains(tFxBase))
		{
			listActiveFxBase.Remove(tFxBase);
		}
	}

	public void UnRegisterFxBase(ParticleSystem tParticleSystem)
	{
		if (listActiveParticleSystem.Contains(tParticleSystem))
		{
			listActiveParticleSystem.Remove(tParticleSystem);
		}
	}

	public void LockFx(bool bLock)
	{
		for (int num = listActiveFxBase.Count - 1; num >= 0; num--)
		{
			if (!listActiveFxBase[num] || listActiveFxBase[num].pPS == null)
			{
				listActiveFxBase.RemoveAt(num);
			}
			else if (bLock)
			{
				if (!listActiveFxBase[num].pPS.isPaused)
				{
					listActiveFxBase[num].pPS.Pause(true);
				}
			}
			else if (!listActiveFxBase[num].pPS.isPlaying)
			{
				listActiveFxBase[num].pPS.Play(true);
			}
		}
		for (int num2 = listActiveParticleSystem.Count - 1; num2 >= 0; num2--)
		{
			if (!listActiveParticleSystem[num2])
			{
				listActiveParticleSystem.RemoveAt(num2);
			}
			else if (bLock)
			{
				if (!listActiveParticleSystem[num2].isPaused)
				{
					listActiveParticleSystem[num2].Pause(true);
				}
			}
			else if (!listActiveParticleSystem[num2].isPlaying)
			{
				listActiveParticleSystem[num2].Play(true);
			}
		}
		bLastLock = bLock;
	}

	private void ChangeFXColor(FxBase fx, StageFXParam tStageFXParam)
	{
		if (!tStageFXParam.tColor.HasValue)
		{
			return;
		}
		ParticleSystem[] componentsInChildren = fx.transform.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			ParticleSystem.MainModule main = componentsInChildren[i].main;
			Color color = tStageFXParam.tColor ?? Color.white;
			main.startColor = color;
			if (!componentsInChildren[i].trails.enabled)
			{
				continue;
			}
			GradientColorKey[] colorKeys = componentsInChildren[i].trails.colorOverTrail.gradient.colorKeys;
			GradientAlphaKey[] alphaKeys = componentsInChildren[i].trails.colorOverTrail.gradient.alphaKeys;
			for (int j = 0; j < colorKeys.Length; j++)
			{
				if (j > 0)
				{
					colorKeys[j].color = new Color(color.r * ((float)j / (float)colorKeys.Length), color.g * ((float)j / (float)colorKeys.Length), color.b * ((float)j / (float)colorKeys.Length));
				}
			}
			Gradient gradient = new Gradient();
			gradient.SetKeys(colorKeys, alphaKeys);
			ParticleSystem.TrailModule trails = componentsInChildren[i].trails;
			trails.colorOverTrail = gradient;
		}
	}

	public void Play<T>(string p_fxName, Vector3 p_worldPos, Quaternion p_quaternion, params object[] p_params) where T : FxBase
	{
		if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(p_fxName))
		{
			T poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<T>(p_fxName);
			poolObj.transform.SetParent(null);
			poolObj.transform.SetPositionAndRotation(p_worldPos, p_quaternion);
			RegisterFxBase(poolObj);
			poolObj.Active(p_params);
			if (p_params == null || p_params.Length == 0)
			{
				return;
			}
			StageFXParam stageFXParam = p_params[0] as StageFXParam;
			if (stageFXParam != null)
			{
				if (stageFXParam.tFOL != null)
				{
					stageFXParam.tFOL.tObj = poolObj.gameObject;
				}
				ChangeFXColor(poolObj, stageFXParam);
			}
		}
		else
		{
			PreloadFx(p_fxName, 1, delegate
			{
				Play<T>(p_fxName, p_worldPos, p_quaternion, p_params);
			});
		}
	}

	public void Play<T>(string pFxName, Transform pTransform, Quaternion pQuaternion, Color mColor, params object[] pParams) where T : FxBase
	{
		if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(pFxName))
		{
			T poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<T>(pFxName);
			Vector3 localScale = poolObj.transform.localScale;
			poolObj.transform.SetParent(pTransform);
			poolObj.transform.localPosition = Vector3.zero;
			poolObj.transform.localRotation = pQuaternion;
			poolObj.transform.localScale = localScale;
			ParticleSystem[] componentsInChildren = poolObj.transform.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				ParticleSystem.MainModule main = componentsInChildren[i].main;
				main.startColor = mColor;
				if (!componentsInChildren[i].trails.enabled)
				{
					continue;
				}
				GradientColorKey[] colorKeys = componentsInChildren[i].trails.colorOverTrail.gradient.colorKeys;
				GradientAlphaKey[] alphaKeys = componentsInChildren[i].trails.colorOverTrail.gradient.alphaKeys;
				for (int j = 0; j < colorKeys.Length; j++)
				{
					if (j > 0)
					{
						colorKeys[j].color = new Color(mColor.r * ((float)j / (float)colorKeys.Length), mColor.g * ((float)j / (float)colorKeys.Length), mColor.b * ((float)j / (float)colorKeys.Length));
					}
				}
				Gradient gradient = new Gradient();
				gradient.SetKeys(colorKeys, alphaKeys);
				ParticleSystem.TrailModule trails = componentsInChildren[i].trails;
				trails.colorOverTrail = gradient;
			}
			RegisterFxBase(poolObj);
			poolObj.Active(pParams);
			if (pParams != null && pParams.Length != 0)
			{
				StageFXParam stageFXParam = pParams[0] as StageFXParam;
				if (stageFXParam != null && stageFXParam.tFOL != null)
				{
					stageFXParam.tFOL.tObj = poolObj.gameObject;
				}
			}
		}
		else
		{
			PreloadFx(pFxName, 1, delegate
			{
				Play<T>(pFxName, pTransform, pQuaternion, mColor, pParams);
			});
		}
	}

	public void Play<T>(string pFxName, Transform pTransform, Quaternion pQuaternion, params object[] pParams) where T : FxBase
	{
		if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(pFxName))
		{
			T poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<T>(pFxName);
			Vector3 localScale = poolObj.transform.localScale;
			poolObj.transform.SetParent(pTransform);
			poolObj.transform.localPosition = Vector3.zero;
			poolObj.transform.localRotation = pQuaternion;
			poolObj.transform.localScale = localScale;
			RegisterFxBase(poolObj);
			poolObj.Active(pParams);
			if (pParams == null || pParams.Length == 0)
			{
				return;
			}
			StageFXParam stageFXParam = pParams[0] as StageFXParam;
			if (stageFXParam != null)
			{
				if (stageFXParam.tFOL != null)
				{
					stageFXParam.tFOL.tObj = poolObj.gameObject;
				}
				ChangeFXColor(poolObj, stageFXParam);
			}
		}
		else
		{
			PreloadFx(pFxName, 1, delegate
			{
				Play<T>(pFxName, pTransform, pQuaternion, pParams);
			});
		}
	}

	public void Play<T>(string pFxName, Transform pTransform, Quaternion pQuaternion, Vector3 pScale, params object[] pParams) where T : FxBase
	{
		if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(pFxName))
		{
			T poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<T>(pFxName);
			poolObj.transform.SetParent(pTransform);
			poolObj.transform.localPosition = Vector3.zero;
			poolObj.transform.localRotation = pQuaternion;
			poolObj.transform.localScale = pScale;
			RegisterFxBase(poolObj);
			poolObj.Active(pParams);
			if (pParams == null || pParams.Length == 0)
			{
				return;
			}
			StageFXParam stageFXParam = pParams[0] as StageFXParam;
			if (stageFXParam != null)
			{
				if (stageFXParam.tFOL != null)
				{
					stageFXParam.tFOL.tObj = poolObj.gameObject;
				}
				ChangeFXColor(poolObj, stageFXParam);
			}
		}
		else
		{
			PreloadFx(pFxName, 1, delegate
			{
				Play<T>(pFxName, pTransform, pQuaternion, pScale, pParams);
			});
		}
	}

	public void PlayWihtOffset<T>(string pFxName, Transform pTransform, Quaternion pQuaternion, Vector3 pOffset, params object[] pParams) where T : FxBase
	{
		if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(pFxName))
		{
			T poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<T>(pFxName);
			poolObj.transform.SetParent(pTransform);
			poolObj.transform.localPosition = pOffset;
			poolObj.transform.localRotation = pQuaternion;
			RegisterFxBase(poolObj);
			poolObj.Active(pParams);
			if (pParams == null || pParams.Length == 0)
			{
				return;
			}
			StageFXParam stageFXParam = pParams[0] as StageFXParam;
			if (stageFXParam != null)
			{
				if (stageFXParam.tFOL != null)
				{
					stageFXParam.tFOL.tObj = poolObj.gameObject;
				}
				ChangeFXColor(poolObj, stageFXParam);
			}
		}
		else
		{
			PreloadFx(pFxName, 1, delegate
			{
				PlayWihtOffset<T>(pFxName, pTransform, pQuaternion, pOffset, pParams);
			});
		}
	}

	public T PlayReturn<T>(string pFxName, Transform pTransform, Quaternion pQuaternion, params object[] pParams) where T : FxBase
	{
		if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(pFxName))
		{
			T poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<T>(pFxName);
			Vector3 localScale = poolObj.transform.localScale;
			poolObj.transform.SetParent(pTransform);
			poolObj.transform.localPosition = Vector3.zero;
			poolObj.transform.localRotation = pQuaternion;
			poolObj.transform.localScale = localScale;
			RegisterFxBase(poolObj);
			poolObj.Active(pParams);
			return poolObj;
		}
		return null;
	}

	public T PlayReturn<T>(string pFxName, Vector3 p_worldPos, Quaternion pQuaternion, params object[] pParams) where T : FxBase
	{
		if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(pFxName))
		{
			T poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<T>(pFxName);
			poolObj.transform.SetParent(null);
			poolObj.transform.SetPositionAndRotation(p_worldPos, pQuaternion);
			RegisterFxBase(poolObj);
			poolObj.Active(pParams);
			return poolObj;
		}
		return null;
	}

	public void UpdateFx(ParticleSystem[] p_fxArray, bool p_active, ParticleSystemStopBehavior particleSystemStopBehavior = ParticleSystemStopBehavior.StopEmitting)
	{
		ParticleSystem[] array;
		if (p_active)
		{
			array = p_fxArray;
			foreach (ParticleSystem particleSystem in array)
			{
				RegisterFxBase(particleSystem);
				particleSystem.Simulate(0f, false, true);
				particleSystem.Play(false);
			}
			return;
		}
		array = p_fxArray;
		foreach (ParticleSystem particleSystem2 in array)
		{
			UnRegisterFxBase(particleSystem2);
			if (bLastLock)
			{
				particleSystem2.Play(false);
			}
			particleSystem2.Stop(false, particleSystemStopBehavior);
		}
	}
}
