using System;
using System.Linq;
using UnityEngine;

public class CH070_Controller : CharacterControllerProxyBaseGen1
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL0_STAND_START = 65u,
		ANI_SKILL0_STAND_LOOP = 66u,
		ANI_SKILL0_STAND_END = 67u,
		ANI_SKILL0_JUMP_START = 68u,
		ANI_SKILL0_JUMP_LOOP = 69u,
		ANI_SKILL0_JUMP_END = 70u,
		ANI_SKILL1_STAND = 71u,
		ANI_SKILL1_JUMP = 72u,
		ANI_SKILL1_CROUCH = 73u
	}

	private enum FxName
	{
		p_colonel_skill1_000 = 0,
		p_colonel_skill1_001 = 1,
		fxuse_colonel_skill1_000 = 2,
		fxuse_colonel_skill1_001 = 3,
		fxuse_colonel_skill1_002 = 4
	}

	private readonly int FRAME_SKILL_0_FX = (int)(0.11f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int FRAME_SKILL_0_CANCEL = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int FRAME_SKILL_1_START = (int)(0.15f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int FRAME_SKILL_1_LOOP = (int)(0.7f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int FRAME_SKILL_1_CANCEL = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int FRAME_SKILL_1_END = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private int _skillEndFrame;

	private int _skillFxFrame;

	private int _skillCancelFrame;

	private ParticleSystem _fxIllusionParticle;

	private Vector3? _targetPos;

	private bool _isTeleporation;

	private bool _hasIllusion = true;

	private string[] _extraMeshes = new string[2] { "SaberMeshMain_m", "SaberMeshSub_g" };

	private bool IsPVPMode
	{
		get
		{
			return MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp;
		}
	}

	private void ToggleExtraMesh(bool isOpen)
	{
		_refEntity.ToggleExtraMesh(isOpen);
	}

	private void SetIllusion()
	{
		if (_refEntity.ShootDirection.x > 0f)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.p_colonel_skill1_001.ToString(), _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.p_colonel_skill1_001.ToString(), _refEntity.ModelTransform.position, OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
		}
	}

	private void ToggleIllusion(bool isOpen)
	{
		if (_fxIllusionParticle != null)
		{
			if (isOpen && !_fxIllusionParticle.isPlaying)
			{
				_fxIllusionParticle.Play();
			}
			else if (!isOpen && _fxIllusionParticle.isPlaying)
			{
				_fxIllusionParticle.Stop();
			}
		}
	}

	public override void Start()
	{
		base.Start();
		Transform[] childs = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2]
		{
			OrangeBattleUtility.FindChildRecursive(ref childs, "L WeaponPoint", true),
			OrangeBattleUtility.FindChildRecursive(ref childs, "R WeaponPoint", true)
		};
		OrangeCharacter refEntity = _refEntity;
		Renderer[] extraMeshOpen = (from meshName in _extraMeshes
			select OrangeBattleUtility.FindChildRecursive(ref childs, meshName) into transform
			select transform.GetComponent<SkinnedMeshRenderer>()).ToArray();
		refEntity.ExtraMeshOpen = extraMeshOpen;
		_refEntity.ExtraMeshClose = new Renderer[0];
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);
		});
	}

	public override void ExtraVariableInit()
	{
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[9] { "ch070_skill_01_stand_start", "ch070_skill_01_stand_loop", "ch070_skill_01_stand_end", "ch070_skill_01_jump_start", "ch070_skill_01_jump_loop", "ch070_skill_01_jump_end", "ch070_skill_02_stand", "ch070_skill_02_jump", "ch070_skill_02_crouch" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[2] { "login", "logout" };
		target = new string[2] { "ch070_login", "ch070_logout" };
	}

	protected override void ToggleWeapon(WeaponState weaponState)
	{
		switch (weaponState)
		{
		case WeaponState.NONE:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableCurrentWeapon();
			}
			ToggleExtraMesh(false);
			break;
		case WeaponState.TELEPORT_OUT:
		case WeaponState.SKILL_0:
		case WeaponState.SKILL_1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableCurrentWeapon();
			}
			ToggleExtraMesh(true);
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			ToggleExtraMesh(false);
			break;
		}
	}

	protected override void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 1.5f && currentFrame <= 2f)
			{
				ToggleWeapon(WeaponState.NONE);
			}
		}
	}

	public override void ControlCharacterDead()
	{
	}

	protected override void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			ToggleWeapon(WeaponState.NORMAL);
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				if (_isTeleporation && !_targetPos.HasValue)
				{
					_refEntity.SetStatus(_refEntity.CurMainStatus, _refEntity.CurSubStatus + 2);
				}
				else
				{
					_refEntity.SetStatus(_refEntity.CurMainStatus, _refEntity.CurSubStatus + 1);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SetSkillEnd();
				break;
			}
			break;
		}
	}

	protected override void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		int gameFrame = GameLogicUpdateManager.GameFrame;
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		PlayerAutoAimSystem playerAutoAimSystem = _refEntity.PlayerAutoAimSystem;
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if ((uint)subStatus <= 1u)
			{
				ToggleWeapon(WeaponState.TELEPORT_OUT);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			{
				_refEntity.IgnoreGravity = true;
				WeaponStruct weaponStruct3 = _refEntity.PlayerSkills[currentActiveSkill];
				_refEntity.CheckUsePassiveSkill(currentActiveSkill, weaponStruct3.weaponStatus, _refEntity.ExtraTransforms[0]);
				OrangeBattleUtility.UpdateSkillCD(weaponStruct3);
				if (_refEntity.IsInGround)
				{
					_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				}
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
						_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
					}
				}
				int num = 0;
				_skillEndFrame = gameFrame + num;
				break;
			}
			case OrangeCharacter.SubStatus.SKILL0_2:
			{
				_refEntity.SetSpeed(0, 0);
				if (_refEntity.IsInGround)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				}
				WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[currentActiveSkill];
				_refEntity.BulletCollider.UpdateBulletData(weaponStruct2.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(weaponStruct2.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = weaponStruct2.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_skillFxFrame = gameFrame + FRAME_SKILL_0_FX;
				_skillCancelFrame = gameFrame + FRAME_SKILL_0_CANCEL;
				break;
			}
			case OrangeCharacter.SubStatus.SKILL1:
				if (_refEntity.IsInGround)
				{
					if (_refEntity.IsCrouching)
					{
						_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
					}
					else
					{
						_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
					}
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
					_refEntity.IgnoreGravity = true;
				}
				_skillEndFrame = gameFrame + FRAME_SKILL_1_START;
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
			{
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[currentActiveSkill];
				_refEntity.CheckUsePassiveSkill(currentActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				_refEntity.IsShoot = 3;
				_refEntity.StartShootTimer();
				_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ModelTransform.root, weaponStruct.SkillLV, Vector2.right * _refEntity.direction);
				_skillEndFrame = gameFrame + FRAME_SKILL_1_LOOP;
				_skillCancelFrame = gameFrame + FRAME_SKILL_1_CANCEL;
				break;
			}
			case OrangeCharacter.SubStatus.SKILL1_2:
				_skillEndFrame = gameFrame + FRAME_SKILL_1_END;
				_skillCancelFrame = gameFrame;
				break;
			}
			break;
		}
	}

	protected override void OnPlayerPressSkillCharacterCall(SkillID skillId)
	{
		PlayerAutoAimSystem playerAutoAimSystem = _refEntity.PlayerAutoAimSystem;
		switch (skillId)
		{
		case SkillID.SKILL_0:
		{
			if (!CheckCanTriggerSkill(skillId))
			{
				break;
			}
			bool isInGround = _refEntity.IsInGround;
			_refEntity.PlayerStopDashing();
			_refEntity.SetSpeed(0, 0);
			_refEntity.SkillEnd = false;
			_refEntity.CurrentActiveSkill = (int)skillId;
			ToggleWeapon(WeaponState.SKILL_0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			IAimTarget aimTarget = playerAutoAimSystem.AutoAimTarget;
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[(int)skillId];
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
			_hasIllusion = true;
			if (IsPVPMode)
			{
				if (aimTarget == null)
				{
					_hasIllusion = false;
				}
				else
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
				int num2 = Math.Sign((_targetPos.Value - _refEntity.AimPosition).normalized.x);
				_refEntity.direction = ((num2 != 0) ? num2 : _refEntity.direction);
				if (_hasIllusion)
				{
					SetIllusion();
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_colonel_skill1_000.ToString(), _refEntity.ModelTransform, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				}
			}
			break;
		}
		case SkillID.SKILL_1:
		{
			if (!CheckCanTriggerSkill(skillId))
			{
				break;
			}
			bool isInGround2 = _refEntity.IsInGround;
			_refEntity.PlayerStopDashing();
			_refEntity.SetSpeed(0, 0);
			_refEntity.SkillEnd = false;
			_refEntity.CurrentActiveSkill = (int)skillId;
			ToggleWeapon(WeaponState.SKILL_1);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			IAimTarget autoAimTarget = playerAutoAimSystem.AutoAimTarget;
			if (autoAimTarget != null)
			{
				int num = Math.Sign((autoAimTarget.AimPosition - _refEntity.transform.position).x);
				_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
			}
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_colonel_skill1_002.ToString(), _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			if (_refEntity.IsInGround)
			{
				if (_refEntity.ShootDirection.x > 0f)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_colonel_skill1_001.ToString(), _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				}
				else
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_colonel_skill1_001.ToString(), _refEntity.ModelTransform.position, OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				}
			}
			break;
		}
		}
	}

	protected override void OnCheckSkill(int nowFrame)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= _skillEndFrame)
			{
				_refEntity.SetStatus(_refEntity.CurMainStatus, _refEntity.CurSubStatus + 1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (nowFrame == _skillFxFrame)
			{
				if (_refEntity.direction > 0)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.p_colonel_skill1_000.ToString(), _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				}
				else
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.p_colonel_skill1_000.ToString(), _refEntity.ModelTransform.position, OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				}
			}
			if (nowFrame >= _skillCancelFrame && ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
			{
				SetSkillEnd();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= _skillEndFrame)
			{
				_refEntity.SetStatus(_refEntity.CurMainStatus, _refEntity.CurSubStatus + 1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= _skillEndFrame)
			{
				_refEntity.SetStatus(_refEntity.CurMainStatus, _refEntity.CurSubStatus + 1);
			}
			if (nowFrame >= _skillCancelFrame && ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
			{
				SetSkillEnd();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (nowFrame >= _skillEndFrame)
			{
				SetSkillEnd();
			}
			if (nowFrame >= _skillCancelFrame && ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
			{
				SetSkillEnd();
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		SkillID currentActiveSkill = (SkillID)_refEntity.CurrentActiveSkill;
		if ((uint)currentActiveSkill <= 1u)
		{
			SetSkillEnd();
		}
	}
}
