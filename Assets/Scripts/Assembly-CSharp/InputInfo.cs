using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class InputInfo
{
	public byte TouchChainCount;

	public Vector3 ShootDir = Vector3.zero;

	public Vector3 UpdatePos = Vector3.zero;

	public bool bLockInput;

	public int nRecordNO;

	public OcSyncData tOcSyncData;

	public int[] _buttonStatus { get; set; }

	public float[] _pressTimer { get; set; }

	public VInt2[] _analogStickValue { get; set; }

	public InputInfo()
	{
		ResetInput();
	}

	public void ResetInput()
	{
		_buttonStatus = new int[19];
		_pressTimer = new float[19];
		for (int i = 0; i < 19; i++)
		{
			_buttonStatus[i] = 0;
			_pressTimer[i] = 0f;
		}
		_analogStickValue = new VInt2[6];
		for (int j = 0; j < 6; j++)
		{
			_analogStickValue[j] = VInt2.zero;
		}
	}

	public void MakeRuntimeDiff(InputInfo compareInput, out int inputRuntimeDiff, out int ocRuntimeDiff)
	{
		inputRuntimeDiff = 0;
		for (int i = 0; i < 19; i++)
		{
			if (_buttonStatus[i] != compareInput._buttonStatus[i])
			{
				inputRuntimeDiff |= 1;
			}
			if (_pressTimer[i] != compareInput._pressTimer[i])
			{
				inputRuntimeDiff |= 8;
			}
		}
		for (int j = 0; j < 6; j++)
		{
			if (_analogStickValue[j] != compareInput._analogStickValue[j])
			{
				inputRuntimeDiff |= 16;
			}
		}
		if (TouchChainCount != compareInput.TouchChainCount)
		{
			inputRuntimeDiff |= 64;
		}
		if (ShootDir != compareInput.ShootDir)
		{
			inputRuntimeDiff |= 128;
		}
		inputRuntimeDiff |= 256;
		if (bLockInput != compareInput.bLockInput)
		{
			inputRuntimeDiff |= 512;
		}
		if (nRecordNO != compareInput.nRecordNO)
		{
			inputRuntimeDiff |= 1024;
		}
		if (tOcSyncData == null)
		{
			tOcSyncData = new OcSyncData();
		}
		ocRuntimeDiff = tOcSyncData.MakeRuntimeDiff(compareInput.tOcSyncData);
		if (ocRuntimeDiff != 0)
		{
			inputRuntimeDiff |= 2048;
		}
	}

	public void RecordByRuntimeDiff(BinaryWriter bw, int inputRuntimeDiff, int ocRuntimeDiff)
	{
		bw.Write(inputRuntimeDiff);
		if ((inputRuntimeDiff & 1) > 0)
		{
			for (int i = 0; i < 19; i++)
			{
				bw.Write((byte)_buttonStatus[i]);
			}
		}
		if ((inputRuntimeDiff & 8) > 0)
		{
			bw.WriteExFloat(_pressTimer[6]);
		}
		if ((inputRuntimeDiff & 0x10) > 0)
		{
			for (int j = 0; j < 6; j++)
			{
				bw.Write(_analogStickValue[j].x);
				bw.Write(_analogStickValue[j].y);
			}
		}
		if ((inputRuntimeDiff & 0x40) > 0)
		{
			bw.Write(TouchChainCount);
		}
		if ((inputRuntimeDiff & 0x80) > 0)
		{
			bw.WriteExFloat(ShootDir.x);
			bw.WriteExFloat(ShootDir.y);
		}
		if ((inputRuntimeDiff & 0x100) > 0)
		{
			bw.WriteExFloat(UpdatePos.x);
			bw.WriteExFloat(UpdatePos.y);
		}
		if ((inputRuntimeDiff & 0x200) > 0)
		{
			bw.Write(bLockInput);
		}
		if ((inputRuntimeDiff & 0x400) > 0)
		{
			bw.Write(nRecordNO);
		}
		if ((inputRuntimeDiff & 0x800) > 0)
		{
			tOcSyncData.RecordByRuntimeDiff(bw, ocRuntimeDiff);
		}
	}

	public void CombineRuntimeDiff(BinaryReader br)
	{
		int num = br.ReadInt32();
		if ((num & 1) > 0)
		{
			for (int i = 0; i < 19; i++)
			{
				_buttonStatus[i] = br.ReadByte();
			}
		}
		if ((num & 8) > 0)
		{
			_pressTimer[6] = br.ReadExFloat();
		}
		if ((num & 0x10) > 0)
		{
			for (int j = 0; j < 6; j++)
			{
				_analogStickValue[j].x = br.ReadInt32();
				_analogStickValue[j].y = br.ReadInt32();
			}
		}
		if ((num & 0x40) > 0)
		{
			TouchChainCount = br.ReadByte();
		}
		if ((num & 0x80) > 0)
		{
			ShootDir.x = br.ReadExFloat();
			ShootDir.y = br.ReadExFloat();
		}
		if ((num & 0x100) > 0)
		{
			UpdatePos.x = br.ReadExFloat();
			UpdatePos.y = br.ReadExFloat();
		}
		if ((num & 0x200) > 0)
		{
			bLockInput = br.ReadBoolean();
		}
		if ((num & 0x400) > 0)
		{
			nRecordNO = br.ReadInt32();
		}
		if ((num & 0x800) > 0)
		{
			if (tOcSyncData == null)
			{
				tOcSyncData = new OcSyncData();
			}
			tOcSyncData.CombineRuntimeDiff(br);
		}
	}

	public void CopyTo(InputInfo targetInfo)
	{
		_buttonStatus.CopyTo(targetInfo._buttonStatus, 0);
		_pressTimer.CopyTo(targetInfo._pressTimer, 0);
		_analogStickValue.CopyTo(targetInfo._analogStickValue, 0);
		targetInfo.ShootDir = ShootDir;
		targetInfo.UpdatePos = UpdatePos;
		targetInfo.bLockInput = bLockInput;
		targetInfo.nRecordNO = nRecordNO;
		targetInfo.TouchChainCount = TouchChainCount;
		if (tOcSyncData == null)
		{
			targetInfo.tOcSyncData = new OcSyncData();
			return;
		}
		string value = JsonConvert.SerializeObject(tOcSyncData);
		targetInfo.tOcSyncData = JsonConvert.DeserializeObject<OcSyncData>(value);
	}
}
