using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailSelectUI : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private MailSelectCell mailSelectCell;

	[SerializeField]
	private Button btnTabMail;

	[SerializeField]
	private Button btnTabHistory;

	[SerializeField]
	private Button btnRetrieveAll;

	[SerializeField]
	private Text textMailCount;

	[SerializeField]
	private Text textDescription;

	[SerializeField]
	private Image imgEmptyMsgBg;

	[SerializeField]
	private OrangeText textEmptyMsg;

	private readonly Color colorBlue = new Color(37f / 51f, 78f / 85f, 1f);

	public MailHelper.MailType MailSelected { get; set; }

	public void Setup()
	{
		mailSelectCell.Parent = this;
		int num = OrangeConst.MAIL_REMOVECOUNT / 60 / 60 / 24;
		string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MAIL_PROMPT"), OrangeConst.MAIL_BOX_MAX, num);
		textDescription.text = text;
		OnMailSelected(MailHelper.MailType.NEW);
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveMailListReq(delegate
		{
			RedrawScrollViewCell();
		}, delegate
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("EVENT_IS_CALCULATING");
		});
	}

	public void RedrawScrollViewCell()
	{
		ManagedSingleton<MailHelper>.Instance.PrepareMailList(MailSelected);
		textMailCount.text = string.Format("{0}/{1}", ManagedSingleton<MailHelper>.Instance.PreparedMailCount, OrangeConst.MAIL_BOX_MAX);
		btnRetrieveAll.gameObject.SetActive(MailSelected == MailHelper.MailType.NEW);
		if (MailSelected == MailHelper.MailType.NEW)
		{
			btnRetrieveAll.gameObject.SetActive(true);
			btnRetrieveAll.interactable = ((ManagedSingleton<MailHelper>.Instance.PreparedMailCount > 0) ? true : false);
			UpdateEmptyMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EMPTY_MAIL"), ManagedSingleton<MailHelper>.Instance.PreparedMailCount);
		}
		else
		{
			btnRetrieveAll.gameObject.SetActive(false);
			UpdateEmptyMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EMPTY_GET_HISTORY"), ManagedSingleton<MailHelper>.Instance.PreparedMailCount);
		}
		scrollRect.ClearCells();
		scrollRect.OrangeInit(mailSelectCell, 5, ManagedSingleton<MailHelper>.Instance.PreparedMailCount);
	}

	public void OnClickRetrieveAllBtn()
	{
		bool epExcluded;
		List<int> list2 = ManagedSingleton<MailHelper>.Instance.CollectRetrieveAllMailIDList(out epExcluded);
		if (list2.Count <= 0)
		{
			if (epExcluded)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("EPMAX_MESSAGE", 1f);
			}
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		int originalAP = ManagedSingleton<PlayerHelper>.Instance.GetStamina();
		int originalEP = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveMailItemListReq(list2, delegate(List<NetRewardInfo> list)
		{
			RedrawScrollViewCell();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.MAIL);
			if (list.Count > 0)
			{
				int aPCount = Math.Max(ManagedSingleton<PlayerHelper>.Instance.GetStamina() - originalAP, 0);
				int ePCount = Math.Max(ManagedSingleton<PlayerHelper>.Instance.GetEventStamina() - originalEP, 0);
				MonoBehaviourSingleton<OrangeGameManager>.Instance.AddAPEPToRewardList(ref list, aPCount, ePCount);
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					ui.Setup(list, 0.3f);
				});
			}
		}, delegate
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("EVENT_IS_CALCULATING");
		});
	}

	public void OnMailSelected(MailHelper.MailType type)
	{
		MailSelected = type;
		if (type == MailHelper.MailType.NEW)
		{
			btnTabMail.interactable = false;
			btnTabHistory.interactable = true;
			btnTabMail.GetComponentInChildren<OrangeText>(true).color = new Color(0.20392157f, 0.18431373f, 0.22745098f);
			btnTabHistory.GetComponentInChildren<OrangeText>(true).color = colorBlue;
		}
		else
		{
			btnTabMail.interactable = true;
			btnTabHistory.interactable = false;
			btnTabMail.GetComponentInChildren<OrangeText>(true).color = colorBlue;
			btnTabHistory.GetComponentInChildren<OrangeText>(true).color = new Color(0.20392157f, 0.18431373f, 0.22745098f);
		}
	}

	public void OnClickMailListBtn()
	{
		OnMailSelected(MailHelper.MailType.NEW);
		RedrawScrollViewCell();
	}

	public void OnClickHistoryListBtn()
	{
		OnMailSelected(MailHelper.MailType.HISTORY);
		RedrawScrollViewCell();
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.CHANGE_DAY, DayChange);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_MAILBOX, UpdateMailBoxFromDetail);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.CHANGE_DAY, DayChange);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_MAILBOX, UpdateMailBoxFromDetail);
	}

	public void DayChange()
	{
		MonoBehaviourSingleton<UIManager>.Instance.BackToHometop();
	}

	public void UpdateMailBoxFromDetail()
	{
		RedrawScrollViewCell();
	}

	private void UpdateEmptyMsg(string msg, int listCount)
	{
		if (!(textEmptyMsg == null))
		{
			if (listCount > 0)
			{
				imgEmptyMsgBg.color = Color.clear;
				textEmptyMsg.text = string.Empty;
			}
			else
			{
				imgEmptyMsgBg.color = new Color(0.7f, 0.7f, 0.7f, 1f);
				textEmptyMsg.text = msg.ToString();
			}
		}
	}
}
