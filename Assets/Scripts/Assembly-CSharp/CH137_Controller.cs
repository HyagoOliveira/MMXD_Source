using System;
using System.Collections;
using UnityEngine;

public class CH137_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private Transform _tfWind;

	private FxBase fxUseSkl0;

	private Vector3? _targetPos;

	private bool _isTeleporation;

	private SKILL_TABLE linkSkl0;

	private SKILL_TABLE linkSkl1;

	private CharacterMaterial saberCM;

	private readonly int SKL0_TRIGGER = (int)(0.28f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.667f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_START_END = (int)(0.133f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_LOOP_END = (int)(0.417f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_END = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_TRIGGER = 1;

	private readonly int SKL1_END_BREAK = (int)(0.48f / GameLogicUpdateManager.m_fFrameLen);

	private readonly string FX_000 = "fxuse_Ringblade_000";

	private readonly string FX_001 = "fxuse_Dashblade_000";

	private bool IsPVPMode
	{
		get
		{
			return MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp;
		}
	}

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
		_refEntity.ExtraTransforms = new Transform[1];
		_refEntity.ExtraTransforms[0] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_tfWind = OrangeBattleUtility.FindChildRecursive(ref target, "Fx_JWing_01_G", true);
		GameObject gameObject = OrangeBattleUtility.FindChildRecursive(ref target, "Saber_008_G", true).gameObject;
		if ((bool)gameObject)
		{
			saberCM = gameObject.GetComponent<CharacterMaterial>();
			saberCM.Appear();
		}
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<BasicBullet>(_refEntity, 0, out linkSkl0);
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<BasicBullet>(_refEntity, 1, out linkSkl1);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_000, 2);
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
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
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
			if (currentFrame > 1.7f && currentFrame <= 2f)
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
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
				UpdateCustomWeaponRenderer(true);
				_refEntity.CurrentActiveSkill = id;
				WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[0];
				OrangeBattleUtility.UpdateSkillCD(weaponStruct2);
				_refEntity.CheckUsePassiveSkill(0, weaponStruct2.BulletData, weaponStruct2.weaponStatus, weaponStruct2.ShootTransform[0]);
				_refEntity.IsShoot = 1;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				fxUseSkl0 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_000, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
				PlayVoiceSE("v_ir4_skill01");
			}
			break;
		case 1:
		{
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
			UpdateCustomWeaponRenderer(true);
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_START_END, SKL1_START_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			PlayerAutoAimSystem playerAutoAimSystem = _refEntity.PlayerAutoAimSystem;
			IAimTarget aimTarget = playerAutoAimSystem.AutoAimTarget;
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[id];
			PlayVoiceSE("v_ir4_skill02");
			PlaySkillSE("ir4_blade01");
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
					target = ((!_refEntity.IsInGround) ? _refEntity.AimPosition : new Vector3(_refEntity.AimPosition.x, _refEntity.transform.position.y, _refEntity.AimPosition.z));
					target.x += Mathf.Sign(_refEntity.ShootDirection.x) * f_DISTANCE;
				}
				else
				{
					target = aimTarget.AimPosition;
				}
				_targetPos = Vector3.MoveTowards(_refEntity.AimPosition, target, f_DISTANCE);
			}
			if (_targetPos.HasValue)
			{
				int num = Math.Sign((_targetPos.Value - _refEntity.AimPosition).normalized.x);
				_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
			}
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_001, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			break;
		}
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
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
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, weaponStruct.ShootTransform[0], MagazineType.NORMAL, -1, 1, false);
				isSkillEventEnd = true;
				if (linkSkl0 != null)
				{
					PushLinkSkl(linkSkl0, _refEntity.PlayerSkills[0].ShootTransform[0]);
				}
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= endFrame)
			{
				isSkillEventEnd = false;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL1_END_TRIGGER, SKL1_END_END, OrangeCharacter.SubStatus.SKILL1_2, out skillEventFrame, out endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ModelTransform, MagazineType.NORMAL, 0, 0, false);
				if (linkSkl1 != null)
				{
					PushLinkSkl(linkSkl1, _refEntity.PlayerSkills[1].ShootTransform[0], _refEntity.direction * Vector3.right);
				}
				_refEntity.PlaySE("SkillSE_IRIS4", "ir4_blade03", 0.1f);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endBreakFrame);
			}
			break;
		}
	}

	private void PushLinkSkl(SKILL_TABLE bulletData, Transform shootTransform, Vector3? ShotDir = null)
	{
		WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
		_refEntity.PushBulletDetail(bulletData, currentSkillObj.weaponStatus, shootTransform, currentSkillObj.SkillLV, ShotDir);
	}

	private void OnSkillEnd()
	{
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		fxUseSkl0 = null;
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

	private void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL1:
		{
			_refEntity.IgnoreGravity = true;
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
			_refEntity.CheckUsePassiveSkill(1, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0], _refEntity.direction * Vector2.right, 0);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)68u, false);
			break;
		}
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
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)69u, false);
				}
			}
			int num = 0;
			endFrame = nowFrame + num;
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.SetSpeed(0, 0);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)70u, (HumanBase.AnimateId)70u, (HumanBase.AnimateId)71u);
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
				isSkillEventEnd = false;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL1_END_TRIGGER, SKL1_END_END, OrangeCharacter.SubStatus.SKILL1_2, out skillEventFrame, out endFrame);
			}
			else
			{
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL1_LOOP_END, SKL1_LOOP_END, OrangeCharacter.SubStatus.SKILL1_1, out skillEventFrame, out endFrame);
			}
		}
	}

	public override void ClearSkill()
	{
		if (fxUseSkl0 != null)
		{
			fxUseSkl0.BackToPool();
		}
		fxUseSkl0 = null;
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
			saberCM.Appear(null, 0f);
		}
		else
		{
			saberCM.Disappear(null, 0f);
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[7] { "ch137_skill_01_crouch", "ch137_skill_01_stand", "ch137_skill_01_jump", "ch137_skill_02_step1_start", "ch137_skill_02_step1_loop", "ch137_skill_02_step2_stand", "ch137_skill_02_step2_jump" };
	}
}
