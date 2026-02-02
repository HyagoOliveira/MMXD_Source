using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonthActivityCell : ScrollIndexCallback
{
	[Header("PoorReward")]
	private const int MAX_POOR_REWARD_COUNT = 3;

	[SerializeField]
	private GameObject[] transPoorRewardItemPos = new GameObject[3];

	[SerializeField]
	private GameObject[] PoorRewardReceived = new GameObject[3];

	[Header("PaidReward")]
	private const int MAX_PAID_REWARD_COUNT = 3;

	[SerializeField]
	private GameObject[] transPaidRewardItemPos = new GameObject[3];

	[SerializeField]
	private GameObject[] PaidRewardReceived = new GameObject[3];

	[SerializeField]
	private GameObject[] PaidRewardlock = new GameObject[3];

	[SerializeField]
	private Text activityText;

	[SerializeField]
	private GameObject ReachImage;

	[SerializeField]
	private GameObject NoReachImage;

	[SerializeField]
	private Image ReachBarImage;

	[SerializeField]
	private Image ReachBarOverImage;

	[SerializeField]
	private Text textButton;

	[SerializeField]
	private Button receiveBtn;

	[SerializeField]
	private Image arrowImage;

	[SerializeField]
	private Image frameImage;

	private int activityValue;

	private bool bPoorRewardRetrieved;

	private bool bPaidRewardRetrieved;

	private bool bPaid;

	private List<NetRewardInfo> rewardList = new List<NetRewardInfo>();

	private int listIndex;

	private CommonIconBase[] arrIcon = new CommonIconBase[6];

	private MissionUI parentMissionUI;

	public override void ResetStatus()
	{
		for (int i = 0; i < 6; i++)
		{
			if (!(arrIcon[i] == null))
			{
				continue;
			}
			if (i < 3)
			{
				GameObject gameObject = transPoorRewardItemPos[i];
				CommonIconBase componentInChildren = gameObject.GetComponentInChildren<CommonIconBase>(true);
				if (null == componentInChildren)
				{
					Object.Instantiate(MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<GameObject>(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall"), gameObject.transform);
					componentInChildren = gameObject.GetComponentInChildren<CommonIconBase>();
					componentInChildren.gameObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
					componentInChildren.gameObject.SetActive(false);
				}
				arrIcon[i] = componentInChildren;
				gameObject.gameObject.SetActive(true);
			}
			else
			{
				GameObject gameObject2 = transPaidRewardItemPos[i - 3];
				CommonIconBase componentInChildren2 = gameObject2.GetComponentInChildren<CommonIconBase>(true);
				if (null == componentInChildren2)
				{
					Object.Instantiate(MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<GameObject>(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall"), gameObject2.transform);
					componentInChildren2 = gameObject2.GetComponentInChildren<CommonIconBase>();
					componentInChildren2.gameObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
					componentInChildren2.gameObject.SetActive(false);
				}
				arrIcon[i] = componentInChildren2;
				gameObject2.gameObject.SetActive(true);
			}
			arrIcon[i].gameObject.SetActive(true);
		}
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_idx, out item))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				CARD_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)item.f_VALUE_Y, out value))
				{
					ui.CanShowHow2Get = false;
					ui.Setup(value, item);
				}
			}
			else
			{
				ui.CanShowHow2Get = false;
				ui.Setup(item);
			}
		});
	}

	private void SetItemWithAmountForCard(int idx, int ItemID, int ItemCount)
	{
		ITEM_TABLE item = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(ItemID, out item))
		{
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				arrIcon[idx].SetItemWithAmountForCard(ItemID, ItemCount, OnClickItem);
			}
			else
			{
				arrIcon[idx].SetItemWithAmount(ItemID, ItemCount, OnClickItem);
			}
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		listIndex = p_idx;
		parentMissionUI = GetComponentInParent<MissionUI>();
		ResetStatus();
		MISSION_TABLE mISSION_TABLE = parentMissionUI.OnGetMissionTable(listIndex, false);
		MISSION_TABLE mISSION_TABLE2 = parentMissionUI.OnGetMissionTable(listIndex, true);
		SetItemWithAmountForCard(0, mISSION_TABLE.n_ITEMID_1, mISSION_TABLE.n_ITEMCOUNT_1);
		SetItemWithAmountForCard(1, mISSION_TABLE.n_ITEMID_2, mISSION_TABLE.n_ITEMCOUNT_2);
		SetItemWithAmountForCard(2, mISSION_TABLE.n_ITEMID_3, mISSION_TABLE.n_ITEMCOUNT_3);
		SetItemWithAmountForCard(3, mISSION_TABLE2.n_ITEMID_1, mISSION_TABLE2.n_ITEMCOUNT_1);
		SetItemWithAmountForCard(4, mISSION_TABLE2.n_ITEMID_2, mISSION_TABLE2.n_ITEMCOUNT_2);
		SetItemWithAmountForCard(5, mISSION_TABLE2.n_ITEMID_3, mISSION_TABLE2.n_ITEMCOUNT_3);
		int[] array = new int[6] { mISSION_TABLE.n_ITEMCOUNT_1, mISSION_TABLE.n_ITEMCOUNT_2, mISSION_TABLE.n_ITEMCOUNT_3, mISSION_TABLE2.n_ITEMCOUNT_1, mISSION_TABLE2.n_ITEMCOUNT_2, mISSION_TABLE2.n_ITEMCOUNT_3 };
		activityValue = ManagedSingleton<MissionHelper>.Instance.CurrentMonthlyActivityValue;
		activityText.text = mISSION_TABLE.n_CONDITION_Y.ToString();
		textButton.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MISSION_RETRIEVE");
		bool flag = activityValue >= mISSION_TABLE.n_CONDITION_Y;
		ReachImage.SetActive(flag);
		ReachBarImage.gameObject.SetActive(flag);
		NoReachImage.SetActive(!flag);
		ReachBarOverImage.gameObject.SetActive(activityValue > mISSION_TABLE.n_CONDITION_Y);
		bPaid = ManagedSingleton<MissionHelper>.Instance.CurrentMontlyActivityPaid;
		bPoorRewardRetrieved = ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(mISSION_TABLE.n_ID);
		bPaidRewardRetrieved = ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(mISSION_TABLE2.n_ID);
		for (int i = 0; i < arrIcon.Length; i++)
		{
			bool active = array[i] > 0;
			arrIcon[i].gameObject.SetActive(active);
		}
		for (int j = 0; j < 3; j++)
		{
			bool flag2 = array[j] > 0;
			PoorRewardReceived[j].SetActive(bPoorRewardRetrieved && flag2);
		}
		for (int k = 0; k < 3; k++)
		{
			bool flag3 = array[k + 3] > 0;
			PaidRewardReceived[k].SetActive(bPaidRewardRetrieved && flag3);
			PaidRewardlock[k].SetActive(!bPaid && flag3);
		}
		bool flag4 = !bPoorRewardRetrieved || (!bPaidRewardRetrieved && bPaid);
		receiveBtn.interactable = flag4;
		if (!flag4)
		{
			textButton.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MISSION_RETRIEVED");
		}
		receiveBtn.gameObject.SetActive(flag);
		if (flag)
		{
			LeanTween.moveLocalX(arrowImage.gameObject, 75f, 1f).setLoopPingPong().setEaseInCubic();
			LeanTween.scale(frameImage.gameObject, new Vector3(2f, 2f), 1f).setLoopClamp();
			LeanTween.value(frameImage.gameObject, 1f, 0f, 1f).setOnUpdate(delegate(float alpha)
			{
				frameImage.color = new Color(1f, 1f, 1f, alpha);
			}).setLoopClamp();
		}
	}

	public void ShowRewardPopup()
	{
		if (rewardList != null && rewardList.Count > 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
			{
				ui.Setup(rewardList);
				parentMissionUI.OnResetAllRetrieveBtn();
				ScrollCellIndex(listIndex);
			});
		}
	}

	public void OnClickRetrievePaidBtn()
	{
		MISSION_TABLE mISSION_TABLE = parentMissionUI.OnGetMissionTable(listIndex, true);
		bool flag = false;
		if (bPaid && mISSION_TABLE != null && !ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(mISSION_TABLE.n_ID) && ManagedSingleton<MissionHelper>.Instance.CheckMissionCompleted(mISSION_TABLE.n_ID))
		{
			flag = true;
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ReceiveMissionRewardReq(mISSION_TABLE.n_ID, delegate(object p_param)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.MISSION);
				rewardList.AddRange(p_param as List<NetRewardInfo>);
				ShowRewardPopup();
			});
		}
		if (!flag)
		{
			ShowRewardPopup();
		}
		ScrollCellIndex(listIndex);
	}

	public void OnClickRetrieveBtn()
	{
		MISSION_TABLE mISSION_TABLE = parentMissionUI.OnGetMissionTable(listIndex, false);
		if (mISSION_TABLE == null)
		{
			return;
		}
		rewardList.Clear();
		if (!ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(mISSION_TABLE.n_ID))
		{
			if (ManagedSingleton<MissionHelper>.Instance.CheckMissionCompleted(mISSION_TABLE.n_ID))
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ReceiveMissionRewardReq(mISSION_TABLE.n_ID, delegate(object p_param)
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.MISSION);
					rewardList.AddRange(p_param as List<NetRewardInfo>);
					OnClickRetrievePaidBtn();
				});
			}
		}
		else
		{
			OnClickRetrievePaidBtn();
		}
	}
}
