#define RELEASE
using Coffee.UIExtensions;
using UnityEngine;

public class OrangeBgExt : OrangeBgBase
{
	private string bgName = string.Empty;

	private OrangeBgBase m_effectImage;

	private int m_effectTweenId;

	public bool ApplyEft { get; set; }

	protected override void Awake()
	{
		base.Awake();
		ApplyEft = false;
	}

	private void Init()
	{
		m_effectImage = this;
	}

	public void ChangeBackground(string p_bgName, float fadeOutTime = 0.3f)
	{
		if (string.IsNullOrEmpty(p_bgName) || p_bgName == "null")
		{
			Debug.LogWarning("Null background, using placeholder Bg_Black.");
			p_bgName = "Bg_Black";
		}
		if (bgName == p_bgName)
		{
			return;
		}
		bgName = p_bgName;
		if (m_effectImage == null)
		{
			Init();
		}
		string bundleName = AssetBundleScriptableObject.Instance.m_uiPath + "background/" + bgName;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(bundleName, bgName, delegate(GameObject obj)
		{
			if (!(this == null) && !(base.gameObject == null))
			{
				if (null != obj)
				{
					GameObject go = Object.Instantiate(obj, base.transform, false);
					OnStartUpdateBg(go, m_effectImage != this);
				}
				else
				{
					GameObject go2 = Object.Instantiate(base.gameObject, base.transform, false);
					OnStartUpdateBg(go2, m_effectImage != this);
				}
			}
		});
	}

	private void OnStartUpdateBg(GameObject go, bool destory)
	{
		m_effectImage.FadeOut(0.3f, destory);
		go.transform.SetParent(base.transform, false);
		go.layer = base.gameObject.layer;
		go.transform.SetAsFirstSibling();
		m_effectImage = go.GetComponent<OrangeBgBase>();
		if (ApplyEft)
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<float>(EventManager.ID.UPDATE_LOADING_EFT, DisplayCloseEft);
		}
	}

	protected override void OnDisable()
	{
		LeanTween.cancel(ref m_effectTweenId);
		base.OnDisable();
		Singleton<GenericEventManager>.Instance.DetachEvent<float>(EventManager.ID.UPDATE_LOADING_EFT, DisplayCloseEft);
	}

	private void DisplayCloseEft(float fadeTime)
	{
		UITransitionEffect eft = m_effectImage.GetComponent<UITransitionEffect>();
		if (!(eft != null))
		{
			return;
		}
		LeanTween.cancel(ref m_effectTweenId);
		m_effectTweenId = LeanTween.value(eft.gameObject, eft.effectFactor, 0f, fadeTime).setOnUpdate(delegate(float val)
		{
			if (null == eft)
			{
				LeanTween.cancel(eft.gameObject);
			}
			else
			{
				eft.effectFactor = val;
			}
		}).uniqueId;
	}
}
