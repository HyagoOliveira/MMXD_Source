#define RELEASE
using System.Collections.Generic;
using CriWare;
using UnityEngine;

[RequireComponent(typeof(OrangeCharacter))]
public class ChargeShootObj : MonoBehaviour, IManagedUpdateBehavior
{
	private const int MAX_SKILL_COUNT = 2;

	private const int MAX_SE_COUNT = 3;

	public bool[] _Charging = new bool[2];

	private string[] _chargeSE;

	private bool LV2SEPlaying;

	private bool LV3SEPlaying;

	public string[] _ChargeLV2SE = new string[2]
	{
		string.Empty,
		string.Empty
	};

	public string[] _ChargeLV3SE = new string[2] { "null", "null" };

	public string[] _ChargeMaxSE = new string[2] { "null", "null" };

	public ParticleSystem[] ChargeStartParticleSystem = new ParticleSystem[2];

	public ParticleSystem[] ChargeLv1ParticleSystem = new ParticleSystem[2];

	public ParticleSystem[] ChargeLv2ParticleSystem = new ParticleSystem[2];

	public ParticleSystem[] ChargeLv3ParticleSystem = new ParticleSystem[2];

	public int[][] nUpdateInAdvance = new int[2][]
	{
		new int[4],
		new int[4]
	};

	private OrangeCharacter _refEntity;

	[SerializeField]
	private List<OrangeCriPoint> chargePlayer = new List<OrangeCriPoint>();

	private bool refreashMask;

	public string ShootChargeVoiceSE = "";

	public bool Charging
	{
		get
		{
			return _Charging[0];
		}
		set
		{
			_Charging[0] = value;
		}
	}

	public string[] ChargeSE
	{
		get
		{
			return _chargeSE;
		}
		set
		{
			_chargeSE = value;
		}
	}

	public string ChargeLV2SE
	{
		get
		{
			return _ChargeLV2SE[0];
		}
		set
		{
			_ChargeLV2SE[0] = value;
		}
	}

	public string ChargeLV3SE
	{
		get
		{
			return _ChargeLV3SE[0];
		}
		set
		{
			_ChargeLV3SE[0] = value;
		}
	}

	public string ChargeMaxSE
	{
		get
		{
			return _ChargeMaxSE[0];
		}
		set
		{
			_ChargeMaxSE[0] = value;
		}
	}

	public void ResetPlayer()
	{
		chargePlayer.Clear();
		for (int i = 0; i < 4; i++)
		{
			OrangeCriPoint item = new OrangeCriPoint();
			chargePlayer.Add(item);
		}
	}

	public void PlayPlayerSE(int id, string acb, string cue)
	{
		CueInfo cueInfo = new CueInfo();
		cueInfo.Parse(acb, cue);
		CriAtomExAcb acb2 = MonoBehaviourSingleton<AudioManager>.Instance.GetAcb(acb, cue);
		if (acb2 != null)
		{
			OrangeCriPoint orangeCriPoint = chargePlayer[id];
			orangeCriPoint.playerpb.Stop();
			orangeCriPoint.CueInfo = cueInfo;
			if (cueInfo.eType == CueType.CT_LOOP)
			{
				orangeCriPoint.e_LoopType = OrangeCriPoint.LOOPTYPE.SE_LOOP;
			}
			else
			{
				orangeCriPoint.e_LoopType = OrangeCriPoint.LOOPTYPE.NONE;
			}
			orangeCriPoint.sourceObj = _refEntity.SoundSource;
			orangeCriPoint.Player.SetCue(acb2, cue);
			orangeCriPoint.Play(acb, cue);
		}
	}

	public void StartSE(int id, string acb, string cue)
	{
		PlayPlayerSE(id, acb, cue);
	}

	public void StopSE(int id, string acb, string cue)
	{
		PlayPlayerSE(id, acb, cue);
	}

	public void ClearSE()
	{
		foreach (OrangeCriPoint item in chargePlayer)
		{
			item.Reset();
		}
	}

	private void PauseSE(bool sw)
	{
		foreach (OrangeCriPoint item in chargePlayer)
		{
			if (item.playerpb.GetStatus() != CriAtomExPlayback.Status.Removed)
			{
				item.Player.Pause(sw);
			}
		}
	}

	public void Awake()
	{
		_refEntity = GetComponent<OrangeCharacter>();
		ResetPlayer();
	}

	private void OnDestroy()
	{
		ClearSE();
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
		PauseSE(false);
	}

	public void OnDisable()
	{
		PauseSE(true);
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	public void UpdateFunc()
	{
		if (!(_refEntity == null) && _refEntity.Activate)
		{
			UpdateChargeStatus();
		}
	}

	protected void UpdateChargeStatus()
	{
		if ((int)_refEntity.Hp <= 0 || (BattleInfoUI.Instance != null && BattleInfoUI.Instance.IsPlayClearBgm))
		{
			return;
		}
		long num = 0L;
		int idx = -1;
		float emissionIntensity = 0f;
		bool flag = false;
		int num2 = 0;
		for (int i = 0; i < _refEntity.PlayerSkills.Length; i++)
		{
			if (!_refEntity.PlayerSkills[i].ChargeTimer.IsStarted())
			{
				_refEntity.PlayerSkills[i].ChargeLevel = 0;
				if (_Charging[i])
				{
					PlayCharge(false, false, false, false, i);
				}
				continue;
			}
			num = _refEntity.PlayerSkills[i].ChargeTimer.GetMillisecond();
			sbyte b = 0;
			b = 0;
			while (b < _refEntity.PlayerSkills[i].ChargeTime.Length)
			{
				if (num >= _refEntity.PlayerSkills[i].ChargeTime[b])
				{
					_refEntity.PlayerSkills[i].ChargeLevel = b;
					num2 = b;
					b++;
					continue;
				}
				if ((float)num + GameLogicUpdateManager.m_fFrameLenMS * (float)nUpdateInAdvance[i][b] >= (float)_refEntity.PlayerSkills[i].ChargeTime[b])
				{
					num2 = b;
				}
				break;
			}
			if (_refEntity.PlayerSkills[i].ChargeLevel > 0 && _refEntity.PlayerSkills[i].ChargeTimer.GetTicks() % 6 <= 3)
			{
				idx = 0;
				emissionIntensity = 1.5f;
			}
			if (_refEntity.PlayerSkills[i].ChargeLevel == 0 && !ChargeStartParticleSystem[i].isPlaying)
			{
				ChargeStartParticleSystem[i].Play();
			}
			if (_refEntity.PlayerSkills[i].ChargeLevel == 1)
			{
				if (ChargeStartParticleSystem[i] != ChargeLv1ParticleSystem[i] && ChargeStartParticleSystem[i].isPlaying)
				{
					ChargeStartParticleSystem[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				}
				if (!ChargeLv1ParticleSystem[i].isPlaying)
				{
					ChargeLv1ParticleSystem[i].Play();
					if (!LV2SEPlaying && _ChargeLV2SE[i] != string.Empty)
					{
						_refEntity.PlaySE(ChargeSE[0], _ChargeLV2SE[i]);
						LV2SEPlaying = true;
					}
				}
			}
			if (num2 == 1 && !ChargeLv1ParticleSystem[i].isPlaying)
			{
				ChargeLv1ParticleSystem[i].Play();
			}
			if (_refEntity.PlayerSkills[i].ChargeLevel == 2)
			{
				if (ChargeLv1ParticleSystem[i].isPlaying)
				{
					ChargeLv1ParticleSystem[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				}
				if (!ChargeLv2ParticleSystem[i].isPlaying)
				{
					ChargeLv2ParticleSystem[i].Play();
					if (!LV3SEPlaying && _ChargeLV3SE[i] != "null")
					{
						_refEntity.PlaySE(ChargeSE[0], _ChargeLV3SE[i]);
						LV3SEPlaying = true;
					}
				}
			}
			if (num2 == 2 && !ChargeLv2ParticleSystem[i].isPlaying)
			{
				ChargeLv2ParticleSystem[i].Play();
				if (!LV3SEPlaying && _ChargeLV3SE[i] != "null")
				{
					_refEntity.PlaySE(ChargeSE[0], _ChargeLV3SE[i]);
					LV3SEPlaying = true;
				}
			}
			if (_refEntity.PlayerSkills[i].ChargeLevel == 3)
			{
				if (ChargeLv2ParticleSystem[i].isPlaying)
				{
					ChargeLv2ParticleSystem[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				}
				if (!ChargeLv3ParticleSystem[i].isPlaying)
				{
					ChargeLv3ParticleSystem[i].Play();
					if (_ChargeMaxSE[i] != "null")
					{
						_refEntity.PlaySE(_refEntity.SkillSEID, _ChargeMaxSE[i]);
					}
				}
			}
			if (num2 == 3 && !ChargeLv3ParticleSystem[i].isPlaying)
			{
				ChargeLv3ParticleSystem[i].Play();
				if (_ChargeMaxSE[i] != "null")
				{
					_refEntity.PlaySE(_refEntity.SkillSEID, _ChargeMaxSE[i]);
				}
			}
			if (!_Charging[i])
			{
				if (ChargeSE != null && ChargeSE.Length > i * 3 + 1)
				{
					PlayPlayerSE(i, ChargeSE[i * 3], ChargeSE[i * 3 + 1]);
					flag = true;
				}
				_Charging[i] = true;
			}
			if (_Charging[i])
			{
				chargePlayer[i].Update();
			}
		}
		if (flag && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_IN)
		{
			refreashMask = true;
			_refEntity.CharacterMaterials.UpdateMask(idx, emissionIntensity);
		}
		else if (refreashMask)
		{
			refreashMask = false;
			_refEntity.CharacterMaterials.RecoverOriginalMaskSetting();
		}
	}

	public void PlayCharge(bool bLv1, bool bLv2, bool bLv3 = false, bool bStart = false, int nSkillID = 0)
	{
		int num = 0;
		if (ChargeStartParticleSystem[nSkillID].isPlaying)
		{
			num++;
		}
		if (ChargeLv1ParticleSystem[nSkillID].isPlaying && ChargeLv1ParticleSystem[nSkillID] != ChargeStartParticleSystem[nSkillID])
		{
			num++;
		}
		if (ChargeLv2ParticleSystem[nSkillID].isPlaying)
		{
			num++;
		}
		if (ChargeLv3ParticleSystem[nSkillID].isPlaying)
		{
			num++;
		}
		bool num2 = num > 0;
		if (bStart)
		{
			if (!ChargeStartParticleSystem[nSkillID].isPlaying)
			{
				ChargeStartParticleSystem[nSkillID].Play();
				num++;
			}
		}
		else if (ChargeStartParticleSystem[nSkillID].isPlaying)
		{
			ChargeStartParticleSystem[nSkillID].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			num--;
		}
		if (bLv1)
		{
			if (!ChargeLv1ParticleSystem[nSkillID].isPlaying)
			{
				ChargeLv1ParticleSystem[nSkillID].Play();
				num++;
			}
		}
		else if (ChargeLv1ParticleSystem[nSkillID].isPlaying)
		{
			ChargeLv1ParticleSystem[nSkillID].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			num--;
		}
		if (bLv2)
		{
			if (!ChargeLv2ParticleSystem[nSkillID].isPlaying)
			{
				ChargeLv2ParticleSystem[nSkillID].Play();
				num++;
			}
		}
		else if (ChargeLv2ParticleSystem[nSkillID].isPlaying)
		{
			ChargeLv2ParticleSystem[nSkillID].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			num--;
		}
		if (bLv3)
		{
			if (ChargeLv3ParticleSystem[nSkillID].isPlaying)
			{
				ChargeLv3ParticleSystem[nSkillID].Play();
				num++;
			}
		}
		else if (ChargeLv3ParticleSystem[nSkillID].isPlaying)
		{
			ChargeLv3ParticleSystem[nSkillID].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			num--;
		}
		if (num2 && num == 0)
		{
			_Charging[nSkillID] = false;
			if (_refEntity != null && _refEntity.PlayerSkills[nSkillID].ChargeTimer.IsStarted())
			{
				_refEntity.PlayerSkills[nSkillID].ChargeTimer.TimerStop();
			}
			if (ChargeSE != null)
			{
				if (ChargeSE.Length > 3)
				{
					StopSE(nSkillID, ChargeSE[nSkillID * 3], ChargeSE[nSkillID * 3 + 2]);
				}
				else
				{
					StopSE(nSkillID, ChargeSE[0], ChargeSE[2]);
				}
			}
		}
		else if (num > 0)
		{
			_Charging[nSkillID] = true;
			if (_refEntity != null && !_refEntity.PlayerSkills[nSkillID].ChargeTimer.IsStarted())
			{
				_refEntity.PlayerSkills[nSkillID].ChargeTimer.TimerStart();
			}
		}
	}

	public void StartCharge(int nSkillID = 0)
	{
	}

	public void StopCharge(int id = 0)
	{
		for (int i = 0; i < _refEntity.PlayerSkills.Length; i++)
		{
			if (id == -1 || i == id)
			{
				if (_refEntity.PlayerSkills[i].ChargeTimer.IsStarted())
				{
					_refEntity.PlayerSkills[i].ChargeTimer.TimerStop();
				}
				ChargeStartParticleSystem[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				ChargeLv1ParticleSystem[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				ChargeLv2ParticleSystem[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				ChargeLv3ParticleSystem[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				if (_Charging[i] && ChargeSE != null && ChargeSE.Length > i * 3 + 1)
				{
					Debug.Log("Stop Charge[ id:" + id + "=" + i + "]");
					StopSE(i, ChargeSE[i * 3], ChargeSE[i * 3 + 2]);
					_Charging[i] = false;
				}
			}
		}
	}

	public void ResetChargeSEFlg()
	{
		LV2SEPlaying = false;
		LV3SEPlaying = false;
	}

	public void ShootChargeBuster(int id, bool isNonStopType = false, bool changeWeaponMesh = true)
	{
		StopCharge(id);
		if (_refEntity.PlayerSkills[id].ChargeLevel > 0)
		{
			LV2SEPlaying = false;
			LV3SEPlaying = false;
			if (ShootChargeVoiceSE == "")
			{
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
			}
			else
			{
				_refEntity.PlaySE(_refEntity.VoiceID, ShootChargeVoiceSE);
			}
			_refEntity.CurrentActiveSkill = id;
			_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, _refEntity.PlayerSkills[id].ChargeLevel, isNonStopType ? new Vector3?(_refEntity.ChargeShootDirectionXType) : null, changeWeaponMesh);
			_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0]);
		}
	}

	public void ChangeLayer(int layer, bool includeSelf)
	{
		for (int i = 0; i < 2; i++)
		{
			OrangeBattleUtility.ChangeLayersRecursively(ChargeStartParticleSystem[i].transform, layer, includeSelf);
			OrangeBattleUtility.ChangeLayersRecursively(ChargeLv1ParticleSystem[i].transform, layer, includeSelf);
			OrangeBattleUtility.ChangeLayersRecursively(ChargeLv2ParticleSystem[i].transform, layer, includeSelf);
			OrangeBattleUtility.ChangeLayersRecursively(ChargeLv3ParticleSystem[i].transform, layer, includeSelf);
		}
	}
}
