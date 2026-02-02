using System;
using UnityEngine;

public class CH108_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private FxBase fxuse_skl001;

	protected PlayerAutoAimSystem _pSkill0AimSystem;

	protected IAimTarget _pSkill0Target;

	protected bool bInSkill;

	protected bool _bOldSkill0Flag;

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly string sFxuse_skl000 = "fxuse_loverico_000";

	private readonly string sFxuse_skl001 = "fxuse_loverico_001";

	private readonly int SKL_TRIGGER = (int)(0.24f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_END = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_END_BREAK = (int)(0.28f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	protected virtual void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[1];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		SKILL_TABLE linkSkl = null;
		for (int i = 0; i < _refEntity.PlayerSkills.Length; i++)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[i];
			if (weaponStruct != null && weaponStruct.BulletData.n_LINK_SKILL != 0 && linkSkl == null && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out linkSkl))
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/bullet/" + linkSkl.s_MODEL, linkSkl.s_MODEL, delegate(GameObject obj)
				{
					BulletBase component = obj.GetComponent<BulletBase>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BulletBase>(UnityEngine.Object.Instantiate(component), linkSkl.s_MODEL, 5);
				});
				break;
			}
		}
		InitSkl0AimSystem();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl000);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl001);
		if (_refEntity.PlayerWeapons[0] != null && _refEntity.PlayerWeapons[0].ChipEfx != null)
		{
			_refEntity.PlayerWeapons[0].ChipEfx.MeshActiveColor /= 2f;
		}
		if (_refEntity.PlayerWeapons[1] != null && _refEntity.PlayerWeapons[1].ChipEfx != null)
		{
			_refEntity.PlayerWeapons[1].ChipEfx.MeshActiveColor /= 2f;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UseSkill0(0);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			PlayVoiceSE("v_ri_skill03");
			_refEntity.CurrentActiveSkill = id;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL_TRIGGER, SKL_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
			fxuse_skl001 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(sFxuse_skl001, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		nowFrame = GameLogicUpdateManager.GameFrame;
		if (!_refEntity.IsLocalPlayer && !bInSkill)
		{
			bool flag = _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(LineLinkBullet.nFlagBuffLineLink);
			if (flag && !_bOldSkill0Flag)
			{
				DoSkill0(0);
				_bOldSkill0Flag = flag;
				return;
			}
			_bOldSkill0Flag = flag;
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[0], MagazineType.ENERGY, -1, 1);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		}
	}

	private void OnSkillEnd()
	{
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		bInSkill = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		_pSkill0Target = null;
		switch (_refEntity.AnimateID)
		{
		default:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			break;
		case (HumanBase.AnimateId)66u:
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case HumanBase.AnimateId.ANI_SKILL_START:
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		_refEntity.EnableCurrentWeapon();
		isSkillEventEnd = false;
		bInSkill = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		if (fxuse_skl001 != null)
		{
			fxuse_skl001.BackToPool();
		}
		fxuse_skl001 = null;
		_pSkill0Target = null;
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			_refEntity.EnableCurrentWeapon();
		}
	}

	private void InitSkl0AimSystem()
	{
		GameObject gameObject = new GameObject("Skill0AutoAimSystem");
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localPosition = Vector3.zero;
		_pSkill0AimSystem = gameObject.AddOrGetComponent<PlayerAutoAimSystem>();
		_pSkill0AimSystem.targetMask = _refEntity.PlayerAutoAimSystem.targetMask;
		_pSkill0AimSystem.Init(false, _refEntity.IsLocalPlayer);
		_pSkill0AimSystem.UpdateAimRange(_refEntity.PlayerSkills[0].BulletData.f_DISTANCE);
	}

	private void FindSkill0Target()
	{
		_pSkill0Target = null;
		if (_refEntity.PlayerAutoAimSystem.AutoAimTarget != null && Vector2.Distance(_refEntity.PlayerAutoAimSystem.AutoAimTarget.AimPosition, _refEntity.AimPosition) < _refEntity.PlayerSkills[0].BulletData.f_DISTANCE)
		{
			_pSkill0Target = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
		}
		if (_pSkill0Target == null)
		{
			_pSkill0Target = _pSkill0AimSystem.GetClosestTarget();
		}
	}

	private void UseSkill0(int skillId)
	{
		if (!_refEntity.IsLocalPlayer)
		{
			PlayVoiceSE("v_ri_skill04");
			return;
		}
		FindSkill0Target();
		if (_pSkill0Target != null)
		{
			PlayVoiceSE("v_ri_skill04");
			DoSkill0(skillId);
		}
	}

	private void DoSkill0(int skillId)
	{
		isSkillEventEnd = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		if (_pSkill0Target != null)
		{
			Vector3 normalized = (_pSkill0Target.AimPosition - _refEntity.AimPosition).normalized;
			int num = Math.Sign(normalized.x);
			if (_refEntity.direction != num && Mathf.Abs(normalized.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = normalized;
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse_skl000, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[skillId];
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.AimTransform, _pSkill0Target, weaponStruct.SkillLV);
		_refEntity.CheckUsePassiveSkill(skillId, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		endBreakFrame = GameLogicUpdateManager.GameFrame + SKL_END_BREAK;
		ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, skillId, SKL_TRIGGER, SKL_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
		ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch108_skill_01_crouch", "ch108_skill_01_stand", "ch108_skill_01_jump" };
	}
}
