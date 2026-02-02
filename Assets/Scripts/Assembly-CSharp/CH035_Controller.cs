#define RELEASE
using System;
using UnityEngine;

public class CH035_Controller : CharacterControlBase
{
	private SkinnedMeshRenderer tfBusterMesh;

	private SkinnedMeshRenderer tfHandMesh;

	protected int _enhanceSlot;

	protected ChargeShootObj _refChargeShootObj;

	private OrangeTimer NOVASTRIKETimer;

	protected bool _haveChange_Mat_Tex;

	private string[] elemnts = new string[4] { "", "f", "i", "t" };

	private readonly int hashDirection = Animator.StringToHash("fDirection");

	public override string[] GetCharacterDependAnimations()
	{
		return new string[1] { "ch035_skill_02_start" };
	}

	public override string[] GetCharacterDependBlendAnimations()
	{
		return new string[2] { "ch035_skill_02_start", "ch035_skill_02_loop" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_crouch_charge_atk" };
		target = new string[3] { "ch035_skill_01_stand_mid", "ch035_skill_01_jump_mid", "ch035_skill_01_crouch_mid" };
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
		InitEnhanceSkill();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_COPY-X_001");
		NOVASTRIKETimer = OrangeTimerManager.GetTimer();
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeSE = new string[3] { "SkillSE_CH035_000", "ch035_charge_lp", "ch035_charge_stop" };
		}
		else
		{
			_refChargeShootObj.ChargeSE = new string[3] { "BattleSE02", "bt_ch035_charge_lp", "bt_ch035_charge_stop" };
		}
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m", true);
		tfBusterMesh = transform.GetComponent<SkinnedMeshRenderer>();
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_m", true);
		tfHandMesh = transform2.GetComponent<SkinnedMeshRenderer>();
	}

	private void InitEnhanceSkill()
	{
		_enhanceSlot = _refEntity.PlayerSkills[0].EnhanceEXIndex;
		_haveChange_Mat_Tex = false;
		if (_enhanceSlot == 0)
		{
			return;
		}
		int skillId = (new int[4] { 14001, 14006, 14011, 14016 })[_enhanceSlot];
		_refEntity.ReInitSkillStruct(0, skillId);
		for (int i = 0; i < _refEntity.PlayerSkills[0].FastBulletDatas.Length; i++)
		{
			if (!MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(_refEntity.PlayerSkills[0].FastBulletDatas[i].s_MODEL))
			{
				BulletBase.PreloadBullet<BasicBullet>(_refEntity.PlayerSkills[0].FastBulletDatas[i]);
			}
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.LeaveRideArmorEvt = LeaveRideArmor;
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			switch (_refEntity.CurrentActiveSkill)
			{
			case 0:
				ToggleWeapon(0);
				_refEntity.CancelBusterChargeAtk();
				break;
			case 1:
				_refEntity.BulletCollider.BackToPool();
				ToggleWeapon(0);
				break;
			}
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = true;
			_refEntity.CurrentActiveSkill = -1;
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged())
		{
			return;
		}
		if (_refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.CheckSkillEndByShootTimer();
			if (_refEntity.SkillEnd)
			{
				ToggleWeapon(0);
			}
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.RIDE_ARMOR:
			if (_refEntity.CurrentActiveSkill != 1)
			{
				Debug.LogError("_CurrentActiveSkill != 1 => " + _refEntity.CurrentActiveSkill);
				_refEntity.CurrentActiveSkill = 1;
			}
			if (_refEntity.Velocity.y <= 0)
			{
				Debug.Log("Trigger Skill!");
				bool flag = false;
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				SKILL_TABLE sKILL_TABLE = currentSkillObj.BulletData;
				if (currentSkillObj.ComboCheckDatas.Length != 0 && currentSkillObj.ComboCheckDatas[0].CheckHasAllBuff(_refEntity.selfBuffManager))
				{
					flag = true;
					sKILL_TABLE = currentSkillObj.FastBulletDatas[currentSkillObj.Reload_index];
				}
				OrangeBattleUtility.UpdateSkillCD(currentSkillObj);
				NOVASTRIKETimer.TimerStart();
				_refEntity.SetSpeed((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 2.5f), 0);
				_refEntity.BulletCollider.UpdateBulletData(sKILL_TABLE, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(currentSkillObj.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = currentSkillObj.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.SetStatus(_refEntity.CurMainStatus, OrangeCharacter.SubStatus.IDLE);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[0], null, currentSkillObj.Reload_index);
				if (flag)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_COPY-X_001", _refEntity.ModelTransform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
					_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
				}
				else
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_COPY-X_001", _refEntity.ModelTransform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				}
			}
			break;
		case OrangeCharacter.SubStatus.IDLE:
			if (_refEntity.CurrentActiveSkill != 1)
			{
				Debug.LogError("_CurrentActiveSkill != 1 => " + _refEntity.CurrentActiveSkill);
				_refEntity.CurrentActiveSkill = 1;
			}
			if (NOVASTRIKETimer.GetMillisecond() > 417)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SetSpeed(0, 0);
				_refEntity.SkillEnd = true;
				ToggleWeapon(0);
				_refEntity.BulletCollider.BackToPool();
			}
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			if (_refEntity.PlayerSetting.AutoCharge == 1)
			{
				if (!_refEntity.PlayerSkills[id].ChargeTimer.IsStarted())
				{
					ChargeStart();
				}
				else if (_refEntity.CurrentActiveSkill == -1)
				{
					ShootSkill0();
				}
			}
			else if (_refEntity.CurrentActiveSkill == -1)
			{
				ShootSkill0();
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				if (_refEntity.Dashing)
				{
					_refEntity.PlayerStopDashing();
				}
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				_refEntity.SetSpeed((int)_refEntity._characterDirection * OrangeCharacter.WalkSpeed, (int)((float)OrangeCharacter.JumpSpeed * 0.5f));
				_refEntity.StopShootTimer();
				ToggleWeapon(1);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.RIDE_ARMOR);
				_refEntity.StartJumpThroughCorutine();
				PlayVoiceSE("v_ch035_skill02");
				PlaySkillSE("ch035_starstrike");
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id != 0)
		{
			int num = 1;
		}
		else if (_refEntity.CurrentActiveSkill == -1 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			ShootSkill0();
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if (subStatus != 0)
			{
				int num = 1;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			if (subStatus != OrangeCharacter.SubStatus.SKILL0)
			{
				int num2 = 49;
			}
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
			{
				ToggleWeapon(0);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			if (subStatus != OrangeCharacter.SubStatus.SKILL0)
			{
				int num = 49;
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus != OrangeCharacter.SubStatus.SKILL0)
			{
				int num = 49;
			}
		}
	}

	public void TeleportInCharacterDepend()
	{
		if (!(_refEntity.CurrentFrame >= 0.5f) || _haveChange_Mat_Tex)
		{
			return;
		}
		_haveChange_Mat_Tex = true;
		CharacterMaterial component = _refEntity.ModelTransform.GetComponent<CharacterMaterial>();
		if ((bool)component)
		{
			if (_enhanceSlot > 0)
			{
				component.UpdateTex(_enhanceSlot - 1);
			}
			else
			{
				component.UpdateTex();
			}
		}
	}

	public void TeleportInExtraEffect()
	{
		if (_enhanceSlot > 0)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			PlaySkillSE("ch035_start01");
		}
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_copyx_in";
	}

	public void LeaveRideArmor(RideBaseObj targetRideArmor)
	{
		_refEntity.LeaveRideArmor(targetRideArmor);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
		if (weaponStruct.ComboCheckDatas.Length != 0 && weaponStruct.ComboCheckDatas[0].CheckHasAllBuff(_refEntity.selfBuffManager))
		{
			_refEntity.UpdateSkillIcon(_refEntity.PlayerSkills);
			_refEntity.ForceChangeSkillIcon(2, weaponStruct.FastBulletDatas[1].s_ICON);
		}
	}

	protected void ChargeStart()
	{
		_refEntity.PlayerSkills[0].ChargeTimer.TimerStart();
		_refChargeShootObj.StartCharge();
	}

	protected void ShootSkill0()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		if (weaponStruct.ChargeLevel == 0)
		{
			PlayVoiceSE("v_ch035_skill01");
		}
		else
		{
			if (_enhanceSlot > 0 && weaponStruct.ChargeLevel >= 1)
			{
				string text = string.Format("ch035_shot_0{0}_", weaponStruct.ChargeLevel + 1);
				text += elemnts[_enhanceSlot];
				PlaySkillSE(text);
			}
			PlayVoiceSE("v_ch035_skill01");
		}
		_refChargeShootObj.StopCharge();
		ToggleWeapon(1);
		_refEntity.CurrentActiveSkill = 0;
		_refEntity.IsShoot = (sbyte)(weaponStruct.ChargeLevel + 1);
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct, weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel].n_USE_COST, -1f);
		_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
		_refEntity.Animator.SetAnimatorEquip(1);
		float value = Mathf.Abs(Vector2.SignedAngle(Vector2.up, (float)_refEntity._characterDirection * Vector3.right)) / 180f;
		_refEntity.Animator._animator.SetFloat(hashDirection, value);
	}

	private void ToggleWeapon(int style)
	{
		if (style == 1)
		{
			_refEntity.DisableCurrentWeapon();
			tfBusterMesh.enabled = true;
			tfHandMesh.enabled = false;
		}
		else
		{
			tfBusterMesh.enabled = false;
			tfHandMesh.enabled = true;
			_refEntity.EnableCurrentWeapon();
		}
	}
}
