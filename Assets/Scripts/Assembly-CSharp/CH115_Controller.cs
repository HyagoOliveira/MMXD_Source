using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CH115_Controller : CharacterControllerProxyBaseGen4
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL2_START = 65u,
		ANI_SKILL2_LOOP = 66u,
		ANI_SKILL2_END = 67u
	}

	private enum SkillBlendAnimationId : uint
	{
		ANI_SKILL1_STAND_START = 127u,
		ANI_SKILL1_STAND_LOOP = 128u,
		ANI_SKILL1_STAND_END = 129u,
		ANI_SKILL1_CROUCH_START = 130u,
		ANI_SKILL1_CROUCH_LOOP = 131u,
		ANI_SKILL1_CROUCH_END = 132u,
		ANI_SKILL1_JUMP_START = 133u,
		ANI_SKILL1_JUMP_LOOP = 134u,
		ANI_SKILL1_JUMP_END = 135u
	}

	private enum FxName
	{
		fxuse_icedragon_001 = 0,
		fxuse_icedragon_002 = 1,
		fxuse_flamedragon_001 = 2,
		fxuse_ch115_skill1_000 = 3,
		fxuse_ch115_skill2_000 = 4
	}

	public float TIME_SKILL_0_LOOP = 0.5f;

	public float TIME_SKILL_0_CANCEL = 0.05f;

	public float OFFSET_SKILL_0_Y_STAND = 1f;

	public float OFFSET_SKILL_0_Y_CROUCH = 0.75f;

	public float OFFSET_SKILL_0_Y_JUMP = 1.2f;

	public float OFFSET_SKILL_0_RADIUS = 1.5f;

	public float FX_SKILL_0_VORTEX_SCALE = 1.5f;

	public float FX_SKILL_0_ICE_SCALE = 2f;

	public float TIME_SKILL_1_LOOP = 0.05f;

	public float TIME_SKILL_1_CANCEL = 0.2f;

	public Vector2 JUMP_SPEED_SKILL_1_LOOP = new Vector2(2f, 2f);

	public Vector2 JUMP_SPEED_SKILL_1_END = new Vector2(1f, 1f);

	private SKILL_TABLE _skillTableLinkSkill0;

	private SKILL_TABLE _skillTableLinkSkill1;

	private bool _canTriggerLinkSkill;

	private Vector3 _shootDirection;

	private float _shootPointY;

	private void PreloadSkillPoolObject()
	{
		string text = "prefab/bullet/";
		SKILL_TABLE sKILL_TABLE = _refEntity.PlayerSkills[0].BulletData;
		while (sKILL_TABLE.n_LINK_SKILL > 0)
		{
			sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[sKILL_TABLE.n_LINK_SKILL];
			if (!sKILL_TABLE.s_MODEL.IsNullString())
			{
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<PoolBaseObject>(text + sKILL_TABLE.s_MODEL, sKILL_TABLE.s_MODEL, 10, null);
			}
			_skillTableLinkSkill0 = sKILL_TABLE;
		}
		sKILL_TABLE = _refEntity.PlayerSkills[1].BulletData;
		while (sKILL_TABLE.n_LINK_SKILL > 0)
		{
			sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[sKILL_TABLE.n_LINK_SKILL];
			if (!sKILL_TABLE.s_MODEL.IsNullString())
			{
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<PoolBaseObject>(text + sKILL_TABLE.s_MODEL, sKILL_TABLE.s_MODEL, 5, null);
			}
			_skillTableLinkSkill1 = sKILL_TABLE;
		}
	}

	private bool CheckTriggerLinkSkill(SKILL_TABLE linkSkillTable)
	{
		if (linkSkillTable != null && linkSkillTable.n_MAGAZINE_TYPE == 2 && _refEntity.BuffManager.nMeasureNow >= linkSkillTable.n_USE_COST)
		{
			return true;
		}
		return false;
	}

	private void TriggerLinkSkill(SKILL_TABLE skillTable, WeaponStruct weaponStruct, Transform bulletTransform, Vector3? shootDirection = null)
	{
		_refEntity.BuffManager.AddMeasure(-skillTable.n_USE_COST);
		_refEntity.tRefPassiveskill.ReCalcuSkill(ref skillTable);
		_refEntity.PushBulletDetail(skillTable, weaponStruct.weaponStatus, bulletTransform, weaponStruct.SkillLV, shootDirection);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, skillTable, weaponStruct.weaponStatus, _refEntity.ModelTransform);
	}

	private void ActionStatusChanged_0_0()
	{
		ResetSpeed();
		SetIgnoreGravity();
		PlayVoiceSE("v_ri2_skill01");
		PlaySkillSE("ri2_ice01");
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_shootDirection = _refEntity.ShootDirection;
		_refEntity.direction = ((_shootDirection.x != 0f) ? Math.Sign(_shootDirection.x) : _refEntity.direction);
		if (_refEntity.IsInGround)
		{
			if (_refEntity.IsCrouching)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)130u);
				_shootPointY = OFFSET_SKILL_0_Y_CROUCH;
			}
			else
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				_shootPointY = OFFSET_SKILL_0_Y_STAND;
			}
		}
		else
		{
			_refEntity.SetAnimateId((HumanBase.AnimateId)133u);
			_shootPointY = OFFSET_SKILL_0_Y_JUMP;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_ch115_skill1_000.ToString(), new Vector3(base.transform.position.x, base.transform.position.y, (_refEntity._characterDirection == CharacterDirection.RIGHT) ? 0f : 1f), (_refEntity._characterDirection == CharacterDirection.RIGHT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
		MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FxName.fxuse_icedragon_001.ToString(), _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>()).transform.localScale = Vector3.one * FX_SKILL_0_VORTEX_SCALE;
	}

	private void ActionStatusChanged_0_1()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity.ModelTransform.position + new Vector3(0f, _shootPointY) + OFFSET_SKILL_0_RADIUS * _shootDirection, weaponStruct.SkillLV, _shootDirection);
		if (_canTriggerLinkSkill && _skillTableLinkSkill0 != null)
		{
			TriggerLinkSkill(_skillTableLinkSkill0, weaponStruct, _refEntity.ModelTransform);
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FxName.fxuse_icedragon_002.ToString(), _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>()).transform.localScale = Vector3.one * FX_SKILL_0_ICE_SCALE;
		}
		_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
		SetSkillFrame(TIME_SKILL_0_LOOP);
	}

	private void ActionLogicUpdate_0_1()
	{
		_refEntity.Animator.SetAttackLayerActive(_shootDirection);
		ActionCheckNextSkillStatus();
	}

	private void ActionStatusChanged_0_2()
	{
		_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
		SetSkillCancelFrame(TIME_SKILL_0_CANCEL);
	}

	private void ActionStatusChanged_1_0()
	{
		ResetSpeed();
		SetIgnoreGravity(false);
		PlayVoiceSE("v_ri2_skill02");
		ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.BulletCollider.UpdateBulletData(weaponStruct.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
		_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
		_refEntity.BulletCollider.Active(_refEntity.TargetMask);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_ch115_skill2_000.ToString(), new Vector3(base.transform.position.x, base.transform.position.y, (_refEntity._characterDirection == CharacterDirection.RIGHT) ? 0f : 1f), (_refEntity._characterDirection == CharacterDirection.RIGHT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
		_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
	}

	private void ActionStatusChanged_1_1()
	{
		SetSpeed((float)OrangeCharacter.WalkSpeed * JUMP_SPEED_SKILL_1_LOOP.x * (float)_refEntity.direction, (float)OrangeCharacter.JumpSpeed * JUMP_SPEED_SKILL_1_LOOP.y);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_flamedragon_001.ToString(), _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
		SetSkillFrame(TIME_SKILL_1_LOOP);
	}

	private void ActionStatusChanged_1_2()
	{
		_refEntity.IgnoreGravity = false;
		SetSpeed((float)OrangeCharacter.WalkSpeed * JUMP_SPEED_SKILL_1_END.x * (float)_refEntity.direction, (float)OrangeCharacter.JumpSpeed * JUMP_SPEED_SKILL_1_END.y);
		if (_canTriggerLinkSkill && _skillTableLinkSkill1 != null)
		{
			PlaySkillSE("ri2_fire02");
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
			TriggerLinkSkill(_skillTableLinkSkill1, weaponStruct, _refEntity.ExtraTransforms[0], Vector3.up);
		}
		_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
		SetSkillCancelFrame(TIME_SKILL_1_CANCEL);
	}

	public override void Awake()
	{
		base.Awake();
		_refEntity.ExtraTransforms = new Transform[2]
		{
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "L WeaponPoint", true),
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "R WeaponPoint", true)
		};
	}

	public override void Start()
	{
		base.Start();
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);
		});
		PreloadSkillPoolObject();
		InitializeSkillDependDelegators(new Dictionary<OrangeCharacter.SubStatus, SkillStateDelegateData>
		{
			{
				OrangeCharacter.SubStatus.SKILL0,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_0,
					OnAnimationEnd = base.ActionSetNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_1,
					OnLogicUpdate = ActionLogicUpdate_0_1
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_2,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_2,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_0,
					OnAnimationEnd = base.ActionSetNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1_1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_1,
					OnLogicUpdate = base.ActionCheckNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1_2,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_2,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			}
		});
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch115_skill_02_start", "ch115_skill_02_loop", "ch115_skill_02_end" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string prefix = "ch115_skill_01";
		string[] source = new string[3] { "stand", "crouch", "jump" };
		string[] directions = new string[3] { "up", "mid", "down" };
		string[] steps = new string[3] { "start", "loop", "end" };
		return source.SelectMany((string action) => steps.Select((string step) => directions.Select((string direction) => prefix + "_" + action + "_" + direction + "_" + step).ToArray()).ToArray()).ToArray();
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "login", "logout", "win" };
		target = new string[3] { "ch115_login", "ch115_logout", "ch115_win" };
	}

	protected override void TeleportInCharacterDepend()
	{
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID == HumanBase.AnimateId.ANI_TELEPORT_IN_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
		}
	}

	protected override void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
		}
	}

	protected override void StageTeleportInCharacterDepend()
	{
	}

	protected override void StageTeleportOutCharacterDepend()
	{
	}

	public override void ControlCharacterContinue()
	{
	}

	protected override void ToggleExtraTransforms(bool isActive)
	{
	}

	protected override bool OnEnterRideArmor(RideBaseObj targetRideArmor)
	{
		return base.OnEnterRideArmor(targetRideArmor);
	}

	protected override void ToggleWeapon(WeaponState weaponState)
	{
		if ((uint)weaponState <= 1u || (uint)(weaponState - 3) <= 1u)
		{
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(true);
		}
		else
		{
			ToggleNormalWeapon(true);
		}
	}

	protected override void AttachSkillDelegateEvent()
	{
		OnPlayerReleaseSkill0Events[0] = OnPlayerReleaseSkill0;
		OnPlayerPressSkill1Events[0] = OnPlayerPressSkill1;
	}

	protected override void OnPlayerReleaseSkill0(SkillID skillID)
	{
		if (CheckTriggerLinkSkill(_skillTableLinkSkill0))
		{
			_canTriggerLinkSkill = true;
		}
		base.OnPlayerReleaseSkill0(skillID);
	}

	protected override void OnPlayerPressSkill1(SkillID skillID)
	{
		if (CheckTriggerLinkSkill(_skillTableLinkSkill1))
		{
			_canTriggerLinkSkill = true;
		}
		base.OnPlayerPressSkill1(skillID);
	}

	protected override void SetSkillEnd()
	{
		_canTriggerLinkSkill = false;
		base.SetSkillEnd();
	}
}
