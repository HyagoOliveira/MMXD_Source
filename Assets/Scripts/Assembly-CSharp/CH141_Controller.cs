using System;
using UnityEngine;

public class CH141_Controller : CharacterControlBase, ILogicUpdate
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private SKILL_TABLE linkSkl1_0;

	private SKILL_TABLE linkSkl1_1;

	private bool isFrenzyStatus;

	private ParticleSystem[] _FrenzyEffect;

	private ParticleSystem _teleportEffect;

	private ParticleSystem _saberEffect;

	private bool isInit;

	private CharacterMaterial cmSaber;

	private CharacterMaterial cmSaberSide;

	private Vector3? _targetPos;

	private bool _isTeleporation;

	private int seEventFrame;

	private int fxEventFrame;

	private bool IsSkl1InGround = true;

	private bool isWinPose;

	private int winFxFrame = -1;

	private int logoutFxFrame = -1;

	private readonly string FX_0_00 = "fxuse_DMCZHell_000";

	private readonly string FX_1_00 = "fxuse_DMCZJudgmentCutEnd_000";

	private readonly string FX_1_01 = "fxuse_DMCZJudgmentCutEnd_001";

	private readonly string FX_2_00 = "fxuse_DMCZDevilTrigger_000";

	protected readonly int SKL0_TRIGGER = (int)(0.369f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_END_BREAK = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_START_END = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_LOOP_END = (int)(0.417f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_END = (int)(2f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_TRIGGER = 1;

	private readonly int SKL1_END_SE_TRIGGER = (int)(1.18f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_FX_TRIGGER = (int)(1.4f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(1.6f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int WIN_TRIGGER = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int LOGOUT_TRIGGER = (int)(0.46f / GameLogicUpdateManager.m_fFrameLen);

	private bool IsPVPMode
	{
		get
		{
			return MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp;
		}
	}

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
		_refEntity.ExtraTransforms = new Transform[1];
		_refEntity.ExtraTransforms[0] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_FrenzyEffect = new ParticleSystem[1];
		_FrenzyEffect[0] = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_DMCZDevilTrigger_000_(work)").GetComponent<ParticleSystem>();
		_FrenzyEffect[0].gameObject.SetActive(false);
		_teleportEffect = OrangeBattleUtility.FindChildRecursive(ref target, "fxdemo_DMCZ_003_(work)").GetComponent<ParticleSystem>();
		_saberEffect = OrangeBattleUtility.FindChildRecursive(ref target, "fxdemo_DMCZ_004_(work)").GetComponent<ParticleSystem>();
		GameObject gameObject = OrangeBattleUtility.FindChildRecursive(ref target, "Hand_KatanaBladeMesh_A_m", true).gameObject;
		if ((bool)gameObject)
		{
			cmSaber = gameObject.GetComponent<CharacterMaterial>();
		}
		GameObject gameObject2 = OrangeBattleUtility.FindChildRecursive(ref target, "Waist_KatanaHandleMesh_m", true).gameObject;
		if ((bool)gameObject2)
		{
			cmSaberSide = gameObject2.GetComponent<CharacterMaterial>();
		}
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<BulletBase>(_refEntity, 1, out linkSkl1_0);
		if (linkSkl1_0 != null && linkSkl1_0.n_COMBO_SKILL != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(linkSkl1_0.n_COMBO_SKILL, out linkSkl1_1))
		{
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref linkSkl1_1);
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_01, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_2_00, 2);
		isInit = true;
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.PlayTeleportOutEffectEvt = PlayTeleportOutEffect;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
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

	protected void StageTeleportInCharacterDepend()
	{
		if ((bool)cmSaberSide)
		{
			cmSaberSide.Appear();
		}
	}

	protected void StageTeleportOutCharacterDepend()
	{
		if ((bool)cmSaberSide)
		{
			cmSaberSide.Disappear();
		}
	}

	public void LogicUpdate()
	{
		if (isInit)
		{
			CheckFrenzyBuff();
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
			StopFrenzyFx();
		}
	}

	private void PlayFrenzyFx()
	{
		isFrenzyStatus = true;
		_FrenzyEffect[0].gameObject.SetActive(true);
		_FrenzyEffect[0].Play(true);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_2_00, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		PlaySkillSE("zc_majin01");
	}

	private void StopFrenzyFx()
	{
		isFrenzyStatus = false;
		_FrenzyEffect[0].gameObject.SetActive(false);
		_FrenzyEffect[0].Stop(true);
		PlaySkillSE("zc_majin02");
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				isSkillEventEnd = false;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				_refEntity.IsShoot = 0;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[0];
				OrangeBattleUtility.UpdateSkillCD(weaponStruct2);
				_refEntity.CheckUsePassiveSkill(0, weaponStruct2.weaponStatus, weaponStruct2.ShootTransform[0]);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_0_00, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				PlayVoiceSE("v_zc_skill01");
				PlaySkillSE("zc_earth01");
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				_refEntity.CheckUsePassiveSkill(1, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_START_END, SKL1_START_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				UseSkill1();
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)71u);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.CurrentActiveSkill == -1)
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
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, weaponStruct.ShootTransform[0], MagazineType.NORMAL, _refEntity.GetCurrentSkillObj().Reload_index, 0, false);
				PlaySkillSE("zc_earth02");
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= endFrame)
			{
				SetStatusSkill_2();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (nowFrame >= endFrame)
			{
				_refEntity.BulletCollider.BackToPool();
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				if (isFrenzyStatus)
				{
					if (linkSkl1_1 != null)
					{
						PushLinkSkl(linkSkl1_1, _refEntity.PlayerSkills[1].ShootTransform[0].position);
					}
				}
				else if (linkSkl1_0 != null)
				{
					PushLinkSkl(linkSkl1_0, _refEntity.PlayerSkills[1].ShootTransform[0].position);
				}
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endBreakFrame);
			}
			if (nowFrame == seEventFrame)
			{
				PlaySkillSE("zc_jigen03");
			}
			else if (nowFrame == fxEventFrame)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_01, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				if ((bool)_saberEffect)
				{
					_saberEffect.Play(true);
				}
			}
			break;
		}
	}

	private void UseSkill1()
	{
		PlayerAutoAimSystem playerAutoAimSystem = _refEntity.PlayerAutoAimSystem;
		IAimTarget aimTarget = playerAutoAimSystem.AutoAimTarget;
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
		if (IsPVPMode && aimTarget != null)
		{
			float magnitude = (aimTarget.AimPosition - _refEntity.AimPosition).magnitude;
			if (!playerAutoAimSystem.IsInsideScreenExactly(aimTarget.AimPosition) || magnitude > weaponStruct.BulletData.f_DISTANCE)
			{
				aimTarget = null;
			}
		}
		_targetPos = null;
		_isTeleporation = false;
		IsSkl1InGround = _refEntity.IsInGround;
		if (IsPVPMode)
		{
			if (aimTarget != null)
			{
				_targetPos = aimTarget.AimPosition;
				_isTeleporation = true;
				OrangeCharacter orangeCharacter = aimTarget as OrangeCharacter;
				if (orangeCharacter != null)
				{
					_targetPos = orangeCharacter._transform.position;
				}
			}
		}
		else
		{
			float f_DISTANCE = weaponStruct.BulletData.f_DISTANCE;
			Vector3 target;
			if (aimTarget == null)
			{
				target = ((!IsSkl1InGround) ? _refEntity.AimPosition : new Vector3(_refEntity.AimPosition.x, _refEntity.transform.position.y, _refEntity.AimPosition.z));
				target.x += Mathf.Sign(_refEntity.ShootDirection.x) * f_DISTANCE;
			}
			else
			{
				target = aimTarget.AimPosition;
				if (IsSkl1InGround && Mathf.Abs(target.y - _refEntity.AimPosition.y) > 0.2f)
				{
					IsSkl1InGround = false;
				}
			}
			_targetPos = Vector3.MoveTowards(_refEntity.AimPosition, target, f_DISTANCE);
		}
		if (_targetPos.HasValue)
		{
			int num = Math.Sign((_targetPos.Value - _refEntity.AimPosition).normalized.x);
			_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_00, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		PlayVoiceSE("v_zc_skill02");
		PlaySkillSE("zc_jigen01");
	}

	private void SetStatusSkill_2()
	{
		isSkillEventEnd = false;
		endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
		seEventFrame = GameLogicUpdateManager.GameFrame + SKL1_END_SE_TRIGGER;
		fxEventFrame = GameLogicUpdateManager.GameFrame + SKL1_END_FX_TRIGGER;
		ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL1_END_TRIGGER, SKL1_END_END, OrangeCharacter.SubStatus.SKILL1_2, out skillEventFrame, out endFrame);
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1_1:
			{
				if (_targetPos.HasValue)
				{
					Vector3 vector = _targetPos.Value - _refEntity.AimPosition;
					if (_isTeleporation)
					{
						Vector3 value = _targetPos.Value;
						if (_refEntity.IsLocalPlayer)
						{
							_refEntity.Controller.LogicPosition = new VInt3(value);
							_refEntity.transform.position = value;
						}
					}
					else
					{
						VInt2 vInt = new VInt2(vector / OrangeBattleUtility.PPU / OrangeBattleUtility.FPS / GameLogicUpdateManager.m_fFrameLen);
						_refEntity.SetSpeed(vInt.x, vInt.y);
						if (IsSkl1InGround)
						{
							_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
						}
						else
						{
							_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
						}
					}
				}
				int num = 0;
				endFrame = nowFrame + num;
				break;
			}
			case OrangeCharacter.SubStatus.SKILL1_2:
			{
				_refEntity.SetSpeed(0, 0);
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				_refEntity.BulletCollider.UpdateBulletData(weaponStruct.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.BulletCollider.SetDirection(_refEntity.direction * Vector3.right);
				if (IsSkl1InGround)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
				}
				ToggleSaber(true);
				break;
			}
			case OrangeCharacter.SubStatus.SKILL1:
				break;
			}
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
				isWinPose = true;
				winFxFrame = GameLogicUpdateManager.GameFrame + WIN_TRIGGER;
				ToggleSaber(true);
				SetSaberToEntitySub();
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				if (isWinPose)
				{
					logoutFxFrame = GameLogicUpdateManager.GameFrame + LOGOUT_TRIGGER;
				}
				SetSaberToEntitySub();
				break;
			}
			break;
		}
	}

	protected void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL && subStatus == OrangeCharacter.SubStatus.SKILL1)
		{
			_refEntity.IgnoreGravity = true;
			if (_isTeleporation && !_targetPos.HasValue)
			{
				SetStatusSkill_2();
			}
			else
			{
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL1_LOOP_END, SKL1_LOOP_END, OrangeCharacter.SubStatus.SKILL1_1, out skillEventFrame, out endFrame);
			}
		}
	}

	private void TeleportOutCharacterDepend()
	{
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.WIN_POSE:
			if (GameLogicUpdateManager.GameFrame == winFxFrame && (bool)_teleportEffect)
			{
				_teleportEffect.Play(true);
			}
			break;
		case OrangeCharacter.SubStatus.TELEPORT_POSE:
			if (GameLogicUpdateManager.GameFrame == logoutFxFrame && (bool)_saberEffect)
			{
				_saberEffect.Play(true);
			}
			break;
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
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		ToggleSaber(false);
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
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

	private void ToggleSaber(bool enable)
	{
		if ((bool)cmSaber)
		{
			if (enable)
			{
				cmSaber.Appear();
			}
			else
			{
				cmSaber.Disappear();
			}
		}
	}

	private void ToggleSideSaber(bool enable)
	{
		if ((bool)cmSaberSide)
		{
			if (enable)
			{
				cmSaberSide.Appear();
			}
			else
			{
				cmSaberSide.Disappear();
			}
		}
	}

	private void SetSaberToEntitySub()
	{
		if ((bool)cmSaber && (bool)cmSaberSide && cmSaber.GetSubCharacterMaterials == null)
		{
			cmSaber.ChangeDissolveTime(0.8f);
			cmSaberSide.ChangeDissolveTime(0.8f);
			_refEntity.CharacterMaterials.SetSubCharacterMaterial(cmSaber);
			cmSaber.SetSubCharacterMaterial(cmSaberSide);
		}
	}

	public override void ClearSkill()
	{
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		ToggleSaber(false);
		ToggleSideSaber(true);
		if (_refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		ToggleSaber(false);
		ToggleSideSaber(true);
		_refEntity.EnableCurrentWeapon();
	}

	public override void ControlCharacterDead()
	{
		ToggleSaber(false);
		ToggleSideSaber(false);
	}

	public override void ControlCharacterContinue()
	{
		ToggleSideSaber(true);
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[9] { "ch141_skill_01_crouch", "ch141_skill_01_stand", "ch141_skill_01_jump", "ch141_skill_02_stand_start", "ch141_skill_02_stand_loop", "ch141_skill_02_stand_end", "ch141_skill_02_jump_start", "ch141_skill_02_jump_loop", "ch141_skill_02_jump_end" };
	}
}
