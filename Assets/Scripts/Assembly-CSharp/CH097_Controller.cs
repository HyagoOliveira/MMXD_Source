using System;
using UnityEngine;

public class CH097_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private Transform shootPointTransform0;

	private GameObject umbrellaMesh_m;

	private Vector3? _targetPos;

	private bool _isTeleporation;

	private BulletBase mSkillBullet;

	private int[] buffIds = new int[2] { -1, -2 };

	private bool isPlayTeleportOut;

	private Fxlooptheloop fxlooptheloop;

	private readonly string sCustomShootPoint = "CustomShootPoint";

	protected readonly string sUmbrellaMesh_m = "UmbrellaMesh_m";

	private readonly string Fxuse000 = "fxuse_flyingattack_000";

	private readonly string Fxuse001 = "fxuse_looptheloop_001";

	private readonly int SKL0_START_END = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_LOOP_END = (int)(0.417f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.333f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.08f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.43f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.33f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_TRIGGER = (int)(0.08f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_END = (int)(0.33f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_END_BREAK = (int)(0.16f / GameLogicUpdateManager.m_fFrameLen);

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
		InitializeSkill();
	}

	private void InitializeSkill()
	{
		shootPointTransform0 = new GameObject(sCustomShootPoint + "0").transform;
		shootPointTransform0.SetParent(base.transform);
		shootPointTransform0.localPosition = new Vector3(0f, 0.8f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		umbrellaMesh_m = OrangeBattleUtility.FindChildRecursive(ref target, sUmbrellaMesh_m, true).gameObject;
		umbrellaMesh_m.SetActive(true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Fxuse000);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Fxuse001);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		_refEntity.TeleportInCharacterDependeEndEvt = TeleportInCharacterDependeEnd;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.PlayTeleportOutEffectEvt = PlayTeleportOutEffect;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
	}

	public void TeleportInCharacterDependeEnd()
	{
		umbrellaMesh_m.SetActive(false);
	}

	private void PlayTeleportOutEffect()
	{
		Vector3 p_worldPos = base.transform.position;
		if (_refEntity != null)
		{
			p_worldPos = _refEntity.AimPosition;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_TELEPORT_OUT", p_worldPos, Quaternion.identity, Array.Empty<object>());
	}

	public void TeleportOutCharacterDepend()
	{
		if (!isPlayTeleportOut)
		{
			isPlayTeleportOut = true;
			umbrellaMesh_m.SetActive(true);
			CharacterMaterial[] componentsInChildren = _refEntity.GetComponentsInChildren<CharacterMaterial>();
			if (componentsInChildren != null && componentsInChildren.Length > 1)
			{
				componentsInChildren[0].SetSubCharacterMaterial(componentsInChildren[1]);
			}
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1 || id != 0 || !_refEntity.CheckUseSkillKeyTrigger(id))
		{
			return;
		}
		PlayVoiceSE("v_pd_skill01");
		PlaySkillSE("pd_flying");
		ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_START_END, SKL0_START_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
		umbrellaMesh_m.SetActive(true);
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
			PlayVoiceSE("v_pd_skill02_1");
			PlaySkillSE("pd_majou01");
			_refEntity.CurrentActiveSkill = id;
			ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)68u);
			umbrellaMesh_m.SetActive(true);
		}
		else if (!_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(buffIds[1]))
		{
			PlayVoiceSE("v_pd_skill02_2");
			PlaySkillSE("pd_majou03");
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_1_END_BREAK;
			if (_refEntity.IsLocalPlayer)
			{
				_refEntity.selfBuffManager.AddBuff(buffIds[1], 0, 0, 0);
			}
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
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_END_BREAK, SKL0_END_BREAK, OrangeCharacter.SubStatus.SKILL0_2, out skillEventFrame, out endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (nowFrame >= endFrame)
			{
				_refEntity.BulletCollider.BackToPool();
				OnSkillEnd();
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
			{
				endFrame = nowFrame + 1;
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
				if (_refEntity.IsLocalPlayer)
				{
					currentSkillObj.LastUseTimer.SetTime(99999f);
					currentSkillObj.MagazineRemain = currentSkillObj.FastBulletDatas[1].n_MAGAZINE;
				}
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
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
		case OrangeCharacter.SubStatus.SKILL0:
		{
			_refEntity.IgnoreGravity = true;
			WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[0];
			_refEntity.CheckUsePassiveSkill(0, weaponStruct2.weaponStatus, weaponStruct2.ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct2);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START, false);
			break;
		}
		case OrangeCharacter.SubStatus.SKILL0_1:
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
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)66u, false);
				}
			}
			int num = 0;
			endFrame = nowFrame + num;
			break;
		}
		case OrangeCharacter.SubStatus.SKILL0_2:
		{
			_refEntity.SetSpeed(0, 0);
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
			_refEntity.BulletCollider.UpdateBulletData(weaponStruct.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
			_refEntity.BulletCollider.Active(_refEntity.TargetMask);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)67u, (HumanBase.AnimateId)67u, (HumanBase.AnimateId)67u);
			break;
		}
		}
	}

	protected void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL && subStatus == OrangeCharacter.SubStatus.SKILL0)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Fxuse000, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			_refEntity.IgnoreGravity = true;
			if (_isTeleporation && !_targetPos.HasValue)
			{
				endBreakFrame = GameLogicUpdateManager.GameFrame + 1;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_END_BREAK, SKL0_END_BREAK, OrangeCharacter.SubStatus.SKILL0_2, out skillEventFrame, out endFrame);
			}
			else
			{
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_LOOP_END, SKL0_LOOP_END, OrangeCharacter.SubStatus.SKILL0_1, out skillEventFrame, out endFrame);
			}
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
			}
			else if (mSkillBullet.bIsEnd && _refEntity.IsLocalPlayer)
			{
				_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(buffIds[0]);
				mSkillBullet = null;
				_refEntity.PlayerSkills[1].MagazineRemain = 0f;
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				_refEntity.RemoveComboSkillBuff(weaponStruct.FastBulletDatas[weaponStruct.Reload_index].n_ID);
			}
		}
		else if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(buffIds[0]) && mSkillBullet == null)
		{
			mSkillBullet = _refEntity.CreateFSBulletEx(weaponStruct, 0);
			mSkillBullet.transform.localRotation = Quaternion.identity;
		}
		if (!_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(buffIds[1]))
		{
			return;
		}
		if (mSkillBullet != null)
		{
			Animation componentInChildren = mSkillBullet.GetComponentInChildren<Animation>();
			float num = 0.1f;
			if ((bool)componentInChildren)
			{
				num += componentInChildren[Fxlooptheloop.CLIP_NAME].normalizedTime;
			}
			if (_refEntity.IsLocalPlayer)
			{
				_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(buffIds[0]);
			}
			mSkillBullet.BackToPool();
			mSkillBullet = null;
			fxlooptheloop = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<Fxlooptheloop>(Fxuse001, _refEntity.ModelTransform, Quaternion.identity, new object[1] { num });
			fxlooptheloop.transform.position = _refEntity.AimTransform.position;
		}
		if (weaponStruct.MagazineRemain > 0f)
		{
			if (weaponStruct.LastUseTimer.IsStarted())
			{
				if (weaponStruct.LastUseTimer.GetMillisecond() <= weaponStruct.BulletData.n_FIRE_SPEED)
				{
					return;
				}
			}
			else
			{
				weaponStruct.LastUseTimer.TimerStart();
			}
			_refEntity.UpdateAimDirection();
			CreateSkillBullet(weaponStruct);
			_refEntity.CheckUsePassiveSkill(1, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0], null, weaponStruct.Reload_index);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
			if (fxlooptheloop != null)
			{
				fxlooptheloop.SetInactive((int)weaponStruct.MagazineRemain);
			}
		}
		else
		{
			SKILL_TABLE sKILL_TABLE = weaponStruct.FastBulletDatas[weaponStruct.Reload_index];
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(buffIds[1]);
			_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
			if (fxlooptheloop != null)
			{
				fxlooptheloop.BackToPool();
				fxlooptheloop = null;
			}
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[1], weaponStruct.weaponStatus, shootPointTransform0, weaponStruct.SkillLV);
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
		umbrellaMesh_m.SetActive(false);
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
		if ((int)_refEntity.Hp <= 0 && fxlooptheloop != null)
		{
			_refEntity.PlayerSkills[1].MagazineRemain = 1f;
			fxlooptheloop.BackToPool();
			fxlooptheloop = null;
		}
		umbrellaMesh_m.SetActive(false);
		_refEntity.EnableCurrentWeapon();
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.BulletCollider.BackToPool();
	}

	public override void SetStun(bool enable)
	{
		umbrellaMesh_m.SetActive(false);
		if (enable)
		{
			_refEntity.EnableCurrentWeapon();
		}
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
		return new string[4] { "ch097_skill_01_jump_start", "ch097_skill_01_jump_loop", "ch097_skill_01_jump_end", "ch097_skill_02_stand" };
	}
}
