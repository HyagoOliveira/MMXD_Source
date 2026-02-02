using System;
using System.Collections.Generic;
using UnityEngine;

public class CH112_Controller : CharacterControlBase, IPetSummoner
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private SKILL_TABLE linkSkl;

	protected List<SCH022Controller> _liPets = new List<SCH022Controller>();

	private ParticleSystem characterFx;

	private bool IsStopCharacterFx;

	private readonly string sFxuse_skl000 = "fxuse_sacred_000";

	private readonly string sFxuse_skl100 = "fxuse_apostle_000";

	private readonly int SKL0_TRIGGER = (int)(0.135f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.175f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);

	public int PetID { get; set; } = -1;


	public long PetTime { get; set; }

	public int PetCount { get; set; }

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	protected virtual void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "ParticleTrans", true);
		if ((bool)transform)
		{
			characterFx = transform.GetComponent<ParticleSystem>();
		}
		linkSkl = null;
		for (int i = 0; i < _refEntity.PlayerSkills.Length; i++)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[i];
			if (weaponStruct != null && weaponStruct.BulletData.n_LINK_SKILL != 0 && linkSkl == null && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out linkSkl))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref linkSkl);
				GameObject obj = new GameObject();
				CollideBullet go = obj.AddComponent<CollideBullet>();
				obj.name = linkSkl.s_MODEL;
				obj.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(go, linkSkl.s_MODEL);
				break;
			}
		}
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH022Controller>(this, _refEntity);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl000, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl100, 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	public void TeleportOutCharacterDepend()
	{
		if (!IsStopCharacterFx && (bool)characterFx)
		{
			IsStopCharacterFx = true;
			characterFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_er_skill01");
				PlaySkillSE("er_holy01");
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				_refEntity.DisableCurrentWeapon();
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_er_skill02");
				PlaySkillSE("er_ghost01");
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse_skl100, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
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
		case OrangeCharacter.SubStatus.SKILL0:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ModelTransform, MagazineType.ENERGY, -1, 0);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse_skl000, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				Vector3 vec = _refEntity.Controller.LogicPosition.vec3;
				if (linkSkl != null)
				{
					_refEntity.PushBulletDetail(linkSkl, _refEntity.GetCurrentSkillObj().weaponStatus, vec, _refEntity.GetCurrentSkillObj().SkillLV, Vector3.zero, false, 1);
					_refEntity.CheckUsePassiveSkill(0, linkSkl, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
				}
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
				ManagedSingleton<CharacterControlHelper>.Instance.PushPet(this, _refEntity, PetID);
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

	public override void ClearSkill()
	{
		_refEntity.EnableCurrentWeapon();
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID, Vector3? vSetPos = null)
	{
		SCH022Controller sCH022Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH022Controller>(this, _refEntity, PetID, nSetNumID, true, false, false);
		if (!(sCH022Controller != null))
		{
			return;
		}
		sCH022Controller.activeSE = new string[2] { "SkillSE_ERATO", "er_ghost02_lp" };
		sCH022Controller.unactiveSE = new string[2] { "SkillSE_ERATO", "er_ghost02_stop" };
		sCH022Controller.transform.SetParentNull();
		sCH022Controller.SetFollowOffset(new Vector3(0f, 0.8f, 0f));
		sCH022Controller.SetActive(true);
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
		}
		_liPets.Add(sCH022Controller);
	}

	public bool CheckPetActive(int petId)
	{
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
			else if (_liPets[num].PetID == petId)
			{
				return true;
			}
		}
		return false;
	}

	public override void ControlCharacterDead()
	{
		RemovePet();
	}

	private new void RemovePet()
	{
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || _liPets[num].Activate)
			{
				_liPets[num].SetActive(false);
				_liPets.RemoveAt(num);
			}
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch112_skill_01_crouch", "ch112_skill_01_jump", "ch112_skill_01_jump", "ch112_skill_02_crouch", "ch112_skill_02_stand", "ch112_skill_02_jump" };
	}
}
