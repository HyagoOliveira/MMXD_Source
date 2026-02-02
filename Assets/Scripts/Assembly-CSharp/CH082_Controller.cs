using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

public class CH082_Controller : CharacterControlBase, IPetSummoner
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private GameObject WeaponMesh_c;

	private Transform shootPointTransform0;

	private Transform shootPointTransform1;

	private Transform shootPointTransform2;

	protected List<SCH006Controller> _liPets = new List<SCH006Controller>();

	private bool isPlayTeleportOut;

	private readonly string sFxuse000_0 = "fxuse_machinegunstring_000";

	private readonly string sFxuse001 = "fxuse_pulsesong_000";

	private readonly string sFxuseWin = "fxuse_ch082_win";

	private readonly string sWeaponMesh_c = "WeaponMesh_c";

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly int SKL0_0_TRIGGER = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_0_END = (int)(0.533f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_0_END_BREAK = (int)(0.25f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_1_TRIGGER = (int)(0.22f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_1_END = (int)(0.533f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_1_END_BREAK = (int)(0.28f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.22f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.533f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.28f / GameLogicUpdateManager.m_fFrameLen);

	public int PetID { get; set; } = -1;


	public long PetTime { get; set; }

	public int PetCount { get; set; }

	private float GetSkl0Range
	{
		get
		{
			return _refEntity.PlayerSkills[0].BulletData.f_DISTANCE + 2f;
		}
	}

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		_refEntity.AnimatorModelShiftYOverride = new Better.Dictionary<OrangeCharacter.MainStatus, float>
		{
			{
				OrangeCharacter.MainStatus.TELEPORT_IN,
				0f
			},
			{
				OrangeCharacter.MainStatus.TELEPORT_OUT,
				0f
			},
			{
				OrangeCharacter.MainStatus.SKILL,
				0f
			}
		};
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportInCharacterDependeEndEvt = TeleportInCharacterDependEnd;
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.GetCurrentAimRangeEvt = GetCurrentAimRange;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	public void TeleportInCharacterDepend()
	{
		if (_refEntity.CurrentFrame >= 0.78f)
		{
			UpdateCustomWeaponRenderer(false);
		}
	}

	public void TeleportInCharacterDependEnd()
	{
		UpdateCustomWeaponRenderer(false);
	}

	public void TeleportOutCharacterDepend()
	{
		if (!isPlayTeleportOut)
		{
			isPlayTeleportOut = true;
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_WIN_POSE)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuseWin, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			}
			UpdateCustomWeaponRenderer(true);
		}
	}

	private void InitializeSkill()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse000_0);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse001);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		shootPointTransform0 = new GameObject(sCustomShootPoint + "0").transform;
		shootPointTransform0.SetParent(base.transform);
		shootPointTransform0.localPosition = new Vector3(0f, 0.8f, 0f);
		shootPointTransform1 = new GameObject(sCustomShootPoint + "1").transform;
		shootPointTransform1.SetParent(shootPointTransform0);
		shootPointTransform1.localPosition = new Vector3(0f, 0f, 0f);
		shootPointTransform2 = new GameObject(sCustomShootPoint + "2").transform;
		shootPointTransform2.SetParent(base.transform);
		shootPointTransform2.localPosition = new Vector3(0f, 0.8f, 0f);
		_refEntity.PlayerSkills[0].ShootTransform[0] = shootPointTransform1;
		_refEntity.PlayerSkills[1].ShootTransform2[0] = shootPointTransform2;
		WeaponMesh_c = OrangeBattleUtility.FindChildRecursive(ref target, sWeaponMesh_c, true).gameObject;
		if (_refEntity.tRefPassiveskill.listUsePassiveskill.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < _refEntity.tRefPassiveskill.listUsePassiveskill.Count; i++)
		{
			SKILL_TABLE tSKILL_TABLE = _refEntity.tRefPassiveskill.listUsePassiveskill[i].tSKILL_TABLE;
			if (tSKILL_TABLE.n_EFFECT == 16)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH006Controller>(this, _refEntity, 1, tSKILL_TABLE);
				break;
			}
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
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
				_refEntity.CurrentActiveSkill = id;
				_refEntity.IsShoot = 1;
				int reload_index = _refEntity.GetCurrentSkillObj().Reload_index;
				UpdateSkillDistance();
				if (reload_index == 0 || reload_index != 1)
				{
					PlaySkillSE("hn_string01");
					endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_0_END_BREAK;
					ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_0_TRIGGER, SKL0_0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				}
				else
				{
					PlaySkillSE("hn_string02");
					endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_1_END_BREAK;
					ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_1_TRIGGER, SKL0_1_END, OrangeCharacter.SubStatus.SKILL0_1, out skillEventFrame, out endFrame);
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
				}
				UpdateCustomWeaponRenderer(true);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.IsShoot = 1;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
				UpdateCustomWeaponRenderer(true);
				PlaySkillSE("hn_pulsesong01");
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse001, shootPointTransform2.position, Quaternion.identity, Array.Empty<object>());
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
				SKILL0();
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				CheckBreakFrame();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				SKILL0_1();
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				CheckBreakFrame();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				SKILL1();
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				CheckBreakFrame();
			}
			break;
		}
	}

	private void UpdateSkillDistance(bool useSklDistance = true)
	{
		if (useSklDistance)
		{
			if (!(GetSkl0Range <= _refEntity.PlayerAutoAimSystem.Range))
			{
				_refEntity.PlayerAutoAimSystem.UpdateAimRange(GetSkl0Range);
				_refEntity.UpdateAimDirection();
			}
		}
		else
		{
			_refEntity.UpdateAimRangeByWeapon(_refEntity.GetCurrentWeaponObj());
		}
	}

	public override float GetCurrentAimRange()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 19) <= 1u)
			{
				return GetSkl0Range;
			}
		}
		return _refEntity.GetCurrentAimRange();
	}

	private void UpdateCustomWeaponRenderer(bool enableWeapon)
	{
		if (WeaponMesh_c.activeSelf != enableWeapon)
		{
			WeaponMesh_c.SetActive(enableWeapon);
		}
	}

	public override void ClearSkill()
	{
		if (!_refEntity.CheckIsLocalPlayer())
		{
			OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
			if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
			{
				switch (_refEntity.CurSubStatus)
				{
				case OrangeCharacter.SubStatus.SKILL0:
					SKILL0();
					break;
				case OrangeCharacter.SubStatus.SKILL0_1:
					SKILL0_1();
					break;
				case OrangeCharacter.SubStatus.SKILL1:
					SKILL1();
					break;
				}
			}
		}
		UpdateCustomWeaponRenderer(false);
		_refEntity.EnableCurrentWeapon();
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		UpdateSkillDistance(false);
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			UpdateCustomWeaponRenderer(false);
			_refEntity.EnableCurrentWeapon();
		}
	}

	private void CheckBreakFrame()
	{
		if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
		{
			endFrame = nowFrame + 1;
		}
	}

	private void SKILL0()
	{
		isSkillEventEnd = true;
		float z = Vector2.SignedAngle(Vector2.right, _refEntity.ShootDirection);
		shootPointTransform0.eulerAngles = new Vector3(0f, 0f, z);
		int reload_index = _refEntity.GetCurrentSkillObj().Reload_index;
		ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, shootPointTransform1, MagazineType.NORMAL, reload_index, 1);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse000_0, shootPointTransform1.position, shootPointTransform0.rotation, Array.Empty<object>());
	}

	private void SKILL0_1()
	{
		isSkillEventEnd = true;
		WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
		int reload_index = currentSkillObj.Reload_index;
		ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, shootPointTransform2, MagazineType.ENERGY, reload_index, 1);
		ComboCheckData[] comboCheckDatas = currentSkillObj.ComboCheckDatas;
		for (int i = 0; i < comboCheckDatas.Length; i++)
		{
			_refEntity.RemoveComboSkillBuff(comboCheckDatas[i].nComboSkillID);
		}
		currentSkillObj.Reload_index = 0;
	}

	private void SKILL1()
	{
		isSkillEventEnd = true;
		int reload_index = _refEntity.GetCurrentSkillObj().Reload_index;
		ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, shootPointTransform2, MagazineType.ENERGY, reload_index, 0);
	}

	private void OnSkillEnd()
	{
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		UpdateCustomWeaponRenderer(false);
		_refEntity.EnableCurrentWeapon();
		UpdateSkillDistance(false);
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

	public override void CallPet(int petID, bool isHurt, int nSetNumID, Vector3? vSetPos = null)
	{
		SCH006Controller sCH006Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH006Controller>(this, _refEntity, PetID, nSetNumID, true, false, false);
		if (!(sCH006Controller != null))
		{
			return;
		}
		sCH006Controller.activeSE = new string[2] { "SkillSE_HARPNOTE", "hn_speaker01_lp" };
		sCH006Controller.unactiveSE = new string[2] { "SkillSE_HARPNOTE", "hn_speaker01_lp" };
		sCH006Controller.StartOffset = new Vector3(0f, 1f, -0.5f);
		sCH006Controller.EndOffset = new Vector3(0f, 1f, 0f);
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

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length == 2)
		{
			int num = (int)parameters[0];
			int num2 = (int)parameters[1];
			if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_IN && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT && (int)_refEntity.Hp > 0 && num == 0 && _refEntity.PlayerSkills[0].Reload_index != num2)
			{
				_refEntity.PlayerSkills[0].Reload_index = num2;
			}
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
		return new string[6] { "ch082_skill_01_crouch", "ch082_skill_01_stand", "ch082_skill_01_jump", "ch082_skill_02_crouch", "ch082_skill_02_stand", "ch082_skill_02_jump" };
	}
}
