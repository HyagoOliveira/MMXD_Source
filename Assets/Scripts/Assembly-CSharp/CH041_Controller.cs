using System;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;

public class CH041_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private List<SKILL_TABLE> listLinkSkl = new List<SKILL_TABLE>();

	private SKILL_TABLE currentLinkSkl;

	private FxBase fx_Skl_0;

	private List<Transform> hitList = new List<Transform>();

	private List<FxBase> hitFxList = new List<FxBase>();

	private bool toggleWeaponFlg;

	private readonly string FX_0_00 = "p_waterfly_000";

	private readonly string FX_0_01 = "p_waterfly_001";

	private readonly string FX_1_00 = "fxuse_watermelon_000";

	private readonly string FX_1_001 = "fxuse_watermelon_0001";

	private readonly string FX_1_002 = "fxuse_watermelon_0002";

	private readonly string FX_1_01 = "fxuse_watermelon_001";

	private readonly string FX_1_02 = "fxhit_watermelon_002";

	private readonly float[] FX_1_02_ROTATION = new float[13]
	{
		-60f, -50f, -40f, -30f, -20f, -10f, 0f, 10f, 20f, 30f,
		40f, 50f, 60f
	};

	private int FX_1_02_ROTATION_Length = 1;

	private readonly int SKL0_START = (int)(0.133f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_START = 1;

	private readonly int SKL1_TRIGGER = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.8f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_HIT_STOP = 2;

	private int SKL0_LOOP_TIME;

	private int SKL0_DISTANCE;

	private int SKL0_SPEED;

	private readonly string SpWeaponMesh = "BokutoMesh_c";

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.STAGE_TIMESCALE_CHANGE, TimeScaleChange);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.STAGE_TIMESCALE_CHANGE, TimeScaleChange);
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
		InitThisSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ref target, SpWeaponMesh);
		Renderer[] extraMeshOpen;
		if (array != null)
		{
			OrangeCharacter refEntity = _refEntity;
			extraMeshOpen = new SkinnedMeshRenderer[array.Length];
			refEntity.ExtraMeshOpen = extraMeshOpen;
			for (int i = 0; i < array.Length; i++)
			{
				_refEntity.ExtraMeshOpen[i] = array[i].GetComponent<SkinnedMeshRenderer>();
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

	private void InitThisSkill()
	{
		listLinkSkl.Clear();
		for (int i = 0; i < _refEntity.PlayerSkills.Length; i++)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[i];
			if (weaponStruct != null && weaponStruct.BulletData.n_COMBO_SKILL != 0 && weaponStruct.BulletData.n_LINK_SKILL != 0)
			{
				AddLinkSkill(weaponStruct.BulletData.n_LINK_SKILL);
				SKILL_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(weaponStruct.BulletData.n_COMBO_SKILL, out value))
				{
					AddLinkSkill(value.n_LINK_SKILL);
				}
				break;
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_01, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_001, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_002, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_01, 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_02, 5);
		FX_1_02_ROTATION_Length = FX_1_02_ROTATION.Length;
	}

	private void AddLinkSkill(int linkSkillId)
	{
		SKILL_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(linkSkillId, out value))
		{
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref value);
			listLinkSkl.Add(value);
		}
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
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				return;
			}
			skillEventFrame = GameLogicUpdateManager.GameFrame + SKL0_START;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			_refEntity.PlaySE(_refEntity.VoiceID, "v_ly_skill03");
			break;
		case 1:
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				return;
			}
			skillEventFrame = GameLogicUpdateManager.GameFrame + SKL1_START;
			endFrame = GameLogicUpdateManager.GameFrame + SKL1_END;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			_refEntity.PlaySE(_refEntity.VoiceID, "v_ly_skill04");
			break;
		}
		_refEntity.SkillEnd = false;
		_refEntity.DisableCurrentWeapon();
		_refEntity.CurrentActiveSkill = id;
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

    [Obsolete]
    public override void CheckSkill()
	{
		nowFrame = GameLogicUpdateManager.GameFrame;
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1 || _refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			{
				if (nowFrame < skillEventFrame)
				{
					break;
				}
				SKILL_TABLE useSkill2 = null;
				bool num2 = HasComboSkill(0, out useSkill2);
				SKL0_LOOP_TIME = (int)((float)useSkill2.n_SPEED * 0.001f / GameLogicUpdateManager.m_fFrameLen);
				SKL0_DISTANCE = (int)useSkill2.f_DISTANCE;
				SKL0_SPEED = 1000 * SKL0_DISTANCE / SKL0_LOOP_TIME;
				skillEventFrame = nowFrame + SKL0_LOOP_TIME;
				_refEntity.BulletCollider.UpdateBulletData(useSkill2, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[0].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[0].SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[0]);
				if (num2)
				{
					_refEntity.RemoveComboSkillBuff(useSkill2.n_ID);
					fx_Skl_0 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_0_01, _refEntity.BulletCollider.transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
					if ((bool)fx_Skl_0)
					{
						fx_Skl_0.transform.localPosition = ((_refEntity._characterDirection == CharacterDirection.LEFT) ? new Vector3(-1f, 0f, 0f) : new Vector3(1f, 0f, 0f));
					}
				}
				else
				{
					fx_Skl_0 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_0_00, _refEntity.BulletCollider.transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
					if ((bool)fx_Skl_0)
					{
						fx_Skl_0.transform.localPosition = ((_refEntity._characterDirection == CharacterDirection.LEFT) ? new Vector3(-1f, 0f, 0f) : new Vector3(1f, 0f, 0f));
					}
				}
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
				break;
			}
			case OrangeCharacter.SubStatus.SKILL0_1:
				if (!isSkillEventEnd && nowFrame >= skillEventFrame)
				{
					isSkillEventEnd = true;
					_refEntity.SetHorizontalSpeed(0);
					endFrame = nowFrame + SKL0_END;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
				}
				else
				{
					_refEntity.SetHorizontalSpeed((int)_refEntity._characterDirection * SKL0_SPEED);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				if (nowFrame >= endFrame)
				{
					CancelSkl000();
				}
				else if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
				{
					endFrame = nowFrame + 1;
				}
				break;
			}
			break;
		case 1:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1:
				if (nowFrame >= skillEventFrame)
				{
					_refEntity.ToggleExtraMesh(true);
					skillEventFrame = nowFrame + SKL1_TRIGGER - SKL1_START;
					SKILL_TABLE useSkill = null;
					bool num = HasComboSkill(1, out useSkill);
					CollideBullet bulletCollider2 = _refEntity.BulletCollider;
					bulletCollider2.HitCallback = (CallbackObj)Delegate.Combine(bulletCollider2.HitCallback, new CallbackObj(OnHit));
					_refEntity.BulletCollider.UpdateBulletData(useSkill, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
					_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[1].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
					_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[1].SkillLV;
					_refEntity.BulletCollider.Active(_refEntity.TargetMask);
					_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[1]);
					OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
					if (num)
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_001, _refEntity._transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("DistortionFx", _refEntity._transform, Quaternion.identity, Array.Empty<object>());
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_002, _refEntity._transform.position, Quaternion.identity, Array.Empty<object>());
						currentLinkSkl = listLinkSkl[1];
						_refEntity.RemoveComboSkillBuff(useSkill.n_ID);
					}
					else
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_00, _refEntity._transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
						currentLinkSkl = listLinkSkl[0];
					}
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				if (nowFrame >= skillEventFrame)
				{
					_refEntity.BulletCollider.IsActivate = false;
					CollideBullet bulletCollider = _refEntity.BulletCollider;
					bulletCollider.HitCallback = (CallbackObj)Delegate.Remove(bulletCollider.HitCallback, new CallbackObj(OnHit));
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				if (!isSkillEventEnd)
				{
					isSkillEventEnd = true;
					if (hitList.Count > 0)
					{
						skillEventFrame = nowFrame + SKL1_HIT_STOP;
						endFrame += SKL1_HIT_STOP * 2;
						_refEntity.Animator._animator.speed = 0f;
						_refEntity.PlaySE(_refEntity.SkillSEID, "ly_meloncut02");
						_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
					}
					else
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_5);
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				if (nowFrame >= skillEventFrame)
				{
					skillEventFrame = nowFrame + SKL1_HIT_STOP;
					for (int i = 0; i < hitList.Count; i++)
					{
						hitFxList[i].BackToPool();
						_refEntity.BulletCollider.CaluDmg(currentLinkSkl, hitList[i]);
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_02, hitList[i].position, Quaternion.Euler(0f, 0f, FX_1_02_ROTATION[UnityEngine.Random.Range(0, FX_1_02_ROTATION_Length)]), Array.Empty<object>());
					}
					hitList.Clear();
					hitFxList.Clear();
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
				}
				else
				{
					_refEntity.Animator._animator.speed = ((nowFrame % 2 == 0) ? 0.05f : 0f);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				if (nowFrame >= skillEventFrame)
				{
					_refEntity.Animator._animator.speed = 1f;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_5);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				if (nowFrame >= endFrame)
				{
					CancelSkl001();
				}
				else if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
				{
					endFrame = nowFrame + 1;
				}
				break;
			}
			break;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			_refEntity.IgnoreGravity = true;
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
			}
			else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			}
			else
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (!_refEntity.Solid_meeting(0f, -1f, (int)_refEntity.Controller.collisionMask | (int)_refEntity.Controller.collisionMaskThrough))
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			}
			_refEntity.IgnoreGravity = false;
			_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)76u);
			}
			else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)75u);
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			_refEntity.EnableCurrentWeapon();
			_refEntity.ToggleExtraMesh(false);
			if (_refEntity.CurrentActiveSkill == 0)
			{
				if ((bool)_refEntity.BulletCollider)
				{
					_refEntity.BulletCollider.BackToPool();
				}
				if ((bool)fx_Skl_0)
				{
					fx_Skl_0.transform.SetParent(null);
					fx_Skl_0.pPS.Stop(true);
					fx_Skl_0 = null;
				}
			}
			else if (_refEntity.CurrentActiveSkill == 1)
			{
				CancelSkl001();
			}
		}
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		hitList.Clear();
		hitFxList.Clear();
	}

	private void CancelSkl000()
	{
		if ((bool)_refEntity.BulletCollider)
		{
			_refEntity.BulletCollider.BackToPool();
		}
		if ((bool)fx_Skl_0)
		{
			fx_Skl_0.transform.SetParent(null);
			fx_Skl_0.pPS.Stop(true);
			fx_Skl_0 = null;
		}
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		ResetLastStatus();
	}

	private void CancelSkl001()
	{
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.BulletCollider.HitCallback = null;
		_refEntity.BulletCollider.BackToPool();
		hitList.Clear();
		hitFxList.Clear();
		_refEntity.EnableCurrentWeapon();
		_refEntity.ToggleExtraMesh(false);
		if (_refEntity.Animator._animator.speed < 1f)
		{
			_refEntity.Animator._animator.speed = 1f;
		}
		ResetLastStatus();
	}

	private void ResetLastStatus()
	{
		switch (_refEntity.AnimateID)
		{
		case (HumanBase.AnimateId)67u:
		case (HumanBase.AnimateId)74u:
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case (HumanBase.AnimateId)70u:
		case (HumanBase.AnimateId)75u:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			break;
		case (HumanBase.AnimateId)73u:
		case (HumanBase.AnimateId)76u:
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
		case (HumanBase.AnimateId)68u:
		case (HumanBase.AnimateId)69u:
		case (HumanBase.AnimateId)71u:
		case (HumanBase.AnimateId)72u:
			break;
		}
	}

	private void OnHit(object obj)
	{
		Collider2D collider2D = obj as Collider2D;
		if (collider2D != null)
		{
			Transform transform = collider2D.transform;
			PlayerCollider component = transform.GetComponent<PlayerCollider>();
			if (!(component != null) || !component.IsDmgReduceShield())
			{
				hitList.Add(transform);
				hitFxList.Add(MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_1_01, transform, Quaternion.identity, Array.Empty<object>()));
			}
		}
	}

	public void TeleportInCharacterDepend()
	{
		if (_refEntity.CurrentFrame >= 0.7f)
		{
			_refEntity.ToggleExtraMesh(false);
		}
		else if (_refEntity.CurrentFrame > 0.3f)
		{
			_refEntity.ToggleExtraMesh(true);
		}
	}

	public void TeleportInExtraEffect()
	{
		_refEntity.SoundSource.PlaySE(_refEntity.SkillSEID, 10, 0.3f);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public void TeleportOutCharacterDepend()
	{
		if (!toggleWeaponFlg)
		{
			toggleWeaponFlg = true;
			_refEntity.ToggleExtraMesh(true);
		}
	}

	private bool HasComboSkill(int SkillIdx, out SKILL_TABLE useSkill)
	{
		if (_refEntity.PlayerSkills[SkillIdx].ComboCheckDatas.Length != 0 && _refEntity.PlayerSkills[SkillIdx].ComboCheckDatas[0].CheckHasAllBuff(_refEntity.selfBuffManager))
		{
			useSkill = _refEntity.PlayerSkills[SkillIdx].FastBulletDatas[1];
			return true;
		}
		useSkill = _refEntity.PlayerSkills[SkillIdx].BulletData;
		return false;
	}

	private void TimeScaleChange(bool isChange)
	{
		if (isChange)
		{
			if (_refEntity.CurrentActiveSkill == 1)
			{
				_refEntity.Animator._animator.speed = 1f;
			}
		}
		else if (_refEntity.CurrentActiveSkill != 1 && _refEntity.Animator._animator.speed != 1f)
		{
			_refEntity.Animator._animator.speed = 1f;
		}
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_layerswimsuit_in";
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[12]
		{
			"ch041_skill_01_stand_start", "ch041_skill_01_stand_loop", "ch041_skill_01_stand_end", "ch041_skill_01_jump_start", "ch041_skill_01_jump_loop", "ch041_skill_01_jump_end", "ch041_skill_01_crouch_start", "ch041_skill_01_crouch_loop", "ch041_skill_01_crouch_end", "ch041_skill_02_stand",
			"ch041_skill_02_jump", "ch041_skill_02_crouch"
		};
	}
}
