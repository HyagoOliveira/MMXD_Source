using System;
using UnityEngine;

public class CH121_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private BulletBase mSkillBullet;

	private int[] buffIds = new int[2] { -1, -2 };

	private CharacterMaterial weaponSurf;

	private SkinnedMeshRenderer meshWing;

	private int _enhanceSlot;

	private SKILL_TABLE linkSkl;

	private CharacterDirection sklDirection = CharacterDirection.LEFT;

	private ParticleSystem[] psWings = new ParticleSystem[4];

	private bool isPlayTeleportIn;

	private bool isPlayTeleportOut;

	[SerializeField]
	private int dashSpd = 21607;

	[SerializeField]
	private float dashTime = 0.35f;

	[SerializeField]
	private float linkSklTime = 0.2f;

	[SerializeField]
	private int startSpd = 2000;

	[SerializeField]
	private int upSpd = 12000;

	private readonly string sFX_SKL0_00 = "fxuse_surf_000";

	protected readonly int SKL0_START = (int)(0.233f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_END = (int)(0.556f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_TRIGGER = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END = (int)(0.833f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END_BREAK = (int)(0.45 / (double)GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_1_END_BREAK = (int)(0.45 / (double)GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	private void InitializeSkill()
	{
		ManagedSingleton<CharacterControlHelper>.Instance.InitEnhanceSkill(_refEntity, 1, new int[4] { 21731, 21733, 21731, 21731 }, ref _enhanceSlot);
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 2.4f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[1];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[2];
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "SurfMesh_c", true);
		weaponSurf = transform2.GetComponent<CharacterMaterial>();
		ToggleSkillWeapon(false);
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "TailMesh_g", true);
		meshWing = transform3.GetComponent<SkinnedMeshRenderer>();
		meshWing.enabled = true;
		Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref target, "bits_R1", true);
		Transform transform5 = OrangeBattleUtility.FindChildRecursive(ref target, "bits_R2", true);
		Transform transform6 = OrangeBattleUtility.FindChildRecursive(ref target, "bits_L1", true);
		Transform transform7 = OrangeBattleUtility.FindChildRecursive(ref target, "bits_L2", true);
		psWings[0] = transform4.GetComponent<ParticleSystem>();
		psWings[1] = transform5.GetComponent<ParticleSystem>();
		psWings[2] = transform6.GetComponent<ParticleSystem>();
		psWings[3] = transform7.GetComponent<ParticleSystem>();
		linkSkl = null;
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		if (weaponStruct != null && weaponStruct.BulletData.n_LINK_SKILL != 0 && linkSkl == null && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out linkSkl))
		{
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref linkSkl);
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<BasicBullet>("prefab/bullet/" + linkSkl.s_MODEL, linkSkl.s_MODEL, 4, null);
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFX_SKL0_00);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportInCharacterDependeEndEvt = TeleportInCharacterDependeEnd;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
	}

	public void TeleportInCharacterDepend()
	{
		if (!isPlayTeleportIn)
		{
			isPlayTeleportIn = true;
			ParticleSystem[] array = psWings;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play(false);
			}
			meshWing.enabled = true;
		}
	}

	public void TeleportInCharacterDependeEnd()
	{
		ParticleSystem[] array = psWings;
		foreach (ParticleSystem particleSystem in array)
		{
			if (!particleSystem.isPlaying)
			{
				particleSystem.Play(false);
			}
		}
		meshWing.enabled = true;
	}

	public void TeleportOutCharacterDepend()
	{
		if (isPlayTeleportOut || _refEntity.CurSubStatus != 0)
		{
			return;
		}
		float currentFrame = _refEntity.CurrentFrame;
		if (currentFrame > 1.5f && currentFrame <= 2f)
		{
			isPlayTeleportOut = true;
			ParticleSystem[] array = psWings;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
			meshWing.enabled = false;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			sklDirection = _refEntity._characterDirection;
			PlayVoiceSE("v_ic_skill03");
			PlaySkillSE("ic_surf01");
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
			_refEntity.CurrentActiveSkill = id;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, 1, SKL0_START, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			_refEntity.DisableWeaponMesh(weaponStruct, 0f);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
			_refEntity.CheckUsePassiveSkill(0, weaponStruct.BulletData, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
			ToggleSkillWeapon(true);
			CreateColliderBullet();
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1 || id != 1 || !_refEntity.CheckUseSkillKeyTrigger(id))
		{
			return;
		}
		int reload_index = _refEntity.PlayerSkills[1].Reload_index;
		if (reload_index == 0 || reload_index != 1)
		{
			PlayVoiceSE("v_ic_skill02");
			PlaySkillSE("ic_water01");
			_refEntity.CurrentActiveSkill = id;
			ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
			if (_enhanceSlot == 1)
			{
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
			}
			return;
		}
		endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_1_END_BREAK;
		if (mSkillBullet != null)
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(buffIds[0]);
			mSkillBullet.BackToPool();
			mSkillBullet = null;
		}
		PlaySkillSE("ic_water03");
		SKILL_TABLE sKILL_TABLE = _refEntity.PlayerSkills[1].FastBulletDatas[_refEntity.PlayerSkills[1].Reload_index];
		_refEntity.UpdateAimDirection();
		if (_refEntity.IsLocalPlayer)
		{
			CreateSkillBullet(_refEntity.PlayerSkills[1]);
			_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[0], null, _refEntity.PlayerSkills[1].Reload_index);
		}
		OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
		_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
	}

	public override void ClearSkill()
	{
		_refEntity.CancelBusterChargeAtk();
		ToggleSkillWeapon(false);
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
		if (enable)
		{
			ToggleSkillWeapon(false);
			_refEntity.EnableCurrentWeapon();
		}
	}

	public override void CheckSkill()
	{
		CheckSkillBullet();
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
				int p_endFrame = (int)(dashTime / GameLogicUpdateManager.m_fFrameLen);
				int p_sklTriggerFrame = Mathf.Clamp((int)(dashTime * linkSklTime / GameLogicUpdateManager.m_fFrameLen), 1, endFrame);
				isSkillEventEnd = false;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, p_sklTriggerFrame, p_endFrame, OrangeCharacter.SubStatus.SKILL0_1, out skillEventFrame, out endFrame);
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				_refEntity.IgnoreGravity = true;
				_refEntity.SetSpeed((int)sklDirection * dashSpd, 0);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFX_SKL0_00, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				_refEntity.IgnoreGravity = true;
				_refEntity.SetSpeed(0, startSpd);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_END - 2, SKL0_END, OrangeCharacter.SubStatus.SKILL0_2, out skillEventFrame, out endFrame);
				isSkillEventEnd = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				_refEntity.IgnoreGravity = false;
				_refEntity.SetSpeed((int)sklDirection * upSpd / 2, upSpd);
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
				if (linkSkl != null)
				{
					_refEntity.PushBulletDetail(linkSkl, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0], weaponStruct.SkillLV);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (nowFrame >= endFrame)
			{
				_refEntity.IgnoreGravity = false;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, 1, 1, OrangeCharacter.SubStatus.SKILL0_3, out skillEventFrame, out endFrame);
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ToggleSkillWeapon(false);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (nowFrame >= endFrame)
			{
				_refEntity.BulletCollider.BackToPool();
				OnSkillEnd();
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
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				if (_refEntity.IsLocalPlayer)
				{
					_refEntity.selfBuffManager.AddBuff(buffIds[0], 0, 0, 0);
				}
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[0]);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
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
		if (!_refEntity.IsLocalPlayer)
		{
			ToggleSkillWeapon(false);
		}
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != (HumanBase.AnimateId)68u)
		{
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

	private void ToggleSkillWeapon(bool enable)
	{
		if (enable)
		{
			weaponSurf.Appear(null, 0f);
		}
		else
		{
			weaponSurf.Disappear(null, 0f);
		}
	}

	private void CheckSkillBullet()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
		if (mSkillBullet != null)
		{
			if (_refEntity.IsDead())
			{
				mSkillBullet.BackToPool();
				mSkillBullet = null;
				int[] array = buffIds;
				foreach (int cONDITIONID in array)
				{
					_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(cONDITIONID);
				}
			}
			else if (!_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(buffIds[0]))
			{
				mSkillBullet.BackToPool();
				mSkillBullet = null;
				_refEntity.RemoveComboSkillBuff(weaponStruct.FastBulletDatas[weaponStruct.Reload_index].n_ID);
			}
			else if (mSkillBullet.bIsEnd)
			{
				_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(buffIds[0]);
				mSkillBullet = null;
				if (_enhanceSlot != 1)
				{
					_refEntity.PlayerSkills[1].MagazineRemain = 0f;
					OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				}
				_refEntity.RemoveComboSkillBuff(weaponStruct.FastBulletDatas[weaponStruct.Reload_index].n_ID);
			}
		}
		else if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(buffIds[0]) && mSkillBullet == null)
		{
			mSkillBullet = _refEntity.CreateFSBulletEx(weaponStruct, 0);
			mSkillBullet.transform.localRotation = Quaternion.identity;
		}
	}

	private void CreateColliderBullet()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		_refEntity.FreshBullet = true;
		_refEntity.BulletCollider.UpdateBulletData(weaponStruct.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
		_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
		_refEntity.BulletCollider.Active(_refEntity.TargetMask);
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[1], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[2], weaponStruct.SkillLV);
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(buffIds[1]))
		{
			_refEntity.PlayerSkills[1].MagazineRemain = 0f;
		}
		else if (mSkillBullet != null)
		{
			mSkillBullet.BackToPool();
		}
		CheckSkillBullet();
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length == 2)
		{
			int num = (int)parameters[0];
			int num2 = (int)parameters[1];
			if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_IN && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT && (int)_refEntity.Hp > 0 && num == 1 && _refEntity.PlayerSkills[1].Reload_index != num2)
			{
				_refEntity.PlayerSkills[1].Reload_index = num2;
			}
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch121_skill_01_start", "ch121_skill_01_loop", "ch121_skill_01_end", "ch121_skill_02_crouch", "ch121_skill_02_stand", "ch121_skill_02_jump" };
	}
}
