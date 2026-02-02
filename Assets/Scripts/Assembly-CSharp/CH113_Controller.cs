using System;
using NaughtyAttributes;
using UnityEngine;

public class CH113_Controller : CharacterControlBase
{
	[SerializeField]
	[ReadOnly]
	private int nowFrame;

	[SerializeField]
	[ReadOnly]
	private int skillEventFrame;

	[SerializeField]
	[ReadOnly]
	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	protected int _enhanceSlot;

	private Vector3? _targetPos;

	private bool _isTeleporation;

	private CharacterMaterial saberCM;

	private MeleeWeaponTrail saberTrail;

	private FxBase fxUseSkl0;

	private CH113_FxSet fxSet;

	private bool IsStopFxSet;

	private readonly string sFxuse_skl000 = "fxuse_rekkohr_000";

	private readonly string sFxuse_skl001 = "fxuse_rekkohr_001";

	private readonly string sFxuse_skl100 = "fxuse_shippuuga_000";

	private readonly string sFxuse_skl101 = "p_shippuuga_000";

	private readonly int SKL0_TRIGGER = (int)(0.45f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.7f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_START_END = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_LOOP_END = (int)(0.417f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_END = (int)(0.511f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.333f / GameLogicUpdateManager.m_fFrameLen);

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
		InitEnhanceSkill();
		InitializeSkill();
	}

	protected virtual void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		GameObject gameObject = OrangeBattleUtility.FindChildRecursive(ref target, "CH113_Saber", true).gameObject;
		if ((bool)gameObject)
		{
			saberTrail = gameObject.GetComponent<MeleeWeaponTrail>();
			saberCM = gameObject.GetComponent<CharacterMaterial>();
			saberCM.Appear();
		}
		fxSet = _refEntity.GetComponentInChildren<CH113_FxSet>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl000);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl001);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl100);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl101);
	}

	private void InitEnhanceSkill()
	{
		_enhanceSlot = _refEntity.PlayerSkills[0].EnhanceEXIndex;
		int skillId = (new int[4] { 20701, 20701, 20701, 20702 })[_enhanceSlot];
		_refEntity.ReInitSkillStruct(0, skillId);
		for (int i = 0; i < _refEntity.PlayerSkills[0].FastBulletDatas.Length; i++)
		{
			string s_MODEL = _refEntity.PlayerSkills[0].FastBulletDatas[i].s_MODEL;
			if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(s_MODEL) && !MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(s_MODEL))
			{
				BulletBase.PreloadBullet<BasicBullet>(_refEntity.PlayerSkills[0].FastBulletDatas[i]);
			}
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
	}

	public void TeleportInCharacterDepend()
	{
		if (_refEntity.CurrentFrame >= 0.9f)
		{
			UpdateCustomWeaponRenderer(false);
		}
	}

	public void TeleportOutCharacterDepend()
	{
		if (!IsStopFxSet && (bool)fxSet)
		{
			IsStopFxSet = true;
			fxSet.FxStop();
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
				PlayVoiceSE("v_vi_skill01");
				OrangeCharacter.SubStatus subStatus = OrangeCharacter.SubStatus.SKILL0;
				int enhanceSlot = _enhanceSlot;
				string pFxName;
				if ((uint)enhanceSlot <= 2u || enhanceSlot != 3)
				{
					PlaySkillSE("vi_sou01");
					subStatus = OrangeCharacter.SubStatus.SKILL0;
					pFxName = sFxuse_skl000;
				}
				else
				{
					PlaySkillSE("vi_sou02");
					subStatus = OrangeCharacter.SubStatus.SKILL0_1;
					pFxName = sFxuse_skl001;
				}
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, subStatus, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				fxUseSkl0 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(pFxName, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			}
			break;
		case 1:
		{
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			PlayVoiceSE("v_vi_skill02");
			UpdateCustomWeaponRenderer(true, true);
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_START_END, SKL1_START_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			PlayerAutoAimSystem playerAutoAimSystem = _refEntity.PlayerAutoAimSystem;
			IAimTarget aimTarget = playerAutoAimSystem.AutoAimTarget;
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[id];
			if (IsPVPMode && aimTarget != null)
			{
				float magnitude = (aimTarget.AimPosition - _refEntity.AimPosition).magnitude;
				if (!playerAutoAimSystem.IsInsideScreenExactly(aimTarget.AimPosition) || magnitude > weaponStruct.BulletData.f_DISTANCE)
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
			}
			else
			{
				float f_DISTANCE = weaponStruct.BulletData.f_DISTANCE;
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
			}
			if (_targetPos.HasValue)
			{
				int num = Math.Sign((_targetPos.Value - _refEntity.AimPosition).normalized.x);
				_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
			}
			PlaySkillSE("vi_zin01");
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse_skl100, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			break;
		}
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
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, -1, 0);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ModelTransform, MagazineType.ENERGY, -1, 0);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= endFrame)
			{
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL1_END_END, SKL1_END_END, OrangeCharacter.SubStatus.SKILL1_2, out skillEventFrame, out endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (nowFrame >= endFrame)
			{
				_refEntity.BulletCollider.BackToPool();
				OnSkillEnd();
			}
			else if (nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
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
		case OrangeCharacter.SubStatus.SKILL1:
		{
			_refEntity.IgnoreGravity = true;
			WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[1];
			OrangeBattleUtility.UpdateSkillCD(weaponStruct2);
			_refEntity.CheckUsePassiveSkill(1, weaponStruct2.weaponStatus, weaponStruct2.ShootTransform[0], _refEntity.direction * Vector2.right, 0);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)68u, false);
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1_1:
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
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)69u, false);
				}
			}
			int num = 0;
			endFrame = nowFrame + num;
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1_2:
		{
			_refEntity.SetSpeed(0, 0);
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
			_refEntity.BulletCollider.UpdateBulletData(weaponStruct.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
			_refEntity.BulletCollider.Active(_refEntity.TargetMask);
			_refEntity.BulletCollider.SetDirection(_refEntity.direction * Vector3.right);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)70u, (HumanBase.AnimateId)70u, (HumanBase.AnimateId)70u);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse_skl101, _refEntity.ModelTransform, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			break;
		}
		}
	}

	protected void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL && subStatus == OrangeCharacter.SubStatus.SKILL1)
		{
			_refEntity.IgnoreGravity = true;
			if (_isTeleporation && !_targetPos.HasValue)
			{
				endBreakFrame = GameLogicUpdateManager.GameFrame + 1;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL1_END_BREAK, SKL1_END_BREAK, OrangeCharacter.SubStatus.SKILL1_2, out skillEventFrame, out endFrame);
			}
			else
			{
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL1_LOOP_END, SKL1_LOOP_END, OrangeCharacter.SubStatus.SKILL1_1, out skillEventFrame, out endFrame);
			}
		}
	}

	private void OnSkillEnd()
	{
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		fxUseSkl0 = null;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		UpdateCustomWeaponRenderer(false);
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
		if (fxUseSkl0 != null)
		{
			fxUseSkl0.BackToPool();
		}
		fxUseSkl0 = null;
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

	private void UpdateCustomWeaponRenderer(bool enableWeapon, bool enableTrail = false)
	{
		if (enableWeapon)
		{
			saberCM.Appear(null, 0f);
		}
		else
		{
			saberCM.Disappear(null, 0f);
		}
		saberTrail.Emit = enableTrail;
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch113_skill_02_crouch", "ch113_skill_02_stand", "ch113_skill_02_jump", "ch113_skill_01_start", "ch113_skill_01_loop", "ch113_skill_01_jump_end" };
	}
}
