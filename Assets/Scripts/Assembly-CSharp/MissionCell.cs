using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class MissionCell : ScrollIndexCallback
{
	[Header("Mission Base Info")]
	[SerializeField]
	private Text textMissionTitle;

	[SerializeField]
	private Text textMissionDesc;

	[SerializeField]
	private Text textMissionProgress;

	[SerializeField]
	private Image imgMissionProgress;

	[Header("Mission Reward Info")]
	private const int MAX_MISSION_REWARD_COUNT = 3;

	[SerializeField]
	private GameObject[] transRewardItemPos = new GameObject[3];

	private CommonIconBase[] arrIcon = new CommonIconBase[3];

	[Header("Mission Button")]
	[SerializeField]
	private Button btnRetrieve;

	[SerializeField]
	private Image imgRetrieve;

	[SerializeField]
	private Text textButton;

	[SerializeField]
	private Image imgCoverMask;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickGetSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickMoveSE;

	private int listIndex;

	public override void ResetStatus()
	{
		listIndex = 0;
		imgRetrieve.gameObject.SetActive(false);
		for (int i = 0; i < 3; i++)
		{
			if (arrIcon[i] == null)
			{
				GameObject gameObject = transRewardItemPos[i];
				CommonIconBase componentInChildren = gameObject.GetComponentInChildren<CommonIconBase>(true);
				if (null == componentInChildren)
				{
					UnityEngine.Object.Instantiate(MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<GameObject>(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall"), gameObject.transform);
					componentInChildren = gameObject.GetComponentInChildren<CommonIconBase>();
					componentInChildren.gameObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
					componentInChildren.gameObject.SetActive(false);
				}
				arrIcon[i] = componentInChildren;
			}
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

	public override void ScrollCellIndex(int p_idx)
	{
		ResetStatus();
		listIndex = p_idx;
		MISSION_TABLE uIViewData = ManagedSingleton<MissionHelper>.Instance.GetUIViewData(listIndex);
		if (uIViewData == null)
		{
			return;
		}
		textMissionTitle.text = ManagedSingleton<OrangeTextDataManager>.Instance.MISSIONTEXT_TABLE_DICT.GetL10nValue(uIViewData.w_NAME);
		textMissionDesc.text = ManagedSingleton<OrangeTextDataManager>.Instance.MISSIONTEXT_TABLE_DICT.GetL10nValue(uIViewData.w_TIP);
		int missionProgressCount = ManagedSingleton<MissionHelper>.Instance.GetMissionProgressCount(uIViewData.n_ID);
		int missionProgressTotalCount = ManagedSingleton<MissionHelper>.Instance.GetMissionProgressTotalCount(uIViewData.n_ID);
		textMissionProgress.text = string.Format("[{0}/{1}]", missionProgressCount, missionProgressTotalCount);
		imgMissionProgress.fillAmount = (float)missionProgressCount / (float)missionProgressTotalCount;
		textMissionProgress.color = Color.white;
		if (ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(uIViewData.n_ID))
		{
			imgCoverMask.gameObject.SetActive(false);
			btnRetrieve.gameObject.SetActive(false);
			textMissionProgress.color = new Color(12f / 85f, 67f / 85f, 1f);
		}
		else
		{
			imgCoverMask.gameObject.SetActive(false);
			btnRetrieve.gameObject.SetActive(true);
			if (ManagedSingleton<MissionHelper>.Instance.CheckMissionCompleted(uIViewData.n_ID))
			{
				textButton.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MISSION_RETRIEVE");
				imgRetrieve.gameObject.SetActive(true);
				textMissionProgress.color = new Color(12f / 85f, 67f / 85f, 1f);
			}
			else if (uIViewData.n_UILINK == 0)
			{
				btnRetrieve.gameObject.SetActive(false);
			}
			else
			{
				btnRetrieve.gameObject.SetActive(true);
				textButton.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MISSION_GOTO");
			}
		}
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		if (uIViewData.n_TYPE == 1)
		{
			if (uIViewData.n_EXP > 0)
			{
				list.Add(OrangeConst.ITEMID_PLAYER_EXP);
				list2.Add(uIViewData.n_EXP);
			}
			if (uIViewData.n_ACTIVITY > 0)
			{
				list.Add(OrangeConst.ITEMID_DAILY_ACTIVE);
				list2.Add(uIViewData.n_ACTIVITY);
			}
		}
		if (uIViewData.n_ITEMCOUNT_1 > 0)
		{
			list.Add(uIViewData.n_ITEMID_1);
			list2.Add(uIViewData.n_ITEMCOUNT_1);
		}
		if (uIViewData.n_TYPE != 1)
		{
			if (uIViewData.n_ITEMCOUNT_2 > 0)
			{
				list.Add(uIViewData.n_ITEMID_2);
				list2.Add(uIViewData.n_ITEMCOUNT_2);
			}
			if (uIViewData.n_ITEMCOUNT_3 > 0)
			{
				list.Add(uIViewData.n_ITEMID_3);
				list2.Add(uIViewData.n_ITEMCOUNT_3);
			}
		}
		if (uIViewData.n_AP > 0)
		{
			list.Add(OrangeConst.ITEMID_AP);
			list2.Add(uIViewData.n_AP);
		}
		if (uIViewData.n_EP > 0)
		{
			list.Add(OrangeConst.ITEMID_EVENTAP);
			list2.Add(uIViewData.n_EP);
		}
		if (list.Count > 3 || list.Count > 3)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			if (i >= list.Count)
			{
				arrIcon[i].gameObject.SetActive(false);
				continue;
			}
			int num = list[i];
			int amount = list2[i];
			GameObject gameObject = transRewardItemPos[i];
			if (gameObject == null)
			{
				continue;
			}
			gameObject.gameObject.SetActive(true);
			arrIcon[i].gameObject.SetActive(true);
			ITEM_TABLE item = null;
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(num, out item))
			{
				if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
				{
					arrIcon[i].SetItemWithAmountForCard(num, amount, OnClickItem);
				}
				else
				{
					arrIcon[i].SetItemWithAmount(num, amount, OnClickItem);
				}
			}
		}
	}

	public void OnClickRetrieveBtn()
	{
		MISSION_TABLE uIViewData = ManagedSingleton<MissionHelper>.Instance.GetUIViewData(listIndex);
		if (uIViewData == null)
		{
			return;
		}
		int originalAP = ManagedSingleton<PlayerHelper>.Instance.GetStamina();
		int originalEP = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
		if (ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(uIViewData.n_ID))
		{
			return;
		}
		if (ManagedSingleton<MissionHelper>.Instance.CheckMissionCompleted(uIViewData.n_ID))
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickGetSE);
			if (uIViewData.n_EP > 0 && ManagedSingleton<PlayerHelper>.Instance.GetEventStamina() + uIViewData.n_EP > OrangeConst.EP_MAX)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("EPMAX_MESSAGE", 1f);
				return;
			}
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ReceiveMissionRewardReq(uIViewData.n_ID, delegate(object p_param)
			{
				SendMessageUpwards("UpdateMissionList", listIndex, SendMessageOptions.DontRequireReceiver);
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.MISSION);
				List<NetRewardInfo> rewardList = p_param as List<NetRewardInfo>;
				int aPCount = Math.Max(ManagedSingleton<PlayerHelper>.Instance.GetStamina() - originalAP, 0);
				int ePCount = Math.Max(ManagedSingleton<PlayerHelper>.Instance.GetEventStamina() - originalEP, 0);
				MonoBehaviourSingleton<OrangeGameManager>.Instance.AddAPEPToRewardList(ref rewardList, aPCount, ePCount);
				if (rewardList != null && rewardList.Count > 0)
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
					{
						if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsLvUp)
						{
							ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
							{
								MonoBehaviourSingleton<OrangeGameManager>.Instance.DisplayLvPerform();
							});
						}
						ui.Setup(rewardList);
					});
				}
				else
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.DisplayLvPerform();
				}
			});
		}
		else
		{
			ManagedSingleton<UILinkHelper>.Instance.LoadUI(uIViewData.n_UILINK, LinkCB);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickMoveSE);
		}
	}

	private void LinkCB()
	{
		if (this != null)
		{
			ScrollCellIndex(listIndex);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.MISSION);
		}
	}
}
