#define RELEASE
using System.Collections.Generic;
using System.Linq;
using CriWare;
using UnityEngine;

public class MultipleCharge
{
	public enum ChargeType
	{
		SKILL0 = 0,
		SKILL1 = 1,
		WEAPON0 = 2,
		WEAPON1 = 3
	}

	public enum ChargeLevel
	{
		START = 0,
		LV1 = 1,
		LV2 = 2,
		LV3 = 3,
		MAX = 4
	}

	public class ChargeData
	{
		public List<ParticleSystem> _listChargeFx = new List<ParticleSystem>();

		public string[] ChargeLVSE;

		public string[] ChargeLV3SE;

		public string[] ChargeMaxSE;

		public string[] SECharging;

		public CueInfo cueinfo;

		public OrangeCriPoint ocp;

		public ChargeData()
		{
			for (int i = 0; i < 4; i++)
			{
				_listChargeFx.Add(null);
			}
		}

		public void PlaySE(int lv, OrangeCharacter oc)
		{
			ocp = MonoBehaviourSingleton<AudioManager>.Instance.GetAPoint();
			if (ocp == null)
			{
				return;
			}
			switch (lv)
			{
			case 1:
				if (ChargeLVSE != null)
				{
					SECharging = ChargeLVSE;
					PlayCharge(SECharging[0], SECharging[1], oc.SoundSource);
				}
				break;
			case 2:
				if (ChargeLV3SE != null)
				{
					SECharging = ChargeLV3SE;
					PlayCharge(SECharging[0], SECharging[1], oc.SoundSource);
				}
				break;
			case 3:
				if (ChargeMaxSE != null)
				{
					SECharging = ChargeMaxSE;
					PlayCharge(SECharging[0], SECharging[1], oc.SoundSource);
				}
				break;
			}
		}

		private void PlayCharge(string s_acb, string cuename, OrangeCriSource ocs)
		{
			CriAtomExAcb acb = MonoBehaviourSingleton<AudioManager>.Instance.GetAcb(s_acb, cuename);
			if (acb != null)
			{
				cueinfo = new CueInfo(s_acb, cuename);
				ocp.CueInfo = cueinfo;
				ocp.e_LoopType = OrangeCriPoint.LOOPTYPE.SE_LOOP;
				ocp.sourceObj = ocs;
				ocp.Player.SetCue(acb, cuename);
				ocp.Play(s_acb, cuename);
			}
		}

		public void StopSE(OrangeCharacter oc)
		{
			if (cueinfo != null && ocp != null)
			{
				CriAtomExAcb acb = MonoBehaviourSingleton<AudioManager>.Instance.GetAcb(SECharging[0], SECharging[2]);
				cueinfo = new CueInfo(SECharging[0], SECharging[2]);
				if (acb != null)
				{
					ocp.e_LoopType = OrangeCriPoint.LOOPTYPE.NONE;
					ocp.Player.SetCue(acb, SECharging[2]);
					ocp.CueInfo = cueinfo;
					ocp.sourceObj = oc.SoundSource;
					ocp.Play(SECharging[0], SECharging[2]);
				}
				cueinfo = null;
				ocp = null;
				SECharging = null;
			}
		}

		public ParticleSystem ChargeFx(ChargeLevel X)
		{
			return _listChargeFx[(int)X];
		}

		public void PlayChargeFx(ChargeLevel lv)
		{
			if (_listChargeFx[(int)lv] != null)
			{
				_listChargeFx[(int)lv].Play();
			}
		}

		public void StopChargeFx(ChargeLevel lv)
		{
			if (_listChargeFx[(int)lv] != null)
			{
				_listChargeFx[(int)lv].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}

		public void StopChargeFxAll()
		{
			for (int i = 0; i < _listChargeFx.Count; i++)
			{
				if (_listChargeFx[i] != null)
				{
					_listChargeFx[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				}
			}
		}

		public void ChangeLayer(int layer, bool includeSelf)
		{
			foreach (ParticleSystem item in _listChargeFx)
			{
				OrangeBattleUtility.ChangeLayersRecursively(item.transform, layer, includeSelf);
			}
		}
	}

	protected OrangeCharacter _refEntity;

	protected bool NeedChargeSE;

	protected Dictionary<ChargeType, ChargeData> _dicChargeData = new Dictionary<ChargeType, ChargeData>();

	public void Initialize(OrangeCharacter oc)
	{
		Reset();
		_refEntity = oc;
	}

	public void Reset()
	{
		_dicChargeData.Clear();
	}

	public void UpdateFunc()
	{
		if ((int)_refEntity.Hp <= 0 || (BattleInfoUI.Instance != null && BattleInfoUI.Instance.IsPlayClearBgm))
		{
			return;
		}
		NeedChargeSE = false;
		foreach (ChargeType key in _dicChargeData.Keys)
		{
			switch (key)
			{
			case ChargeType.SKILL0:
				UpdateSkillCharge(key, 0);
				break;
			case ChargeType.SKILL1:
				UpdateSkillCharge(key, 1);
				break;
			case ChargeType.WEAPON0:
				UpdateWeaponCharge(key, 0);
				break;
			case ChargeType.WEAPON1:
				UpdateWeaponCharge(key, 1);
				break;
			}
		}
		if (!NeedChargeSE && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_IN)
		{
			_refEntity.CharacterMaterials.UpdateMask(-1, 0f);
		}
	}

	protected void UpdateSkillCharge(ChargeType type, int skillIndex)
	{
		int index = 0 + 2;
		ChargeData value;
		if (!_dicChargeData.TryGetValue(type, out value))
		{
			Debug.Log("ChargeData not exist. Type = " + type);
		}
		else if (skillIndex >= _refEntity.PlayerSkills.Length)
		{
			Debug.Log("skillIndex >= _refEntity.PlayerSkills.Length");
		}
		else
		{
			if (value._listChargeFx.Any((ParticleSystem c) => c == null))
			{
				return;
			}
			if (!_refEntity.PlayerSkills[skillIndex].ChargeTimer.IsStarted())
			{
				_refEntity.PlayerSkills[skillIndex].ChargeLevel = 0;
				value.StopChargeFxAll();
				return;
			}
			long millisecond = _refEntity.PlayerSkills[skillIndex].ChargeTimer.GetMillisecond();
			sbyte b = 0;
			b = 0;
			while (b < _refEntity.PlayerSkills[skillIndex].ChargeTime.Length && millisecond >= _refEntity.PlayerSkills[skillIndex].ChargeTime[b])
			{
				_refEntity.PlayerSkills[skillIndex].ChargeLevel = b;
				b++;
			}
			if (_refEntity.PlayerSkills[skillIndex].ChargeLevel > 0)
			{
				if (_refEntity.PlayerSkills[skillIndex].ChargeTimer.GetTicks() % 6 > 3)
				{
					_refEntity.CharacterMaterials.UpdateMask(-1, 0f);
				}
				else
				{
					_refEntity.CharacterMaterials.UpdateMask(0, 1.5f);
				}
				NeedChargeSE = true;
			}
			if (_refEntity.PlayerSkills[skillIndex].ChargeLevel == 0 && value.ChargeFx(ChargeLevel.START) != null && !value.ChargeFx(ChargeLevel.START).isPlaying)
			{
				value.PlayChargeFx(ChargeLevel.START);
				value.StopChargeFx(ChargeLevel.LV1);
			}
			if (_refEntity.PlayerSkills[skillIndex].ChargeLevel == 1 && !value.ChargeFx(ChargeLevel.LV1).isPlaying)
			{
				value.PlayChargeFx(ChargeLevel.LV1);
				value.StopChargeFx(ChargeLevel.LV2);
				if (value.ChargeFx(ChargeLevel.START) != null)
				{
					value.StopChargeFx(ChargeLevel.START);
				}
			}
			if (_refEntity.PlayerSkills[skillIndex].ChargeLevel == 2 && !value.ChargeFx(ChargeLevel.LV2).isPlaying)
			{
				value.PlayChargeFx(ChargeLevel.LV2);
				value.StopChargeFx(ChargeLevel.LV1);
				value.PlaySE(2, _refEntity);
			}
			if (_refEntity.PlayerSkills[skillIndex].ChargeLevel == 3 && !value._listChargeFx[index].isPlaying)
			{
				value.PlayChargeFx(ChargeLevel.LV3);
				value.StopChargeFx(ChargeLevel.LV2);
				value.PlaySE(3, _refEntity);
			}
		}
	}

	protected void UpdateWeaponCharge(ChargeType type, int weaponIndex)
	{
		int num = 0;
		ChargeData value;
		if (!_dicChargeData.TryGetValue(type, out value))
		{
			Debug.Log("ChargeData not exist. Type = " + type);
		}
		else if (weaponIndex >= _refEntity.PlayerWeapons.Length)
		{
			Debug.Log("weaponIndex >= _refEntity.PlayerWeapons.Length");
		}
		else
		{
			if (value._listChargeFx.Any((ParticleSystem c) => c == null) || !_refEntity.PlayerWeapons[weaponIndex].ChargeTimer.IsStarted())
			{
				return;
			}
			long millisecond = _refEntity.PlayerWeapons[weaponIndex].ChargeTimer.GetMillisecond();
			sbyte b = 0;
			while (b < _refEntity.PlayerWeapons[weaponIndex].ChargeTime.Length && millisecond >= _refEntity.PlayerWeapons[weaponIndex].ChargeTime[b])
			{
				_refEntity.PlayerWeapons[weaponIndex].ChargeLevel = b;
				if (b > 0)
				{
					NeedChargeSE = true;
				}
				if (b == 1 && !value.ChargeFx(ChargeLevel.LV1).isPlaying)
				{
					value.ChargeFx(ChargeLevel.LV1).Play();
				}
				if (b == 2 && !value.ChargeFx(ChargeLevel.LV2).isPlaying)
				{
					value.ChargeFx(ChargeLevel.LV2).Play();
				}
				if (b == 3 && !value.ChargeFx(ChargeLevel.LV3).isPlaying)
				{
					value.ChargeFx(ChargeLevel.LV3).Play();
				}
				b++;
			}
		}
	}

	public bool SetChargeSE(ChargeType type, string[] seLv1 = null, string[] seLv3 = null, string[] seLvMax = null)
	{
		ChargeData value;
		if (!_dicChargeData.TryGetValue(type, out value))
		{
			Debug.LogWarning("Must add ChargeFx befor add SE string." + seLv1);
			return false;
		}
		value.ChargeLVSE = seLv1;
		value.ChargeLV3SE = seLv3;
		value.ChargeMaxSE = seLvMax;
		return true;
	}

	public void AddCharge(ChargeType type, ParticleSystem lv1 = null, ParticleSystem lv2 = null, ParticleSystem lv3 = null)
	{
		if (!_dicChargeData.ContainsKey(type))
		{
			ChargeData chargeData = new ChargeData();
			chargeData._listChargeFx[1] = lv1;
			chargeData._listChargeFx[2] = lv2;
			chargeData._listChargeFx[3] = lv3;
			_dicChargeData.Add(type, chargeData);
		}
	}

	public void AddCharge(ChargeType type, string lv1 = "", string lv2 = "", string lv3 = "", bool isRootTransform = false)
	{
		ChargeData data;
		if (!_dicChargeData.TryGetValue(type, out data))
		{
			data = new ChargeData();
			_dicChargeData.Add(type, data);
		}
		if (!string.IsNullOrEmpty(lv1))
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/chargefx", lv1, delegate(GameObject obj)
			{
				GameObject gameObject3 = Object.Instantiate(obj);
				if (isRootTransform)
				{
					gameObject3.transform.SetParent(_refEntity.transform.root, false);
				}
				else
				{
					gameObject3.transform.SetParent(_refEntity.AimTransform, false);
				}
				data._listChargeFx[1] = gameObject3.GetComponent<ParticleSystem>();
			});
		}
		if (!string.IsNullOrEmpty(lv2))
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/chargefx", lv2, delegate(GameObject obj)
			{
				GameObject gameObject2 = Object.Instantiate(obj);
				if (isRootTransform)
				{
					gameObject2.transform.SetParent(_refEntity.transform.root, false);
				}
				else
				{
					gameObject2.transform.SetParent(_refEntity.AimTransform, false);
				}
				data._listChargeFx[2] = gameObject2.GetComponent<ParticleSystem>();
			});
		}
		if (string.IsNullOrEmpty(lv3))
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/chargefx", lv3, delegate(GameObject obj)
		{
			GameObject gameObject = Object.Instantiate(obj);
			if (isRootTransform)
			{
				gameObject.transform.SetParent(_refEntity.transform.root, false);
			}
			else
			{
				gameObject.transform.SetParent(_refEntity.AimTransform, false);
			}
			data._listChargeFx[3] = gameObject.GetComponent<ParticleSystem>();
		});
	}

	public void AddStartCharge(ChargeType type, string lv0 = "", bool isRootTransform = false)
	{
		ChargeData data;
		if (!_dicChargeData.TryGetValue(type, out data))
		{
			data = new ChargeData();
			_dicChargeData.Add(type, data);
		}
		if (!(lv0 != ""))
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/chargefx", lv0, delegate(GameObject obj)
		{
			GameObject gameObject = Object.Instantiate(obj);
			if (isRootTransform)
			{
				gameObject.transform.SetParent(_refEntity.transform.root, false);
			}
			else
			{
				gameObject.transform.SetParent(_refEntity.AimTransform, false);
			}
			data._listChargeFx[0] = gameObject.GetComponent<ParticleSystem>();
		});
	}

	public void StartCharge(ChargeType type)
	{
		ChargeData value;
		if (_dicChargeData.TryGetValue(type, out value))
		{
			int num = 0;
			if (value.ChargeFx(ChargeLevel.START) != null)
			{
				value.ChargeFx(ChargeLevel.START).Play();
				value.StopChargeFx(ChargeLevel.LV1);
				value.StopChargeFx(ChargeLevel.LV2);
				value.StopChargeFx(ChargeLevel.LV3);
			}
			else
			{
				value.PlayChargeFx(ChargeLevel.LV1);
				value.StopChargeFx(ChargeLevel.LV2);
				value.StopChargeFx(ChargeLevel.LV3);
			}
			value.PlaySE(1, _refEntity);
		}
	}

	public void PlayChargeForEvent(bool bLv1, bool bLv2, bool bLv3 = false)
	{
		ChargeData chargeData = null;
		if (_dicChargeData.Count() != 0)
		{
			Dictionary<ChargeType, ChargeData>.Enumerator enumerator = _dicChargeData.GetEnumerator();
			enumerator.MoveNext();
			chargeData = enumerator.Current.Value;
			if (bLv1)
			{
				chargeData.PlayChargeFx(ChargeLevel.LV1);
				chargeData.PlaySE(1, _refEntity);
			}
			else
			{
				chargeData.StopChargeFx(ChargeLevel.LV1);
				chargeData.StopSE(_refEntity);
			}
			if (bLv2)
			{
				chargeData.PlayChargeFx(ChargeLevel.LV2);
				chargeData.PlaySE(2, _refEntity);
			}
			else
			{
				chargeData.StopChargeFx(ChargeLevel.LV2);
				chargeData.StopSE(_refEntity);
			}
			if (bLv3)
			{
				chargeData.PlayChargeFx(ChargeLevel.LV3);
				chargeData.PlaySE(2, _refEntity);
			}
			else
			{
				chargeData.StopChargeFx(ChargeLevel.LV3);
				chargeData.StopSE(_refEntity);
			}
		}
	}

	public void StopCharge(int skillId = -1)
	{
		foreach (ChargeType key in _dicChargeData.Keys)
		{
			if (skillId == -1 || (skillId == 0 && key == ChargeType.SKILL0) || (skillId == 1 && key == ChargeType.SKILL1))
			{
				StopCharge(key);
			}
		}
	}

	protected void StopCharge(ChargeType type)
	{
		switch (type)
		{
		case ChargeType.SKILL0:
			if (_refEntity.PlayerSkills[0].ChargeTimer.IsStarted())
			{
				_refEntity.PlayerSkills[0].ChargeTimer.TimerStop();
			}
			break;
		case ChargeType.SKILL1:
			if (_refEntity.PlayerSkills[1].ChargeTimer.IsStarted())
			{
				_refEntity.PlayerSkills[1].ChargeTimer.TimerStop();
			}
			break;
		case ChargeType.WEAPON0:
			if (_refEntity.PlayerWeapons[0].ChargeTimer.IsStarted())
			{
				_refEntity.PlayerWeapons[0].ChargeTimer.TimerStop();
			}
			break;
		case ChargeType.WEAPON1:
			if (_refEntity.PlayerWeapons[1].ChargeTimer.IsStarted())
			{
				_refEntity.PlayerWeapons[1].ChargeTimer.TimerStop();
			}
			break;
		}
		ChargeData value;
		if (_dicChargeData.TryGetValue(type, out value))
		{
			value.StopChargeFxAll();
			value.StopSE(_refEntity);
		}
	}

	public void ChangeLayer(int layer, bool includeSelf)
	{
		if (_dicChargeData.Count() != 0)
		{
			Dictionary<ChargeType, ChargeData>.Enumerator enumerator = _dicChargeData.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Value.ChangeLayer(layer, includeSelf);
			}
		}
	}
}
