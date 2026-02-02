using System;
using CallbackDefs;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class OrangeUIBase : MonoBehaviour
{
	public enum EscapeEvent
	{
		NONE = 0,
		RETURN_TILE = 1,
		CLOSE_UI = 2,
		CLOSE_GAME = 3,
		CUSTOM = 4
	}

	public bool hasBlackBg;

	public UIManager.EffectType effectTypeOpen = UIManager.EffectType.EXPAND;

	public UIManager.EffectType effectTypeClose = UIManager.EffectType.EXPAND;

	private bool setBackgroundComplete;

	[BoxGroup("TopResident")]
	public bool EnableTopResident;

	[BoxGroup("TopResident")]
	[ShowIf("EnableTopResident")]
	public RectTransform ResidentParent;

	[BoxGroup("TopResident")]
	[ShowIf("EnableTopResident")]
	public string UIName = string.Empty;

	[BoxGroup("Background")]
	public bool EnableBackground;

	[BoxGroup("Background")]
	[ShowIf("EnableBackground")]
	public OrangeBgBase Background;

	[BoxGroup("Sound")]
	public SystemSE m_clickTopBarBackSE = SystemSE.CRI_SYSTEMSE_SYS_BACK01;

	private Canvas canvas;

	private GraphicRaycaster graphicRaycaster;

	[HideInInspector]
	public CanvasGroup canvasGroup;

	[HideInInspector]
	public RectTransform rt;

	public Callback closeCB;

	public Callback loadedCB;

	protected Callback backToHometopCB;

	[HideInInspector]
	public OrangeUIAnimation[] AnimationGroup;

	[BoxGroup("SafeArea")]
	public bool ConformX = true;

	[BoxGroup("SafeArea")]
	public bool ConformY = true;

	private bool isClosing;

	private int[] rand = new int[2] { 1, -1 };

	private Quaternion zero = Quaternion.Euler(0f, 0f, 0f);

	private int shakingId = -1;

	private int shakingXId = -1;

	private int shakingYId = -1;

	private bool isShaking;

	public SystemSE CloseSE { get; set; }

	public bool IsConfirmUI { get; set; }

	public bool SetBackgroundComplete
	{
		get
		{
			return setBackgroundComplete;
		}
		set
		{
			setBackgroundComplete = value;
		}
	}

	public bool IsVisible { get; protected set; }

	public bool IsLock { get; set; }

	public bool EnableBackToHometop { get; set; }

	public EscapeEvent _EscapeEvent { get; set; }

	protected virtual void Awake()
	{
		canvas = GetComponent<Canvas>();
		graphicRaycaster = GetComponent<GraphicRaycaster>();
		canvasGroup = GetComponent<CanvasGroup>();
		rt = GetComponent<RectTransform>();
		AnimationGroup = GetComponents<OrangeUIAnimation>();
		canvasGroup.alpha = 0f;
		EnableBackToHometop = true;
		IsVisible = false;
		IsLock = true;
		IsConfirmUI = false;
		_EscapeEvent = EscapeEvent.NONE;
	}

	public void SetInteractable(bool active)
	{
		canvasGroup.interactable = active;
	}

	public virtual void OnClickCloseBtn()
	{
		if (!IsLock && !isClosing)
		{
			if (CloseSE > SystemSE.NONE)
			{
				PlayUISE(CloseSE);
			}
			CloseUIBase();
		}
	}

	protected void CloseUIBase()
	{
		if (!IsLock && !isClosing)
		{
			IsLock = true;
			if (EnableBackground || hasBlackBg)
			{
				graphicRaycaster.enabled = false;
			}
			DestoryBg();
			closeCB.CheckTargetToInvoke();
			closeCB = null;
			backToHometopCB = null;
			MonoBehaviourSingleton<UIManager>.Instance.CloseUI(this);
		}
	}

	protected void DestoryBg()
	{
		if (EnableBackground && SetBackgroundComplete)
		{
			UnityEngine.Object.Destroy(Background.gameObject);
		}
	}

	public void ForceDestory()
	{
		if (!isClosing)
		{
			isClosing = true;
			backToHometopCB.CheckTargetToInvoke();
			backToHometopCB = null;
			DestoryBg();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void UpdateSiblingIndex(int idx)
	{
		if (EnableBackground)
		{
			Background.transform.SetSiblingIndex(idx);
		}
		base.transform.SetSiblingIndex(idx + 1);
	}

	public void Shake()
	{
		base.transform.localRotation = zero;
		LeanTween.cancel(ref shakingId);
		shakingId = LeanTween.rotateAround(base.gameObject, rand[UnityEngine.Random.Range(0, 2)] * Vector3.forward, 0.4f, 0.08f).setLoopClamp().setRepeat(2)
			.setOnComplete((Action)delegate
			{
				base.transform.localRotation = zero;
				shakingId = -1;
			})
			.uniqueId;
	}

	public void ShakeX(float p_time, float p_start, float p_end)
	{
		if (isShaking || shakingXId != -1 || shakingYId != -1)
		{
			return;
		}
		isShaking = true;
		Transform t = GetComponent(typeof(Transform)) as Transform;
		Vector3 initialPos = t.transform.localPosition;
		shakingId = LeanTween.value(base.gameObject, p_start, p_end, p_time).setOnUpdate(delegate(float val)
		{
			Vector3 vector = new Vector3(UnityEngine.Random.value, 0f, 0f) * val;
			t.transform.localPosition = initialPos + vector;
		}).setOnComplete((Action)delegate
		{
			if (t != null)
			{
				t.localPosition = initialPos;
				shakingXId = -1;
				isShaking = false;
			}
		})
			.uniqueId;
	}

	public void ShakeY(float p_time, float p_start, float p_end)
	{
		if (isShaking || shakingXId != -1 || shakingYId != -1)
		{
			return;
		}
		isShaking = true;
		Transform t = GetComponent(typeof(Transform)) as Transform;
		Vector3 initialPos = t.transform.localPosition;
		shakingId = LeanTween.value(base.gameObject, p_start, p_end, p_time).setOnUpdate(delegate(float val)
		{
			Vector3 vector = new Vector3(0f, UnityEngine.Random.value, 0f) * val;
			t.transform.localPosition = initialPos + vector;
		}).setOnComplete((Action)delegate
		{
			if (t != null)
			{
				t.localPosition = initialPos;
				shakingYId = -1;
				isShaking = false;
			}
		})
			.uniqueId;
	}

	public virtual void SetCanvas(bool enable)
	{
		if (!(canvas == null))
		{
			if (IsConfirmUI)
			{
				enable = true;
			}
			canvas.enabled = enable;
			canvasGroup.interactable = enable;
			IsVisible = enable;
			if (EnableBackground && Background.canvas.isActiveAndEnabled != enable)
			{
				Background.canvas.enabled = enable;
			}
		}
	}

	public virtual void SafeAreaChange(float x, float y)
	{
	}

	public virtual bool CanBackToHometop()
	{
		if (Singleton<GuildSystem>.Instance.MainSceneController != null)
		{
			return GuildUIHelper.BackToHometop(this);
		}
		return true;
	}

	public virtual bool CanBuy()
	{
		return true;
	}

	public void DoEscapeEvent()
	{
		if (!IsEscapeVisible())
		{
			return;
		}
		switch (_EscapeEvent)
		{
		case EscapeEvent.RETURN_TILE:
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RETURN_LOGIN_CONFIRM"), delegate
			{
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_HOME);
				MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
				{
					MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
				});
			}, null);
			break;
		case EscapeEvent.CLOSE_UI:
			OnClickCloseBtn();
			break;
		case EscapeEvent.CLOSE_GAME:
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CLOSE_APP_CONFIRM"), delegate
			{
				Application.Quit();
			}, null);
			break;
		case EscapeEvent.CUSTOM:
			DoCustomEscapeEvent();
			break;
		case EscapeEvent.NONE:
			break;
		}
	}

	public void PlayUISE(SystemSE cueid, Callback pb = null, bool lockinput = false)
	{
		PlayUISE("SystemSE", (int)cueid, pb, lockinput);
	}

	public void PlayUISE(SystemSE02 cueid, Callback pb = null, bool lockinput = false)
	{
		PlayUISE("SystemSE02", (int)cueid, pb, lockinput);
	}

	public void PlayUISE(string s_acb, int cueid, Callback pb = null, bool lockinput = false)
	{
		if (!MonoBehaviourSingleton<AssetsBundleManager>.Instance.IsDownloading())
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySE_AfterCallback(s_acb, cueid, pb, lockinput);
		}
	}

	public void PlayUISE(string strAcbCue)
	{
		string[] array = strAcbCue.Split(',');
		if (array.Length == 2)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Play(array[0], array[1]);
		}
	}

	protected virtual void DoCustomEscapeEvent()
	{
	}

	protected virtual bool IsEscapeVisible()
	{
		if (MonoBehaviourSingleton<UIManager>.Instance.IsConnecting())
		{
			return false;
		}
		if (TurtorialUI.IsTutorialing())
		{
			return false;
		}
		if (MonoBehaviourSingleton<OrangeIAP>.Instance.InProgress)
		{
			return false;
		}
		if (MonoBehaviourSingleton<UIManager>.Instance.IsLoading)
		{
			return false;
		}
		if (MonoBehaviourSingleton<AssetsBundleManager>.Instance.IsDownloading())
		{
			return false;
		}
		return true;
	}

	public virtual void DoJoystickEvent()
	{
	}

	public virtual void TopOpenUICloseCB()
	{
	}

	public virtual void TopbarInitComplete(Topbar topbar)
	{
	}

	protected virtual void OnBackToHometop()
	{
		if (Singleton<GuildSystem>.Instance.MainSceneController != null)
		{
			OnClickCloseBtn();
		}
	}
}
