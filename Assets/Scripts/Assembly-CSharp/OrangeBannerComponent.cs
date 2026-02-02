using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class OrangeBannerComponent : MonoBehaviour
{
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private L10nRawImage[] imgGroup = new L10nRawImage[3];

	private Vector2[] bannerPos = new Vector2[3];

	private List<BANNER_TABLE> listBanner = new List<BANNER_TABLE>();

	private RectTransform scrollRectTransform;

	private RectTransform content;

	private float offsetLast;

	private float offsetNext;

	private float AutoScrollTime = 5f;

	private float AutoScrollTimer;

	private bool isBannerTweening;

	private L10nRawImage.ImageType ImgType = L10nRawImage.ImageType.Banner;

	private Vector2 offset = new Vector2(0f, 0f);

	public bool Pause { get; set; }

	public int NowBannerIdx { get; set; }

	private int LastBannerIdx
	{
		get
		{
			if (NowBannerIdx <= 0)
			{
				return listBanner.Count - 1;
			}
			return NowBannerIdx - 1;
		}
	}

	private int NextBannerIdx
	{
		get
		{
			if (NowBannerIdx >= listBanner.Count - 1)
			{
				return 0;
			}
			return NowBannerIdx + 1;
		}
	}

    [System.Obsolete]
    public CallbackIdx BannerOffsetCB { get; set; }

	protected virtual void Awake()
	{
		scrollRectTransform = scrollRect.GetComponent<RectTransform>();
		content = scrollRect.content;
		Pause = false;
	}

	public void Setup(Transform p_root, List<BANNER_TABLE> p_listBanner, Vector2Int p_sizeDelta, Vector2 p_spacing)
	{
		listBanner = p_listBanner;
		NowBannerIdx = 0;
		if (listBanner.Count > 0)
		{
			L10nRawImage[] array = imgGroup;
			foreach (L10nRawImage obj in array)
			{
				obj.Width = p_sizeDelta.x;
				obj.Height = p_sizeDelta.y;
			}
			base.transform.SetParent(p_root, false);
			BANNER_TABLE bANNER_TABLE = listBanner[NowBannerIdx];
			Vector2 vector = new Vector2((float)p_sizeDelta.x + p_spacing.x, 0f);
			offsetLast = (float)p_sizeDelta.x * 0.25f;
			offsetNext = 0f - offsetLast;
			scrollRectTransform.sizeDelta = p_sizeDelta;
			bannerPos[0] = -vector;
			bannerPos[1] = Vector2.zero;
			bannerPos[2] = vector;
			if (listBanner.Count > 1)
			{
				scrollRect.enabled = true;
				UpdateDisplayImg();
			}
			else
			{
				scrollRect.enabled = false;
				imgGroup[0].gameObject.SetActive(false);
				imgGroup[1].rectTransform.anchoredPosition = bannerPos[1];
				imgGroup[1].Init(ImgType, listBanner[NowBannerIdx].s_IMG);
				imgGroup[2].gameObject.SetActive(false);
				isBannerTweening = false;
			}
			BannerOffsetCB.CheckTargetToInvoke(NowBannerIdx);
			StartCoroutine(OnAutoScollBanner());
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void OnScrollChange()
	{
		if (!isBannerTweening)
		{
			offset = content.offsetMin;
			if (offset.x > offsetLast)
			{
				OnBannerOffset(-1);
			}
			else if (offset.x < offsetNext)
			{
				OnBannerOffset(1);
			}
		}
	}

	public void OnClickBanner()
	{
		if (!isBannerTweening)
		{
			BANNER_TABLE bANNER_TABLE = listBanner[NowBannerIdx];
			if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(bANNER_TABLE.s_URL))
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
				Application.OpenURL(string.Format(bANNER_TABLE.s_URL, MonoBehaviourSingleton<LocalizationManager>.Instance.GetPlatformLan()).Replace(" ", "%20"));
			}
			else if (bANNER_TABLE.n_UILINK > 0)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
				ManagedSingleton<UILinkHelper>.Instance.LoadUI(bANNER_TABLE.n_UILINK);
			}
		}
	}

	public void ResetTimer()
	{
		AutoScrollTimer = 0f;
	}

	private void OnBannerOffset(int add)
	{
		if (TurtorialUI.IsTutorialing() || listBanner.Count == 0)
		{
			return;
		}
		BANNER_TABLE bANNER_TABLE = listBanner[NowBannerIdx];
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(bANNER_TABLE.s_BEGIN_TIME, bANNER_TABLE.s_END_TIME, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC) || !ManagedSingleton<ExtendDataHelper>.Instance.ConfirmBannerVisible(bANNER_TABLE))
		{
			StopAllCoroutines();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_BANNER);
		}
		else
		{
			if (listBanner.Count <= 1)
			{
				return;
			}
			scrollRect.StopMovement();
			isBannerTweening = true;
			NowBannerIdx += add;
			if (NowBannerIdx > listBanner.Count - 1)
			{
				NowBannerIdx = 0;
			}
			else if (NowBannerIdx < 0)
			{
				NowBannerIdx = listBanner.Count - 1;
			}
			if (add > 0)
			{
				LeanTween.value(base.gameObject, imgGroup[0].rectTransform.anchoredPosition.x, bannerPos[0].x * 2f, 0.3f).setOnUpdate(delegate(float f)
				{
					if (imgGroup[0] != null)
					{
						imgGroup[0].rectTransform.anchoredPosition = new Vector2(f, 0f);
					}
				});
				LeanTween.value(base.gameObject, imgGroup[1].rectTransform.anchoredPosition.x, bannerPos[0].x * 2f, 0.3f).setOnUpdate(delegate(float f)
				{
					if (imgGroup[1] != null)
					{
						imgGroup[1].rectTransform.anchoredPosition = new Vector2(f, 0f);
					}
				});
				LeanTween.value(base.gameObject, imgGroup[2].rectTransform.anchoredPosition.x, bannerPos[1].x, 0.3f).setOnUpdate(delegate(float f)
				{
					if (imgGroup[2] != null)
					{
						imgGroup[2].rectTransform.anchoredPosition = new Vector2(f, 0f);
					}
				}).setOnComplete(OnBannerTweenCB);
				return;
			}
			LeanTween.value(base.gameObject, imgGroup[0].rectTransform.anchoredPosition.x, bannerPos[1].x, 0.3f).setOnUpdate(delegate(float f)
			{
				if (imgGroup[0] != null)
				{
					imgGroup[0].rectTransform.anchoredPosition = new Vector2(f, 0f);
				}
			});
			LeanTween.value(base.gameObject, imgGroup[1].rectTransform.anchoredPosition.x, bannerPos[2].x, 0.3f).setOnUpdate(delegate(float f)
			{
				if (imgGroup[1] != null)
				{
					imgGroup[1].rectTransform.anchoredPosition = new Vector2(f, 0f);
				}
			});
			LeanTween.value(imgGroup[2].rectTransform.anchoredPosition.x, bannerPos[2].x * 2f, 0.3f).setOnUpdate(delegate(float f)
			{
				if (imgGroup[2] != null)
				{
					imgGroup[2].rectTransform.anchoredPosition = new Vector2(f, 0f);
				}
			}).setOnComplete(OnBannerTweenCB);
		}
	}

	private void OnBannerTweenCB()
	{
		BannerOffsetCB.CheckTargetToInvoke(NowBannerIdx);
		UpdateDisplayImg();
		isBannerTweening = false;
	}

	private void UpdateDisplayImg()
	{
		if (imgGroup[0] != null)
		{
			imgGroup[0].gameObject.SetActive(true);
			imgGroup[0].rectTransform.anchoredPosition = bannerPos[0];
			imgGroup[0].Init(ImgType, listBanner[LastBannerIdx].s_IMG);
		}
		if (imgGroup[1] != null)
		{
			imgGroup[1].gameObject.SetActive(true);
			imgGroup[1].rectTransform.anchoredPosition = bannerPos[1];
			imgGroup[1].Init(ImgType, listBanner[NowBannerIdx].s_IMG);
		}
		if (imgGroup[2] != null)
		{
			imgGroup[2].gameObject.SetActive(true);
			imgGroup[2].rectTransform.anchoredPosition = bannerPos[2];
			imgGroup[2].Init(ImgType, listBanner[NextBannerIdx].s_IMG);
		}
	}

	private IEnumerator OnAutoScollBanner()
	{
		ResetTimer();
		while (true)
		{
			yield return CoroutineDefine._1sec;
			AutoScrollTimer += 1f;
			if (Pause)
			{
				ResetTimer();
			}
			else if (AutoScrollTimer >= AutoScrollTime)
			{
				ResetTimer();
				OnBannerOffset(1);
			}
		}
	}
}
