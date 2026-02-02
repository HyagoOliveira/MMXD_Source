using System;
using StageLib;
using UnityEngine;

public class CH045_Controller : CharacterControlBase
{
	private readonly int SKILL0_START = (int)(0.15f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_LOOP = (int)(0.35f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_END = (int)(0.35f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_THROW = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_TELEPORT = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private int skillEndFrame;

	private IAimTarget dashTarget;

	private Vector2 dashVelocity;

	private EventManager.StageCameraFocus stageCameraFocus;

	private ParticleSystem fxKick;

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "FxMiragekick", true);
		if (transform != null)
		{
			fxKick = transform.GetComponent<ParticleSystem>();
			fxKick.Stop();
		}
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<BasicBullet>("prefab/bullet/p_sanka_000", "p_sanka_000", 2, null);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_sanka_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_sanka_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_sanka_002", 2);
		stageCameraFocus = new EventManager.StageCameraFocus();
		stageCameraFocus.bLock = true;
		stageCameraFocus.bRightNow = true;
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
				_refEntity.SetSpeed(0, 0);
				_refEntity.PlayerStopDashing();
				_refEntity.DisableCurrentWeapon();
				_refEntity.SkillEnd = false;
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			}
			break;
		case 1:
			if (_refEntity.PlayerSkills[id].Reload_index == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.PlayerStopDashing();
				_refEntity.DisableCurrentWeapon();
				_refEntity.SkillEnd = false;
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.PlayerSkills[id].Reload_index == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.DisableCurrentWeapon();
			_refEntity.SkillEnd = false;
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		int gameFrame = GameLogicUpdateManager.GameFrame;
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[currentActiveSkill]);
			_refEntity.IgnoreGravity = true;
			dashTarget = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
			if (dashTarget != null)
			{
				Vector3 shootDirection = dashTarget.AimPosition - _refEntity.AimPosition;
				int num3 = Math.Sign(shootDirection.normalized.x);
				_refEntity.direction = ((num3 != 0) ? num3 : _refEntity.direction);
				_refEntity.ShootDirection = shootDirection;
			}
			PlayVoiceSE("v_mr_skill03");
			PlaySkillSE("mr_kick");
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
			skillEndFrame = gameFrame + SKILL0_START;
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[currentActiveSkill];
			_refEntity.BulletCollider.UpdateBulletData(weaponStruct.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
			_refEntity.BulletCollider.Active(_refEntity.TargetMask);
			if (dashTarget != null)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				dashVelocity = (dashTarget.AimPosition - _refEntity.AimPosition).normalized * ((float)OrangeCharacter.DashSpeed * 3f);
			}
			else
			{
				dashVelocity = new Vector2((float)((int)_refEntity._characterDirection * OrangeCharacter.DashSpeed) * 3f, 0f);
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			}
			fxKick.Play();
			fxKick.transform.rotation = Quaternion.LookRotation(dashVelocity);
			fxKick.transform.localScale = new Vector3(1f, 1f, _refEntity.ModelTransform.localScale.z);
			_refEntity.ShootDirection = dashVelocity;
			_refEntity.SetSpeed((int)dashVelocity.x, (int)dashVelocity.y);
			skillEndFrame = gameFrame + SKILL0_LOOP;
			break;
		}
		case OrangeCharacter.SubStatus.SKILL0_2:
			fxKick.Stop();
			_refEntity.IgnoreGravity = false;
			dashTarget = null;
			_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			skillEndFrame = gameFrame + SKILL0_END;
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (_refEntity.PlayerSkills[currentActiveSkill].Reload_index == 1)
			{
				PlayVoiceSE("v_mr_skill04_2");
				SKILL_TABLE sKILL_TABLE = _refEntity.PlayerSkills[currentActiveSkill].FastBulletDatas[_refEntity.PlayerSkills[currentActiveSkill].Reload_index];
				_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[currentActiveSkill]);
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				}
				_refEntity.BulletCollider.UpdateBulletData(sKILL_TABLE, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[currentActiveSkill].SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				PerBuff perBuff = null;
				StageObjBase stageObjBase = null;
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					if (StageUpdate.runPlayers[num].selfBuffManager.CheckHasMarkedEffect(115, _refEntity.sNetSerialID, out perBuff))
					{
						if (perBuff.sPlayerID == _refEntity.sPlayerID && !StageUpdate.runPlayers[num].IsDead())
						{
							stageObjBase = StageUpdate.runPlayers[num];
							break;
						}
						perBuff = null;
					}
				}
				if (perBuff != null && stageObjBase != null)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_sanka_000", _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
					_refEntity.Controller.LogicPosition = new VInt3(stageObjBase._transform.position);
					_refEntity._transform.position = stageObjBase._transform.position;
					if (_refEntity.IsLocalPlayer)
					{
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
					}
				}
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_sanka_001", _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
				skillEndFrame = gameFrame + SKILL1_TELEPORT;
			}
			else
			{
				int num2 = Math.Sign(_refEntity.ShootDirection.normalized.x);
				_refEntity.direction = ((num2 != 0) ? num2 : _refEntity.direction);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_sanka_002", _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
				PlayVoiceSE("v_mr_skill04_1");
				_refEntity.PushBulletDetail(_refEntity.PlayerSkills[1].FastBulletDatas[0], _refEntity.PlayerSkills[1].weaponStatus, _refEntity.Controller.GetCenterPos(), _refEntity.PlayerSkills[1].SkillLV, _refEntity.ShootDirection);
				if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
				}
				else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)130u);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetAnimateId((HumanBase.AnimateId)132u);
				}
				skillEndFrame = gameFrame + SKILL1_THROW;
			}
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
			_refEntity.SetSpeed((int)dashVelocity.x, (int)dashVelocity.y);
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
			}
			else if (dashTarget != null)
			{
				float num = Vector2.Distance(_refEntity.AimPosition, dashTarget.AimPosition);
				float num2 = (float)_refEntity.Velocity.magnitude * 0.001f * GameLogicUpdateManager.m_fFrameLen;
				if (num < num2 * 1.5f)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.SetSpeed(0, 0);
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.BulletCollider.BackToPool();
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				ResetSkill();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (gameFrame < skillEndFrame)
			{
				break;
			}
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				if (_refEntity.AnimateID == (HumanBase.AnimateId)129u)
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
			ResetSkill();
			break;
		}
	}

	public void ChangeComboSkillEvent(object[] parameters)
	{
		try
		{
			if (parameters.Length != 2)
			{
				return;
			}
			int num = (int)parameters[0];
			int num2 = (int)parameters[1];
			if (num == 1)
			{
				if (_refEntity.PlayerSkills[1].Reload_index == 1)
				{
					OrangeConsoleCharacter obj = _refEntity as OrangeConsoleCharacter;
					obj.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, false);
					obj.ClearVirtualButtonStick(VirtualButtonId.SKILL1);
				}
				else
				{
					(_refEntity as OrangeConsoleCharacter).SetVirtualButtonAnalog(VirtualButtonId.SKILL1, true);
				}
			}
		}
		catch (Exception)
		{
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
				fxKick.Stop();
				_refEntity.SetSpeed(0, 0);
				_refEntity.BulletCollider.BackToPool();
				break;
			case 1:
				_refEntity.SetSpeed(0, 0);
				_refEntity.BulletCollider.BackToPool();
				break;
			}
		}
		ResetSkill();
	}

	private void ResetSkill()
	{
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public void TeleportInExtraEffect()
	{
		PlaySkillSE("mr_start01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public void UpdateAndActivateBulletCollider(WeaponStruct data)
	{
		_refEntity.BulletCollider.UpdateBulletData(data.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
		_refEntity.BulletCollider.SetBulletAtk(data.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_refEntity.BulletCollider.BulletLevel = data.SkillLV;
		_refEntity.BulletCollider.Active(_refEntity.TargetMask);
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch045_skill_01_up_start", "ch045_skill_01_mid_start", "ch045_skill_01_down_start" };
		string[] array2 = new string[3] { "ch045_skill_01_up_loop", "ch045_skill_01_mid_loop", "ch045_skill_01_down_loop" };
		string[] array3 = new string[3] { "ch045_skill_02_crouch_up", "ch045_skill_02_crouch_mid", "ch045_skill_02_crouch_down" };
		string[] array4 = new string[3] { "ch045_skill_02_stand_up", "ch045_skill_02_stand_mid", "ch045_skill_02_stand_down" };
		string[] array5 = new string[3] { "ch045_skill_02_run_up_loop", "ch045_skill_02_run_mid_loop", "ch045_skill_02_run_down_loop" };
		string[] array6 = new string[3] { "ch045_skill_02_jump_up", "ch045_skill_02_jump_mid", "ch045_skill_02_jump_down" };
		return new string[6][] { array, array2, array3, array4, array5, array6 };
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[5] { "ch045_skill_01_start", "ch045_skill_01_loop", "ch045_skill_01_end", "ch045_skill_02_teleport_stand", "ch045_skill_02_teleport_jump" };
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_marinohalloween_in";
	}
}
