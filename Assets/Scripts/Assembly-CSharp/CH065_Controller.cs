using System;
using System.Collections.Generic;
using UnityEngine;

public class CH065_Controller : CharacterControlBase, IPetSummoner
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	protected List<SCH006Controller> _liPets = new List<SCH006Controller>();

	private Transform shootPointTransformR;

	private Transform shootPointTransformL;

	private MeleeWeaponTrail trail;

	private readonly string sFxuse000 = "fxuse_spanner_000";

	private readonly string sFxuse001 = "fxuse_remotecharge_000";

	private readonly string sTrail = "Trail";

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly int SKL0_TRIGGER = (int)(0.192f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.51f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_BREAK = (int)(0.27f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.212f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.641f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_BREAK = (int)(0.27f / GameLogicUpdateManager.m_fFrameLen);

	public int PetID { get; set; } = -1;


	public long PetTime { get; set; }

	public int PetCount { get; set; }

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		shootPointTransformR = new GameObject(sCustomShootPoint).transform;
		shootPointTransformR.SetParent(base.transform);
		shootPointTransformR.localPosition = new Vector3(0.5f, 0.8f, 0f);
		shootPointTransformL = new GameObject(sCustomShootPoint).transform;
		shootPointTransformL.SetParent(base.transform);
		shootPointTransformL.localPosition = new Vector3(-0.5f, 0.8f, 0f);
		trail = OrangeBattleUtility.FindChildRecursive(ref target, sTrail, true).GetComponent<MeleeWeaponTrail>();
		trail.Emit = false;
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH006Controller>(this, _refEntity);
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.FreshBullet = true;
				_refEntity.IsShoot = 1;
				trail.Emit = true;
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse001, _refEntity._transform.position, Quaternion.identity, Array.Empty<object>());
				break;
			}
		}
	}

	public override void ClearSkill()
	{
		trail.Emit = false;
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
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
		nowFrame = GameLogicUpdateManager.GameFrame;
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.SKILL0)
			{
				if (nowFrame >= endFrame)
				{
					trail.Emit = false;
					_refEntity.CurrentActiveSkill = -1;
					OnSkillEnd();
				}
				else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
				{
					Transform transform = null;
					isSkillEventEnd = true;
					transform = ((_refEntity._characterDirection != CharacterDirection.LEFT) ? shootPointTransformR : shootPointTransformL);
					ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, transform, MagazineType.ENERGY, -1, 0);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse000, transform.position, Quaternion.identity, Array.Empty<object>());
				}
				else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
				{
					endFrame = nowFrame + 1;
				}
			}
			break;
		}
		case 1:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.SKILL1)
			{
				if (nowFrame >= endFrame)
				{
					_refEntity.CurrentActiveSkill = -1;
					OnSkillEnd();
				}
				else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
				{
					isSkillEventEnd = true;
					ManagedSingleton<CharacterControlHelper>.Instance.PushPet(this, _refEntity, PetID);
				}
				else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
				{
					endFrame = nowFrame + 1;
				}
			}
			break;
		}
		}
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID, Vector3? vSetPos = null)
	{
		SCH006Controller sCH006Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH006Controller>(this, _refEntity, PetID, nSetNumID, true, false, false);
		if (!(sCH006Controller != null))
		{
			return;
		}
		sCH006Controller.activeSE = new string[4] { "BattleSE02", "bt_bit01", "0.5", "0.5" };
		sCH006Controller.unactiveSE = new string[2] { "BattleSE02", "bt_bit02" };
		sCH006Controller.SetSkillLevel(_refEntity.PlayerSkills[1].SkillLV);
		sCH006Controller.SetActive(true);
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
		}
		_liPets.Add(sCH006Controller);
	}

	private void OnSkillEnd()
	{
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		switch (_refEntity.AnimateID)
		{
		default:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			break;
		case (HumanBase.AnimateId)66u:
		case (HumanBase.AnimateId)69u:
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case HumanBase.AnimateId.ANI_SKILL_START:
		case (HumanBase.AnimateId)68u:
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
		}
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
		return new string[6] { "ch065_skill_01_crouch", "ch065_skill_01_stand", "ch065_skill_01_jump", "ch065_skill_02_crouch", "ch065_skill_02_stand", "ch065_skill_02_jump" };
	}
}
