#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using OrangeApi;
using UnityEngine;
using UnityEngine.UI;

public class PrizeTopUI : OrangeUIBase
{
	private const int PRIZE_TYPE = 2;

	private const int REWARD_LEVEL = 8;

	[SerializeField]
	private CanvasGroup canvasPieces;

	[SerializeField]
	private Image iconCost;

	[SerializeField]
	private OrangeText textCost;

	[SerializeField]
	private OrangeText textDate;

	[SerializeField]
	private OrangeText textNaviMsg;

	[SerializeField]
	private Transform stParent;

	[SerializeField]
	private Image imgProgress;

	[SerializeField]
	private OrangeText textProgress;

	[SerializeField]
	private RectTransform rectLayoutParent;

	[SerializeField]
	private GachaBtnUnit gachaBtnUnit;

	[SerializeField]
	private PrizeAnimController prizeAnimController;

	[SerializeField]
	private RectTransform[] rectPieces = new RectTransform[8];

	[SerializeField]
	private GameObject effect_1get;

	[SerializeField]
	private GameObject effect_2get;

	private float[] pieceRotation = new float[8] { -360f, -45f, -90f, -135f, -180f, -225f, -270f, -315f };

	private List<GachaBtnUnit> listGachaBtn = new List<GachaBtnUnit>();

	private int guaranteeValueMax;

	private int guaranteeValueNow;

	private int guaranteeID;

	private List<GACHA_TABLE> listGachaReward = new List<GACHA_TABLE>();

	private ITEM_TABLE costItem;

	private StandNaviDb naviDb;

	private bool isPlaying;

	private readonly float progressAnimSpd = 0.05f;

	private int rewardIdx;

	private GachaRes gachaRes;

	protected override void Awake()
	{
		base.Awake();
		isPlaying = false;
		effect_1get.SetActive(false);
		effect_2get.SetActive(false);
		System.Random random = new System.Random();
		for (int i = 0; i < 8; i++)
		{
			int num = random.Next(0, 8);
			float num2 = pieceRotation[i];
			pieceRotation[i] = pieceRotation[num];
			pieceRotation[num] = num2;
		}
		for (int j = 0; j < 8; j++)
		{
			rectPieces[j].localRotation = Quaternion.Euler(0f, 0f, 0f - pieceRotation[j]);
		}
		canvasPieces.alpha = 1f;
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<GachaRes>(EventManager.ID.GACHA_PRIZE_START, PrizeStart);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_PLAYER_BOX, UpdateLuckyChip);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.Stop("NAVI_MENU");
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_PLAYER_BOX, UpdateLuckyChip);
		Singleton<GenericEventManager>.Instance.DetachEvent<GachaRes>(EventManager.ID.GACHA_PRIZE_START, PrizeStart);
	}

	public void Setup()
	{
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		List<GACHALIST_TABLE> gachaListTableByOepening = ManagedSingleton<ExtendDataHelper>.Instance.GetGachaListTableByOepening(serverUnixTimeNowUTC, 2);
		if (gachaListTableByOepening.Count > 0)
		{
			GACHALIST_TABLE select = gachaListTableByOepening[0];
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(select.n_COIN_ID, out costItem))
			{
				UpdateCostInfo();
			}
			if (ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(select.s_BEGIN_TIME))
			{
				textDate.text = string.Empty;
			}
			else
			{
				textDate.text = OrangeGameUtility.DisplayDatePeriod(select.s_BEGIN_TIME, select.s_END_TIME);
			}
			guaranteeID = select.n_LUCKY_GACHA;
			guaranteeValueMax = select.n_LUCKY;
			listGachaReward = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(select.n_GACHAID_1);
			ManagedSingleton<PlayerNetManager>.Instance.RetrieveGachaRecordReq(delegate
			{
				guaranteeValueNow = 0;
				ManagedSingleton<PlayerNetManager>.Instance.dicGachaGuaranteeRecord.TryGetValue(select.n_GROUP, out guaranteeValueNow);
				SetGuaranteeProgress();
				if (guaranteeValueNow == guaranteeValueMax)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 9);
				}
				else
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 8);
				}
			});
		}
		gachaListTableByOepening = (from x in gachaListTableByOepening
			orderby x.n_SORT, x.n_ID
			select x).ToList();
		foreach (GACHALIST_TABLE item in gachaListTableByOepening)
		{
			GachaBtnUnit gachaBtnUnit = UnityEngine.Object.Instantiate(this.gachaBtnUnit, rectLayoutParent);
			gachaBtnUnit.Setup(item);
			listGachaBtn.Add(gachaBtnUnit);
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, "ch_navi_0"), "ch_navi_0_db", delegate(GameObject obj)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(obj, stParent, false);
			naviDb = gameObject.GetComponent<StandNaviDb>();
			if ((bool)naviDb)
			{
				naviDb.Setup(StandNaviDb.NAVI_DB_TYPE.NORMAL, 2);
			}
			textNaviMsg.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ROULETTE_TALK_DEFAULT");
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM01", 31);
	}

	public void Start()
	{
		closeCB = (Callback)Delegate.Combine(closeCB, (Callback)delegate
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_HOME_BGM);
		});
	}

	private void UpdateCostInfo()
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(costItem.s_ICON), costItem.s_ICON, delegate(Sprite obj)
		{
			iconCost.sprite = obj;
			iconCost.color = Color.white;
			textCost.text = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(costItem.n_ID).ToString();
		});
	}

	private void SetGuaranteeProgress(bool anim = false)
	{
		float num = Mathf.Clamp01((float)guaranteeValueNow / (float)guaranteeValueMax);
		if (anim)
		{
			float time = Mathf.Clamp(progressAnimSpd * Mathf.Abs((float)guaranteeValueNow - imgProgress.fillAmount * 100f), 0.1f, 1f);
			LeanTween.value(imgProgress.fillAmount, num, time).setOnUpdate(delegate(float val)
			{
				textProgress.text = Mathf.CeilToInt(val * 100f) + "%";
				imgProgress.fillAmount = val;
			}).setOnComplete((Action)delegate
			{
				SetGuaranteeProgress();
			});
		}
		else
		{
			textProgress.text = guaranteeValueNow + "%";
			imgProgress.fillAmount = num;
		}
	}

	public void PrizeStart(GachaRes res)
	{
		isPlaying = true;
		foreach (GachaBtnUnit item in listGachaBtn)
		{
			item.Button.interactable = false;
		}
		gachaRes = res;
		textCost.text = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(costItem.n_ID).ToString();
		guaranteeValueNow = gachaRes.GachaGuaranteeRecord.Value;
		List<NetRewardInfo> rewardList = gachaRes.RewardEntities.RewardList;
		int num = int.MaxValue;
		float num2 = 0f;
		bool flag = false;
		foreach (NetRewardInfo item2 in rewardList)
		{
			if (item2.GachaID == guaranteeID)
			{
				flag = true;
				break;
			}
			if (item2.GachaID < num)
			{
				num = item2.GachaID;
			}
		}
		if (flag)
		{
			rewardIdx = 0;
		}
		else
		{
			for (int i = 0; i < listGachaReward.Count; i++)
			{
				if (listGachaReward[i].n_ID == num)
				{
					rewardIdx = i;
					break;
				}
			}
		}
		num2 = pieceRotation[rewardIdx];
		Debug.Log("Get Reward Number:" + (rewardIdx + 1));
		prizeAnimController.StartAnim(num2);
	}

	public void OnClickStopPrizeRotate()
	{
		if (!prizeAnimController.CanSkip || gachaRes.RewardEntities == null)
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		prizeAnimController.StopAnim(delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STOP);
			bool flag = false;
			if (rewardIdx == 0)
			{
				flag = true;
				if ((bool)naviDb)
				{
					naviDb.Play(1);
					StartCoroutine(PlayVoice(0.2f, 16, effect_1get));
				}
			}
			else if (rewardIdx == 1)
			{
				flag = true;
				if ((bool)naviDb)
				{
					naviDb.Play(2);
					StartCoroutine(PlayVoice(0.2f, 16, effect_2get));
				}
			}
			if (flag)
			{
				LeanTween.value(base.gameObject, 0f, 1f, 0.9f).setOnComplete((Action)delegate
				{
					ShowRewardPopup(true);
				});
			}
			else
			{
				StartCoroutine(PlayVoiceOnly(0.25f, 25));
				LeanTween.value(base.gameObject, 0f, 1f, 0.4f).setOnComplete((Action)delegate
				{
					ShowRewardPopup(false);
				});
			}
		});
	}

	private void ShowRewardPopup(bool closeFx)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
		{
			if (closeFx)
			{
				effect_1get.SetActive(false);
				effect_2get.SetActive(false);
			}
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				if (guaranteeValueNow == guaranteeValueMax)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 9);
				}
				if (closeFx && (bool)naviDb)
				{
					naviDb.Play(0);
				}
			});
			ui.Setup(gachaRes.RewardEntities);
			SetGuaranteeProgress(true);
			foreach (GachaBtnUnit item in listGachaBtn)
			{
				item.Button.interactable = true;
			}
			isPlaying = false;
		});
	}

	public void OnClickRuleBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonScrollMsg", delegate(CommonScrollMsgUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ROULETTE_RULE"));
		});
	}

	public void OnClickRewardInfoBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PrizeReward", delegate(PrizeRewardUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(ref listGachaReward);
		});
	}

	public override void OnClickCloseBtn()
	{
		if (isPlaying)
		{
			OnClickStopPrizeRotate();
		}
		else
		{
			base.OnClickCloseBtn();
		}
	}

	public override bool CanBackToHometop()
	{
		if (isPlaying)
		{
			OnClickStopPrizeRotate();
			return false;
		}
		return true;
	}

	public override bool CanBuy()
	{
		if (isPlaying)
		{
			OnClickStopPrizeRotate();
			return false;
		}
		return true;
	}

	protected override bool IsEscapeVisible()
	{
		if (isPlaying)
		{
			return false;
		}
		return base.IsEscapeVisible();
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		if (!enable)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Stop("NAVI_MENU");
		}
	}

	private IEnumerator PlayVoice(float delay, int voiceID, GameObject eff)
	{
		yield return new WaitForSeconds(0.3f);
		eff.SetActive(true);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OPEN01);
		yield return new WaitForSeconds(delay);
		MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", voiceID);
	}

	private IEnumerator PlayVoiceOnly(float delay, int voiceID)
	{
		yield return new WaitForSeconds(delay);
		MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", voiceID);
	}

	private void UpdateLuckyChip()
	{
		textCost.text = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(costItem.n_ID).ToString();
	}
}
