using System;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class RollController : CharacterControlBase
{
	private bool bInSkill;

	private Vector3 CtrlShotDir;

	private bool bInShootBullet;

	private bool b_use_fast_skill;

	public int n_rand_index;

	private int n_total_rand;

	private Transform mBroomMesh_c;

	private Sprite[] m_Rand_Icon;

	private CollideBullet tCB;

	private FxBase mFxBase_skill2;

	public SCH003Controller mPCB;

	private int SkilleStatusId;

	private int nLastSkillIndex0;

	private OrangeCharacter.MainStatus LastMainStatus;

	private OrangeCharacter.SubStatus LastSubStatus;

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch029_skill_02_stand_atk_up", "ch029_skill_02_stand_atk_mid", "ch029_skill_02_stand_atk_down" };
		string[] array2 = new string[3] { "ch029_skill_02_jump_atk_up", "ch029_skill_02_jump_atk_mid", "ch029_skill_02_jump_atk_down" };
		return new string[2][] { array, array2 };
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[9] { "ch029_skill_01_jump_first_atk", "ch029_skill_01_stand_first_atk", "ch029_skill_01_stand_second_atk_start", "ch029_skill_01_stand_second_atk_loop", "ch029_skill_01_stand_second_atk_end", "ch029_skill_02_jump_call_eddie", "ch029_skill_02_stand_call_eddie", "ch029_skill_02_buffer_atk_jump", "ch029_skill_02_buffer_atk_stand" };
	}

	private void InitPetMode()
	{
		bool flag = false;
		int petID = 0;
		int follow_skill_id = 0;
		if (_refEntity.PlayerSkills[0].BulletData.n_EFFECT == 17)
		{
			flag = true;
			petID = (int)_refEntity.PlayerSkills[0].BulletData.f_EFFECT_X;
			follow_skill_id = 0;
		}
		else if (_refEntity.PlayerSkills[1].BulletData.n_EFFECT == 17)
		{
			flag = true;
			petID = (int)_refEntity.PlayerSkills[1].BulletData.f_EFFECT_X;
			follow_skill_id = 1;
		}
		if (flag)
		{
			PetBuilder petBuilder = new GameObject().AddComponent<PetBuilder>();
			petBuilder.PetID = petID;
			petBuilder.follow_skill_id = follow_skill_id;
			petBuilder.CreatePet(delegate(SCH003Controller obj)
			{
				mPCB = obj;
				mPCB._follow_Player = _refEntity;
				mPCB._master_Roll = this;
				mPCB.SetFollowOffset(new Vector3(1f, 1f, 0f));
				mPCB.transform.localScale = new Vector3(2f, 2f, mPCB.transform.localScale.z);
			});
		}
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		mBroomMesh_c = OrangeBattleUtility.FindChildRecursive(ref target, "BroomMesh_c", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_rolls_skill2_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_rolls_skill2_001", 2);
		SkilleStatusId = 0;
		GameObject gameObject = new GameObject();
		tCB = gameObject.AddComponent<CollideBullet>();
		tCB.gameObject.layer = base.gameObject.layer;
		if (_refEntity.PlayerSkills[1].BulletData.n_EFFECT != 17)
		{
			return;
		}
		InitPetMode();
		m_Rand_Icon = new Sprite[_refEntity.PlayerSkills[1].BulletData.n_CHARGE_MAX_LEVEL + 1];
		int i;
		for (i = 0; i < _refEntity.PlayerSkills[1].BulletData.n_CHARGE_MAX_LEVEL + 1; i++)
		{
			if (_refEntity.PlayerSkills[1].FastBulletDatas[i].n_TRIGGER_RATE > n_total_rand)
			{
				n_total_rand = _refEntity.PlayerSkills[1].FastBulletDatas[i].n_TRIGGER_RATE;
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(_refEntity.PlayerSkills[1].FastBulletDatas[i].s_ICON), _refEntity.PlayerSkills[1].FastBulletDatas[i].s_ICON, delegate(Sprite obj)
			{
				if (obj != null)
				{
					m_Rand_Icon[i] = obj;
				}
			});
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.UpdateAimRangeByWeaponEvt = UpdateAimRangeByWeapon;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	private Vector3 GetShotDir(Vector3 tShotPos)
	{
		return CtrlShotDir;
	}

	private void setBroomActive(bool value)
	{
		mBroomMesh_c.gameObject.SetActive(value);
	}

	private bool checkCancelAnimate(int skilliD)
	{
		if (skilliD == 0)
		{
			if ((ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) || ManagedSingleton<InputStorage>.Instance.IsAnyPress(_refEntity.UserID)) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
		}
		else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
		{
			return true;
		}
		return false;
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurrentFrame > 0.6f && mBroomMesh_c.gameObject.activeSelf && _refEntity.CurMainStatus == OrangeCharacter.MainStatus.TELEPORT_IN)
		{
			setBroomActive(false);
		}
		if (_refEntity.IsAnimateIDChanged() || !bInSkill)
		{
			return;
		}
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.SKILL:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
				if ((double)_refEntity.CurrentFrame > 0.3 && !bInShootBullet)
				{
					bInShootBullet = true;
					_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().BulletData, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ModelTransform, _refEntity.GetCurrentSkillObj().SkillLV);
					_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
			case OrangeCharacter.SubStatus.SKILL1_5:
				if ((double)_refEntity.CurrentFrame > 0.2 && !bInShootBullet)
				{
					_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().FastBulletDatas[n_rand_index], _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0], _refEntity.GetCurrentSkillObj().SkillLV);
					bInShootBullet = true;
				}
				break;
			}
			break;
		case OrangeCharacter.MainStatus.IDLE:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.SKILL_IDLE)
			{
				UpdateSkillDash();
			}
			break;
		}
		case OrangeCharacter.MainStatus.FALL:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.IDLE)
			{
				UpdateSkillDash();
			}
			break;
		}
		}
	}

	private void UpdateSkillDash()
	{
		if ((double)_refEntity.CurrentFrame < 0.5)
		{
			int num = (int)((0.5f - _refEntity.CurrentFrame) / 2f * ((float)OrangeCharacter.DashSpeed * 1.1f));
			if (CtrlShotDir.x > 0f)
			{
				_refEntity.SetSpeed(num, 0);
			}
			else
			{
				_refEntity.SetSpeed(-1 * num, 0);
			}
		}
		else
		{
			_refEntity.SetSpeed(0, 0);
		}
	}

	public override void ClearSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			setBroomActive(false);
			_refEntity.EnableCurrentWeapon();
			if (mFxBase_skill2 != null && mFxBase_skill2.isActiveAndEnabled)
			{
				mFxBase_skill2.BackToPool();
			}
			break;
		}
		if (_refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
		_refEntity.SkillEnd = true;
		bInSkill = false;
		_refEntity.CurrentActiveSkill = -1;
		SkilleStatusId = 0;
		if (!_refEntity.UsingVehicle)
		{
			b_use_fast_skill = false;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.SKILL:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.IDLE:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.SKILL_IDLE)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
			}
			break;
		}
		case OrangeCharacter.MainStatus.FALL:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.IDLE)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
			}
			break;
		}
		}
		switch (LastMainStatus)
		{
		case OrangeCharacter.MainStatus.IDLE:
		{
			OrangeCharacter.SubStatus curSubStatus = LastSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.SKILL_IDLE)
			{
				if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.DASH && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.AIRDASH)
				{
					_refEntity.Dashing = false;
				}
				if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.IDLE && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.IDLE)
				{
					_refEntity.SetSpeed(0, 0);
				}
				setBroomActive(false);
				_refEntity.EnableCurrentWeapon();
			}
			break;
		}
		case OrangeCharacter.MainStatus.FALL:
		{
			OrangeCharacter.SubStatus curSubStatus = LastSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.IDLE)
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.IgnoreGravity = false;
				setBroomActive(false);
				_refEntity.EnableCurrentWeapon();
			}
			break;
		}
		}
		LastMainStatus = _refEntity.CurMainStatus;
		LastSubStatus = _refEntity.CurSubStatus;
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				setBroomActive(false);
				_refEntity.EnableCurrentWeapon();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.Dashing = false;
				setBroomActive(false);
				_refEntity.EnableCurrentWeapon();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				_refEntity.SkillEnd = true;
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.SKILL_IDLE);
					_refEntity.SkillEnd = true;
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
					_refEntity.SkillEnd = true;
					_refEntity.SetSpeed(0, 0);
					_refEntity.IgnoreGravity = false;
					setBroomActive(false);
					_refEntity.EnableCurrentWeapon();
				}
				_refEntity.BulletCollider.BackToPool();
				if (mFxBase_skill2 != null)
				{
					mFxBase_skill2.BackToPool();
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_4:
				_refEntity.SetSpeed(0, 0);
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
					_refEntity.SkillEnd = true;
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
					_refEntity.SkillEnd = true;
					_refEntity.IgnoreGravity = false;
				}
				setBroomActive(false);
				_refEntity.EnableCurrentWeapon();
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				_refEntity.SkillEnd = true;
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				_refEntity.SkillEnd = true;
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				_refEntity.SkillEnd = true;
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
				break;
			}
			break;
		case OrangeCharacter.MainStatus.IDLE:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.SKILL_IDLE)
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.Dashing = false;
				setBroomActive(false);
				_refEntity.EnableCurrentWeapon();
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
			}
			break;
		}
		case OrangeCharacter.MainStatus.FALL:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.IDLE)
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.IgnoreGravity = false;
				setBroomActive(false);
				_refEntity.EnableCurrentWeapon();
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
			}
			break;
		}
		}
	}

	protected void CheckCombo_PushBulletDetail(ref WeaponStruct weaponStruct, Transform ShootPosition, int bulletlv = 0, Vector3? ShotDir = null)
	{
		SKILL_TABLE sKILL_TABLE = weaponStruct.FastBulletDatas[0];
		if (sKILL_TABLE.n_COMBO_SKILL != 0)
		{
			bool flag = false;
			if (sKILL_TABLE.s_COMBO != "null")
			{
				string[] array = sKILL_TABLE.s_COMBO.Split(',');
				flag = _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(int.Parse(array[1]));
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				for (int i = 1; i < weaponStruct.FastBulletDatas.Length; i++)
				{
					if (sKILL_TABLE.n_COMBO_SKILL == weaponStruct.FastBulletDatas[i].n_ID)
					{
						weaponStruct.FastBulletDatas[0] = weaponStruct.FastBulletDatas[i];
						_refEntity.BulletCollider.UpdateBulletData(weaponStruct.FastBulletDatas[0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
						_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
						_refEntity.BulletCollider.BulletLevel = bulletlv;
						_refEntity.BulletCollider.Active(_refEntity.TargetMask);
						if (weaponStruct.FastBulletDatas[0].n_RELOAD > 0)
						{
							_refEntity.GetCurrentSkillObj().Reload_index = i;
							OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
						}
						weaponStruct.FastBulletDatas[0] = weaponStruct.BulletData;
						_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, weaponStruct.weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
					}
				}
			}
			else
			{
				_refEntity.BulletCollider.UpdateBulletData(weaponStruct.FastBulletDatas[0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = bulletlv;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
			}
		}
		else
		{
			_refEntity.BulletCollider.UpdateBulletData(weaponStruct.FastBulletDatas[0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = bulletlv;
			_refEntity.BulletCollider.Active(_refEntity.TargetMask);
		}
	}

	private bool Check_Skill_Status(int eff_id)
	{
		if (_refEntity.IsLocalPlayer)
		{
			if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(eff_id))
			{
				return true;
			}
			return false;
		}
		if (SkilleStatusId != eff_id)
		{
			SkilleStatusId = eff_id;
			return false;
		}
		SkilleStatusId = 0;
		return true;
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
		{
			if (_refEntity.CurrentActiveSkill != -1 || !_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			_refEntity.SetSpeed(0, 0);
			_refEntity.StopShootTimer();
			bInShootBullet = false;
			nLastSkillIndex0 = _refEntity.GetCurrentSkillObj().Reload_index;
			int num2 = Math.Sign(_refEntity.ShootDirection.x);
			if (_refEntity._characterDirection != (CharacterDirection)num2 && Mathf.Abs(_refEntity.ShootDirection.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			}
			if (nLastSkillIndex0 == 0)
			{
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_rolls_skill2_000", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_rolls_skill2_0001", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
				}
				setBroomActive(true);
				_refEntity.PlaySE(_refEntity.VoiceID, 7);
			}
			else
			{
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
				}
				CtrlShotDir = _refEntity.ShootDirection;
				if (CtrlShotDir.x > 0f)
				{
					_refEntity.SetSpeed((int)((float)OrangeCharacter.DashSpeed * 1.1f), 0);
				}
				else
				{
					_refEntity.SetSpeed(-1 * (int)((float)OrangeCharacter.DashSpeed * 1.1f), 0);
				}
				mFxBase_skill2 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_rolls_skill2_001", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
				_refEntity.BulletCollider.UpdateBulletData(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkillIndex0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.GetCurrentSkillObj().SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0], null, nLastSkillIndex0);
				_refEntity.RemoveComboSkillBuff(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkillIndex0].n_ID);
				setBroomActive(true);
				if (_refEntity.GetCurrentSkillObj().BulletData.n_RELOAD > 0)
				{
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				}
			}
			_refEntity.DisableCurrentWeapon();
			break;
		}
		case 1:
			if (_refEntity.CurrentActiveSkill != -1 || !_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			if (!b_use_fast_skill)
			{
				if ((bool)_refEntity.GetCurrentWeaponObj().MeleeBullet)
				{
					_refEntity.DeActivateMeleeAttack(_refEntity.GetCurrentWeaponObj());
					_refEntity.ClearSlashAction();
					_refEntity.DisableCurrentWeapon();
				}
				else
				{
					_refEntity.DisableCurrentWeapon();
				}
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.SetSpeed(0, 0);
				b_use_fast_skill = true;
				if (_refEntity.IsLocalPlayer)
				{
					int num = OrangeBattleUtility.Random(0, n_total_rand - 1);
					for (int i = 0; i < _refEntity.GetCurrentSkillObj().BulletData.n_CHARGE_MAX_LEVEL; i++)
					{
						if (num < _refEntity.GetCurrentSkillObj().FastBulletDatas[i + 1].n_TRIGGER_RATE)
						{
							n_rand_index = i + 1;
							_refEntity.ForceChangeSkillIcon(2, m_Rand_Icon[n_rand_index]);
							StageUpdate.RegisterPetSendAndRun(mPCB.sNetSerialID, 1, JsonConvert.SerializeObject(n_rand_index), true);
							UpdateAimRangeByWeapon(_refEntity.GetCurrentWeaponObj());
							break;
						}
					}
					_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
					_refEntity.PlaySE(_refEntity.VoiceID, 9);
				}
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				}
				mPCB.Player_Effect((int)_refEntity._characterDirection);
				if (_refEntity is OrangeConsoleCharacter)
				{
					OrangeConsoleCharacter orangeConsoleCharacter = _refEntity as OrangeConsoleCharacter;
					if (orangeConsoleCharacter.PlayerSkills[orangeConsoleCharacter.CurrentActiveSkill].FastBulletDatas[n_rand_index].n_USE_TYPE == 1)
					{
						orangeConsoleCharacter.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, true);
					}
					else
					{
						orangeConsoleCharacter.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, false);
					}
				}
			}
			else if (_refEntity.PlayerSkills[id].FastBulletDatas[n_rand_index].n_TYPE == 7)
			{
				if ((bool)_refEntity.GetCurrentWeaponObj().MeleeBullet)
				{
					_refEntity.DeActivateMeleeAttack(_refEntity.GetCurrentWeaponObj());
					_refEntity.ClearSlashAction();
					_refEntity.DisableCurrentWeapon();
				}
				else
				{
					_refEntity.DisableCurrentWeapon();
				}
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.SetSpeed(0, 0);
				b_use_fast_skill = false;
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				}
				int nowRecordNO = _refEntity.GetNowRecordNO();
				tCB.transform.position = base.transform.position;
				tCB.UpdateBulletData(_refEntity.GetCurrentSkillObj().FastBulletDatas[n_rand_index], _refEntity.sPlayerName, nowRecordNO, _refEntity.nBulletRecordID++);
				tCB.SetBulletAtk(_refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				tCB.BulletLevel = _refEntity.GetCurrentSkillObj().SkillLV;
				tCB.Active(BulletScriptableObject.Instance.BulletLayerMaskPlayer);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0], null, n_rand_index);
				if (_refEntity.GetCurrentSkillObj().BulletData.n_RELOAD > 0)
				{
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				}
				if (_refEntity is OrangeConsoleCharacter)
				{
					OrangeConsoleCharacter obj = _refEntity as OrangeConsoleCharacter;
					obj.ForceChangeSkillIcon(2, m_Rand_Icon[0]);
					obj.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, false);
					_refEntity.PlaySE(_refEntity.VoiceID, 9);
				}
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if ((int)_refEntity.Hp > 0 && id != 0 && id == 1 && _refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id) && b_use_fast_skill && _refEntity.PlayerSkills[id].FastBulletDatas[n_rand_index].n_TYPE != 7)
		{
			if ((bool)_refEntity.GetCurrentWeaponObj().MeleeBullet)
			{
				_refEntity.DeActivateMeleeAttack(_refEntity.GetCurrentWeaponObj());
				_refEntity.ClearSlashAction();
				_refEntity.DisableCurrentWeapon();
			}
			else
			{
				_refEntity.DisableCurrentWeapon();
			}
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			_refEntity.SetSpeed(0, 0);
			CtrlShotDir = _refEntity.ShootDirection;
			int num = Math.Sign(CtrlShotDir.x);
			if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(CtrlShotDir.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			}
			b_use_fast_skill = false;
			float num2 = 0f;
			bInShootBullet = false;
			num2 = Mathf.Abs(Vector2.SignedAngle(Vector2.down, -_refEntity.ShootDirection)) / 180f;
			_refEntity.Animator._animator.SetFloat(_refEntity.Animator.hashDirection, num2);
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_5);
			}
			_refEntity.PlaySE(_refEntity.SkillSEID, 5);
			_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0], null, n_rand_index);
			if (_refEntity.GetCurrentSkillObj().BulletData.n_RELOAD > 0)
			{
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			}
			if (_refEntity is OrangeConsoleCharacter)
			{
				PlayVoiceSE("v_rl_skill02");
				OrangeConsoleCharacter obj = _refEntity as OrangeConsoleCharacter;
				obj.ForceChangeSkillIcon(2, m_Rand_Icon[0]);
				obj.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, false);
			}
		}
	}

	protected void UpdateAimRangeByWeapon(WeaponStruct weapon)
	{
		if ((short)weapon.WeaponData.n_TYPE == 8)
		{
			float f_DISTANCE = _refEntity.PlayerSkills[1].FastBulletDatas[n_rand_index].f_DISTANCE;
			_refEntity.PlayerAutoAimSystem.UpdateAimRange(f_DISTANCE);
		}
		else
		{
			_refEntity.PlayerAutoAimSystem.UpdateAimRange(weapon.BulletData.f_DISTANCE);
		}
	}

	public bool CheckPetActive(int petId)
	{
		if (mPCB != null && mPCB.Activate && mPCB.PetID == petId)
		{
			return true;
		}
		return false;
	}
}
