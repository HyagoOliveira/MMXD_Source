using System;
using UnityEngine;

public class CH050_Controller : CharacterControlBase
{
	private readonly int SKILL0_START = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_END = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_CANCEL = (int)(0.18f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1PVP_END = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_START = (int)(0.06f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_LOOP = (int)(0.11f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_END = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_CANCEL = (int)(0.25f / GameLogicUpdateManager.m_fFrameLen);

	private int skillEndFrame;

	private int skillCancelFrame;

	private EventManager.StageCameraFocus stageCameraFocus;

	private IAimTarget dashTarget;

	private Vector2 dashVelocity;

	private bool isDashEnd;

	private Transform shootPointTransform;

	private Vector3 lastPosition;

	public float speed = 9f;

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		InitializeExtraMesh();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.UpdateFuncEndEvt = UpdatePosition;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		shootPointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "CustomShootPoint", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_surprisesword_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_surprisesword_002", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_sonicboom_000", 2);
		stageCameraFocus = new EventManager.StageCameraFocus();
		stageCameraFocus.bLock = true;
		stageCameraFocus.bRightNow = true;
	}

	private void InitializeExtraMesh()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ref target, "WeaponMainMesh_m");
		Transform[] array2 = OrangeBattleUtility.FindAllChildRecursive(ref target, "WeaponSubMesh_m");
		Transform[] array3 = new Transform[array.Length + array2.Length];
		for (int i = 0; i < array3.Length; i++)
		{
			if (i < array.Length)
			{
				array3[i] = array[i];
			}
			else
			{
				array3[i] = array2[i - array.Length];
			}
		}
		Renderer[] extraMeshOpen;
		if (array3 != null)
		{
			OrangeCharacter refEntity = _refEntity;
			extraMeshOpen = new SkinnedMeshRenderer[array3.Length];
			refEntity.ExtraMeshOpen = extraMeshOpen;
			for (int j = 0; j < array3.Length; j++)
			{
				_refEntity.ExtraMeshOpen[j] = array3[j].GetComponent<SkinnedMeshRenderer>();
			}
		}
		else
		{
			OrangeCharacter refEntity2 = _refEntity;
			extraMeshOpen = new SkinnedMeshRenderer[0];
			refEntity2.ExtraMeshOpen = extraMeshOpen;
		}
		OrangeCharacter refEntity3 = _refEntity;
		extraMeshOpen = new SkinnedMeshRenderer[0];
		refEntity3.ExtraMeshClose = extraMeshOpen;
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.DisableCurrentWeapon();
			_refEntity.SkillEnd = false;
			_refEntity.CurrentActiveSkill = id;
			_refEntity.ToggleExtraMesh(true);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.DisableCurrentWeapon();
			_refEntity.SkillEnd = false;
			_refEntity.CurrentActiveSkill = id;
			_refEntity.ToggleExtraMesh(true);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		int gameFrame = GameLogicUpdateManager.GameFrame;
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		if (mainStatus == OrangeCharacter.MainStatus.TELEPORT_IN || mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (_refEntity.PlayerAutoAimSystem.AutoAimTarget != null)
			{
				int num3 = Math.Sign((_refEntity.PlayerAutoAimSystem.AutoAimTarget.AimPosition - _refEntity._transform.position).x);
				_refEntity.direction = ((num3 != 0) ? num3 : _refEntity.direction);
			}
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			}
			else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
			}
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_sonicboom_000", _refEntity.BulletCollider.transform, (_refEntity.ShootDirection.x > 0f) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
			skillEndFrame = gameFrame + SKILL0_START;
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[currentActiveSkill]);
			CreateSkillBullet(_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill]);
			PlaySkillSE("ch050_sonic01");
			PlayVoiceSE("v_ch050_skill01");
			skillEndFrame = gameFrame + SKILL0_END;
			skillCancelFrame = gameFrame + SKILL1_CANCEL;
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[currentActiveSkill]);
			_refEntity.IgnoreGravity = true;
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
			{
				IAimTarget autoAimTarget = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
				if (autoAimTarget != null)
				{
					Vector2.Distance(_refEntity._transform.position, autoAimTarget.AimPosition);
				}
				if (autoAimTarget != null && _refEntity.PlayerAutoAimSystem.IsInsideScreen(autoAimTarget.AimPosition))
				{
					int num4 = Math.Sign((_refEntity.PlayerAutoAimSystem.AutoAimTarget.AimPosition - _refEntity._transform.position).x);
					_refEntity.direction = ((num4 != 0) ? num4 : _refEntity.direction);
					OrangeCharacter orangeCharacter = autoAimTarget as OrangeCharacter;
					Vector3 position = _refEntity.PlayerAutoAimSystem.AutoAimTarget.AimPosition;
					if (orangeCharacter != null)
					{
						position = orangeCharacter._transform.position;
					}
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_surprisesword_001", _refEntity.Controller.GetRealCenterPos(), OrangeCharacter.NormalQuaternion, Array.Empty<object>());
					if (_refEntity.IsLocalPlayer)
					{
						_refEntity.Controller.LogicPosition = new VInt3(position);
						_refEntity._transform.position = position;
					}
					if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
					{
						_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
					}
					else
					{
						_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
					}
				}
				else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
				}
				MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_surprisesword_002", _refEntity.BulletCollider.transform, (_refEntity.ShootDirection.x > 0f) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[currentActiveSkill].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[currentActiveSkill].SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				PlayVoiceSE("v_ch050_skill02");
				skillEndFrame = gameFrame + SKILL1PVP_END;
				skillCancelFrame = gameFrame + SKILL1_CANCEL;
				break;
			}
			isDashEnd = false;
			dashTarget = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
			if (dashTarget != null)
			{
				if (_refEntity.PlayerAutoAimSystem.IsInsideScreen(dashTarget.AimPosition))
				{
					Vector3 shootDirection = dashTarget.AimPosition - _refEntity.AimPosition;
					_refEntity.direction = Math.Sign(shootDirection.normalized.x);
					_refEntity.ShootDirection = shootDirection;
				}
				else
				{
					dashTarget = null;
				}
			}
			_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
			skillEndFrame = gameFrame + SKILL1_START;
			skillCancelFrame = gameFrame + SKILL1_START;
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_surprisesword_001", _refEntity.Controller.GetRealCenterPos(), OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			if (dashTarget != null)
			{
				dashVelocity = (dashTarget.AimPosition - _refEntity.AimPosition).normalized * ((float)OrangeCharacter.DashSpeed * speed);
			}
			else
			{
				dashVelocity = new Vector2((float)((int)_refEntity._characterDirection * OrangeCharacter.DashSpeed) * speed, 0f);
			}
			lastPosition = _refEntity._transform.position;
			_refEntity.ShootDirection = dashVelocity;
			_refEntity.SetSpeed((int)dashVelocity.x, (int)dashVelocity.y);
			skillEndFrame = gameFrame + SKILL1_LOOP;
			if (dashTarget != null)
			{
				float num = Vector2.Distance(_refEntity.AimPosition, dashTarget.AimPosition);
				float num2 = dashVelocity.magnitude * 0.001f * GameLogicUpdateManager.m_fFrameLen;
				if (num < num2)
				{
					_refEntity.SetSpeed(0, 0);
					Vector3 velocity = dashTarget.AimPosition - _refEntity.AimPosition;
					velocity.z = 0f;
					_refEntity.Controller.Move(velocity);
					skillEndFrame = gameFrame;
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.SetSpeed(0, 0);
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
			}
			else
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
			}
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_surprisesword_002", _refEntity.BulletCollider.transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[currentActiveSkill].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[currentActiveSkill].SkillLV;
			_refEntity.BulletCollider.Active(_refEntity.TargetMask);
			PlayVoiceSE("v_ch050_skill02");
			skillEndFrame = gameFrame + SKILL1_END;
			skillCancelFrame = gameFrame + SKILL1_CANCEL;
			break;
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		int gameFrame = GameLogicUpdateManager.GameFrame;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (gameFrame < skillEndFrame && (!CheckCancelAnimate(_refEntity.CurrentActiveSkill) || gameFrame < skillCancelFrame))
			{
				break;
			}
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				if (_refEntity.AnimateID == (HumanBase.AnimateId)68u)
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
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			_refEntity.BulletCollider.BackToPool();
			ResetSkillStatus();
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
			{
				if (gameFrame < skillEndFrame && (!CheckCancelAnimate(_refEntity.CurrentActiveSkill) || gameFrame < skillCancelFrame))
				{
					break;
				}
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					if (_refEntity.AnimateID == (HumanBase.AnimateId)68u)
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
					else
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
					}
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				_refEntity.BulletCollider.BackToPool();
				ResetSkillStatus();
			}
			else if (gameFrame >= skillEndFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (gameFrame >= skillEndFrame || isDashEnd)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.SetSpeed(0, 0);
			if (gameFrame >= skillEndFrame || (CheckCancelAnimate(_refEntity.CurrentActiveSkill) && gameFrame >= skillCancelFrame))
			{
				dashTarget = null;
				_refEntity.BulletCollider.BackToPool();
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				ResetSkillStatus();
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			_refEntity.EnableCurrentWeapon();
			switch (_refEntity.CurrentActiveSkill)
			{
			case 0:
				dashTarget = null;
				isDashEnd = true;
				_refEntity.BulletCollider.BackToPool();
				_refEntity.SetSpeed(0, 0);
				break;
			case 1:
				_refEntity.BulletCollider.BackToPool();
				_refEntity.SetSpeed(0, 0);
				break;
			}
		}
		ResetSkillStatus();
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, shootPointTransform, weaponStruct.SkillLV);
	}

	private void UpdatePosition()
	{
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 50) <= 1u && !CheckOverdash())
			{
				lastPosition = _refEntity._transform.position;
			}
		}
	}

	private bool CheckOverdash()
	{
		if (dashTarget != null && Vector2.Dot(dashTarget.AimPosition - _refEntity.AimPosition, dashVelocity) < 0f)
		{
			float num = Vector2.Distance(_refEntity._transform.position, dashTarget.AimPosition);
			if (Vector2.Distance(lastPosition, dashTarget.AimPosition) < num)
			{
				_refEntity.Controller.LogicPosition = new VInt3(lastPosition);
				_refEntity._transform.position = lastPosition;
			}
			_refEntity.SetSpeed(0, 0);
			isDashEnd = true;
			return true;
		}
		return false;
	}

	private bool CheckCancelAnimate(int skillId)
	{
		return ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID);
	}

	public override void ControlCharacterDead()
	{
		dashTarget = null;
		isDashEnd = true;
	}

	public void TeleportInCharacterDepend()
	{
		if (_refEntity.CurrentFrame >= 0.8f)
		{
			_refEntity.ToggleExtraMesh(false);
		}
	}

	private void ResetSkillStatus()
	{
		_refEntity.ToggleExtraMesh(false);
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public void TeleportInExtraEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
		PlaySkillSE("ch050_start01");
	}

	protected void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if (curSubStatus != OrangeCharacter.SubStatus.SKILL1)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ch050_startin_000";
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[10] { "ch050_skill_02_crouch", "ch050_skill_02_stand", "ch050_skill_02_stand", "ch050_skill_01_crouch", "ch050_skill_01_stand", "ch050_skill_01_jump", "ch050_skill_02_dash_start", "ch050_skill_02_dash_loop", "ch050_skill_02_dash_end_stand", "ch050_skill_02_dash_end_jump" };
	}
}
