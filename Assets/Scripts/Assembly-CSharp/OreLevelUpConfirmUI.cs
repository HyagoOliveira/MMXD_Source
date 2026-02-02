using System;
using UnityEngine;
using UnityEngine.UI;

public class OreLevelUpConfirmUI : OrangeUIBase
{
	[SerializeField]
	private CommonIconBase _iconBefore;

	[SerializeField]
	private CommonIconBase _iconAfter;

	[SerializeField]
	private Text _textDesc;

	[SerializeField]
	private PanelValueInfoHelper _valueInfoHelper;

	[SerializeField]
	private PanelValueBeforeAfterHelper _valueBeforeAfterHelper;

	[SerializeField]
	private Button _buttonConfirm;

	public event Action OnConfirmEvent;

	private void OnEnable()
	{
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerTowerRankupEvent += OnSocketPowerTowerRankupEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketOreChangedEvent += OnSocketOreChangedEvent;
	}

	private void OnDisable()
	{
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerTowerRankupEvent -= OnSocketPowerTowerRankupEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketOreChangedEvent -= OnSocketOreChangedEvent;
	}

	public void Setup(int itemID, string desc, int cost)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_iconBefore.SetupItem(OrangeConst.ITEMID_MONEY);
		_iconAfter.SetupItem(itemID);
		_textDesc.text = desc.Replace("\\n", "\n");
		NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
		_valueInfoHelper.Setup(cost);
		_valueBeforeAfterHelper.Setup(guildInfoCache.Money, guildInfoCache.Money - cost);
		_buttonConfirm.interactable = guildInfoCache.Money >= cost;
	}

	public void OnClickConfirmBtn()
	{
		Action onConfirmEvent = this.OnConfirmEvent;
		if (onConfirmEvent != null)
		{
			onConfirmEvent();
		}
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_STORE02;
		OnClickCloseBtn();
	}

	private void OnSocketPowerTowerRankupEvent()
	{
	}

	private void OnSocketOreChangedEvent()
	{
		OnClickCloseBtn();
	}
}
