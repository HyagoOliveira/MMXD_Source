using System;
using System.Collections.Generic;
using UnityEngine;

public class CH039_Controller : CharacterControlBase, IPetSummoner
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	protected Transform shootPointTransform;

	private GameObject tabletMesh_c;

	private List<SKILL_TABLE> listPetSkillTable = new List<SKILL_TABLE>();

	protected List<SCH013Controller> _liPets = new List<SCH013Controller>();

	protected OrangeTimer ayTimer;

	protected int analyzePlayTime = 7200;

	private readonly string sCustomShootPoint = "CustomShootPoint";

	public int PetID { get; set; } = -1;


	public long PetTime { get; set; }

	public int PetCount { get; set; }

	protected virtual string[] Pcb_activeSE
	{
		get
		{
			return new string[2] { "SkillSE_RICO", "ri_replo02_lp" };
		}
	}

	protected virtual string[] Pcb_unactiveSE
	{
		get
		{
			return new string[2] { "SkillSE_RICO", "ri_replo02_stop" };
		}
	}

	protected virtual string[] AnalyzeSE
	{
		get
		{
			return new string[2] { "SkillSE_RICO", "ri_analyze02" };
		}
	}

	protected virtual string Fxuse000
	{
		get
		{
			return "fxuse_datamining_000";
		}
	}

	protected virtual string Fxuse001
	{
		get
		{
			return "fxuse_datamining_001";
		}
	}

	protected virtual string TabletMesh_c
	{
		get
		{
			return "TabletMesh_c";
		}
	}

	protected virtual int SKL0_TRIGGER
	{
		get
		{
			return (int)(0.44f / GameLogicUpdateManager.m_fFrameLen);
		}
	}

	protected virtual int SKL0_END
	{
		get
		{
			return (int)(1f / GameLogicUpdateManager.m_fFrameLen);
		}
	}

	protected virtual int SKL0_END_BREAK
	{
		get
		{
			return (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);
		}
	}

	protected virtual int SKL1_TRIGGER
	{
		get
		{
			return (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);
		}
	}

	protected virtual int SKL1_END
	{
		get
		{
			return (int)(1f / GameLogicUpdateManager.m_fFrameLen);
		}
	}

	protected virtual int SKL1_END_BREAK
	{
		get
		{
			return (int)(0.45 / (double)GameLogicUpdateManager.m_fFrameLen);
		}
	}

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.PlayerHeldSkillCB = PlayerHeldSkill;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	public virtual void TeleportInCharacterDepend()
	{
		if (_refEntity.CurrentFrame >= 0.9f)
		{
			UpdateCustomWeaponRenderer(false);
		}
	}

	public void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 0.9f && currentFrame <= 1f)
			{
				UpdateCustomWeaponRenderer(false);
			}
		}
	}

	protected virtual void InitializeSkill()
	{
		shootPointTransform = new GameObject(sCustomShootPoint).transform;
		shootPointTransform.SetParent(base.transform);
		shootPointTransform.localPosition = new Vector3(0.7f, 1.1f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		tabletMesh_c = OrangeBattleUtility.FindChildRecursive(ref target, TabletMesh_c, true).gameObject;
		tabletMesh_c.SetActive(true);
		listPetSkillTable = ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH013Controller>(this, _refEntity);
		ayTimer = OrangeTimerManager.GetTimer();
		analyzePlayTime = 7200;
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
			if (_refEntity.selfBuffManager.nMeasureNow >= _refEntity.PlayerSkills[0].BulletData.n_USE_COST && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				UpdateCustomWeaponRenderer(true);
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				UpdateCustomWeaponRenderer(true);
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public void PlayerHeldSkill(int id)
	{
	}

	public override void ClearSkill()
	{
		UpdateCustomWeaponRenderer(false);
		_refEntity.EnableCurrentWeapon();
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.BulletCollider.BackToPool();
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			UpdateCustomWeaponRenderer(false);
			_refEntity.EnableCurrentWeapon();
		}
	}

	public override void CheckSkill()
	{
		nowFrame = GameLogicUpdateManager.GameFrame;
		if (ayTimer.IsStarted() && ayTimer.GetMillisecond() > analyzePlayTime)
		{
			string[] analyzeSE = AnalyzeSE;
			if (analyzeSE.Length == 2)
			{
				PlaySE(analyzeSE[0], analyzeSE[1]);
			}
			ayTimer.TimerStop();
		}
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
					_refEntity.CurrentActiveSkill = -1;
					OnSkillEnd();
				}
				else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
				{
					isSkillEventEnd = true;
					SKILL0();
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
					SKILL1();
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

	protected virtual void SKILL0()
	{
		CallPet(PetID, false, -1, null);
		_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[_refEntity.CurrentActiveSkill]);
		_refEntity.selfBuffManager.AddMeasure(-_refEntity.PlayerSkills[0].BulletData.n_USE_COST);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Fxuse000, _refEntity._transform.position, Quaternion.identity, Array.Empty<object>());
	}

	protected virtual void SKILL1()
	{
		WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
		_refEntity.PushBulletDetail(currentSkillObj.BulletData, currentSkillObj.weaponStatus, _refEntity.Controller.LogicPosition.vec3, currentSkillObj.SkillLV, Vector3.zero, false, 1);
		_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj.weaponStatus, _refEntity.ModelTransform);
		OrangeBattleUtility.UpdateSkillCD(currentSkillObj);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Fxuse001, _refEntity._transform, OrangeCharacter.NormalQuaternion, new Vector3((float)_refEntity._characterDirection, 1f, 1f), Array.Empty<object>());
		ayTimer.TimerStart();
	}

	private void OnSkillEnd()
	{
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		UpdateCustomWeaponRenderer(false);
		_refEntity.EnableCurrentWeapon();
		switch (_refEntity.AnimateID)
		{
		default:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			break;
		case HumanBase.AnimateId.ANI_SKILL_START:
		case (HumanBase.AnimateId)68u:
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case (HumanBase.AnimateId)67u:
		case (HumanBase.AnimateId)70u:
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

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			UpdateTeleportOut(subStatus);
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)67u, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)70u, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u);
				break;
			}
			break;
		}
	}

	protected virtual void UpdateTeleportOut(OrangeCharacter.SubStatus subStatus)
	{
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.WIN_POSE:
			UpdateCustomWeaponRenderer(true);
			break;
		case OrangeCharacter.SubStatus.TELEPORT_POSE:
			UpdateCustomWeaponRenderer(true);
			break;
		}
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID, Vector3? vSetPos = null)
	{
		SCH013Controller sCH013Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH013Controller>(this, _refEntity, petID, nSetNumID, true, false, false);
		if (!sCH013Controller)
		{
			return;
		}
		sCH013Controller.ReplaceListBulletSkillTable(listPetSkillTable, nSetNumID == -1);
		sCH013Controller.SetSkillLevel(_refEntity.PlayerSkills[0].SkillLV);
		Vector3 localPosition = shootPointTransform.localPosition;
		sCH013Controller.StartOffset = new Vector3(0f, localPosition.y, localPosition.z);
		sCH013Controller.EndOffset = new Vector3(localPosition.x * (float)_refEntity._characterDirection, localPosition.y, localPosition.z);
		sCH013Controller.activeSE = Pcb_activeSE;
		sCH013Controller.unactiveSE = Pcb_unactiveSE;
		sCH013Controller.SetActive(true);
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
		}
		_liPets.Add(sCH013Controller);
	}

	protected void UpdateCustomWeaponRenderer(bool enableWeapon)
	{
		tabletMesh_c.SetActive(enableWeapon);
	}

	public virtual bool CheckPetActive(int petId)
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
		return new string[6] { "ch039_skill_01_stand", "ch039_skill_01_jump", "ch039_skill_01_crouch", "ch039_skill_02_stand", "ch039_skill_02_jump", "ch039_skill_02_crouch" };
	}
}
