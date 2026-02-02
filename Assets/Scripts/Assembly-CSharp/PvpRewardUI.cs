using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class PvpRewardUI : OrangeUIBase
{
	private readonly string prefskey = "pvp_reward_ui_toggle";

	private readonly int RewardMax = 16;

	private int typeKey;

	private int GachaGroupId;

	[SerializeField]
	private Toggle[] typeToggles;

	[SerializeField]
	private GridLayoutGroup gridLayoutGroup;

	[SerializeField]
	private ItemIconWithAmount itemIcon;

	[SerializeField]
	private GameObject checkMark;

	[SerializeField]
	private Button[] btnsGacha;

	[SerializeField]
	private OrangeText textGachaAmount;

	[SerializeField]
	private Image[] imgProgress;

	[SerializeField]
	private OrangeText[] textProgress;

	[SerializeField]
	private OrangeText[] textProgressAmout;

	[SerializeField]
	private OrangeText[] textBtnGhara;

	private List<CommonIconBase> listItemIcon = new List<CommonIconBase>();

	private List<GameObject> listCheckMark = new List<GameObject>();

	private int canGachaCount;

	private int totalGachaCount;

	private int[] progressMAX;

	private int[] progressGachaCount;

	private int[] counterVal;

	protected override void Awake()
	{
		base.Awake();
		typeKey = PlayerPrefs.GetInt(prefskey, 0);
	}

	public void Setup()
	{
		InitRewardIcon();
		InitToggle();
		UpdateRewardGrid();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void InitRewardIcon()
	{
		Vector3 localPosition = Vector3.zero;
		Transform parent = gridLayoutGroup.transform;
		GameObject assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<GameObject>(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall");
		if (!(assstSync == null))
		{
			assstSync.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
			for (int i = 0; i < RewardMax; i++)
			{
				GameObject gameObject = Object.Instantiate(assstSync, parent, false);
				listItemIcon.Add(gameObject.GetComponentInChildren<CommonIconBase>());
				GameObject gameObject2 = Object.Instantiate(checkMark, gameObject.transform, false);
				gameObject2.transform.localPosition = localPosition;
				gameObject2.gameObject.SetActive(false);
				listCheckMark.Add(gameObject2);
			}
		}
	}

	private void InitToggle()
	{
		for (int i = 0; i < typeToggles.Length; i++)
		{
			typeToggles[i].isOn = i == typeKey;
			typeToggles[i].onValueChanged.AddListener(OnClickToggle);
		}
	}

	public void OnClickToggle(bool p_isOn)
	{
		if (!p_isOn)
		{
			return;
		}
		for (int i = 0; i < typeToggles.Length; i++)
		{
			if (typeToggles[i].isOn)
			{
				if (i != typeKey)
				{
					typeKey = i;
					UpdateRewardGrid();
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				}
				break;
			}
		}
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_idx, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.CanShowHow2Get = false;
				ui.Setup(item);
			});
		}
	}

	private void UpdateRewardGrid()
	{
		for (int i = 0; i < typeToggles.Length; i++)
		{
			typeToggles[i].interactable = false;
		}
		Button[] array = btnsGacha;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].onClick.RemoveAllListeners();
		}
		MultiPlayGachaType multiPlayGachaType = (MultiPlayGachaType)((short)typeKey + 1);
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveMultiPlayGachaRecordReq(multiPlayGachaType, delegate
		{
			List<NetMultiPlayerGachaInfo> value = null;
			if (!ManagedSingleton<PlayerNetManager>.Instance.mmapMultiPlayGachaRecord.TryGetValue(multiPlayGachaType, out value))
			{
				value = new List<NetMultiPlayerGachaInfo>();
			}
			GachaGroupId = ManagedSingleton<OrangeTableHelper>.Instance.GetPvpRewardGachaGroupId(multiPlayGachaType);
			List<GACHA_TABLE> listGachaByGroup = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(GachaGroupId);
			UpdateGachaCount(value.Count, ref multiPlayGachaType);
			for (int k = 0; k < listItemIcon.Count; k++)
			{
				CommonIconBase commonIconBase = listItemIcon[k];
				GACHA_TABLE gachaTable = listGachaByGroup[k];
				NetRewardInfo netGachaRewardInfo = new NetRewardInfo
				{
					RewardType = (sbyte)gachaTable.n_REWARD_TYPE,
					RewardID = gachaTable.n_REWARD_ID,
					Amount = gachaTable.n_AMOUNT_MAX
				};
				string bundlePath = string.Empty;
				string assetPath = string.Empty;
				int rare = 0;
				MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(netGachaRewardInfo, ref bundlePath, ref assetPath, ref rare);
				commonIconBase.SetItemWithAmount(gachaTable.n_REWARD_ID, gachaTable.n_AMOUNT_MIN, OnClickItem);
				if (value.FirstOrDefault((NetMultiPlayerGachaInfo x) => x.GachaID == gachaTable.n_ID) != null)
				{
					listCheckMark[k].SetActive(true);
				}
				else
				{
					listCheckMark[k].SetActive(false);
				}
			}
			for (int l = 0; l < typeToggles.Length; l++)
			{
				typeToggles[l].interactable = true;
			}
		});
	}

	private void UpdateGachaCount(int alreadyGachaCount, ref MultiPlayGachaType multiPlayGachaType)
	{
		List<PVP_REWARD_TABLE> pvpRewardTableByType = ManagedSingleton<OrangeTableHelper>.Instance.GetPvpRewardTableByType(multiPlayGachaType);
		List<PVP_REWARD_TABLE> list = (from x in pvpRewardTableByType
			group x by x.n_COUNTER into x
			select x.First()).ToList();
		int count = list.Count;
		progressMAX = new int[count];
		progressGachaCount = new int[count];
		counterVal = new int[count];
		canGachaCount = 0;
		for (int i = 0; i < count; i++)
		{
			int counterKey = list[i].n_COUNTER;
			counterVal[i] = ManagedSingleton<MissionHelper>.Instance.GetMissionCounter(counterKey);
			PVP_REWARD_TABLE[] array = (from x in pvpRewardTableByType
				where x.n_COUNTER == counterKey
				orderby x.n_CONDITION_X
				select x).ToArray();
			bool flag = false;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].n_CONDITION_X <= counterVal[i])
				{
					canGachaCount += array[j].n_GACHACOUNT;
					progressMAX[i] = array[j].n_CONDITION_X;
					progressGachaCount[i] = array[j].n_GACHACOUNT;
				}
				else if (!flag)
				{
					flag = true;
					progressMAX[i] = array[j].n_CONDITION_X;
					progressGachaCount[i] = array[j].n_GACHACOUNT;
				}
			}
		}
		canGachaCount -= alreadyGachaCount;
		UpdateGachaDisplayInfo();
	}

	private void UpdateGachaDisplayInfo()
	{
		btnsGacha[0].gameObject.SetActive(canGachaCount > 1);
		btnsGacha[1].interactable = canGachaCount > 0;
		textBtnGhara[0].text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_REWARD_GACHA") + "x" + canGachaCount;
		textBtnGhara[1].text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_REWARD_GACHA") + "x1";
		for (int i = 0; i < btnsGacha.Length; i++)
		{
			int amount = ((i != 0) ? 1 : canGachaCount);
			if (btnsGacha[i].isActiveAndEnabled)
			{
				btnsGacha[i].onClick.AddListener(delegate
				{
					OnClickGacha(amount);
				});
			}
			imgProgress[i].fillAmount = Mathf.Clamp01((float)counterVal[i] / (float)progressMAX[i]);
			textProgress[i].text = counterVal[i] + "/" + progressMAX[i];
			textProgressAmout[i].text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_REWARD_GACHA_COUNT") + " + " + progressGachaCount[i];
		}
		textGachaAmount.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_REWARD_GACHA_COUNT") + " : " + canGachaCount;
	}

	private void OnClickGacha(int amount)
	{
		ManagedSingleton<PlayerNetManager>.Instance.MultiPlayGachaReq((sbyte)(typeKey + 1), amount, delegate(int param, List<NetMultiPlayerGachaInfo> multiPlayerGachaInfo, List<NetRewardInfo> rewardList)
		{
			if (param != 25000)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)param, false);
			}
			else if (rewardList != null && rewardList.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					ui.Setup(rewardList);
					UpdateRewardGrid();
				});
			}
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
	}

	public override void OnClickCloseBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.PVP_REWARD);
		PlayerPrefs.SetInt(prefskey, typeKey);
		base.OnClickCloseBtn();
	}
}
