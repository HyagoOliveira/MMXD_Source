using System;
using System.Collections.Generic;
using UnityEngine;

public class CH090_Controller : CharacterControlBase, IPetSummoner
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	protected List<SCH006Controller> _liPets = new List<SCH006Controller>();

	private Transform shootPointTransform;

	private GameObject Object_BeachChair_Mesh_c;

	private GameObject Object_Drink_Mesh_c;

	private CH090_Kobun _refKobun;

	private float shootDirectionX;

	private bool isBossPose;

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly string sObject_BeachChair_Mesh_c = "Object_BeachChair_Mesh_c";

	private readonly string sObject_Drink_Mesh_c = "Object_Drink_Mesh_c";

	private readonly string sFxuse000 = "fxuse_balkon_000";

	private readonly string sFxuse001 = "fxuse_jagd_000";

	private readonly int SKL0_TRIGGER = (int)(0.28f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.433f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.62f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.215f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.667f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.467f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK_AIR = (int)(0.467f / GameLogicUpdateManager.m_fFrameLen);

	public int PetID { get; set; } = -1;


	public long PetTime { get; set; }

	public int PetCount { get; set; }

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		shootPointTransform = new GameObject(sCustomShootPoint).transform;
		shootPointTransform.SetParent(base.transform);
		shootPointTransform.localPosition = new Vector3(0f, 0.85f, 0f);
		Object_BeachChair_Mesh_c = OrangeBattleUtility.FindChildRecursive(ref target, sObject_BeachChair_Mesh_c, true).gameObject;
		Object_Drink_Mesh_c = OrangeBattleUtility.FindChildRecursive(ref target, sObject_Drink_Mesh_c, true).gameObject;
		Object_Drink_Mesh_c.SetActive(false);
		_refKobun = GetComponentInChildren<CH090_Kobun>();
		_refKobun.gameObject.SetActive(false);
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH006Controller>(this, _refEntity);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportInCharacterDependeEndEvt = TeleportInCharacterDependeEnd;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	public void TeleportInExtraEffect()
	{
		PlayVoiceSE("v_tr2_start01");
	}

	public void TeleportInCharacterDependeEnd()
	{
		Object_BeachChair_Mesh_c.SetActive(false);
	}

	public void TeleportOutCharacterDepend()
	{
		float currentFrame = _refEntity.CurrentFrame;
		switch (_refEntity.AnimateIDPrev)
		{
		case HumanBase.AnimateId.ANI_WIN_POSE:
			isBossPose = true;
			if (_refEntity.CurrentFrame >= 0.83f)
			{
				Object_Drink_Mesh_c.SetActive(false);
			}
			else if (_refEntity.CurrentFrame >= 0.1f)
			{
				Object_Drink_Mesh_c.SetActive(true);
			}
			Object_BeachChair_Mesh_c.SetActive(true);
			if (!_refKobun.gameObject.activeSelf)
			{
				_refKobun.gameObject.SetActive(true);
				_refKobun.Play(_refEntity.AnimateID);
				PlayVoiceSE("v_tr2_win01");
			}
			break;
		case HumanBase.AnimateId.ANI_TELEPORT_OUT_POSE:
			if (isBossPose)
			{
				isBossPose = false;
				_refKobun.gameObject.SetActive(true);
				_refEntity.CharacterMaterials.SetSubCharacterMaterial(_refKobun.gameObject);
				_refKobun.Play(_refEntity.AnimateID);
			}
			Object_BeachChair_Mesh_c.SetActive(false);
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
			shootDirectionX = 0.6f * (float)_refEntity._characterDirection;
			WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
			_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[_refEntity.CurrentActiveSkill]);
			OrangeBattleUtility.UpdateSkillCD(currentSkillObj);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse000, _refEntity._transform.position + new Vector3(shootDirectionX, 0f, 0f), Quaternion.identity, Array.Empty<object>());
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START);
			PlayVoiceSE("v_tr2_skill01");
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)66u);
			PlayVoiceSE("v_tr2_skill02");
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

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			_refEntity.EnableCurrentWeapon();
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
				_refEntity.CurrentActiveSkill = -1;
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushPet(this, _refEntity, PetID, false);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				_refEntity.CurrentActiveSkill = -1;
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				isSkillEventEnd = true;
				_refEntity.PushBulletDetail(currentSkillObj.BulletData, currentSkillObj.weaponStatus, shootPointTransform, currentSkillObj.SkillLV, Vector3.right * _refEntity.direction);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(currentSkillObj);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse001, shootPointTransform, Quaternion.identity, Array.Empty<object>());
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		}
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID, Vector3? vSetPos = null)
	{
		SCH006Controller sCH006Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH006Controller>(this, _refEntity, PetID, nSetNumID, true, false, false);
		if (!(sCH006Controller != null))
		{
			return;
		}
		sCH006Controller.activeSE = new string[2] { "SkillSE_TRON2", "tr2_b01" };
		sCH006Controller.unactiveSE = new string[2] { "SkillSE_TRON2", "tr2_b03" };
		if (shootDirectionX == 0f)
		{
			shootDirectionX = 0.6f * (float)_refEntity._characterDirection;
		}
		sCH006Controller.StartOffset = new Vector3(shootDirectionX, 0.5f, 0f);
		sCH006Controller.EndOffset = new Vector3(shootDirectionX, 1f, 0f);
		sCH006Controller.SetSkillLevel(_refEntity.PlayerSkills[0].SkillLV);
		sCH006Controller.SetActive(true);
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
		}
		_liPets.Add(sCH006Controller);
		shootDirectionX = 0f;
	}

	private void OnSkillEnd()
	{
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.Dashing = false;
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			return;
		}
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
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

	public override string[] GetCharacterDependAnimations()
	{
		return new string[2] { "ch090_skill_01_stand", "ch090_skill_02_stand" };
	}
}
