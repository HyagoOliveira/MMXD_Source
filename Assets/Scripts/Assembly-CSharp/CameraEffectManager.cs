#define RELEASE
using System.Collections.Generic;
using UnityEngine;

public class CameraEffectManager : MonoBehaviour
{
	private class EffectData
	{
		public bool isEnable;

		public CameraEffectControllerBase controller;
	}

	private const string bundleName = "prefab/fx/";

	private Dictionary<string, EffectData> effectList = new Dictionary<string, EffectData>();

	private void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<bool, string>(EventManager.ID.CAMERA_EFFECT_CTRL, CameraEffectControl);
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<bool, string>(EventManager.ID.CAMERA_EFFECT_CTRL, CameraEffectControl);
	}

	private void CameraEffectControl(bool isEnable, string effectName)
	{
		if (string.IsNullOrEmpty(effectName) || effectName == "null")
		{
			return;
		}
		EffectData value;
		if (effectList.TryGetValue(effectName, out value))
		{
			value.isEnable = isEnable;
			if (value.controller != null)
			{
				if (isEnable)
				{
					if (!value.controller.IsPlaying())
					{
						value.controller.gameObject.SetActive(true);
						value.controller.Play();
					}
				}
				else
				{
					value.controller.Stop();
					value.controller.gameObject.SetActive(false);
				}
			}
		}
		else
		{
			value = new EffectData();
			value.isEnable = isEnable;
			effectList.Add(effectName, value);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<GameObject>("prefab/fx/" + effectName, effectName, CameraEffectLoadDone);
		}
		Debug.Log(string.Format("Camera effect event catched : {0} {1}", isEnable, effectName));
	}

	private void CameraEffectLoadDone(GameObject effectObject)
	{
		if (effectObject == null)
		{
			return;
		}
		if (effectList[effectObject.name].controller != null)
		{
			Debug.LogError("已經有一個了卻又載入一次");
			return;
		}
		GameObject obj = Object.Instantiate(effectObject);
		obj.transform.parent = base.transform;
		obj.transform.localPosition = new Vector3(0f, 0f, 1f);
		CameraEffectControllerBase component = obj.GetComponent<CameraEffectControllerBase>();
		if (component == null)
		{
			return;
		}
		component.Initialize(GetComponent<Camera>());
		effectList[effectObject.name].controller = component;
		if (effectList[effectObject.name].isEnable)
		{
			if (!effectList[effectObject.name].controller.IsPlaying())
			{
				effectList[effectObject.name].controller.gameObject.SetActive(true);
				effectList[effectObject.name].controller.Play();
			}
		}
		else
		{
			effectList[effectObject.name].controller.Stop();
			effectList[effectObject.name].controller.gameObject.SetActive(false);
		}
	}
}
