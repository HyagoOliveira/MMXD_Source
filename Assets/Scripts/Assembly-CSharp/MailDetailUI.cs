using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class MailDetailUI : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private Text textMailTitle;

	[SerializeField]
	private Text textMailContext;

	[SerializeField]
	private Text textMailDeliveryTime;

	[SerializeField]
	private Text textMailReservedTime;

	[SerializeField]
	private Text textMailRemainTime;

	[SerializeField]
	private Button btnRetrieve;

	private MailInfo info;

	private const int MAX_MAIL_REWARD_COUNT = 3;

	[SerializeField]
	private GameObject[] transRewardItemPos = new GameObject[3];

	public bool isNoItemMail = true;

	private bool isNotify;

	public void Setup(int mailID, bool notify)
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicMail.TryGetValue(mailID, out info) && !ManagedSingleton<PlayerNetManager>.Instance.dicReservedMail.TryGetValue(mailID, out info))
		{
			base.OnClickCloseBtn();
			return;
		}
		isNotify = notify;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			if (asset != null)
			{
				SetRewardIcon(info.netMailInfo.AttachmentType1, info.netMailInfo.AttachmentID1, info.netMailInfo.AttachmentAmount1, 0, asset);
				SetRewardIcon(info.netMailInfo.AttachmentType2, info.netMailInfo.AttachmentID2, info.netMailInfo.AttachmentAmount2, 1, asset);
				SetRewardIcon(info.netMailInfo.AttachmentType3, info.netMailInfo.AttachmentID3, info.netMailInfo.AttachmentAmount3, 2, asset);
			}
		});
		string p_key = string.Format("TITLE_{0}", info.netMailInfo.TitleID);
		string p_key2 = string.Format("MESSAGE_{0}", info.netMailInfo.ContextID);
		string text = OrangeGameUtility.GetOperationText(info.netMailInfo.TitleID);
		if (string.IsNullOrEmpty(text))
		{
			text = ManagedSingleton<OrangeTextDataManager>.Instance.MAILTEXT_TABLE_DICT.GetL10nValue(p_key);
		}
		textMailTitle.text = text;
		string text2 = OrangeGameUtility.GetOperationText(info.netMailInfo.ContextID);
		if (string.IsNullOrEmpty(text2))
		{
			text2 = ManagedSingleton<OrangeTextDataManager>.Instance.MAILTEXT_TABLE_DICT.GetL10nValue(p_key2);
		}
		textMailContext.alignByGeometry = false;
		textMailContext.text = text2;
		textMailDeliveryTime.text = CapUtility.UnixTimeToDate(info.netMailInfo.DeliveryTime).ToLocalTime().ToString("yyyy/MM/dd hh:mm:ss tt");
		textMailRemainTime.text = OrangeGameUtility.GetRemainTimeText(info.netMailInfo.RecycleTime);
		if (info.netMailInfo.ReservedTime == 0)
		{
			textMailReservedTime.text = "";
			btnRetrieve.gameObject.SetActive(true);
		}
		else
		{
			textMailReservedTime.text = CapUtility.UnixTimeToDate(info.netMailInfo.ReservedTime).ToLocalTime().ToString("yyyy/MM/dd hh:mm:ss tt");
			btnRetrieve.gameObject.SetActive(false);
		}
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void SetRewardIcon(sbyte rewardType, int rewardID, int rewardAmount, int slot, GameObject asset)
	{
		if (rewardType == 0 || rewardID == 0 || rewardAmount == 0)
		{
			return;
		}
		GameObject gameObject = transRewardItemPos[slot];
		UnityEngine.Object.Instantiate(asset, gameObject.transform, false);
		CommonIconBase componentInChildren = gameObject.GetComponentInChildren<CommonIconBase>();
		componentInChildren.gameObject.SetActive(true);
		ITEM_TABLE item = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(rewardID, out item))
		{
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				componentInChildren.SetItemWithAmountForCard(rewardID, rewardAmount, ShowItemDetail);
			}
			else
			{
				componentInChildren.SetItemWithAmount(rewardID, rewardAmount, ShowItemDetail);
			}
		}
		isNoItemMail = false;
	}

	private void ShowItemDetail(int itemId)
	{
		ITEM_TABLE itemTable = null;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.GetItem(itemId, out itemTable))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			ui.CanShowHow2Get = false;
			if (itemTable.n_TYPE == 5 && itemTable.n_TYPE_X == 1 && (int)itemTable.f_VALUE_Y > 0)
			{
				CARD_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)itemTable.f_VALUE_Y, out value))
				{
					ui.Setup(value, itemTable);
				}
			}
			else
			{
				ui.Setup(itemTable);
			}
		});
	}

	public void OnClickRetrieveBtn()
	{
		if (info.netMailInfo.RecycleTime < MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("MAIL_EXPIRED");
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_MAILBOX);
			base.OnClickCloseBtn();
			return;
		}
		int amount;
		if (ManagedSingleton<MailHelper>.Instance.ContainsSpecificItem(info.netMailInfo, RewardType.Item, OrangeConst.ITEMID_EVENTAP, out amount) && ManagedSingleton<PlayerHelper>.Instance.GetEventStamina() + amount > OrangeConst.EP_MAX)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("EPMAX_MESSAGE", 1f);
			return;
		}
		int originalAP = ManagedSingleton<PlayerHelper>.Instance.GetStamina();
		int originalEP = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
		List<int> list2 = new List<int>();
		list2.Add(info.netMailInfo.MailID);
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveMailItemListReq(list2, delegate(List<NetRewardInfo> list)
		{
			if (isNotify)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_MAILBOX);
			}
			int aPCount = Math.Max(ManagedSingleton<PlayerHelper>.Instance.GetStamina() - originalAP, 0);
			int ePCount = Math.Max(ManagedSingleton<PlayerHelper>.Instance.GetEventStamina() - originalEP, 0);
			MonoBehaviourSingleton<OrangeGameManager>.Instance.AddAPEPToRewardList(ref list, aPCount, ePCount);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			base.CloseSE = SystemSE.NONE;
			_003C_003En__0();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.MAIL);
			if (list.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					ui.Setup(list);
				});
			}
		}, delegate
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("EVENT_IS_CALCULATING");
		});
	}

	public new void OnClickCloseBtn()
	{
		if (!isNoItemMail || info.netMailInfo.ReservedTime != 0)
		{
			base.OnClickCloseBtn();
			return;
		}
		List<int> list = new List<int>();
		list.Add(info.netMailInfo.MailID);
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveMailItemListReq(list, delegate
		{
			if (isNotify)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_MAILBOX);
			}
			base.OnClickCloseBtn();
		}, delegate
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("EVENT_IS_CALCULATING");
			base.OnClickCloseBtn();
		});
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.OnClickCloseBtn();
	}
}
