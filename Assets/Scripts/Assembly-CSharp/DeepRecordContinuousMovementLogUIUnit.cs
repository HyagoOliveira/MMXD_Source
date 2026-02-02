using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class DeepRecordContinuousMovementLogUIUnit : ScrollIndexCallback
{
	private static string RichTextGreen = "<color=#00FF00>{0}</color>";

	private static int MAX_REWARD_COUNT = 3;

	[SerializeField]
	private Text textDesc;

	[SerializeField]
	private Text textLogTime;

	[SerializeField]
	private Text textRound;

	[SerializeField]
	private Image imgHexIcon;

	[SerializeField]
	private Image imgWin;

	[SerializeField]
	private Image imgLose;

	[SerializeField]
	private Canvas[] itemCanvas = new Canvas[MAX_REWARD_COUNT];

	[SerializeField]
	private CommonIconBase[] itemIcons = new CommonIconBase[MAX_REWARD_COUNT];

	private RECORDGRID_TABLE recordGridTable;

	public int NowIdx { get; private set; }

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		NetRecordGridMultiMoveLogInfo netRecordGridMultiMoveLogInfo = ManagedSingleton<DeepRecordHelper>.Instance.ListMultiMoveLog[NowIdx];
		if (GetRecordGridTable(netRecordGridMultiMoveLogInfo.GridID, out recordGridTable))
		{
			switch ((RecordGridLatticeType)(short)recordGridTable.n_TYPE)
			{
			case RecordGridLatticeType.Battle:
			case RecordGridLatticeType.Explore:
			case RecordGridLatticeType.Hold:
				textDesc.text = string.Empty;
				textRound.text = string.Empty;
				UpdateHexIcon();
				UpdateWinLose(netRecordGridMultiMoveLogInfo.Result);
				UpdateLogTime(netRecordGridMultiMoveLogInfo.LogTime);
				break;
			case RecordGridLatticeType.Ability:
				imgWin.color = Color.clear;
				imgLose.color = Color.clear;
				textRound.text = string.Empty;
				UpdateReward(false);
				UpdateHexIcon();
				UpdateAbilityLogDesc();
				UpdateLogTime(netRecordGridMultiMoveLogInfo.LogTime);
				break;
			case RecordGridLatticeType.Random:
				imgWin.color = Color.clear;
				imgLose.color = Color.clear;
				UpdateReward(false);
				UpdateHexIcon();
				UpdateRandomLogDesc(netRecordGridMultiMoveLogInfo);
				UpdateLogTime(netRecordGridMultiMoveLogInfo.LogTime);
				UpdateRound(netRecordGridMultiMoveLogInfo.RandomEventID, netRecordGridMultiMoveLogInfo.Count);
				break;
			}
		}
	}

	private bool GetRecordGridTable(int p_id, out RECORDGRID_TABLE table)
	{
		return ManagedSingleton<OrangeDataManager>.Instance.RECORDGRID_TABLE_DICT.TryGetValue(p_id, out table);
	}

	private void UpdateHexIcon()
	{
		if (recordGridTable != null)
		{
			DeepRecordMainUI mainUI = ManagedSingleton<DeepRecordHelper>.Instance.MainUI;
			if ((bool)mainUI)
			{
				imgHexIcon.sprite = mainUI.GetHexIcon((RecordGridLatticeType)recordGridTable.n_TYPE);
			}
		}
	}

	private void UpdateRandomLogDesc(NetRecordGridMultiMoveLogInfo log)
	{
		if (recordGridTable != null)
		{
			textDesc.alignByGeometry = false;
			textDesc.text = DeepRecordHelper.GetRandomLatticeMsg(log.RandomEventID);
		}
	}

	private void UpdateAbilityLogDesc()
	{
		if (recordGridTable != null)
		{
			textDesc.alignByGeometry = false;
			textDesc.text = DeepRecordHelper.GetAbilityTip(recordGridTable);
		}
	}

	private void UpdateLogTime(int logTime)
	{
		textLogTime.text = DateTimeHelper.FromEpochLocalTime(logTime).ToFullDateString(LocalizationScriptableObject.Instance.m_Language);
	}

	private void UpdateRound(int eventId, int round)
	{
		RANDOMLATTICE_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.RANDOMLATTICE_TABLE_DICT.TryGetValue(eventId, out value) && (value.n_TYPE == 1 || value.n_TYPE == 5))
		{
			textRound.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_MESSAGE_8", string.Format(RichTextGreen, round));
		}
		else
		{
			textRound.text = string.Empty;
		}
	}

	private void UpdateReward(bool activeReward)
	{
		int i = 0;
		if (recordGridTable == null || !activeReward)
		{
			for (; i < MAX_REWARD_COUNT; i++)
			{
				itemCanvas[i].enabled = false;
			}
			return;
		}
		List<GACHA_TABLE> listGachaByGroup = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(recordGridTable.n_REWARD);
		int num = Mathf.Min(listGachaByGroup.Count, MAX_REWARD_COUNT);
		for (i = 0; i < num; i++)
		{
			GACHA_TABLE gACHA_TABLE = listGachaByGroup[i];
			itemCanvas[i].enabled = true;
			itemIcons[i].SetItemWithAmount(gACHA_TABLE.n_REWARD_ID, gACHA_TABLE.n_AMOUNT_MIN, OpenHow2GetUI);
		}
		for (; i < MAX_REWARD_COUNT; i++)
		{
			itemCanvas[i].enabled = false;
		}
	}

	private void UpdateWinLose(sbyte result)
	{
		switch ((RecordGridChallengeResult)result)
		{
		case RecordGridChallengeResult.Success:
			imgWin.color = Color.white;
			imgLose.color = Color.clear;
			UpdateReward(true);
			break;
		case RecordGridChallengeResult.Failure:
			imgWin.color = Color.clear;
			imgLose.color = Color.white;
			UpdateReward(false);
			break;
		}
	}

	private void OpenHow2GetUI(int itemId)
	{
		ITEM_TABLE itemTable;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(itemId, out itemTable))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.CanShowHow2Get = false;
				ui.Setup(itemTable);
			});
		}
	}
}
