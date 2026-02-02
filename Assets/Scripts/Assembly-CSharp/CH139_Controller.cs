using System;
using System.Collections;
using UnityEngine;

public class CH139_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private Transform _tfWind;

	private FxBase fxUseSkl0;

	private FxBase fxUseSkl1;

	private SKILL_TABLE linkSkl0;

	private SKILL_TABLE linkSkl0_2;

	private CharacterMaterial busterCM;

	private readonly int skl1StartAngle = 45;

	private readonly float skl1Offset = 2f;

	private readonly int SKL1_TRIGGER = 1;

	private readonly int SKL1_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly string FX_001 = "fxuse_FormatWorld_000";

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = transform;
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[1];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_tfWind = OrangeBattleUtility.FindChildRecursive(ref target, "Fx_ICOWing_01", true);
		GameObject gameObject = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m", true).gameObject;
		if ((bool)gameObject)
		{
			busterCM = gameObject.GetComponent<CharacterMaterial>();
			busterCM.Appear();
		}
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<BulletBase>(_refEntity, 0, out linkSkl0);
		if (linkSkl0 != null && linkSkl0.n_LINK_SKILL != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(linkSkl0.n_LINK_SKILL, out linkSkl0_2))
		{
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref linkSkl0_2);
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<BulletBase>("prefab/bullet/" + linkSkl0_2.s_MODEL, linkSkl0_2.s_MODEL, 3, null);
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_001, 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.PlayTeleportOutEffectEvt = PlayTeleportOutEffect;
	}

	public void TeleportInCharacterDepend()
	{
		_tfWind.gameObject.SetActive(true);
		if (_refEntity.CurrentFrame >= 0.9f)
		{
			UpdateCustomWeaponRenderer(false);
		}
	}

	private void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 1.4f && currentFrame <= 1.8f)
			{
				ToggleWing(false);
			}
		}
	}

	public override void ControlCharacterDead()
	{
		ToggleWing(false);
	}

	public override void ControlCharacterContinue()
	{
		StartCoroutine(OnToggleWing(true, 0.6f));
	}

	protected void StageTeleportInCharacterDepend()
	{
		if (_tfWind != null && _tfWind.gameObject.activeSelf)
		{
			StopAllCoroutines();
			return;
		}
		ToggleWing(false);
		StopAllCoroutines();
		StartCoroutine(OnToggleWing(true, 0.6f));
	}

	protected void StageTeleportOutCharacterDepend()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT)
		{
			StartCoroutine(OnToggleWing(false, 0.2f));
		}
		else if (!_refEntity.Animator.IsDefaultAnimator)
		{
			StartCoroutine(OnToggleWing(false, 0.2f));
		}
	}

	private IEnumerator OnToggleWing(bool isActive, float delay)
	{
		yield return new WaitForSeconds(delay);
		ToggleWing(isActive);
	}

	private void ToggleWing(bool isActive)
	{
		_tfWind.gameObject.SetActive(isActive);
	}

	private void PlayTeleportOutEffect()
	{
		Vector3 p_worldPos = base.transform.position;
		if (_refEntity != null)
		{
			p_worldPos = _refEntity.AimPosition;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_TELEPORT_OUT", p_worldPos, Quaternion.identity, Array.Empty<object>());
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
			UpdateCustomWeaponRenderer(true);
			_refEntity.CurrentActiveSkill = id;
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
			_refEntity.CheckUsePassiveSkill(1, weaponStruct.BulletData, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
			_refEntity.IsShoot = 1;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
			fxUseSkl1 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_001, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			PlayVoiceSE("v_ic_skill01");
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			Vector3 shootDirection = _refEntity.ShootDirection;
			_refEntity.CurrentActiveSkill = id;
			_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, _refEntity.PlayerSkills[id].ChargeLevel, shootDirection, true, false);
			_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0]);
			_refEntity.Animator.SetAnimatorEquip(1);
			UpdateCustomWeaponRenderer(true);
			PlayVoiceSE("v_ic_skill04");
			PlaySkillSE("ic_lazer01");
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0 && _refEntity.CheckSkillEndByShootTimer())
		{
			UpdateCustomWeaponRenderer(false);
		}
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		nowFrame = GameLogicUpdateManager.GameFrame;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if (curSubStatus == OrangeCharacter.SubStatus.SKILL1)
		{
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, weaponStruct.ShootTransform[0], MagazineType.NORMAL, -1, 1, false);
				isSkillEventEnd = true;
				PlaySkillSE("ic_delete01");
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
		}
	}

	private void PushLinkSkl(SKILL_TABLE bulletData, Vector3 shootPosition, Vector3? ShotDir = null)
	{
		WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
		_refEntity.PushBulletDetail(bulletData, currentSkillObj.weaponStatus, shootPosition, currentSkillObj.SkillLV, ShotDir);
	}

	private void OnSkillEnd()
	{
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		fxUseSkl0 = null;
		fxUseSkl1 = null;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		UpdateCustomWeaponRenderer(false);
		_refEntity.EnableCurrentWeapon();
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != HumanBase.AnimateId.ANI_SKILL_START)
		{
			if (_refEntity.IsInGround)
			{
				_refEntity.Dashing = false;
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
		}
		else
		{
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
		}
	}

	public override void ClearSkill()
	{
		if (fxUseSkl0 != null)
		{
			fxUseSkl0.BackToPool();
		}
		if (fxUseSkl1 != null)
		{
			fxUseSkl1.BackToPool();
		}
		fxUseSkl0 = null;
		fxUseSkl1 = null;
		UpdateCustomWeaponRenderer(false);
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		UpdateCustomWeaponRenderer(false);
		_refEntity.EnableCurrentWeapon();
	}

	private void UpdateCustomWeaponRenderer(bool enableWeapon)
	{
		if (enableWeapon)
		{
			busterCM.Appear(null, 0f);
		}
		else
		{
			busterCM.Disappear(null, 0f);
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch139_skill_02_crouch", "ch139_skill_02_stand", "ch139_skill_02_jump" };
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}
}
