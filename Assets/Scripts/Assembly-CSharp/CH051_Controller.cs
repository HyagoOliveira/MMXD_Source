using System;
using CriWare;
using StageLib;
using UnityEngine;

public class CH051_Controller : CharacterControlBase
{
	private bool bInSkill;

	private bool bInSkillEffect;

	protected ObjInfoBar mEffect_Hide_obj;

	protected SkinnedMeshRenderer LBusterMesh;

	protected SkinnedMeshRenderer RBusterMesh;

	protected SkinnedMeshRenderer LHandMesh;

	protected SkinnedMeshRenderer RHandMesh;

	protected Vector3 Skill0Directioon = Vector3.right;

	protected Vector3 Skill0Position = Vector3.zero;

	[SerializeField]
	protected float Skill0FireSpeed = 0.6f;

	[SerializeField]
	protected float Skill0Distance = 1f;

	protected int Skill0ShootCount;

	protected OrangeTimer Skill0Timer;

	protected ParticleSystem LBusterFireFX;

	protected ParticleSystem RBusterFireFX;

	protected ParticleSystem Skill1ChargeFX;

	private string[] Cues = new string[2] { "BattleSE", "bt_boss05" };

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch051_skill_01_stand_mid", "ch051_skill_01_jump_mid", "ch051_skill_01_crouch_mid", "ch051_skill_02_stand", "ch051_skill_02_jump", "ch051_skill_02_crouch" };
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
		Skill0Timer = OrangeTimerManager.GetTimer();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill1ShootPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_L_m", true);
		LBusterMesh = transform.GetComponent<SkinnedMeshRenderer>();
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_R_m", true);
		RBusterMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_c", true);
		LHandMesh = transform3.GetComponent<SkinnedMeshRenderer>();
		Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_R_c", true);
		RHandMesh = transform4.GetComponent<SkinnedMeshRenderer>();
		Transform transform5 = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_airburst_000_L", true);
		LBusterFireFX = transform5.GetComponentInChildren<ParticleSystem>();
		LBusterFireFX.Stop();
		Transform transform6 = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_airburst_000_R", true);
		RBusterFireFX = transform6.GetComponentInChildren<ParticleSystem>();
		RBusterFireFX.Stop();
		Transform transform7 = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_explosion_000", true);
		Skill1ChargeFX = transform7.GetComponentInChildren<ParticleSystem>();
		Skill1ChargeFX.Stop();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch051_explosion_001", 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		if (base.gameObject.GetComponent<OrangeNPCCharacter>() != null && StageUpdate.gStageName == "stage04_3301_e1")
		{
			_refEntity.TeleportOutCharacterSE = TeleportoutSE;
		}
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.SkillEnd = true;
			LBusterFireFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			RBusterFireFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		else if (_refEntity.CurrentActiveSkill == 1)
		{
			_refEntity.SkillEnd = true;
			_refEntity.Animator._animator.speed = 1f;
			Skill1ChargeFX.Stop();
		}
		_refEntity.CurrentActiveSkill = -1;
		ToggleWeapon(0);
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged())
		{
			return;
		}
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
			if (_refEntity.CurrentFrame > 1f)
			{
				OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
				int num = 21;
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.07f)
			{
				if (Skill0ShootCount < _refEntity.PlayerSkills[0].BulletData.n_NUM_SHOOT && (float)Skill0Timer.GetMillisecond() > (float)Skill0ShootCount * Skill0FireSpeed)
				{
					CreateSkillBullet(_refEntity.PlayerSkills[0]);
				}
			}
			else if (!bInSkill && CheckCancelAnimate(0))
			{
				bool isCrouch3 = _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2;
				CancelSkill0(isCrouch3);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (_refEntity.CurrentFrame > 1f)
			{
				bool isCrouch = _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2;
				SkillEndChnageToIdle(isCrouch);
			}
			else if (bInSkill)
			{
				if (!bInSkillEffect && _refEntity.CurrentFrame > 0.5f)
				{
					bInSkillEffect = true;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch051_explosion_001", Skill1ChargeFX.transform.position, Quaternion.identity, Array.Empty<object>());
				}
				if (_refEntity.CurrentFrame > 0.65f)
				{
					bInSkill = false;
					Skill1ChargeFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
					bInSkillEffect = false;
					CreateSkillBullet(_refEntity.PlayerSkills[1]);
				}
				else if (!Skill1ChargeFX.isPlaying && _refEntity.CurrentFrame > 0.2f)
				{
					Skill1ChargeFX.Play();
					PlaySkillSE("fe_explosion");
				}
			}
			else if (CheckCancelAnimate(1))
			{
				bool isCrouch2 = _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2;
				CancelSkill1(isCrouch2);
			}
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id == 0 && _refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.SkillEnd = false;
			_refEntity.CurrentActiveSkill = id;
			ToggleWeapon(1);
			bInSkill = true;
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.WALK && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.RIDE_ARMOR)
			{
				Skill0Directioon = Vector3.right * (0 - _refEntity._characterDirection);
			}
			else
			{
				Skill0Directioon = Vector3.right * (float)_refEntity._characterDirection;
			}
			_refEntity.IsShoot = 1;
			_refEntity.ShootDirection = Skill0Directioon;
			Skill0Position = _refEntity._transform.position + Skill0Directioon;
			Skill0ShootCount = 0;
			Skill0Timer.TimerStart();
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
			}
			else if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			}
			PlayVoiceSE("v_fe_skill01");
			PlaySkillSE("fe_airburst01");
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id == 1 && _refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.SkillEnd = false;
			_refEntity.CurrentActiveSkill = id;
			ToggleWeapon(2);
			bInSkill = true;
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			}
			else if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			PlayVoiceSE("v_fe_skill02");
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
			{
				PlaySkillSE("fe_start01");
			}
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if (subStatus != 0)
			{
				int num = 1;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				_refEntity.Animator._animator.speed = 1.7f;
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				_refEntity.Animator._animator.speed = 1.7f;
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				_refEntity.Animator._animator.speed = 1.7f;
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SLASH:
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
				if ((bool)mEffect_Hide_obj)
				{
					mEffect_Hide_obj.gameObject.SetActive(true);
				}
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SkillEndChnageToIdle(true);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				SkillEndChnageToIdle(true);
				break;
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
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
		{
			_refEntity.IsShoot = 1;
			Vector3 shootPosition = Skill0Position + Skill0Directioon * (Skill0Distance + (float)Skill0ShootCount * wsSkill.BulletData.f_DISTANCE);
			_refEntity.ShootDirection = Skill0Directioon;
			_refEntity._characterDirection = ((Skill0Directioon.x > 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
			_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, shootPosition, wsSkill.SkillLV, (float)_refEntity._characterDirection * Vector3.right);
			if (Skill0ShootCount == 0)
			{
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
			}
			Skill0ShootCount++;
			if (Skill0ShootCount >= _refEntity.PlayerSkills[0].BulletData.n_NUM_SHOOT)
			{
				bInSkill = false;
			}
			LBusterFireFX.Simulate(0f);
			LBusterFireFX.Play();
			RBusterFireFX.Simulate(0f);
			RBusterFireFX.Play();
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.IsShoot = 1;
			_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[2], wsSkill.SkillLV);
			_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ExtraTransforms[2]);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			break;
		}
	}

	public CriAtomExPlayback TeleportoutSE()
	{
		return _refEntity.SoundSource.PlaySE(Cues[0], Cues[1]);
	}

	public void StageTeleportInCharacterDepend()
	{
		Skill1ChargeFX.Stop();
	}

	public void TeleportInExtraEffect()
	{
		mEffect_Hide_obj = _refEntity.transform.GetComponentInChildren<ObjInfoBar>();
		if ((bool)mEffect_Hide_obj)
		{
			mEffect_Hide_obj.gameObject.SetActive(false);
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
		PlaySkillSE("fe_start01");
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ch051_startin_000";
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 19) <= 2u)
		{
			_refEntity._characterDirection = ((Skill0Directioon.x >= 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
		}
		else
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	private void ShootSkill0()
	{
	}

	private void ShootSkill1()
	{
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		switch (skilliD)
		{
		case 0:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
			break;
		case 1:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private void CancelSkill0(bool isCrouch)
	{
		LBusterFireFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		RBusterFireFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		SkillEndChnageToIdle(isCrouch);
	}

	private void CancelSkill1(bool isCrouch)
	{
		Skill1ChargeFX.Stop();
		SkillEndChnageToIdle(isCrouch);
	}

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.Animator._animator.speed = 1f;
		bInSkill = false;
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 19) <= 2u)
			{
				_refEntity.ShootDirection = ((_refEntity._characterDirection == CharacterDirection.RIGHT) ? Vector3.right : Vector3.left);
			}
		}
		ToggleWeapon(0);
		if (isCrouch)
		{
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -1:
			_refEntity.DisableCurrentWeapon();
			LBusterMesh.enabled = false;
			RBusterMesh.enabled = false;
			LHandMesh.enabled = true;
			RHandMesh.enabled = true;
			break;
		case 1:
			_refEntity.DisableCurrentWeapon();
			LBusterMesh.enabled = true;
			RBusterMesh.enabled = true;
			LHandMesh.enabled = false;
			RHandMesh.enabled = false;
			break;
		case 2:
			_refEntity.DisableCurrentWeapon();
			RBusterMesh.enabled = false;
			RHandMesh.enabled = true;
			break;
		default:
			_refEntity.EnableCurrentWeapon();
			LBusterMesh.enabled = false;
			RBusterMesh.enabled = false;
			LHandMesh.enabled = true;
			RHandMesh.enabled = true;
			break;
		}
	}
}
