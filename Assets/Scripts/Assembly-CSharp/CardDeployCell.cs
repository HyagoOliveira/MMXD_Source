using UnityEngine;
using UnityEngine.UI;
using enums;

public class CardDeployCell : ScrollIndexCallback
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

	private CharacterInfoCard tCharacterInfoCard;

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	private void Update()
	{
		if (!(tCharacterInfoCard != null))
		{
			return;
		}
		int num = tCharacterInfoCard.OnGetCurrentDeployIndex();
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
			if (!(tCharacterInfoCard != null))
			{
				return;
			}
			if (tCharacterInfoCard.bCurrentDeploySlotDirty)
			{
				string msg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_DEPLOY_WARN2");
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
					ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_DEPLOY_WARN4"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
					{
						if (tCharacterInfoCard != null)
						{
							tCharacterInfoCard.OnDeployRevert();
							tCharacterInfoCard.OnClickDeployBtn(pid + 1);
						}
					});
				});
			}
			else
			{
				tCharacterInfoCard.OnClickDeployBtn(pid + 1);
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
			if (tCharacterInfoCard == null)
			{
				tCharacterInfoCard = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoCard>("UI_CharacterInfo_Card");
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
			if (tCharacterInfoCard != null)
			{
				active = tCharacterInfoCard.CurrentDeployIndex == pid + 1 && tCharacterInfoCard.bCurrentDeploySlotDirty;
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
