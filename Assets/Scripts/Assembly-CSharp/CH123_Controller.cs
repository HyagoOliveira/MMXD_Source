using System;
using System.Collections;
using UnityEngine;

public class CH123_Controller : CharacterControlBase, ILogicUpdate
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	protected IAimTarget _pRushTarget;

	protected Transform _tfHitTransform;

	protected Vector3 _vSkillStartPosition;

	protected Vector2 _vSkillVelocity;

	protected RushCollideBullet _pRushCollideBullet;

	protected bool _bSyncRushSkillCompleted;

	protected Vector3 _vSyncDirection;

	private int fxStopFrame;

	private SKILL_TABLE linkSkl0;

	private SKILL_TABLE linkSkl1;

	private FxBase fxSkl0;

	private FxBase fxSkl1;

	private CharacterMaterial weaponSwordL;

	private CharacterMaterial weaponSwordR;

	private SkinnedMeshRenderer spMeshRenderer;

	private ParticleSystem _wingEffect;

	private bool isInit;

	private OrangeConsoleCharacter _refPlayer;

	private readonly string SpWeaponMesh = "Saber_008_G_L";

	private readonly string SpWeaponMesh2 = "Saber_008_G_R";

	private readonly string SpMesh = "ch_123_wing";

	private readonly string FX_0_00 = "fxuse_flamebreak_000";

	private readonly string FX_0_01 = "fxuse_flamebreak_001";

	private readonly string FX_0_02 = "fxuse_flamebreak_002";

	private readonly string FX_1_00 = "fxuse_waterslash_000";

	protected readonly int SKL0_START = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_LOOP = (int)(0.35f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_END_TRIGGER = (int)(0.38f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_END_BREAK = (int)(0.55f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_TRIGGER_1ST = (int)(0.14f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_TRIGGER_2ND = (int)(0.26f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END = (int)(0.833f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END_BREAK = (int)(0.52f / GameLogicUpdateManager.m_fFrameLen);

	private void OnEnable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	private void InitializeSkill()
	{
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[1];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[2];
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, SpWeaponMesh, true);
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, SpWeaponMesh2, true);
		weaponSwordL = transform2.GetComponent<CharacterMaterial>();
		weaponSwordR = transform3.GetComponent<CharacterMaterial>();
		ToggleSkillWeapon(false);
		Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref target, SpMesh, true);
		spMeshRenderer = transform4.GetComponent<SkinnedMeshRenderer>();
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<CollideBullet>(_refEntity, 0, out linkSkl0);
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<BasicBullet>(_refEntity, 1, out linkSkl1);
		_refPlayer = _refEntity as OrangeConsoleCharacter;
		_wingEffect = OrangeBattleUtility.FindChildRecursive(ref target, "CH062_WingEffect").GetComponent<ParticleSystem>();
		_wingEffect.Play(true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_01, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_02, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_00, 2);
		isInit = true;
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
	}

	private void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 1.5f && currentFrame <= 2f)
			{
				ToggleWing(false);
			}
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			PlayVoiceSE("v_zd_skill02");
			PlaySkillSE("zd_zanrin");
			_refEntity.CurrentActiveSkill = id;
			ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER_1ST, SKL1_TRIGGER_1ST, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)72u, (HumanBase.AnimateId)73u, (HumanBase.AnimateId)74u);
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
			ToggleSkillWeapon(true);
			fxSkl1 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_1_00, _refEntity.AimTransform.position, (_refEntity.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1 || id != 0 || !_refEntity.CheckUseSkillKeyTrigger(id))
		{
			return;
		}
		PlayVoiceSE("v_zd_skill01");
		PlaySkillSE("zd_dragon01");
		_refEntity.CurrentActiveSkill = id;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		_bSyncRushSkillCompleted = false;
		_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
		if (_refEntity.IsLocalPlayer)
		{
			if (_refEntity.UseAutoAim && _refEntity.PlayerAutoAimSystem.AutoAimTarget != null)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				_pRushTarget = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
				_vSkillVelocity = (_pRushTarget.AimPosition - _refEntity.AimPosition).normalized * GetRushSpd();
				UpdateSkill1Direction(_vSkillVelocity.x);
			}
			else
			{
				_pRushTarget = null;
				_vSkillVelocity = _refEntity.ShootDirection.normalized * GetRushSpd();
				UpdateSkill1Direction(_vSkillVelocity.x);
			}
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
			_refEntity.CheckUsePassiveSkill(0, weaponStruct.BulletData, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_0_02, _refEntity.AimTransform, Quaternion.identity, Array.Empty<object>());
		isSkillEventEnd = false;
		_vSkillStartPosition = _refEntity.AimPosition;
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
		_refEntity.IgnoreGravity = true;
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged())
		{
			return;
		}
		UpdateVirtualButtonAnalog();
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		nowFrame = GameLogicUpdateManager.GameFrame;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (_refEntity.CurrentFrame > 1f)
			{
				bool flag = false;
				if (_refEntity.IsLocalPlayer)
				{
					flag = true;
				}
				else if (_bSyncRushSkillCompleted)
				{
					_bSyncRushSkillCompleted = false;
					flag = true;
				}
				if (flag)
				{
					WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
					_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.AimTransform, weaponStruct.SkillLV, _vSkillVelocity, true, null, RushSkillHitCB);
					_vSkillVelocity = _vSkillVelocity.normalized * GetRushSpd();
					UpdateSkill1Direction(_vSkillVelocity.x);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
					_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
					_refEntity.IgnoreGravity = true;
					_refEntity.SetSpeed((int)_vSkillVelocity.x, (int)_vSkillVelocity.y);
					endFrame = GameLogicUpdateManager.GameFrame + SKL0_START;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_0_00, _refEntity.AimTransform.position, (_refEntity.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
					fxSkl0 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_0_01, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
					fxSkl0.transform.localScale = new Vector3(1f * _refEntity.ModelTransform.localScale.z, 1f, 1f);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
		{
			bool flag2 = false;
			if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
			{
				flag2 = true;
				Vector3 vector = _refEntity.AimPosition + new Vector3(1.5f, 0f, 0f) * _refEntity.direction;
				if (_refEntity.IsLocalPlayer && _tfHitTransform != null)
				{
					Vector3 position = _tfHitTransform.position;
				}
				else if (_pRushTarget != null)
				{
					Vector3 aimPosition = _pRushTarget.AimPosition;
				}
			}
			else if (Vector2.Distance(_vSkillStartPosition, _refEntity.AimPosition) > _refEntity.PlayerSkills[0].BulletData.f_DISTANCE || _refEntity.PlayerSkills[0].LastUseTimer.GetMillisecond() > 350)
			{
				flag2 = true;
				_pRushTarget = null;
			}
			if (flag2)
			{
				if (!_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetSpeed(_refEntity.direction * OrangeCharacter.WalkSpeed, 0);
				}
				else
				{
					_refEntity.SetSpeed(0, 0);
				}
				RecoverRushCollideBullet();
				if (linkSkl0 != null)
				{
					isSkillEventEnd = false;
					fxStopFrame = GameLogicUpdateManager.GameFrame + 1;
					endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
					PlaySkillSE("zd_dragon02");
					ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, 0, SKL0_END_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0_2, out skillEventFrame, out endFrame);
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u, (HumanBase.AnimateId)71u);
				}
				else
				{
					OnSkillEnd();
				}
			}
			break;
		}
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (nowFrame == fxStopFrame && fxSkl0 != null && !fxSkl0.IsEnd)
			{
				fxSkl0.pPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
				fxSkl0 = null;
			}
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				PushLinkSkl(linkSkl0, _refEntity.ModelTransform, true);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				isSkillEventEnd = false;
				int p_sklTriggerFrame = SKL1_TRIGGER_2ND - SKL1_TRIGGER_1ST;
				int p_endFrame = SKL1_END - SKL1_TRIGGER_1ST;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[2], MagazineType.ENERGY, -1, 1);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, p_sklTriggerFrame, p_endFrame, OrangeCharacter.SubStatus.SKILL1_1, out skillEventFrame, out endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				PushLinkSkl(linkSkl1, _refEntity.ExtraTransforms[2], true);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		}
	}

	private void PushLinkSkl(SKILL_TABLE bulletData, Transform shootTransform, bool triggerPassiveSkl)
	{
		WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
		_refEntity.PushBulletDetail(bulletData, currentSkillObj.weaponStatus, shootTransform, currentSkillObj.SkillLV);
		if (triggerPassiveSkl)
		{
			_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, bulletData, currentSkillObj.weaponStatus, shootTransform);
		}
	}

	private void OnSkillEnd()
	{
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		fxSkl1 = null;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		ToggleSkillWeapon(false);
		RecoverRushCollideBullet(true);
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != HumanBase.AnimateId.ANI_SKILL_START && animateID != (HumanBase.AnimateId)69u && animateID != (HumanBase.AnimateId)72u)
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
		RecoverRushCollideBullet(true);
		_refEntity.CancelBusterChargeAtk();
		fxSkl1 = null;
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		ToggleSkillWeapon(false);
		_refEntity.EnableCurrentWeapon();
		if (_refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		if ((bool)fxSkl1)
		{
			fxSkl1.BackToPool();
		}
		ToggleSkillWeapon(false);
		_refEntity.EnableCurrentWeapon();
		RecoverRushCollideBullet(true);
	}

	private void ToggleSkillWeapon(bool enable)
	{
		if (enable)
		{
			if ((bool)weaponSwordL)
			{
				weaponSwordL.Appear(null, 0f);
			}
			if ((bool)weaponSwordR)
			{
				weaponSwordR.Appear(null, 0f);
			}
		}
		else
		{
			if ((bool)weaponSwordL)
			{
				weaponSwordL.Disappear(null, 0f);
			}
			if ((bool)weaponSwordR)
			{
				weaponSwordR.Disappear(null, 0f);
			}
		}
	}

	public override void ControlCharacterDead()
	{
		ToggleWing(false);
		ToggleSkillWeapon(false);
	}

	public override void ControlCharacterContinue()
	{
		StartCoroutine(OnToggleWing(true, 0.6f));
	}

	protected void StageTeleportInCharacterDepend()
	{
		if (spMeshRenderer != null && spMeshRenderer.enabled)
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
		spMeshRenderer.enabled = isActive;
		if (isActive)
		{
			_wingEffect.Play(true);
		}
		else
		{
			_wingEffect.Stop(true);
		}
	}

	private float GetRushSpd()
	{
		return (float)OrangeCharacter.DashSpeed * 4f * 1.3f;
	}

	private void UpdateVirtualButtonAnalog()
	{
		if (_refEntity.IsLocalPlayer && _refPlayer != null)
		{
			_refPlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL0, true);
		}
	}

	private void RushSkillHitCB(object obj)
	{
		if (!_refEntity.IsLocalPlayer && !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			return;
		}
		if (_refEntity.UsingVehicle)
		{
			RecoverRushCollideBullet(true);
			return;
		}
		Collider2D collider2D = obj as Collider2D;
		if (collider2D == null)
		{
			return;
		}
		Transform transform = collider2D.transform;
		StageObjParam stageObjParam = transform.GetComponent<StageObjParam>();
		if (stageObjParam == null)
		{
			PlayerCollider component = transform.GetComponent<PlayerCollider>();
			if (component != null && component.IsDmgReduceShield())
			{
				stageObjParam = component.GetDmgReduceOwner();
			}
		}
		if (stageObjParam == null || stageObjParam.tLinkSOB == null)
		{
			return;
		}
		OrangeCharacter orangeCharacter = stageObjParam.tLinkSOB as OrangeCharacter;
		EnemyControllerBase enemyControllerBase = stageObjParam.tLinkSOB as EnemyControllerBase;
		if (!orangeCharacter && !enemyControllerBase)
		{
			return;
		}
		RecoverRushCollideBullet();
		if (_refEntity.IsLocalPlayer)
		{
			if ((bool)orangeCharacter)
			{
				_refEntity.selfBuffManager.AddBuff(-1, 0, 0, 0, false, orangeCharacter.sPlayerID);
				_pRushTarget = orangeCharacter;
			}
			else
			{
				_refEntity.selfBuffManager.AddBuff(-1, 0, 0, 0);
				_pRushTarget = enemyControllerBase;
			}
			_tfHitTransform = transform;
		}
	}

	private void UpdateSkill1Direction(float dirX)
	{
		if (Mathf.Abs(dirX) > 0.05f)
		{
			int num = Math.Sign(dirX);
			if (num != _refEntity.direction)
			{
				_refEntity.direction = num;
			}
		}
	}

	public override void SetRushBullet(RushCollideBullet rushCollideBullet)
	{
		RecoverRushCollideBullet();
		_pRushCollideBullet = rushCollideBullet;
		if (_refEntity.UsingVehicle)
		{
			_pRushTarget = null;
			RecoverRushCollideBullet(true);
		}
	}

	public override void SyncSkillDirection(Vector3 dir, IAimTarget target)
	{
		if (_refEntity.UsingVehicle)
		{
			_pRushTarget = null;
			RecoverRushCollideBullet(true);
			return;
		}
		_bSyncRushSkillCompleted = true;
		_vSyncDirection = dir;
		_pRushTarget = target;
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1)
		{
			_vSkillVelocity = _vSyncDirection.normalized * GetRushSpd();
			UpdateSkill1Direction(_vSkillVelocity.x);
		}
	}

	private void CheckRushBuff()
	{
		if (_pRushCollideBullet != null && !_refEntity.IsLocalPlayer && !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			RecoverRushCollideBullet();
		}
	}

	protected void RecoverRushCollideBullet(bool clearBuff = false)
	{
		if (clearBuff && _refEntity.IsLocalPlayer && _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(-1);
		}
		if ((bool)_pRushCollideBullet)
		{
			_pRushCollideBullet.BackToPool();
			_pRushCollideBullet.HitCallback = null;
			_pRushCollideBullet = null;
		}
	}

	public void LogicUpdate()
	{
		if (isInit)
		{
			CheckRushBuff();
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[10] { "ch123_skill_01_crouch_start", "ch123_skill_01_stand_start", "ch123_skill_01_jump_start", "ch123_skill_01_loop", "ch123_skill_01_crouch_end", "ch123_skill_01_stand_end", "ch123_skill_01_jump_end", "ch123_skill_02_crouch", "ch123_skill_02_stand", "ch123_skill_02_jump" };
	}
}
