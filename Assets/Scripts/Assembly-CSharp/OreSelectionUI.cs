#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OreSelectionUI : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private OreSelectionCell _scrollCell;

	[SerializeField]
	private PanelValueBeforeAfterHelper _valueBeforeAfterHelper;

	[SerializeField]
	private Text _textTimeInfo;

	[SerializeField]
	private Button _buttonConfirm;

	private int _pillarId;

	public int SelectedIndex { get; private set; }

	public List<int> OreIDOpening { get; private set; }

	public List<OreInfoData> OreInfoDataList { get; private set; }

	public event Action OnItemSelected;

	private void OnEnable()
	{
		Singleton<PowerTowerSystem>.Instance.OnOpenPowerPillarEvent += OnOpenPowerPillarEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerPillarChangedEvent += OnSocketPowerPillarChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketOreChangedEvent += OnSocketOreChangedEvent;
	}

	private void OnDisable()
	{
		Singleton<PowerTowerSystem>.Instance.OnOpenPowerPillarEvent -= OnOpenPowerPillarEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerPillarChangedEvent -= OnSocketPowerPillarChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketOreChangedEvent -= OnSocketOreChangedEvent;
	}

	public void Setup(int pillarId)
	{
		Debug.Log("[Setup]");
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_pillarId = pillarId;
		_buttonConfirm.interactable = false;
		_textTimeInfo.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWERPILLAR_BEINGTIME", OrangeGameUtility.GetTimeText(OrangeConst.GUILD_POWERPILLAR_MAXTIME * 60, true));
		OreIDOpening = (from pillarInfo in Singleton<PowerTowerSystem>.Instance.PowerPillarInfoDataListCache
			where pillarInfo.OreInfo != null
			select pillarInfo.OreInfo.ID).ToList();
		OreInfoDataList = Singleton<PowerTowerSystem>.Instance.OreInfoDataListCache.ToList();
		SelectedIndex = OreInfoDataList.FindIndex((OreInfoData oreInfoData) => !OreIDOpening.Contains(oreInfoData.ID));
		_scrollRect.OrangeInit(_scrollCell, 5, OreInfoDataList.Count);
		if (SelectedIndex >= 0 && SelectedIndex < OreInfoDataList.Count)
		{
			OnClickCellSelectBtn(SelectedIndex);
		}
		else
		{
			Debug.LogError("[OreSelectionUI.Setup] SelectedIndex out of range");
		}
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
	}

	public void OnClickConfirmBtn()
	{
		OreInfoData oreInfoData = OreInfoDataList[SelectedIndex];
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK10);
		base.CloseSE = SystemSE.NONE;
		Singleton<PowerTowerSystem>.Instance.ReqOpenPowerPillar(_pillarId, oreInfoData.Group, oreInfoData.Level);
	}

	public void OnClickCellSelectBtn(int index)
	{
		SelectedIndex = index;
		_buttonConfirm.interactable = true;
		Action onItemSelected = this.OnItemSelected;
		if (onItemSelected != null)
		{
			onItemSelected();
		}
		OreInfoData oreInfoData = OreInfoDataList[SelectedIndex];
		if (oreInfoData.AttrData != null)
		{
			int money = Singleton<GuildSystem>.Instance.GuildInfoCache.Money;
			int num = money - oreInfoData.AttrData.n_ORE_MONEY;
			bool interactable = num >= 0;
			_valueBeforeAfterHelper.Setup(money, num);
			_buttonConfirm.interactable = interactable;
		}
		else
		{
			_valueBeforeAfterHelper.Setup("---", "---");
			_buttonConfirm.interactable = false;
		}
	}

	private void OnOpenPowerPillarEvent(Code ackCode)
	{
		switch (ackCode)
		{
		case Code.GUILD_ORE_OPEN_SUCCESS:
			OnClickCloseBtn();
			break;
		case Code.GUILD_PILLAR_USED_ERROR:
		case Code.GUILD_ORE_OPENING_ERROR:
			CommonUIHelper.ShowCommonTipUI("GUILD_ORE_TURNON");
			OnClickCloseBtn();
			break;
		case Code.GUILD_PILLAR_OPEN_ORE_LEVEL_ERROR:
			CommonUIHelper.ShowCommonTipUI("GUILD_ORE_LISTRENEW");
			OnClickCloseBtn();
			break;
		default:
			OnClickCloseBtn();
			break;
		}
	}

	private void OnSocketPowerPillarChangedEvent()
	{
	}

	private void OnSocketOreChangedEvent()
	{
	}
}
