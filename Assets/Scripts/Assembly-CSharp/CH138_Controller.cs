using System;
using System.Collections;
using UnityEngine;

public class CH138_Controller : CharacterControlBase
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

	private SKILL_TABLE linkSkl1;

	private Vector3? _targetPos;

	private bool _isTeleporation;

	private CharacterMaterial saberCM;

	private readonly int skl1StartAngle = 45;

	private readonly float skl1Offset = 2f;

	private readonly int SKL0_START_END = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_LOOP_END = (int)(0.417f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_END = (int)(0.6f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_TRIGGER = 1;

	private readonly int SKL0_END_BREAK = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.45f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.7f / GameLogicUpdateManager.m_fFrameLen);

	private readonly string FX_000 = "fxuse_ShadowSlash_000";

	private readonly string FX_001 = "fxuse_Destroylight_000";

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
		_tfWind = OrangeBattleUtility.FindChildRecursive(ref target, "Fx_ViaWing_01", true);
		GameObject gameObject = OrangeBattleUtility.FindChildRecursive(ref target, "Saber_008_G", true).gameObject;
		if ((bool)gameObject)
		{
			saberCM = gameObject.GetComponent<CharacterMaterial>();
			saberCM.Appear();
		}
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<BulletBase>(_refEntity, 0, out linkSkl0);
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<BulletBase>(_refEntity, 1, out linkSkl1);
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
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
		{
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
			UpdateCustomWeaponRenderer(true);
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_START_END, SKL0_START_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
			PlayerAutoAimSystem playerAutoAimSystem = _refEntity.PlayerAutoAimSystem;
			IAimTarget aimTarget = playerAutoAimSystem.AutoAimTarget;
			WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[id];
			PlayVoiceSE("v_vi2_skill01");
			if (IsPVPMode && aimTarget != null)
			{
				float magnitude = (aimTarget.AimPosition - _refEntity.AimPosition).magnitude;
				if (!playerAutoAimSystem.IsInsideScreenExactly(aimTarget.AimPosition) || magnitude > weaponStruct2.BulletData.f_DISTANCE)
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
				PlaySkillSE("vi2_zan03");
			}
			else
			{
				float f_DISTANCE = weaponStruct2.BulletData.f_DISTANCE;
				Vector3 aimPosition;
				if (aimTarget == null)
				{
					aimPosition = _refEntity.AimPosition;
					aimPosition.x += Mathf.Sign(_refEntity.ShootDirection.x) * f_DISTANCE;
				}
				else
				{
					aimPosition = aimTarget.AimPosition;
				}
				_targetPos = Vector3.MoveTowards(_refEntity.AimPosition, aimPosition, f_DISTANCE);
				PlaySkillSE("vi2_zan01");
			}
			if (_targetPos.HasValue)
			{
				int num = Math.Sign((_targetPos.Value - _refEntity.AimPosition).normalized.x);
				_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
			}
			fxUseSkl0 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_000, _refEntity.ModelTransform, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			break;
		}
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
				UpdateCustomWeaponRenderer(true);
				_refEntity.CurrentActiveSkill = id;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				_refEntity.CheckUsePassiveSkill(1, weaponStruct.BulletData, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
				_refEntity.IsShoot = 1;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u, (HumanBase.AnimateId)71u);
				fxUseSkl1 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_001, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
				PlayVoiceSE("v_vi2_skill02");
				PlaySkillSE("vi2_doom01");
			}
			break;
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
		case OrangeCharacter.SubStatus.SKILL1:
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
				if (linkSkl1 != null)
				{
					PlaySkillSE("vi2_doom02");
					Vector3 position = _refEntity.PlayerSkills[1].ShootTransform[0].position;
					for (int i = 0; i < linkSkl1.n_NUM_SHOOT; i++)
					{
						float num = CaluShootAngle(linkSkl1, i);
						Vector3 shootPosition = CaluOffset(position, num);
						PushLinkSkl(linkSkl1, shootPosition, Quaternion.Euler(0f, 0f, num) * Vector3.right);
					}
				}
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				isSkillEventEnd = false;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_END_TRIGGER, SKL0_END_END, OrangeCharacter.SubStatus.SKILL0_2, out skillEventFrame, out endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (nowFrame >= endFrame)
			{
				_refEntity.BulletCollider.BackToPool();
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				if (linkSkl0 != null)
				{
					PushLinkSkl(linkSkl0, _refEntity.PlayerSkills[0].ShootTransform[0].position);
				}
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endBreakFrame);
			}
			break;
		}
	}

	private void PushLinkSkl(SKILL_TABLE bulletData, Vector3 shootPosition, Vector3? ShotDir = null)
	{
		WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
		_refEntity.PushBulletDetail(bulletData, currentSkillObj.weaponStatus, shootPosition, currentSkillObj.SkillLV, ShotDir);
	}

	protected float CaluShootAngle(SKILL_TABLE sklTable, int index)
	{
		return (float)skl1StartAngle + sklTable.f_ANGLE / (float)sklTable.n_NUM_SHOOT * (float)index;
	}

	protected Vector3 CaluOffset(Vector3 p_pos, float p_angle)
	{
		float x = skl1Offset * Mathf.Cos(p_angle * ((float)Math.PI / 180f));
		float y = skl1Offset * Mathf.Sin(p_angle * ((float)Math.PI / 180f));
		return p_pos + new Vector3(x, y);
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

	private void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		{
			_refEntity.IgnoreGravity = true;
			WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[0];
			OrangeBattleUtility.UpdateSkillCD(weaponStruct2);
			_refEntity.CheckUsePassiveSkill(0, weaponStruct2.weaponStatus, weaponStruct2.ShootTransform[0], _refEntity.direction * Vector2.right, 0);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START, false);
			break;
		}
		case OrangeCharacter.SubStatus.SKILL0_1:
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
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)66u, false);
				}
			}
			int num = 0;
			endFrame = nowFrame + num;
			break;
		}
		case OrangeCharacter.SubStatus.SKILL0_2:
		{
			_refEntity.SetSpeed(0, 0);
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
			_refEntity.BulletCollider.UpdateBulletData(weaponStruct.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
			_refEntity.BulletCollider.Active(_refEntity.TargetMask);
			_refEntity.BulletCollider.SetDirection(_refEntity.direction * Vector3.right);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)67u, (HumanBase.AnimateId)67u, (HumanBase.AnimateId)68u);
			break;
		}
		}
	}

	protected void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL && subStatus == OrangeCharacter.SubStatus.SKILL0)
		{
			_refEntity.IgnoreGravity = true;
			if (_isTeleporation && !_targetPos.HasValue)
			{
				isSkillEventEnd = false;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_END_TRIGGER, SKL0_END_END, OrangeCharacter.SubStatus.SKILL0_2, out skillEventFrame, out endFrame);
			}
			else
			{
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_LOOP_END, SKL0_LOOP_END, OrangeCharacter.SubStatus.SKILL0_1, out skillEventFrame, out endFrame);
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
		if (_refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
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
		return new string[7] { "ch138_skill_01_stand_start", "ch138_skill_01_jump_loop", "ch138_skill_01_stand_end", "ch138_skill_01_jump_end", "ch138_skill_02_crouch", "ch138_skill_02_stand", "ch138_skill_02_jump" };
	}
}
