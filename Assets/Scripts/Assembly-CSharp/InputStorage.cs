using System.Collections.Generic;
using System.Linq;
using Better;
using UnityEngine;

public class InputStorage : ManagedSingleton<InputStorage>
{
	private Better.Dictionary<string, InputInfo> dictInputData;

	public override void Initialize()
	{
		dictInputData = new Better.Dictionary<string, InputInfo>();
	}

	public override void Dispose()
	{
		dictInputData = null;
	}

	public Vector2 GetAnalogStatus(string playerID, int btnID)
	{
		return dictInputData[playerID]._analogStickValue[btnID].vec2;
	}

	public Vector2 GetAnalogStatus(string playerID, AnalogSticks btnID)
	{
		return GetAnalogStatus(playerID, (int)btnID);
	}

	public ButtonStatus GetButtonStatus(string playerID, ButtonId btnID)
	{
		return (ButtonStatus)dictInputData[playerID]._buttonStatus[(int)btnID];
	}

	public Vector3 GetPos(string playerID)
	{
		return dictInputData[playerID].UpdatePos;
	}

	public Vector3 GetDir(string playerID)
	{
		return dictInputData[playerID].ShootDir;
	}

	public int GetTouchChainLength(string playerID)
	{
		return dictInputData[playerID].TouchChainCount;
	}

	public bool GetLockInput(string playerID)
	{
		return dictInputData[playerID].bLockInput;
	}

	public float GetHeldButtonFrame(string playerID, ButtonId btnID)
	{
		return dictInputData[playerID]._pressTimer[(int)btnID];
	}

	public bool IsPressed(string playerID, ButtonId btnID)
	{
		switch ((ButtonStatus)dictInputData[playerID]._buttonStatus[(int)btnID])
		{
		case ButtonStatus.NONE:
		case ButtonStatus.HELD:
		case ButtonStatus.RELEASED:
			return false;
		case ButtonStatus.PRESSED:
			return true;
		default:
			return false;
		}
	}

	public bool IsAnyHeld(string playerID)
	{
		int[] buttonStatus = dictInputData[playerID]._buttonStatus;
		for (int i = 0; i < buttonStatus.Length; i++)
		{
			ButtonStatus buttonStatus2 = (ButtonStatus)buttonStatus[i];
			if ((uint)(buttonStatus2 - 1) <= 1u)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyPress(string playerID)
	{
		int[] buttonStatus = GetInputInfo(playerID)._buttonStatus;
		for (int i = 0; i < buttonStatus.Length; i++)
		{
			ButtonStatus buttonStatus2 = (ButtonStatus)buttonStatus[i];
			if ((uint)(buttonStatus2 - 1) <= 2u)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsHeld(string playerID, ButtonId btnID)
	{
		switch ((ButtonStatus)dictInputData[playerID]._buttonStatus[(int)btnID])
		{
		case ButtonStatus.NONE:
		case ButtonStatus.RELEASED:
			return false;
		case ButtonStatus.PRESSED:
		case ButtonStatus.HELD:
			return true;
		default:
			return false;
		}
	}

	public bool IsReleased(string playerID, ButtonId btnID)
	{
		switch ((ButtonStatus)dictInputData[playerID]._buttonStatus[(int)btnID])
		{
		case ButtonStatus.NONE:
		case ButtonStatus.PRESSED:
		case ButtonStatus.HELD:
			return false;
		case ButtonStatus.RELEASED:
			return true;
		default:
			return false;
		}
	}

	public bool IsAnyReleased(string playerID)
	{
		int[] buttonStatus = GetInputInfo(playerID)._buttonStatus;
		for (int i = 0; i < buttonStatus.Length; i++)
		{
			ButtonStatus buttonStatus2 = (ButtonStatus)buttonStatus[i];
			if (buttonStatus2 == ButtonStatus.RELEASED)
			{
				return true;
			}
		}
		return false;
	}

	public InputInfo GetInputInfo(string playerID)
	{
		if (dictInputData.ContainsKey(playerID))
		{
			return dictInputData[playerID];
		}
		dictInputData.Add(playerID, new InputInfo());
		return dictInputData[playerID];
	}

	public void AddInputData(string playerID)
	{
		if (dictInputData.ContainsKey(playerID))
		{
			dictInputData[playerID].ResetInput();
		}
		else
		{
			dictInputData.ContainsAdd(playerID, new InputInfo());
		}
	}

	public void SetInputData(string playerID, InputInfo inputInfo)
	{
		if (dictInputData.ContainsKey(playerID))
		{
			dictInputData[playerID] = inputInfo;
		}
	}

	public bool CheckInputDataNO(string playerID, int nRecord)
	{
		if (dictInputData.ContainsKey(playerID) && dictInputData[playerID].nRecordNO >= nRecord)
		{
			return false;
		}
		return true;
	}

	public int GetInputRecordNO(string playerID)
	{
		if (dictInputData.ContainsKey(playerID))
		{
			return dictInputData[playerID].nRecordNO;
		}
		return 0;
	}

	public void SetInputRecordNO(string playerID, int nRecord)
	{
		if (dictInputData.ContainsKey(playerID))
		{
			dictInputData[playerID].nRecordNO = nRecord;
		}
	}

	public void RemoveInputData(string playerID)
	{
		if ((MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == null || !(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == playerID)) && dictInputData.ContainsKey(playerID))
		{
			dictInputData.Remove(playerID);
		}
	}

	public void ClearInputData()
	{
		List<string> list = dictInputData.Keys.ToList();
		dictInputData.Clear();
		foreach (string item in list)
		{
			dictInputData[item] = new InputInfo();
		}
	}

	public void ResetPlayerInput(string playerID)
	{
		if (dictInputData.ContainsKey(playerID))
		{
			dictInputData[playerID].ResetInput();
		}
	}
}
