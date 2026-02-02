using System;
using UnityEngine;
using enums;

public class HexCell : MonoBehaviour
{
	[Flags]
	public enum CellStatus
	{
		None = 0,
		Available = 1,
		Finished = 2,
		Select = 4,
		CurrentPoint = 8,
		Moveable = 0x10,
		TempMove = 0x20
	}

	private HexCoordinates coordinates;

	private CellStatus status = CellStatus.Available;

	private NetRecordGridCoordinateInfo coordinateInfo;

	private RECORDGRID_TABLE recordGridTable;

	private NetRecordGridMapEventInfo eventInfo;

	[SerializeField]
	private HexUICell uiCell;

	public bool IsAvailable
	{
		get
		{
			return (Status & CellStatus.Available) == CellStatus.Available;
		}
	}

	public bool IsFinished
	{
		get
		{
			return (Status & CellStatus.Finished) == CellStatus.Finished;
		}
	}

	public bool IsSelected
	{
		get
		{
			return (Status & CellStatus.Select) == CellStatus.Select;
		}
	}

	public bool IsMoveable
	{
		get
		{
			return (Status & CellStatus.Moveable) == CellStatus.Moveable;
		}
	}

	public bool IsTempMove
	{
		get
		{
			return (Status & CellStatus.TempMove) == CellStatus.TempMove;
		}
	}

	public HexUICell UI
	{
		get
		{
			return uiCell;
		}
	}

	public HexCoordinates Coordinates
	{
		get
		{
			return coordinates;
		}
	}

	public NetRecordGridCoordinateInfo CoordinateInfo
	{
		get
		{
			return coordinateInfo;
		}
	}

	public CellStatus Status
	{
		get
		{
			return status;
		}
	}

	public void Setup(HexCoordinates p_coordinates, NetRecordGridCoordinateInfo p_coordinateInfo, Transform uiCanvans)
	{
		coordinates = p_coordinates;
		coordinateInfo = p_coordinateInfo;
		uiCell.rectTransform.SetParent(uiCanvans, false);
		uiCell.rectTransform.anchoredPosition = new Vector2(base.transform.position.x, base.transform.position.z);
		if (coordinateInfo == null)
		{
			return;
		}
		RefreashFinishedFlg();
		if (!ManagedSingleton<OrangeDataManager>.Instance.RECORDGRID_TABLE_DICT.TryGetValue(coordinateInfo.ID, out recordGridTable))
		{
			return;
		}
		uiCell.ImgBg.sprite = ManagedSingleton<DeepRecordHelper>.Instance.MainUI.GetHexIcon((RecordGridLatticeType)recordGridTable.n_TYPE);
		if (!IsFinished)
		{
			GACHA_TABLE gachaTable;
			if (ManagedSingleton<ExtendDataHelper>.Instance.GetFirstGachaTableByGroup(recordGridTable.n_REWARD, out gachaTable))
			{
				uiCell.SetReward(gachaTable);
			}
			else
			{
				uiCell.SetReward(null);
			}
		}
	}

	public void RefreashFinishedFlg()
	{
		if (coordinateInfo != null)
		{
			SetFlag(CellStatus.Finished, ManagedSingleton<DeepRecordHelper>.Instance.IsPointFinished(coordinateInfo.X, coordinateInfo.Y));
		}
	}

	public RecordGridLatticeType GetRecordGridLatticeType()
	{
		if (recordGridTable != null)
		{
			return (RecordGridLatticeType)recordGridTable.n_TYPE;
		}
		return RecordGridLatticeType.Battle;
	}

	public string GetCellName()
	{
		if (recordGridTable != null)
		{
			return DeepRecordHelper.GetCellNameByLatticeType((RecordGridLatticeType)recordGridTable.n_TYPE);
		}
		return string.Empty;
	}

	public string GetCellTip()
	{
		if (recordGridTable != null)
		{
			switch ((RecordGridLatticeType)(short)recordGridTable.n_TYPE)
			{
			case RecordGridLatticeType.Battle:
			case RecordGridLatticeType.Explore:
			{
				int teamVal2 = DeepRecordHelper.GetTeamVal(recordGridTable);
				string text2 = ((recordGridTable.n_VALUE_Y > teamVal2) ? string.Format(DeepRecordHelper.RichTextRed, teamVal2) : string.Format(DeepRecordHelper.RichTextGreen, teamVal2));
				return string.Format("{0}\n{1}\n{2}", DeepRecordHelper.SuccessRate(recordGridTable.n_VALUE_Y, teamVal2, ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.ExtraSuccessRate), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_SCREEN_SUGGEST", recordGridTable.n_VALUE_Y, text2), ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(recordGridTable.s_TITLE));
			}
			case RecordGridLatticeType.Hold:
			{
				int teamVal = DeepRecordHelper.GetTeamVal(recordGridTable);
				int num = recordGridTable.n_VALUE_Y;
				if (eventInfo != null && !IsFinished)
				{
					num = eventInfo.Value;
				}
				string text = ((num > teamVal) ? string.Format(DeepRecordHelper.RichTextRed, teamVal) : string.Format(DeepRecordHelper.RichTextGreen, teamVal));
				return string.Format("{0}\n{1}\n{2}", DeepRecordHelper.SuccessRate(num, teamVal, ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.ExtraSuccessRate), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_SCREEN_SUGGEST", num, text), ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(recordGridTable.s_TITLE));
			}
			case RecordGridLatticeType.Ability:
				return ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(recordGridTable.s_TITLE);
			case RecordGridLatticeType.Random:
				return ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(recordGridTable.s_TITLE);
			}
		}
		return string.Empty;
	}

	public bool CostEnough()
	{
		return GetCost() <= ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.ActionPoint;
	}

	public bool CostEnough(int ap)
	{
		return GetCost() <= ap;
	}

	public int GetCost()
	{
		if (IsFinished)
		{
			return 0;
		}
		if (recordGridTable != null)
		{
			return recordGridTable.n_ACTION;
		}
		return 0;
	}

	public string GetCostStr()
	{
		int cost = GetCost();
		string text = (CostEnough() ? string.Format(DeepRecordHelper.RichTextGreen, cost) : string.Format(DeepRecordHelper.RichTextRed, cost));
		return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_SCREEN_ACTIONNEED", text);
	}

	public Sprite GetIcon()
	{
		return uiCell.ImgBg.sprite;
	}

	public string GetTableTip()
	{
		if (recordGridTable != null)
		{
			return DeepRecordHelper.GetAbilityTip(recordGridTable);
		}
		return string.Empty;
	}

	public int GetTableValueX()
	{
		if (recordGridTable != null)
		{
			return recordGridTable.n_VALUE_X;
		}
		return 0;
	}

	public int GetTableValueY()
	{
		if (recordGridTable != null)
		{
			return recordGridTable.n_VALUE_Y;
		}
		return 0;
	}

	public void SetFlag(CellStatus flag, bool isAdd)
	{
		if (flag == CellStatus.None)
		{
			status = CellStatus.None;
		}
		else if (isAdd)
		{
			status |= flag;
		}
		else
		{
			status &= ~flag;
		}
		uiCell.UpdateFlag(status);
	}

	public void SetTempMoveFlag(bool isAdd)
	{
		if (isAdd)
		{
			status |= CellStatus.TempMove;
		}
		else
		{
			status &= ~CellStatus.TempMove;
		}
		uiCell.UpdateTempMove(status);
	}

	public bool AddEventInfo(NetRecordGridMapEventInfo p_eventInfo)
	{
		if (eventInfo != null && eventInfo == p_eventInfo)
		{
			return false;
		}
		eventInfo = p_eventInfo;
		return true;
	}
}
