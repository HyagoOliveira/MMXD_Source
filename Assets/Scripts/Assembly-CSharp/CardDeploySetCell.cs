using UnityEngine;
using UnityEngine.UI;
using enums;

public class CardDeploySetCell : ScrollIndexCallback
{
	[SerializeField]
	private Button Btn;

	[SerializeField]
	private Text BtnText;

	[SerializeField]
	private Image ImgLock;

	[SerializeField]
	private Image ImgRed;

	private int pid = -1;

	private bool bLock = true;

	private CardDeployMain tCardDeployMain;

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	private void Update()
	{
		if (!(tCardDeployMain != null))
		{
			return;
		}
		int num = tCardDeployMain.OnGetCurrentDeployIndex();
		Btn.interactable = num != pid + 1;
		if (bLock)
		{
			bLock = pid >= OrangeConst.CARD_DEPLOY_OPEN + ManagedSingleton<PlayerHelper>.Instance.GetCardDeployExpansion();
			if (!bLock)
			{
				ImgLock.gameObject.SetActive(bLock);
				OnUpdateName();
			}
		}
	}

	public void OnUpdateName()
	{
		BtnText.text = "------";
		int key = pid + 1;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployNameInfo.ContainsKey(key))
		{
			string text = ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployNameInfo[key];
			if (text != null && text != "")
			{
				BtnText.text = ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployNameInfo[key];
			}
		}
	}

	public void OnClick()
	{
		if (pid < OrangeConst.CARD_DEPLOY_OPEN + ManagedSingleton<PlayerHelper>.Instance.GetCardDeployExpansion())
		{
			if (!(tCardDeployMain != null))
			{
				return;
			}
			if (tCardDeployMain.bCurrentDeploySlotDirty)
			{
				string msg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_DEPLOY_WARN2");
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
					ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_DEPLOY_WARN4"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
					{
						if (tCardDeployMain != null)
						{
							tCardDeployMain.OnDeployRevert();
							tCardDeployMain.OnClickDeployBtn(pid + 1, SystemSE.NONE);
						}
					});
				});
			}
			else
			{
				tCardDeployMain.OnClickDeployBtn(pid + 1);
			}
		}
		else
		{
			OnUnlockDeploySlot();
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		if (p_idx != -1)
		{
			if (tCardDeployMain == null)
			{
				tCardDeployMain = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardDeployMain>("UI_CardDeployMain");
			}
			pid = p_idx;
			bLock = p_idx >= OrangeConst.CARD_DEPLOY_OPEN + ManagedSingleton<PlayerHelper>.Instance.GetCardDeployExpansion();
			ImgLock.gameObject.SetActive(bLock);
			if (!bLock)
			{
				OnUpdateName();
			}
			else
			{
				BtnText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_DEPLOY_CLOSE");
			}
			bool active = false;
			if (tCardDeployMain != null)
			{
				active = tCardDeployMain.CurrentDeployIndex == pid + 1 && tCardDeployMain.bCurrentDeploySlotDirty;
			}
			ImgRed.gameObject.SetActive(active);
		}
	}

	public void OnUnlockDeploySlot()
	{
		int totalJewel = ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel();
		string arg = ManagedSingleton<OrangeTableHelper>.Instance.GetItemName(OrangeConst.ITEMID_FREE_JEWEL);
		string title = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_DEPLOY_BUY");
		string msg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_DEPLOY_WARN1");
		string desc = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CHARGE_ITEM_DESC"), arg, totalJewel);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChargeStamina", delegate(ChargeStaminaUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.CustomSetup(ChargeType.CardDeploySlot, OrangeConst.CARD_DEPLOY_COST, 1, OrangeConst.ITEMID_FREE_JEWEL, OrangeConst.CARD_DEPLOY_ITEM, title, msg, desc, delegate
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_STORE01;
			});
		});
	}
}
