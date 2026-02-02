using System;
using CallbackDefs;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class BossChallengeUnit : MonoBehaviour
{
	[SerializeField]
	private UI_Challenge parentUI;

	[SerializeField]
	private Image imgBossHead;

	[SerializeField]
	private Material materialGray;

	[SerializeField]
	private Image imgBlackMask;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_moveSE;

	private const string AssetBossHead = "BossHead_";

	private readonly Vector2 appearPos = new Vector2(131f, 17f);

	private Image imgBossHeadEftFg;

	private Button btnUnit;

	private StarClearComponent starClearComponent;

	private Vector2 localPos;

	private int idx;
    [Obsolete]
    private CallbackIdx m_cb;

	private Vector3 zero = Vector3.zero;

	private Vector3 one = Vector3.one;

	private bool cancelTween;

	public void Awake()
	{
		starClearComponent = GetComponent<StarClearComponent>();
		btnUnit = GetComponent<Button>();
		btnUnit.onClick.AddListener(OnClickUnit);
		localPos = base.transform.localPosition;
		base.transform.localPosition = appearPos;
		base.transform.localScale = zero;
		GameObject gameObject = new GameObject();
		gameObject.layer = base.gameObject.layer;
		imgBossHeadEftFg = gameObject.AddComponent<Image>();
		imgBossHeadEftFg.sprite = null;
		gameObject.GetComponent<RectTransform>().SetParent(imgBossHead.transform);
		gameObject.transform.localScale = one;
	}

    [Obsolete]
    public void Setup(int p_idx, int p_clearStar, bool canChallenge, CallbackIdx p_cb)
	{
		imgBossHead.sprite = null;
		idx = p_idx;
		m_cb = p_cb;
		if (canChallenge)
		{
			imgBossHead.material = null;
			if (imgBlackMask != null)
			{
				imgBlackMask.gameObject.SetActive(true);
			}
		}
		else
		{
			imgBossHead.material = materialGray;
			if (imgBlackMask != null)
			{
				imgBlackMask.gameObject.SetActive(false);
			}
		}
		if (p_cb != null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_iconBossHead, "BossHead_" + (idx + 1).ToString("00"), delegate(Sprite obj)
			{
				if (obj != null)
				{
					imgBossHead.sprite = obj;
					imgBossHead.rectTransform.sizeDelta = obj.rect.size;
					imgBossHead.color = Color.white;
					imgBossHeadEftFg.color = Color.white;
					imgBossHeadEftFg.rectTransform.sizeDelta = imgBossHead.rectTransform.sizeDelta;
				}
				else
				{
					imgBossHead.color = Color.clear;
				}
				if (cancelTween)
				{
					FixedUnit();
				}
				else
				{
					LeanTween.moveLocal(base.gameObject, localPos, 0.2f).setOnComplete((Action)delegate
					{
						if (parentUI.IsBossChallenge)
						{
							MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_moveSE);
						}
					});
					LeanTween.color(imgBossHeadEftFg.rectTransform, Color.clear, 0.2f).setDelay(0.2f);
					LeanTween.scale(base.gameObject, one, 0.3f).setEaseInOutBounce();
				}
			});
		}
		else
		{
			imgBossHead.color = Color.clear;
		}
		starClearComponent.SetActiveStar(p_clearStar);
	}

	private void OnDestroy()
	{
		btnUnit.onClick.RemoveAllListeners();
	}

	private void OnDisable()
	{
		LeanTween.cancel(base.gameObject);
	}

	private void OnClickUnit()
	{
		m_cb.CheckTargetToInvoke(idx);
	}

	public void IgonreTween()
	{
		if (!cancelTween)
		{
			cancelTween = true;
			LeanTween.cancel(base.gameObject);
			FixedUnit();
		}
	}

	private void FixedUnit()
	{
		base.transform.localPosition = localPos;
		imgBossHeadEftFg.color = Color.clear;
		base.transform.localScale = one;
	}

	public void SetInvisable()
	{
		LeanTween.cancel(base.gameObject);
		cancelTween = false;
		imgBossHead.sprite = null;
		imgBossHeadEftFg.color = Color.white;
		base.transform.localPosition = appearPos;
		base.transform.localScale = zero;
	}

	public void SwitchBlackMask(bool toggleon)
	{
		imgBlackMask.enabled = !toggleon;
	}
}
