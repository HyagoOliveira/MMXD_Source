#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Better;
using CallbackDefs;
using Crystal;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
	public enum EffectType
	{
		NONE = -1,
		FADE = 0,
		EXPAND = 1,
		HORIZONTAL_LEFT_2_RIGHT = 2,
		HORIZONTAL_RIGHT_2_LEFT = 3,
		VERTICAL_TOP_2_BOTTOM = 4,
		VERTICAL_BOTTOM_2_TOP = 5
	}

	public delegate void LoadUIComplete<T>(T ui) where T : Component;

	public enum SimDevice
	{
		None = 0,
		iPhoneX = 1
	}

	private LeanTweenType tweenType = LeanTweenType.easeOutCubic;

	private string uiPath = string.Empty;

	private string bgPath = string.Empty;

	private readonly string uiPathLocal = "UI/";

	private readonly float eftStartValue;

	private readonly float eftEndValue = 1f;

	private readonly float eftAnimTime = 0.3f;

	private readonly float translucentBgEndValue = 0.58f;

	private float eftHorizontalValue = 1280f;

	private float eftVerticalValue = 800f;

	private List<OrangeUIBase> ActiveUIList = new List<OrangeUIBase>();

	private System.Collections.Generic.Dictionary<string, OrangeBgBase> DictActiveBg = new Better.Dictionary<string, OrangeBgBase>();

	public Transform JoystickPanelParent;

	[SerializeField]
	private Transform UiParent;

	[SerializeField]
	private LoadingUI loadingUI;

	[SerializeField]
	private ConnectingUI connectingUI;

	[SerializeField]
	private RectTransform translucentBg;

	[SerializeField]
	private NonDrawingGraphic block;

	[SerializeField]
	private DownloadBarUI downloadBarUI;

	[SerializeField]
	private RectTransform ConfirmUIParent;

	[SerializeField]
	private OrangeCamera uiCamera;

	private Image imgTranslucent;

	private Canvas translucentCanvas;

	private Canvas blockCanvas;

	private Canvas uiParentCanvas;

	private bool bLockTurtorialLoad;

	private bool bLockTurtorialClose;

	private string tempActiveUiName = string.Empty;

	private int resolutionTweenID;

	public static SimDevice Sim;

	private int keepScreenWidth = 1920;

	private int keepScreenHeight = 1080;

	private readonly int defaultX = 1920;

	private readonly int defaultY = 1080;

	private bool isUiLoading;

	private Queue<Callback> queueUiWaitLoading = new Queue<Callback>();

	private bool isClosingUI;

	private int blockUid = -1;

	public bool bLockTurtorial
	{
		get
		{
			if (!bLockTurtorialLoad)
			{
				return bLockTurtorialClose;
			}
			return true;
		}
	}

	public bool IsLoading
	{
		get
		{
			return loadingUI.IsOpen();
		}
	}

	public Transform UI_Parent
	{
		get
		{
			return UiParent;
		}
	}

	public Transform CanvasUI
	{
		get
		{
			return UiParent.parent;
		}
	}

	public Camera UICamera
	{
		get
		{
			return uiCamera._camera;
		}
	}

	public bool ForcePowerUPSE { get; set; }

	public OrangeUIBase LastUI { get; private set; }

	public Rect SafeAreaRect { get; set; }

	public int MatchWidthOrHeight { get; private set; }

	public Vector2 CanvasSize { get; set; } = new Vector2(1920f, 1080f);


	public bool IsBlockCloseing
	{
		get
		{
			return blockUid != -1;
		}
	}

	public event Action OnUILinkPrepareEvent;

	private void Awake()
	{
		uiPath = AssetBundleScriptableObject.Instance.m_uiPath;
		bgPath = uiPath + "background/";
		imgTranslucent = translucentBg.GetComponent<Image>();
		translucentCanvas = translucentBg.GetComponent<Canvas>();
		blockCanvas = block.GetComponent<Canvas>();
		blockCanvas.enabled = true;
		uiParentCanvas = UiParent.GetComponent<Canvas>();
		UpdateBlockState();
	}

	private void Update()
	{
		Refresh();
		if (!isUiLoading && !(LastUI == null))
		{
			if (MonoBehaviourSingleton<InputManager>.Instance.ButtonHash != null && cInput.GetKeyDown(MonoBehaviourSingleton<InputManager>.Instance.ButtonHash[18]))
			{
				LastUI.DoEscapeEvent();
			}
			else
			{
				LastUI.DoJoystickEvent();
			}
		}
	}

	public void Refresh()
	{
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.fullscreen != Screen.fullScreen)
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.fullscreen = Screen.fullScreen;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_FULLSCREEN, Screen.fullScreen);
			if (!Screen.fullScreen)
			{
				Vector2 windowModeResolution = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.WindowModeResolution;
				Screen.SetResolution((int)windowModeResolution.x, (int)windowModeResolution.y, false);
			}
		}
		SafeAreaRect = new Rect(0f, 0f, Screen.width, Screen.height);
		if (Screen.width == keepScreenWidth && Screen.height == keepScreenHeight)
		{
			return;
		}
		keepScreenWidth = Screen.width;
		keepScreenHeight = Screen.height;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_RESOLUTION);
		if (!Screen.fullScreen)
		{
			LeanTween.cancel(ref resolutionTweenID);
			Vector2 tempResolution = new Vector2(Screen.width, Screen.height);
			resolutionTweenID = LeanTween.delayedCall(1f, (Action)delegate
			{
				if (this != null)
				{
					Debug.Log("Updating resolution to save data.");
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.WindowModeResolution = tempResolution;
				}
			}).uniqueId;
		}
		SafeArea[] componentsInChildren = GetComponentsInChildren<SafeArea>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ApplySafeArea(SafeAreaRect);
		}
		float num = ((float)Screen.width - SafeAreaRect.x * 2f) / (float)defaultX;
		float num2 = ((float)Screen.height - SafeAreaRect.y * 2f) / (float)defaultY;
		MatchWidthOrHeight = ((num > num2) ? 1 : 0);
		if (MatchWidthOrHeight == 1)
		{
			CanvasSize = new Vector2((float)Screen.width / ((float)Screen.height / (float)defaultY), defaultY);
		}
		else
		{
			CanvasSize = new Vector2(defaultX, (float)Screen.height / ((float)Screen.width / (float)defaultX));
		}
		CanvasScaler[] componentsInChildren2 = GetComponentsInChildren<CanvasScaler>(true);
		foreach (CanvasScaler canvasScaler in componentsInChildren2)
		{
			if (num == num2)
			{
				canvasScaler.referenceResolution = new Vector2(defaultX, defaultY);
				break;
			}
			canvasScaler.matchWidthOrHeight = MatchWidthOrHeight;
		}
	}

	public T LoadResourceUISync<T>(string p_name, bool isConfirmParent = false, bool blackBlock = false) where T : OrangeUIBase
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(new StringBuilder(uiPathLocal).Append(p_name).ToString()), base.transform, false);
		gameObject.name = p_name;
		if (isConfirmParent)
		{
			gameObject.transform.SetParent(ConfirmUIParent, false);
		}
		else
		{
			gameObject.transform.SetParent(UiParent, false);
		}
		T component = gameObject.GetComponent<T>();
		component.IsConfirmUI = isConfirmParent;
		AddUI(component);
		if (blackBlock)
		{
			if (!translucentCanvas.isActiveAndEnabled)
			{
				imgTranslucent.color = new Color(0f, 0f, 0f, translucentBgEndValue);
				translucentCanvas.enabled = true;
			}
			if (isConfirmParent)
			{
				translucentBg.transform.SetSiblingIndex(-1);
			}
			else
			{
				translucentBg.transform.SetSiblingIndex(component.transform.GetSiblingIndex() - 1);
			}
		}
		OverrideL10nText(component);
		component.UpdateSiblingIndex(ActiveUIList.Count * 2);
		component.canvasGroup.alpha = 1f;
		component.IsLock = false;
		return component;
	}

	public void LoadResourceUI<T>(string p_name, LoadUIComplete<T> p_cb, bool isConfirmParent = false, bool ignoreTutorial = false) where T : OrangeUIBase
	{
		if (!(tempActiveUiName == p_name))
		{
			if (!ignoreTutorial)
			{
				bLockTurtorialLoad = true;
				TurtorialUI.CheckTurtorialTriggerName(p_name);
			}
			tempActiveUiName = p_name;
			isUiLoading = true;
			UpdateBlockState();
			StartCoroutine(OnStartLoadResourceUI(p_name, p_cb, isConfirmParent));
		}
	}

	private IEnumerator OnStartLoadResourceUI<T>(string p_name, LoadUIComplete<T> p_cb, bool isConfirmParent = false) where T : OrangeUIBase
	{
		ResourceRequest request = Resources.LoadAsync<GameObject>(new StringBuilder(uiPathLocal).Append(p_name).ToString());
		yield return request;
		tempActiveUiName = string.Empty;
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)request.asset, base.transform, false);
		gameObject.name = p_name;
		if (isConfirmParent)
		{
			gameObject.transform.SetParent(ConfirmUIParent, false);
		}
		else
		{
			gameObject.transform.SetParent(UiParent, false);
		}
		T component = gameObject.GetComponent<T>();
		ActiveUI(component, p_cb, isConfirmParent);
		yield return CoroutineDefine._waitForEndOfFrame;
	}

	public void PreloadUI(string p_name, Callback p_cb = null)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(new string[1] { new StringBuilder(uiPath).Append(p_name).ToString() }, delegate
		{
			p_cb.CheckTargetToInvoke();
		}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
	}

	public void LoadUI<T>(string p_name, LoadUIComplete<T> p_cb = null) where T : OrangeUIBase
	{
		if (tempActiveUiName == p_name)
		{
			return;
		}
		if (isUiLoading)
		{
			Debug.Log("Load UI Queue " + p_name + " , count " + queueUiWaitLoading.Count + " , Last Loading " + tempActiveUiName);
			string keep = p_name;
			LoadUIComplete<T> keepCb = p_cb;
			queueUiWaitLoading.Enqueue(delegate
			{
				LoadUI(keep, keepCb);
			});
			return;
		}
		isUiLoading = true;
		UpdateBlockState();
		bLockTurtorialLoad = true;
		TurtorialUI.CheckTurtorialTriggerName(p_name);
		tempActiveUiName = p_name;
		Debug.Log("Loading UI " + p_name);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(new StringBuilder(uiPath).Append(p_name).ToString(), p_name, delegate(GameObject asset)
		{
			Debug.Log("Load UI End:" + p_name);
			GameObject obj = UnityEngine.Object.Instantiate(asset, base.transform, false);
			obj.name = p_name;
			obj.transform.SetParent(UiParent, false);
			T component = obj.GetComponent<T>();
			tempActiveUiName = string.Empty;
			ActiveUI(component, p_cb);
		});
	}

	private void ActiveUI<T>(T p_currentUI, LoadUIComplete<T> p_cb, bool isConfirmParent = false) where T : OrangeUIBase
	{
		AddUI(p_currentUI);
		if (p_currentUI.EnableBackground)
		{
			OrangeBgBase orangeBgBase = UnityEngine.Object.Instantiate(p_currentUI.Background, base.transform, false);
			orangeBgBase.rt.SetParent(UiParent, false);
			p_currentUI.Background = orangeBgBase;
			p_currentUI.SetBackgroundComplete = true;
		}
		else if (p_currentUI.hasBlackBg)
		{
			if (!translucentCanvas.isActiveAndEnabled)
			{
				imgTranslucent.color = new Color(0f, 0f, 0f, translucentBgEndValue);
				translucentCanvas.enabled = true;
			}
			if (isConfirmParent)
			{
				translucentBg.transform.SetSiblingIndex(UiParent.childCount);
			}
			else
			{
				translucentBg.transform.SetSiblingIndex(p_currentUI.transform.GetSiblingIndex() - 1);
			}
		}
		p_currentUI.IsConfirmUI = isConfirmParent;
		p_currentUI.UpdateSiblingIndex(ActiveUIList.Count * 2);
		AddSafeArea(p_currentUI);
		OverrideL10nText(p_currentUI);
		if (p_cb != null)
		{
			p_cb(p_currentUI);
		}
		float num = 0f;
		float num2 = 0f;
		switch (p_currentUI.effectTypeOpen)
		{
		case EffectType.NONE:
			p_currentUI.canvasGroup.alpha = 1f;
			UIOpenComplete(p_currentUI);
			break;
		case EffectType.FADE:
			LeanTween.value(p_currentUI.gameObject, eftStartValue, eftEndValue, eftAnimTime).setOnUpdate(delegate(float val)
			{
				p_currentUI.canvasGroup.alpha = val;
			}).setOnComplete((Action)delegate
			{
				UIOpenComplete(p_currentUI);
			})
				.setEase(tweenType);
			break;
		case EffectType.EXPAND:
			AdditionalEft_FadeIn(p_currentUI);
			LeanTween.value(p_currentUI.gameObject, eftStartValue, eftEndValue, eftAnimTime).setOnUpdate(delegate(float val)
			{
				p_currentUI.transform.localScale = new Vector3(val, val, 1f);
			}).setOnComplete((Action)delegate
			{
				UIOpenComplete(p_currentUI);
			})
				.setEase(tweenType);
			break;
		case EffectType.HORIZONTAL_LEFT_2_RIGHT:
			if (p_currentUI.rt.anchorMin == Vector2.zero)
			{
				num = 0f - p_currentUI.rt.offsetMax.x + eftHorizontalValue;
				num2 = 0f - p_currentUI.rt.offsetMax.x;
				AdditionalEft_FadeIn(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num, num2, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.Right(val);
				}).setOnComplete((Action)delegate
				{
					UIOpenComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			else
			{
				num = p_currentUI.rt.anchoredPosition.x - eftHorizontalValue;
				num2 = p_currentUI.rt.anchoredPosition.x;
				AdditionalEft_FadeIn(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num, num2, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.anchoredPosition = new Vector2(val, p_currentUI.rt.anchoredPosition.y);
				}).setOnComplete((Action)delegate
				{
					UIOpenComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			break;
		case EffectType.HORIZONTAL_RIGHT_2_LEFT:
			if (p_currentUI.rt.anchorMin == Vector2.zero)
			{
				num = 0f - p_currentUI.rt.offsetMax.x - eftHorizontalValue;
				num2 = 0f - p_currentUI.rt.offsetMax.x;
				AdditionalEft_FadeIn(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num, num2, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.Right(val);
				}).setOnComplete((Action)delegate
				{
					UIOpenComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			else
			{
				num = p_currentUI.rt.anchoredPosition.x + eftHorizontalValue;
				num2 = p_currentUI.rt.anchoredPosition.x;
				AdditionalEft_FadeIn(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num, num2, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.anchoredPosition = new Vector2(val, p_currentUI.rt.anchoredPosition.y);
				}).setOnComplete((Action)delegate
				{
					UIOpenComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			break;
		case EffectType.VERTICAL_TOP_2_BOTTOM:
			if (p_currentUI.rt.anchorMin == Vector2.zero)
			{
				num = 0f - p_currentUI.rt.offsetMin.y - eftVerticalValue;
				num2 = 0f - p_currentUI.rt.offsetMax.y;
				AdditionalEft_FadeIn(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num, num2, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.Top(val);
				}).setOnComplete((Action)delegate
				{
					UIOpenComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			else
			{
				num = p_currentUI.rt.anchoredPosition.y + eftVerticalValue;
				num2 = p_currentUI.rt.anchoredPosition.y;
				AdditionalEft_FadeIn(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num, num2, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.anchoredPosition = new Vector2(p_currentUI.rt.anchoredPosition.x, val);
				}).setOnComplete((Action)delegate
				{
					UIOpenComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			break;
		case EffectType.VERTICAL_BOTTOM_2_TOP:
			if (p_currentUI.rt.anchorMin == Vector2.zero)
			{
				num = 0f - p_currentUI.rt.offsetMin.y + eftVerticalValue;
				num2 = 0f - p_currentUI.rt.offsetMax.y;
				AdditionalEft_FadeIn(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num, num2, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.Top(val);
				}).setOnComplete((Action)delegate
				{
					UIOpenComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			else
			{
				num = p_currentUI.rt.anchoredPosition.y - eftVerticalValue;
				num2 = p_currentUI.rt.anchoredPosition.y;
				AdditionalEft_FadeIn(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num, num2, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.anchoredPosition = new Vector2(p_currentUI.rt.anchoredPosition.x, val);
				}).setOnComplete((Action)delegate
				{
					UIOpenComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			break;
		}
	}

	public void CloseUI<T>(T p_currentUI) where T : OrangeUIBase
	{
		if (ActiveUIList.Count == 0)
		{
			if (p_currentUI != null)
			{
				UnityEngine.Object.Destroy(p_currentUI.gameObject);
			}
			return;
		}
		isClosingUI = true;
		UpdateBlockState();
		bLockTurtorialClose = true;
		if (ActiveUIList.Contains(p_currentUI))
		{
			ActiveUIList.Remove(p_currentUI);
		}
		if (p_currentUI == null)
		{
			isClosingUI = false;
			UpdateBlockState(0.05f);
			return;
		}
		UpdateUISiblingIndex();
		float num = 0f;
		float num2 = 0f;
		switch (p_currentUI.effectTypeClose)
		{
		case EffectType.NONE:
			UICloseComplete(p_currentUI);
			break;
		case EffectType.FADE:
			LeanTween.value(p_currentUI.gameObject, eftEndValue, eftStartValue, eftAnimTime).setOnUpdate(delegate(float val)
			{
				p_currentUI.canvasGroup.alpha = val;
			}).setOnComplete((Action)delegate
			{
				UICloseComplete(p_currentUI);
			})
				.setEase(tweenType);
			break;
		case EffectType.EXPAND:
			AdditionalEft_FadeOut(p_currentUI);
			LeanTween.value(p_currentUI.gameObject, eftEndValue, eftStartValue, eftAnimTime).setOnUpdate(delegate(float val)
			{
				p_currentUI.transform.localScale = new Vector3(val, val, 1f);
			}).setOnComplete((Action)delegate
			{
				UICloseComplete(p_currentUI);
			})
				.setEase(tweenType);
			break;
		case EffectType.HORIZONTAL_LEFT_2_RIGHT:
			if (p_currentUI.rt.anchorMin == Vector2.zero)
			{
				num = 0f - p_currentUI.rt.offsetMax.x - eftHorizontalValue;
				num2 = 0f - p_currentUI.rt.offsetMax.x;
				AdditionalEft_FadeOut(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num2, num, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.Right(val);
				}).setOnComplete((Action)delegate
				{
					UICloseComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			else
			{
				num = p_currentUI.rt.anchoredPosition.x + eftHorizontalValue;
				num2 = p_currentUI.rt.anchoredPosition.x;
				AdditionalEft_FadeOut(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num2, num, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.anchoredPosition = new Vector2(val, p_currentUI.rt.anchoredPosition.y);
				}).setOnComplete((Action)delegate
				{
					UICloseComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			break;
		case EffectType.HORIZONTAL_RIGHT_2_LEFT:
			if (p_currentUI.rt.anchorMin == Vector2.zero)
			{
				num = 0f - p_currentUI.rt.offsetMax.x + eftHorizontalValue;
				num2 = 0f - p_currentUI.rt.offsetMax.x;
				AdditionalEft_FadeOut(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num2, num, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.Right(val);
				}).setOnComplete((Action)delegate
				{
					UICloseComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			else
			{
				num = p_currentUI.rt.anchoredPosition.x - eftHorizontalValue;
				num2 = p_currentUI.rt.anchoredPosition.x;
				AdditionalEft_FadeOut(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num2, num, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.anchoredPosition = new Vector2(val, p_currentUI.rt.anchoredPosition.y);
				}).setOnComplete((Action)delegate
				{
					UICloseComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			break;
		case EffectType.VERTICAL_TOP_2_BOTTOM:
			if (p_currentUI.rt.anchorMin == Vector2.zero)
			{
				num = 0f - p_currentUI.rt.offsetMin.y + eftVerticalValue;
				num2 = 0f - p_currentUI.rt.offsetMax.y;
				AdditionalEft_FadeOut(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num2, num, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.Top(val);
				}).setOnComplete((Action)delegate
				{
					UICloseComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			else
			{
				num = p_currentUI.rt.anchoredPosition.y - eftVerticalValue;
				num2 = p_currentUI.rt.anchoredPosition.y;
				AdditionalEft_FadeOut(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num2, num, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.anchoredPosition = new Vector2(p_currentUI.rt.anchoredPosition.x, val);
				}).setOnComplete((Action)delegate
				{
					UICloseComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			break;
		case EffectType.VERTICAL_BOTTOM_2_TOP:
			if (p_currentUI.rt.anchorMin == Vector2.zero)
			{
				num = 0f - p_currentUI.rt.offsetMin.y - eftVerticalValue;
				num2 = 0f - p_currentUI.rt.offsetMax.y;
				AdditionalEft_FadeOut(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num2, num, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.Top(val);
				}).setOnComplete((Action)delegate
				{
					UICloseComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			else
			{
				num = p_currentUI.rt.anchoredPosition.y + eftVerticalValue;
				num2 = p_currentUI.rt.anchoredPosition.y;
				AdditionalEft_FadeOut(p_currentUI);
				LeanTween.value(p_currentUI.gameObject, num2, num, eftAnimTime).setOnUpdate(delegate(float val)
				{
					p_currentUI.rt.anchoredPosition = new Vector2(p_currentUI.rt.anchoredPosition.x, val);
				}).setOnComplete((Action)delegate
				{
					UICloseComplete(p_currentUI);
				})
					.setEase(tweenType);
			}
			break;
		}
	}

	private void UIOpenComplete(OrangeUIBase p_currentUI)
	{
		if (p_currentUI.EnableTopResident)
		{
			ActiveTopResidentUI(p_currentUI);
		}
		UpdateUISiblingIndex();
		if (p_currentUI.EnableBackground && !p_currentUI.Background.AllowUnderEnable)
		{
			foreach (OrangeUIBase activeUI in ActiveUIList)
			{
				if (activeUI != p_currentUI)
				{
					activeUI.SetCanvas(false);
				}
			}
		}
		bLockTurtorialLoad = false;
		p_currentUI.IsLock = false;
		Callback loadedCB = p_currentUI.loadedCB;
		if (loadedCB != null)
		{
			loadedCB();
		}
		p_currentUI.loadedCB = null;
		isUiLoading = false;
		if (queueUiWaitLoading.Count > 0)
		{
			queueUiWaitLoading.Dequeue()();
		}
		else
		{
			UpdateBlockState();
		}
	}

	private void UICloseComplete(OrangeUIBase p_currentUI)
	{
		TurtorialUI.CheckTurtorialLastUI(null, p_currentUI.EnableBackground);
		UnityEngine.Object.Destroy(p_currentUI.gameObject);
		bLockTurtorialClose = false;
		isClosingUI = false;
		UpdateBlockState(0.05f);
	}

	public void CloseAllUI(Callback p_cb)
	{
		isClosingUI = true;
		for (int i = 0; i < ActiveUIList.Count; i++)
		{
			ActiveUIList[i].ForceDestory();
		}
		ActiveUIList.Clear();
		UpdateUISiblingIndex();
		isClosingUI = false;
		bLockTurtorialClose = false;
		UpdateBlockState(0.05f);
		p_cb.CheckTargetToInvoke();
	}

	public void Block(bool isBlock)
	{
		block.raycastTarget = isBlock;
	}

	public void Connecting(bool isConnecting)
	{
		if (isConnecting)
		{
			connectingUI.ActiveUI();
		}
		else
		{
			connectingUI.DisableUI();
		}
	}

	public bool IsConnecting()
	{
		return connectingUI.IsConnecting;
	}

	public void OpenLoadingUI(Callback p_cb, OrangeSceneManager.LoadingType p_loadingType = OrangeSceneManager.LoadingType.DEFAULT, float fadeInTime = 0.5f)
	{
		if (loadingUI.IsOpen())
		{
			Debug.LogWarning("Already Open Loading UI");
			p_cb.CheckTargetToInvoke();
			return;
		}
		bool flag = false;
		string text = string.Empty;
		object[] p_params = null;
		Color color = Color.black;
		switch (p_loadingType)
		{
		case OrangeSceneManager.LoadingType.WHITE:
			color = Color.white;
			break;
		case OrangeSceneManager.LoadingType.STAGE:
			text = "UI_TipLoading";
			p_params = new object[3] { 0.5f, 0f, 0.5f };
			flag = true;
			break;
		case OrangeSceneManager.LoadingType.PVP:
			text = "UI_PvpLoading";
			break;
		case OrangeSceneManager.LoadingType.FULL:
			text = "UI_TipLoading";
			p_params = new object[3] { 0f, 1f, 0f };
			flag = true;
			break;
		case OrangeSceneManager.LoadingType.TIP:
			text = "UI_TipLoading";
			p_params = new object[3] { 1f, 0f, 0f };
			flag = true;
			break;
		case OrangeSceneManager.LoadingType.PATCH:
			text = "UI_TipLoading";
			p_params = new object[3] { 0.2f, 0.8f, 0f };
			flag = true;
			break;
		}
		if (text != string.Empty)
		{
			if (flag)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(new StringBuilder(uiPathLocal).Append(text).ToString()), base.transform, false);
				ILoadingState component = gameObject.GetComponent<ILoadingState>();
				if (component != null)
				{
					component.Params = p_params;
				}
				gameObject.transform.SetParent(loadingUI.transform, false);
				loadingUI.ActiveUI(p_cb, gameObject, fadeInTime);
				return;
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(new StringBuilder(uiPath).Append(text).ToString(), text, delegate(GameObject asset)
			{
				GameObject gameObject3 = UnityEngine.Object.Instantiate(asset, base.transform, false);
				ILoadingState component2 = gameObject3.GetComponent<ILoadingState>();
				if (component2 != null)
				{
					component2.Params = p_params;
				}
				gameObject3.transform.SetParent(loadingUI.transform, false);
				loadingUI.ActiveUI(p_cb, gameObject3, fadeInTime);
			});
		}
		else
		{
			GameObject gameObject2 = new GameObject();
			gameObject2.layer = base.gameObject.layer;
			Image image = gameObject2.AddComponent<Image>();
			image.sprite = null;
			image.color = color;
			image.rectTransform.sizeDelta = new Vector2(3000f, 3000f);
			gameObject2.transform.SetParent(loadingUI.transform, false);
			loadingUI.ActiveUI(p_cb, gameObject2, fadeInTime);
		}
	}

	public void CloseLoadingUI(Callback p_cb, float fadeTime = 0.5f)
	{
		loadingUI.DisableUI(p_cb, fadeTime);
	}

	public T GetUI<T>(string p_name) where T : OrangeUIBase
	{
		return ActiveUIList.FirstOrDefault((OrangeUIBase x) => x.name == p_name) as T;
	}

	public void GetOrLoadUI<T>(string p_name, LoadUIComplete<T> p_cb) where T : OrangeUIBase
	{
		T val = ActiveUIList.FirstOrDefault((OrangeUIBase x) => x.name == p_name) as T;
		if (val == null)
		{
			LoadUI(p_name, p_cb);
		}
		else
		{
			p_cb(val);
		}
	}

	public bool IsActive(string p_name)
	{
		return ActiveUIList.Where((OrangeUIBase x) => x.name == p_name).Count() > 0;
	}

	public void NotOpenMsgUI()
	{
		LoadUI("UI_Tip", delegate(TipUI ui)
		{
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SYSTEM_NOT_OEPN"));
		});
	}

	public void ActiveTopResidentUI(OrangeUIBase currentUI)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("UI/UI_TopResident", "UI_TopResident", delegate(GameObject asset)
		{
			Topbar component = UnityEngine.Object.Instantiate(asset, base.transform, false).GetComponent<Topbar>();
			component.clickBackSE = currentUI.m_clickTopBarBackSE;
			component.btnHometop.interactable = currentUI.EnableBackToHometop;
			component.Init(currentUI);
			component.GetComponent<OrangeUIAnimation>().PlayAnimation();
			currentUI._EscapeEvent = OrangeUIBase.EscapeEvent.CLOSE_UI;
		});
	}

	public void UpdateLoadingBlock(bool p_active)
	{
		downloadBarUI.UpdateLoadingBlock(p_active);
	}

	private void AddSafeArea(OrangeUIBase currentUI)
	{
		if (SafeAreaRect.x != 0f || SafeAreaRect.y != 0f)
		{
			SafeArea safeArea = currentUI.gameObject.AddComponent<SafeArea>();
			safeArea.ConformX = currentUI.ConformX;
			safeArea.ConformY = currentUI.ConformY;
			safeArea.ApplySafeArea(SafeAreaRect);
		}
	}

	public void AddSafeArea(GameObject go)
	{
		if (SafeAreaRect.x != 0f || SafeAreaRect.y != 0f)
		{
			SafeArea safeArea = go.AddComponent<SafeArea>();
			safeArea.ConformX = true;
			safeArea.ConformY = true;
			safeArea.ApplySafeArea(SafeAreaRect);
		}
	}

	private void OverrideL10nText(OrangeUIBase p_currentUI)
	{
		foreach (OrangeText ot in p_currentUI.GetComponentsInChildren<OrangeText>(true).ToList())
		{
			if (!(ot.LocalizationKey != "NONE") || !MonoBehaviourSingleton<LocalizationManager>.Instance.IsValidKey(ot.LocalizationKey))
			{
				LOCALIZATION_TABLE lOCALIZATION_TABLE = ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.Values.FirstOrDefault((LOCALIZATION_TABLE x) => x.w_CHT == ot.text);
				if (lOCALIZATION_TABLE != null)
				{
					ot.IsLocalizationText = true;
					ot.LocalizationKey = lOCALIZATION_TABLE.w_KEY;
					ot.UpdateTextImmediate();
				}
			}
		}
	}

	public void BackToHometop(bool logoutBattleServer = true, bool logoutMatchServer = true, Callback p_cb = null)
	{
		if (!(MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene == "hometop"))
		{
			return;
		}
		ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.HOMETOP;
		if (logoutBattleServer)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.BattleServerLogout();
		}
		if (logoutMatchServer)
		{
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogout();
		}
		if (MonoBehaviourSingleton<GameServerService>.Instance.DayChange)
		{
			OpenLoadingUI(delegate
			{
				CloseAllUI(delegate
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SCENE_INIT);
				});
			}, OrangeSceneManager.LoadingType.TIP);
			return;
		}
		HometopUI hometopUI = GetUI<HometopUI>("UI_Hometop");
		if (null == hometopUI)
		{
			return;
		}
		ActiveUIList.Remove(hometopUI);
		hometopUI.SetCanvas(true);
		CloseAllUI(delegate
		{
			if (!MonoBehaviourSingleton<GameServerService>.Instance.DayChange)
			{
				hometopUI.OnUpdateHometopData();
			}
			else
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.ALL);
			}
			AddUI(hometopUI);
			PlayerPrefs.Save();
			p_cb.CheckTargetToInvoke();
			TurtorialUI.CheckTurtorialTriggerName(hometopUI.gameObject.name);
		});
	}

	private void AddUI(OrangeUIBase currentUI)
	{
		ActiveUIList.Add(currentUI);
		LastUI = currentUI;
	}

	private void UpdateBlockState(float delayCall = 0.15f)
	{
		if (isClosingUI || isUiLoading)
		{
			LeanTween.cancel(ref blockUid);
			block.raycastTarget = true;
			return;
		}
		blockUid = LeanTween.delayedCall(delayCall, (Action)delegate
		{
			blockUid = -1;
			block.raycastTarget = false;
		}).uniqueId;
	}

	private void UpdateUISiblingIndex()
	{
		if (ActiveUIList.Count == 0)
		{
			LastUI = null;
			translucentBg.SetSiblingIndex(0);
			translucentCanvas.enabled = false;
			return;
		}
		LastUI = ActiveUIList.LastOrDefault();
		bool flag = false;
		for (int num = ActiveUIList.Count - 1; num >= 0; num--)
		{
			OrangeUIBase orangeUIBase = ActiveUIList[num];
			orangeUIBase.SetCanvas(true);
			if (orangeUIBase.EnableBackground && !orangeUIBase.Background.AllowUnderEnable)
			{
				break;
			}
			if (!flag && orangeUIBase.hasBlackBg)
			{
				flag = true;
				if (orangeUIBase.IsConfirmUI)
				{
					translucentBg.transform.SetSiblingIndex(UiParent.childCount);
				}
				else
				{
					int siblingIndex = translucentBg.GetSiblingIndex();
					int siblingIndex2 = orangeUIBase.transform.GetSiblingIndex();
					int num2 = ((siblingIndex2 > siblingIndex) ? (siblingIndex2 - 1) : siblingIndex2);
					if (num2 < 0)
					{
						num2 = 0;
					}
					translucentBg.SetSiblingIndex(num2);
					imgTranslucent.color = new Color(0f, 0f, 0f, translucentBgEndValue);
					translucentCanvas.enabled = true;
				}
			}
		}
		if (!flag)
		{
			translucentCanvas.enabled = false;
			translucentBg.SetSiblingIndex(0);
			imgTranslucent.color = new Color(0f, 0f, 0f, translucentBgEndValue);
		}
	}

	public void UILinkPrepare(Callback p_cb)
	{
		HometopUI uI = GetUI<HometopUI>("UI_Hometop");
		ActiveUIList.Remove(uI);
		List<OrangeUIBase> listkeeps = new List<OrangeUIBase>();
		for (int num = ActiveUIList.Count - 1; num >= 0; num--)
		{
			OrangeUIBase orangeUIBase = ActiveUIList[num];
			listkeeps.Add(orangeUIBase);
			if (orangeUIBase.EnableTopResident && orangeUIBase.EnableBackground)
			{
				break;
			}
		}
		listkeeps.Add(uI);
		listkeeps.Reverse();
		foreach (OrangeUIBase item in listkeeps)
		{
			ActiveUIList.Remove(item);
		}
		Action onUILinkPrepareEvent = this.OnUILinkPrepareEvent;
		if (onUILinkPrepareEvent != null)
		{
			onUILinkPrepareEvent();
		}
		CloseAllUI(delegate
		{
			foreach (OrangeUIBase item2 in listkeeps)
			{
				ActiveUIList.Add(item2);
			}
			UpdateUISiblingIndex();
			p_cb.CheckTargetToInvoke();
		});
	}

	public void AdditionalEft_FadeIn(OrangeUIBase p_currentUI, bool UpdateInteractable = false)
	{
		if (UpdateInteractable)
		{
			p_currentUI.canvasGroup.interactable = true;
		}
		LeanTween.value(p_currentUI.gameObject, eftStartValue, eftEndValue, eftAnimTime).setOnUpdate(delegate(float val)
		{
			p_currentUI.canvasGroup.alpha = val;
		});
	}

	public void AdditionalEft_FadeOut(OrangeUIBase p_currentUI, bool UpdateInteractable = false)
	{
		if (p_currentUI == null)
		{
			return;
		}
		if (UpdateInteractable && (bool)p_currentUI.canvasGroup)
		{
			p_currentUI.canvasGroup.interactable = false;
		}
		LeanTween.value(p_currentUI.gameObject, eftEndValue, eftStartValue, eftAnimTime).setOnUpdate(delegate(float val)
		{
			if ((bool)p_currentUI.canvasGroup)
			{
				p_currentUI.canvasGroup.alpha = val;
			}
		});
	}

	public void Eft_ShakeUI()
	{
		foreach (OrangeUIBase activeUI in ActiveUIList)
		{
			activeUI.Shake();
		}
	}

	public void Eft_ShakeX(float p_time, float p_start, float p_end)
	{
		if (p_time <= 0f)
		{
			return;
		}
		foreach (OrangeUIBase activeUI in ActiveUIList)
		{
			activeUI.ShakeX(p_time, p_start, p_end);
		}
	}

	public void Eft_ShakeY(float p_time, float p_start, float p_end)
	{
		if (p_time <= 0f)
		{
			return;
		}
		foreach (OrangeUIBase activeUI in ActiveUIList)
		{
			activeUI.ShakeY(p_time, p_start, p_end);
		}
	}
}
