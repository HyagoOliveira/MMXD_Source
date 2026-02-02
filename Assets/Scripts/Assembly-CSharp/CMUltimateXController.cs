using System;
using UnityEngine;

public class CMUltimateXController : CharacterControlBase
{
	private bool bInSkill;

	private Vector3 CtrlShotDir;

	private bool bInShootBullet;

	private SKILL_TABLE Source_Bullet;

	private Transform[] mWeaponMeshB = new Transform[2];

	private OrangeTimer _SkillLoopTimer;

	private int SkilleStatusId;

	private int nLastSkillIndex0;

	private bool bWallGrab;

	private SkinnedMeshRenderer lefthandMesh;

	private SkinnedMeshRenderer rightHandMesh;

	private FxBase fx_fullweapon;

	private FxBase fx_cheastbeam;

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch014_skill_01_backward_first_shot_up_loop", "ch014_skill_01_backward_first_shot_mid_loop", "ch014_skill_01_backward_first_shot_down_loop" };
		string[] array2 = new string[3] { "ch014_skill_01_backward_second_shot_up_loop", "ch014_skill_01_backward_second_shot_mid_loop", "ch014_skill_01_backward_second_shot_down_loop" };
		string[] array3 = new string[3] { "ch014_skill_01_crouch_first_shot_up", "ch014_skill_01_crouch_first_shot_mid", "ch014_skill_01_crouch_first_shot_down" };
		string[] array4 = new string[3] { "ch014_skill_01_crouch_second_shot_down", "ch014_skill_01_crouch_second_shot_mid", "ch014_skill_01_crouch_second_shot_up" };
		string[] array5 = new string[3] { "ch014_skill_01_fall_first_shot_up", "ch014_skill_01_fall_first_shot_mid", "ch014_skill_01_fall_first_shot_down" };
		string[] array6 = new string[3] { "ch014_skill_01_fall_second_shot_up", "ch014_skill_01_fall_second_shot_mid", "ch014_skill_01_fall_second_shot_down" };
		string[] array7 = new string[3] { "ch014_skill_01_jump_first_shot_up", "ch014_skill_01_jump_first_shot_mid", "ch014_skill_01_jump_first_shot_down" };
		string[] array8 = new string[3] { "ch014_skill_01_jump_second_shot_up", "ch014_skill_01_jump_second_shot_mid", "ch014_skill_01_jump_second_shot_down" };
		string[] array9 = new string[3] { "ch014_skill_01_run_first_shot_up_loop", "ch014_skill_01_run_first_shot_mid_loop", "ch014_skill_01_run_first_shot_down_loop" };
		string[] array10 = new string[3] { "ch014_skill_01_run_second_shot_up_loop", "ch014_skill_01_run_second_shot_mid_loop", "ch014_skill_01_run_second_shot_down_loop" };
		string[] array11 = new string[3] { "ch014_skill_01_stand_first_shot_up", "ch014_skill_01_stand_first_shot_mid", "ch014_skill_01_stand_first_shot_down" };
		string[] array12 = new string[3] { "ch014_skill_01_stand_second_shot_up", "ch014_skill_01_stand_second_shot_mid", "ch014_skill_01_stand_second_shot_down" };
		return new string[12][]
		{
			array, array2, array3, array4, array5, array6, array7, array8, array9, array10,
			array11, array12
		};
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[12]
		{
			"ch014_skill_01_stand_third_shot_start", "ch014_skill_01_stand_third_shot_loop", "ch014_skill_01_stand_third_shot_end", "ch014_skill_01_jump_third_shot_start", "ch014_skill_01_jump_third_shot_loop", "ch014_skill_01_jump_third_shot_end", "ch014_skill_02_jump_start", "ch014_skill_02_jump_loop", "ch014_skill_02_jump_end", "ch014_skill_02_stand_start",
			"ch014_skill_02_stand_loop", "ch014_skill_02_stand_end"
		};
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[2];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L SkillPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R SkillPoint", true);
		mWeaponMeshB[0] = OrangeBattleUtility.FindChildRecursive(ref target, "WeaponMesh_L_m", true);
		mWeaponMeshB[1] = OrangeBattleUtility.FindChildRecursive(ref target, "WeaponMesh_R_m", true);
		Set_CMX_Weapon(false);
		Source_Bullet = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[_refEntity.PlayerSkills[0].BulletData.n_ID];
		if (_SkillLoopTimer == null)
		{
			_SkillLoopTimer = OrangeTimerManager.GetTimer();
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_fullweapon_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_chestbeam_000", 2);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_m", true);
		lefthandMesh = transform.GetComponent<SkinnedMeshRenderer>();
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_R_m", true);
		rightHandMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		SkilleStatusId = 0;
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
	}

	private void Set_CMX_Weapon(bool val)
	{
		if (mWeaponMeshB[0] != null)
		{
			mWeaponMeshB[0].gameObject.SetActive(val);
		}
		if (mWeaponMeshB[1] != null)
		{
			mWeaponMeshB[1].gameObject.SetActive(val);
		}
		if (val)
		{
			if ((bool)lefthandMesh && (bool)rightHandMesh)
			{
				lefthandMesh.enabled = false;
				rightHandMesh.enabled = false;
			}
		}
		else if ((bool)lefthandMesh && (bool)rightHandMesh)
		{
			rightHandMesh.enabled = true;
		}
	}

	private Vector3 GetShotDir(Vector3 tShotPos)
	{
		return CtrlShotDir;
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		if (weaponStruct.FastBulletDatas.Length >= 2)
		{
			float num = 1f;
			if (_refEntity._characterDirection == CharacterDirection.LEFT)
			{
				num = -1f;
			}
			float num2 = num - Mathf.Abs(_refEntity.ShootDirection.y);
			if (_refEntity._characterDirection == CharacterDirection.LEFT)
			{
				num2 = num + Mathf.Abs(_refEntity.ShootDirection.y);
			}
			if (_refEntity.ShootDirection.y < -0.5f)
			{
				Vector3 shootPosition = new Vector3(_refEntity.AimTransform.position.x + num2, _refEntity.AimTransform.position.y, _refEntity.AimTransform.position.z);
				_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[2], weaponStruct.weaponStatus, shootPosition, weaponStruct.SkillLV, _refEntity.ShootDirection);
			}
			else
			{
				Vector3 shootPosition2 = new Vector3(_refEntity.AimTransform.position.x + num2, _refEntity.AimTransform.position.y + 0.5f, _refEntity.AimTransform.position.z);
				_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[2], weaponStruct.weaponStatus, shootPosition2, weaponStruct.SkillLV, _refEntity.ShootDirection);
			}
			_refEntity.RemoveComboSkillBuff(weaponStruct.FastBulletDatas[2].n_ID);
		}
	}

	private void CheckSkill_Bullet()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
		case OrangeCharacter.SubStatus.SKILL0_3:
		case OrangeCharacter.SubStatus.SKILL0_4:
			if ((double)_refEntity.CurrentFrame > 0.2 && !bInShootBullet)
			{
				_refEntity.RemoveComboSkillBuff(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkillIndex0].n_ID);
				_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkillIndex0], _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0], _refEntity.GetCurrentSkillObj().SkillLV);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0], null, nLastSkillIndex0);
				bInShootBullet = true;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_5:
		case OrangeCharacter.SubStatus.SKILL0_6:
		case OrangeCharacter.SubStatus.SKILL0_7:
		case OrangeCharacter.SubStatus.SKILL0_8:
		case OrangeCharacter.SubStatus.SKILL0_9:
			if ((double)_refEntity.CurrentFrame > 0.2 && !bInShootBullet)
			{
				_refEntity.RemoveComboSkillBuff(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkillIndex0].n_ID);
				_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkillIndex0], _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[1], _refEntity.GetCurrentSkillObj().SkillLV);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0], null, nLastSkillIndex0);
				bInShootBullet = true;
			}
			break;
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || !bInSkill)
		{
			return;
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_5:
				if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
				{
					CharacterDirection destFacing = GetDestFacing();
					int num = _refEntity.CalculateMoveSpeed();
					if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0)
					{
						_refEntity.ForceSetAnimateId((HumanBase.AnimateId)135u);
						_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0_2;
						_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)135u, _refEntity.CurrentFrame);
					}
					else
					{
						_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0_7;
						_refEntity.ForceSetAnimateId((HumanBase.AnimateId)136u);
						_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)136u, _refEntity.CurrentFrame);
					}
					_refEntity.SetHorizontalSpeed((int)destFacing * num);
					_refEntity._characterDirection = destFacing;
				}
				else if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0)
					{
						_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0_1;
						_refEntity.ForceSetAnimateId((HumanBase.AnimateId)129u);
						_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)129u, _refEntity.CurrentFrame);
					}
					else
					{
						_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0_6;
						_refEntity.ForceSetAnimateId((HumanBase.AnimateId)130u);
						_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)130u, _refEntity.CurrentFrame);
					}
				}
				else if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.JUMP))
				{
					_refEntity.SetSpeed(0, OrangeCharacter.JumpSpeed);
					if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0)
					{
						_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0_3;
						_refEntity.ForceSetAnimateId((HumanBase.AnimateId)133u);
						_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)133u, _refEntity.CurrentFrame);
					}
					else
					{
						_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0_8;
						_refEntity.ForceSetAnimateId((HumanBase.AnimateId)134u);
						_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)134u, _refEntity.CurrentFrame);
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
			case OrangeCharacter.SubStatus.SKILL0_6:
				if (!ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_1)
					{
						_refEntity.ForceSetAnimateId((HumanBase.AnimateId)137u);
						_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0;
						_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)137u, _refEntity.CurrentFrame);
					}
					else
					{
						_refEntity.ForceSetAnimateId((HumanBase.AnimateId)138u);
						_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0_5;
						_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)138u, _refEntity.CurrentFrame);
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
			case OrangeCharacter.SubStatus.SKILL0_4:
			case OrangeCharacter.SubStatus.SKILL0_8:
			case OrangeCharacter.SubStatus.SKILL0_9:
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetSpeed(0, 0);
					if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_3 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_4)
					{
						_refEntity.ForceSetAnimateId((HumanBase.AnimateId)137u);
						_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0;
						_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)137u, _refEntity.CurrentFrame);
					}
					else
					{
						_refEntity.ForceSetAnimateId((HumanBase.AnimateId)138u);
						_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0_5;
						_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)138u, _refEntity.CurrentFrame);
					}
				}
				if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
				{
					CharacterDirection characterDirection = GetDestFacing();
					if (bWallGrab)
					{
						characterDirection = ((_refEntity.ShootDirection.x > 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
					}
					else
					{
						int num3 = _refEntity.CalculateMoveSpeed();
						_refEntity.SetHorizontalSpeed((int)characterDirection * num3);
					}
					_refEntity._characterDirection = characterDirection;
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
			case OrangeCharacter.SubStatus.SKILL0_7:
				if (!ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
				{
					_refEntity.SetSpeed(0, 0);
					if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2)
					{
						_refEntity.ForceSetAnimateId((HumanBase.AnimateId)137u);
						_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0;
						_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)137u, _refEntity.CurrentFrame);
					}
					else
					{
						_refEntity.ForceSetAnimateId((HumanBase.AnimateId)138u);
						_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0_5;
						_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)138u, _refEntity.CurrentFrame);
					}
				}
				else
				{
					if (!ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
					{
						break;
					}
					CharacterDirection destFacing2 = GetDestFacing();
					int num2 = _refEntity.CalculateMoveSpeed();
					_refEntity.SetHorizontalSpeed((int)destFacing2 * num2);
					_refEntity._characterDirection = destFacing2;
					if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.JUMP))
					{
						_refEntity.SetSpeed(0, OrangeCharacter.JumpSpeed);
						if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0)
						{
							_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0_3;
							_refEntity.ForceSetAnimateId((HumanBase.AnimateId)133u);
							_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)133u, _refEntity.CurrentFrame);
						}
						else
						{
							_refEntity.CurSubStatus = OrangeCharacter.SubStatus.SKILL0_8;
							_refEntity.ForceSetAnimateId((HumanBase.AnimateId)134u);
							_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)134u, _refEntity.CurrentFrame);
						}
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_11:
			case OrangeCharacter.SubStatus.GIGA_ATTACK_START:
				if (_SkillLoopTimer.GetMillisecond() >= _refEntity.GetCurrentSkillObj().FastBulletDatas[2].n_FIRE_SPEED)
				{
					if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_11)
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_12);
					}
					else
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.GIGA_ATTACK_END);
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
			case OrangeCharacter.SubStatus.SKILL1_4:
				if (_SkillLoopTimer.GetMillisecond() >= 1500)
				{
					if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_1)
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
					}
					else
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_5);
					}
				}
				break;
			}
		}
		CheckSkill_Bullet();
	}

	protected CharacterDirection GetDestFacing()
	{
		if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT))
		{
			if (_refEntity.ReverseRightAndLeft)
			{
				return CharacterDirection.RIGHT;
			}
			return CharacterDirection.LEFT;
		}
		if (_refEntity.ReverseRightAndLeft)
		{
			return CharacterDirection.LEFT;
		}
		return CharacterDirection.RIGHT;
	}

	public override void ClearSkill()
	{
		_refEntity.SkillEnd = true;
		bInSkill = false;
		_refEntity.CurrentActiveSkill = -1;
		SkilleStatusId = 0;
		Set_CMX_Weapon(false);
		_refEntity.EnableCurrentWeapon();
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if (subStatus == OrangeCharacter.SubStatus.WIN_POSE)
			{
				Set_CMX_Weapon(true);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId((HumanBase.AnimateId)137u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)135u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)133u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_4:
				_refEntity.SetAnimateId((HumanBase.AnimateId)131u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_5:
				_refEntity.SetAnimateId((HumanBase.AnimateId)138u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_6:
				_refEntity.SetAnimateId((HumanBase.AnimateId)130u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_7:
				_refEntity.SetAnimateId((HumanBase.AnimateId)136u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_8:
				_refEntity.SetAnimateId((HumanBase.AnimateId)134u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_9:
				_refEntity.SetAnimateId((HumanBase.AnimateId)132u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_10:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_11:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_12:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_13:
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.GIGA_ATTACK_START:
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			case OrangeCharacter.SubStatus.GIGA_ATTACK_END:
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_refEntity.SetAnimateId((HumanBase.AnimateId)75u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				_refEntity.SetAnimateId((HumanBase.AnimateId)76u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_16:
			case OrangeCharacter.SubStatus.SKILL0_17:
			case OrangeCharacter.SubStatus.SLASH1_END:
			case OrangeCharacter.SubStatus.SLASH2_END:
			case OrangeCharacter.SubStatus.SLASH3_END:
			case OrangeCharacter.SubStatus.SLASH4_END:
			case OrangeCharacter.SubStatus.SLASH5_END:
			case OrangeCharacter.SubStatus.SKILL0_23:
			case OrangeCharacter.SubStatus.SKILL0_24:
			case OrangeCharacter.SubStatus.SKILL0_25:
			case OrangeCharacter.SubStatus.SKILL0_26:
			case OrangeCharacter.SubStatus.SKILL0_27:
			case OrangeCharacter.SubStatus.SKILL0_28:
			case OrangeCharacter.SubStatus.SKILL0_29:
				break;
			}
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		Vector3 vector = new Vector3(1f, 0.2f, 0f);
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_5:
			_refEntity.Dashing = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			_refEntity.SkillEnd = true;
			Set_CMX_Weapon(false);
			_refEntity.EnableCurrentWeapon();
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_6:
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
			_refEntity.SkillEnd = true;
			Set_CMX_Weapon(false);
			_refEntity.EnableCurrentWeapon();
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
		case OrangeCharacter.SubStatus.SKILL0_7:
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.WALK, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				_refEntity.SetSpeed(0, 0);
			}
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = true;
			Set_CMX_Weapon(false);
			_refEntity.EnableCurrentWeapon();
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
		case OrangeCharacter.SubStatus.SKILL0_8:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = true;
			Set_CMX_Weapon(false);
			_refEntity.EnableCurrentWeapon();
			break;
		case OrangeCharacter.SubStatus.SKILL0_4:
		case OrangeCharacter.SubStatus.SKILL0_9:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = true;
			Set_CMX_Weapon(false);
			_refEntity.EnableCurrentWeapon();
			break;
		case OrangeCharacter.SubStatus.SKILL0_10:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_11);
			CreateSkillBullet(_refEntity.GetCurrentSkillObj());
			_SkillLoopTimer.TimerStart();
			break;
		case OrangeCharacter.SubStatus.SKILL0_12:
			_refEntity.Dashing = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			_refEntity.SkillEnd = true;
			Set_CMX_Weapon(false);
			_SkillLoopTimer.TimerStop();
			_refEntity.EnableCurrentWeapon();
			break;
		case OrangeCharacter.SubStatus.SKILL0_13:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.GIGA_ATTACK_START);
			CreateSkillBullet(_refEntity.GetCurrentSkillObj());
			_SkillLoopTimer.TimerStart();
			break;
		case OrangeCharacter.SubStatus.GIGA_ATTACK_END:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			_SkillLoopTimer.TimerStop();
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = true;
			_refEntity.IgnoreGravity = false;
			Set_CMX_Weapon(false);
			_refEntity.EnableCurrentWeapon();
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_3:
		{
			if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
			}
			_SkillLoopTimer.TimerStart();
			vector.x = (float)_refEntity._characterDirection;
			fx_fullweapon = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_fullweapon_000", _refEntity.AimTransform.position + vector, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			_refEntity.BulletCollider.UpdateBulletData(_refEntity.GetCurrentSkillObj().BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			_refEntity.BulletCollider.SetBulletAtk(_refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = _refEntity.GetCurrentSkillObj().SkillLV;
			Vector2 offset = _refEntity.BulletCollider.GetOffset();
			offset.x *= (float)_refEntity._characterDirection;
			_refEntity.BulletCollider.SetOffset(offset);
			_refEntity.BulletCollider.Active(_refEntity.BulletCollider.transform.position, Vector2.right * (float)_refEntity._characterDirection, _refEntity.TargetMask);
			_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0], Vector3.up);
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1_2:
		case OrangeCharacter.SubStatus.SKILL1_5:
			if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.IgnoreGravity = false;
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			_refEntity.BulletCollider.BackToPool();
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = true;
			_refEntity.EnableCurrentWeapon();
			Set_CMX_Weapon(false);
			_SkillLoopTimer.TimerStop();
			break;
		case OrangeCharacter.SubStatus.SKILL0_11:
		case OrangeCharacter.SubStatus.GIGA_ATTACK_START:
		case OrangeCharacter.SubStatus.SKILL0_16:
		case OrangeCharacter.SubStatus.SKILL0_17:
		case OrangeCharacter.SubStatus.SLASH1_END:
		case OrangeCharacter.SubStatus.SLASH2_END:
		case OrangeCharacter.SubStatus.SLASH3_END:
		case OrangeCharacter.SubStatus.SLASH4_END:
		case OrangeCharacter.SubStatus.SLASH5_END:
		case OrangeCharacter.SubStatus.SKILL0_23:
		case OrangeCharacter.SubStatus.SKILL0_24:
		case OrangeCharacter.SubStatus.SKILL0_25:
		case OrangeCharacter.SubStatus.SKILL0_26:
		case OrangeCharacter.SubStatus.SKILL0_27:
		case OrangeCharacter.SubStatus.SKILL0_28:
		case OrangeCharacter.SubStatus.SKILL0_29:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_4:
			break;
		}
	}

	protected void CheckCombo_PushBulletDetail(WeaponStruct weaponStruct, Transform ShootPosition, int bulletlv = 0, Vector3? ShotDir = null)
	{
		SKILL_TABLE sKILL_TABLE = weaponStruct.FastBulletDatas[0];
		if (sKILL_TABLE.n_COMBO_SKILL == 0)
		{
			return;
		}
		bool flag = false;
		if (sKILL_TABLE.s_COMBO != "null")
		{
			string[] command = sKILL_TABLE.s_COMBO.Split(',');
			flag = _refEntity.CheckCanCombo(command);
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		for (int i = 1; i < weaponStruct.FastBulletDatas.Length; i++)
		{
			if (sKILL_TABLE.n_COMBO_SKILL != weaponStruct.FastBulletDatas[i].n_ID)
			{
				continue;
			}
			weaponStruct.FastBulletDatas[0] = weaponStruct.FastBulletDatas[i];
			weaponStruct.BulletData = weaponStruct.FastBulletDatas[0];
			_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, weaponStruct.weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
			if (weaponStruct.BulletData.s_COMBO == "null")
			{
				weaponStruct.BulletData = Source_Bullet;
				_refEntity.ForceChangeSkillIcon(_refEntity.CurrentActiveSkill + 1, weaponStruct.Icon);
				if (weaponStruct.FastBulletDatas[0].n_RELOAD > 0)
				{
					weaponStruct.Reload_index = i;
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				}
				weaponStruct.FastBulletDatas[0] = weaponStruct.BulletData;
			}
			else if (weaponStruct.FastBulletDatas[0].n_RELOAD > 0)
			{
				weaponStruct.Reload_index = i;
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			}
		}
	}

	private void Skill1_Shoot_first(int id)
	{
		float value = Mathf.Abs(Vector2.SignedAngle(Vector2.down, -_refEntity.ShootDirection)) / 180f;
		_refEntity.Animator._animator.SetFloat(_refEntity.Animator.hashDirection, value);
		bWallGrab = false;
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.IDLE:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			break;
		case OrangeCharacter.MainStatus.CROUCH:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			break;
		case OrangeCharacter.MainStatus.WALK:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
			break;
		case OrangeCharacter.MainStatus.JUMP:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
			break;
		case OrangeCharacter.MainStatus.FALL:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_4);
			break;
		case OrangeCharacter.MainStatus.WALLGRAB:
			bWallGrab = true;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_4);
			break;
		default:
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			break;
		}
	}

	private void Skill1_Shoot_second(int id)
	{
		float value = Mathf.Abs(Vector2.SignedAngle(Vector2.down, -_refEntity.ShootDirection)) / 180f;
		_refEntity.Animator._animator.SetFloat(_refEntity.Animator.hashDirection, value);
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.IDLE:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_5);
			break;
		case OrangeCharacter.MainStatus.CROUCH:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_6);
			break;
		case OrangeCharacter.MainStatus.WALK:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_7);
			break;
		case OrangeCharacter.MainStatus.JUMP:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_8);
			break;
		case OrangeCharacter.MainStatus.FALL:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_9);
			break;
		case OrangeCharacter.MainStatus.WALLGRAB:
			bWallGrab = true;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_9);
			break;
		default:
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
		{
			int currentActiveSkill = _refEntity.CurrentActiveSkill;
			int num = -1;
			break;
		}
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				bInShootBullet = false;
				_refEntity.SetSpeed(0, 0);
				_refEntity.StopShootTimer();
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				_refEntity.DisableCurrentWeapon();
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetSpeed(0, 0);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
				}
				else
				{
					_refEntity.SetSpeed(0, 0);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
					_refEntity.IgnoreGravity = true;
				}
				Set_CMX_Weapon(true);
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if ((int)_refEntity.Hp <= 0)
		{
			return;
		}
		if (id != 0)
		{
			int num2 = 1;
		}
		else
		{
			if (_refEntity.CurrentActiveSkill != -1 || !_refEntity.CheckUseSkillKeyTrigger(id))
			{
				return;
			}
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			bInShootBullet = false;
			_refEntity.StopShootTimer();
			nLastSkillIndex0 = _refEntity.GetCurrentSkillObj().Reload_index;
			int num = Math.Sign(_refEntity.ShootDirection.x);
			if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(_refEntity.ShootDirection.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			}
			if (nLastSkillIndex0 == 0)
			{
				_refEntity.Dashing = false;
				Skill1_Shoot_first(id);
				_refEntity.PlaySE(_refEntity.VoiceID, 7);
			}
			else if (nLastSkillIndex0 == 1)
			{
				_refEntity.Dashing = false;
				Skill1_Shoot_second(id);
				_refEntity.PlaySE(_refEntity.VoiceID, 7);
			}
			else if (nLastSkillIndex0 == 2)
			{
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetSpeed(0, 0);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_10);
				}
				else
				{
					_refEntity.SetSpeed(0, 0);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_13);
					_refEntity.IgnoreGravity = true;
				}
				fx_cheastbeam = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_chestbeam_000", _refEntity.ModelTransform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
				if (_refEntity.GetCurrentSkillObj().BulletData.n_RELOAD > 0)
				{
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				}
			}
			_refEntity.DisableCurrentWeapon();
			Set_CMX_Weapon(true);
		}
	}

	public override void SetStun(bool enable)
	{
		if (fx_fullweapon != null)
		{
			fx_fullweapon.BackToPool();
			fx_fullweapon = null;
		}
		if (fx_cheastbeam != null)
		{
			fx_cheastbeam.BackToPool();
			fx_cheastbeam = null;
		}
		if (enable)
		{
			Set_CMX_Weapon(false);
			_refEntity.EnableCurrentWeapon();
		}
	}
}
