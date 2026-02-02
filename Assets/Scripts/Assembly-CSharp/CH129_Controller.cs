using System;
using System.Collections;
using UnityEngine;

public class CH129_Controller : CharacterControlBase, ILogicUpdate
{
	private readonly int sklFrenzyId = 22570;

	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private bool isFrenzyStatus;

	private int frenzySkillEventFrame;

	private SKILL_TABLE sklFrenzy;

	[SerializeField]
	private int risingSpdX = 2000;

	[SerializeField]
	private int risingSpdY = 15000;

	[SerializeField]
	private float risingTime = 0.1f;

	private ParticleSystem _regularEffect;

	private ParticleSystem[] _FrenzyEffect;

	private WeaponStatus frenzyBulletStatus = new WeaponStatus();

	private bool canUseFrenzyBullet;

	private bool spIsFull = true;

	private readonly string FX_0_00 = "fxuse_ShagaruX_000";

	private readonly string FX_1_00 = "fxuse_blackpoisonball_000";

	private readonly string FX_2_00 = "fxhit_barrage_002";

	protected readonly int SKL0_STEP_1_LOOP = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_STEP_2_START = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_STEP_2_LOOP = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_STEP_3_START = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_STEP_3_LOOP = (int)(1.5f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_STEP_3_END = (int)(0.267f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_STEP_3_BREAK = (int)(0.187f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_TRIGGER = (int)(0.192f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END = (int)(0.833f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END_BREAK = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int FRENZY_TIMER = (int)(2f / GameLogicUpdateManager.m_fFrameLen);

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
		_refEntity.teleportInVoicePlayed = true;
	}

	private void InitializeSkill()
	{
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = transform;
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[1];
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(sklFrenzyId, out sklFrenzy))
		{
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref sklFrenzy);
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<VirustrapBullet>("prefab/bullet/" + sklFrenzy.s_MODEL, sklFrenzy.s_MODEL, 3, null);
			frenzyBulletStatus.CopyWeaponStatus(_refEntity.PlayerWeapons[1].weaponStatus, 0);
		}
		_regularEffect = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_ShagaruX_000").GetComponent<ParticleSystem>();
		_regularEffect.Play(true);
		_FrenzyEffect = new ParticleSystem[2];
		_FrenzyEffect[0] = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_crazydragon_000_L").GetComponent<ParticleSystem>();
		_FrenzyEffect[1] = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_crazydragon_000_R").GetComponent<ParticleSystem>();
		_FrenzyEffect[0].Stop(true);
		_FrenzyEffect[1].Stop(true);
		SKILL_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(_refEntity.CharacterData.n_PASSIVE_2, out value))
		{
			int n_MOTION_DEF = value.n_MOTION_DEF;
			for (int i = 0; i < _refEntity.tRefPassiveskill.listUsePassiveskill.Count; i++)
			{
				if (_refEntity.tRefPassiveskill.listUsePassiveskill[i].tSKILL_TABLE.n_ID == n_MOTION_DEF)
				{
					canUseFrenzyBullet = true;
					break;
				}
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_2_00, 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
	}

	public void LogicUpdate()
	{
		CheckFrenzyBuff();
		CheckFrenzyBullet();
		PerBuffManager refPBM = _refEntity.selfBuffManager.sBuffStatus.refPBM;
		if (!spIsFull)
		{
			if (refPBM.nMeasureNow == refPBM.nMeasureMax)
			{
				spIsFull = true;
				PlaySkillSE("xa_ark01");
			}
		}
		else if (refPBM.nMeasureNow != refPBM.nMeasureMax)
		{
			spIsFull = false;
		}
	}

	private void CheckFrenzyBuff()
	{
		if (_refEntity.PlayerSkills.Length == 0)
		{
			return;
		}
		if (_refEntity.PlayerSkills[0].Reload_index == 1)
		{
			if (!isFrenzyStatus)
			{
				PlayFrenzyFx();
			}
		}
		else if (isFrenzyStatus)
		{
			StopLightningFx();
		}
	}

	private void CheckFrenzyBullet()
	{
		if (_refEntity.IsLocalPlayer && isFrenzyStatus && GameLogicUpdateManager.GameFrame >= frenzySkillEventFrame && !MonoBehaviourSingleton<StageSyncManager>.Instance.bPauseAllPlayerInput)
		{
			frenzySkillEventFrame = GameLogicUpdateManager.GameFrame + FRENZY_TIMER;
			if (canUseFrenzyBullet && sklFrenzy != null)
			{
				_refEntity.PushBulletDetail(sklFrenzy, frenzyBulletStatus, _refEntity.ModelTransform);
			}
		}
	}

	private void PlayFrenzyFx()
	{
		isFrenzyStatus = true;
		_FrenzyEffect[0].Play(true);
		_FrenzyEffect[1].Play(true);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_2_00, _refEntity.ModelTransform, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		PlaySkillSE("xa_ark02_lp");
	}

	private void StopLightningFx()
	{
		isFrenzyStatus = false;
		_FrenzyEffect[0].Stop(true);
		_FrenzyEffect[1].Stop(true);
		PlaySkillSE("xa_ark02_stop");
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			int num = (int)(risingTime / GameLogicUpdateManager.m_fFrameLen);
			ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, num, num, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			_refEntity.IgnoreGravity = true;
			_refEntity.SetSpeed((int)_refEntity._characterDirection * risingSpdX, risingSpdY);
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
			_refEntity.CheckUsePassiveSkill(0, weaponStruct.weaponStatus, _refEntity.ModelTransform);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			isSkillEventEnd = false;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
			_refEntity.IsShoot = 1;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)71u, (HumanBase.AnimateId)72u, (HumanBase.AnimateId)73u);
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
			_refEntity.CheckUsePassiveSkill(1, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		}
	}

	public override void CheckSkill()
	{
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
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (nowFrame >= endFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_STEP_2_START, SKL0_STEP_2_START, OrangeCharacter.SubStatus.SKILL0_1, out skillEventFrame, out endFrame);
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				_refEntity.Dashing = false;
				_refEntity.SetSpeed(0, 0);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_STEP_2_LOOP, SKL0_STEP_2_LOOP, OrangeCharacter.SubStatus.SKILL0_2, out skillEventFrame, out endFrame);
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (nowFrame >= endFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_STEP_3_START, SKL0_STEP_3_START, OrangeCharacter.SubStatus.SKILL0_3, out skillEventFrame, out endFrame);
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (nowFrame >= endFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_STEP_3_LOOP, SKL0_STEP_3_LOOP, OrangeCharacter.SubStatus.SKILL0_4, out skillEventFrame, out endFrame);
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ModelTransform, MagazineType.NORMAL, _refEntity.GetCurrentSkillObj().Reload_index, 0, false);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_0_00, _refEntity.AimTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_4:
			if (nowFrame >= endFrame)
			{
				endBreakFrame = nowFrame + SKL0_STEP_3_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_STEP_3_END, SKL0_STEP_3_END, OrangeCharacter.SubStatus.SKILL0_5, out skillEventFrame, out endFrame);
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_5:
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
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.PlayerSkills[1].ShootTransform[0], MagazineType.NORMAL, _refEntity.GetCurrentSkillObj().Reload_index, 1, false);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_00, _refEntity.PlayerSkills[1].ShootTransform[0].position, (_refEntity.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
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
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != (HumanBase.AnimateId)71u)
		{
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
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
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		_refEntity.EnableCurrentWeapon();
	}

	public override void ControlCharacterDead()
	{
		ToggleRegularEffect(false);
	}

	public override void ControlCharacterContinue()
	{
		StartCoroutine(OnToggleRegularEffect(true, 0.6f));
	}

	private void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 1.5f && currentFrame <= 2f)
			{
				ToggleRegularEffect(false);
			}
		}
	}

	protected void StageTeleportInCharacterDepend()
	{
		ToggleRegularEffect(false);
		StartCoroutine(OnToggleRegularEffect(true, 0.6f));
	}

	protected void StageTeleportOutCharacterDepend()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT)
		{
			StartCoroutine(OnToggleRegularEffect(false, 0.2f));
		}
		else if (!_refEntity.Animator.IsDefaultAnimator)
		{
			StartCoroutine(OnToggleRegularEffect(false, 0.2f));
		}
	}

	private IEnumerator OnToggleRegularEffect(bool isActive, float delay)
	{
		yield return new WaitForSeconds(delay);
		ToggleRegularEffect(isActive);
	}

	private void ToggleRegularEffect(bool isActive)
	{
		if (isActive)
		{
			_regularEffect.Play(true);
		}
		else
		{
			_regularEffect.Stop(true);
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[9] { "ch129_skill_01_step1_loop", "ch129_skill_01_step2_start", "ch129_skill_01_step2_loop", "ch129_skill_01_step3_start", "ch129_skill_01_step3_loop", "ch129_skill_01_step3_end", "ch129_skill_02_crouch", "ch129_skill_02_stand", "ch129_skill_02_jump" };
	}
}
