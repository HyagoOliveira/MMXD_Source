using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CH073_Controller : CharacterControllerProxyBaseGen2
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL0_STAND_START = 65u,
		ANI_SKILL0_STAND_LOOP = 66u,
		ANI_SKILL0_STAND_END = 67u,
		ANI_SKILL0_CROUCH_START = 68u,
		ANI_SKILL0_CROUCH_LOOP = 69u,
		ANI_SKILL0_CROUCH_END = 70u,
		ANI_SKILL0_JUMP_START = 71u,
		ANI_SKILL0_JUMP_LOOP = 72u,
		ANI_SKILL0_JUMP_END = 73u,
		ANI_SKILL1_STAND_START = 74u,
		ANI_SKILL1_STAND_END = 75u,
		ANI_SKILL1_CROUCH_START = 76u,
		ANI_SKILL1_CROUCH_END = 77u,
		ANI_SKILL1_JUMP_START = 78u,
		ANI_SKILL1_JUMP_END = 79u
	}

	private enum FxName
	{
		fxuse_nightmarebuster_000 = 0,
		fxuse_nightmarezero_001 = 1
	}

	private readonly float SKILL_0_SHIFT_X = 0.5f;

	private readonly float SKILL_0_SHIFT_RANGE_Y = 0.5f;

	private int[] FRAMES_SKILL_0_SHOOT;

	private readonly int FRAME_SKILL_1_SHOOT = (int)(0.15f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int FRAME_SKILL_1_CANCEL = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private int _skillShootIndex;

	private List<int> _skillShootFrames = new List<int>();

	private int _skillCancelFrame;

	private SkinnedMeshRenderer _busterMesh;

	private SkinnedMeshRenderer _saberMesh;

	private ParticleSystem _saberParticle;

	private Transform[] _effectTramsforms;

	private bool _hasWinPose;

	private IEnumerator ToggleEffectTransforms(bool isActive, float delay)
	{
		yield return new WaitForSeconds(delay);
		ToggleEffectTransforms(isActive);
	}

	private void ToggleEffectTransforms(bool isActive)
	{
		_effectTramsforms.ForEach(delegate(Transform trans)
		{
			trans.gameObject.SetActive(isActive);
		});
	}

	public override void Start()
	{
		base.Start();
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2]
		{
			OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true),
			OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true)
		};
		_busterMesh = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m").GetComponent<SkinnedMeshRenderer>();
		_saberMesh = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMesh_m").GetComponent<SkinnedMeshRenderer>();
		_saberParticle = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_nightmarezero").GetComponent<ParticleSystem>();
		_effectTramsforms = new Transform[2]
		{
			OrangeBattleUtility.FindChildRecursive(ref target, "Particle_StartLighting02"),
			OrangeBattleUtility.FindChildRecursive(ref target, "eyeefx")
		};
		string[] array = _refEntity.PlayerSkills[0].BulletData.s_CONTI.Split(',');
		float result;
		if (!float.TryParse(array[0], out result) || result <= 0f)
		{
			result = 0.1f;
		}
		int result2;
		if (!int.TryParse(array[1], out result2) || result2 <= 0)
		{
			result2 = 1;
		}
		int num = (int)(result / GameLogicUpdateManager.m_fFrameLen);
		FRAMES_SKILL_0_SHOOT = new int[result2];
		for (int i = 0; i < FRAMES_SKILL_0_SHOOT.Length; i++)
		{
			FRAMES_SKILL_0_SHOOT[i] = num * i;
		}
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);
		});
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[15]
		{
			"ch073_skill_01_stand_start", "ch073_skill_01_stand_loop", "ch073_skill_01_stand_end", "ch073_skill_01_crouch_start", "ch073_skill_01_crouch_loop", "ch073_skill_01_crouch_end", "ch073_skill_01_jump_start", "ch073_skill_01_jump_loop", "ch073_skill_01_jump_end", "ch073_skill_02_stand_start",
			"ch073_skill_02_stand_end", "ch073_skill_02_crouch_start", "ch073_skill_02_crouch_end", "ch073_skill_02_jump_start", "ch073_skill_02_jump_end"
		};
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "login", "logout", "win" };
		target = new string[3] { "ch073_login", "ch073_logout", "ch073_win" };
	}

	protected override void TeleportInCharacterDepend()
	{
		ToggleWeapon(WeaponState.TELEPORT_IN);
	}

	protected override void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus != 0)
		{
			return;
		}
		float currentFrame = _refEntity.CurrentFrame;
		if (_hasWinPose)
		{
			if (currentFrame > 0.3f && currentFrame <= 2f && _saberParticle.isPlaying)
			{
				_saberParticle.Stop();
			}
			if (currentFrame > 1.2f && currentFrame <= 2f)
			{
				_busterMesh.enabled = false;
				_saberMesh.enabled = false;
				ToggleEffectTransforms(false);
			}
		}
		else if (currentFrame > 1.2f && currentFrame <= 2f)
		{
			ToggleEffectTransforms(false);
		}
	}

	public override void ExtraVariableInit()
	{
		ToggleEffectTransforms(true);
	}

	protected override void StageTeleportInCharacterDepend()
	{
		StartCoroutine(ToggleEffectTransforms(true, 1f));
	}

	protected override void StageTeleportOutCharacterDepend()
	{
		ToggleEffectTransforms(false);
	}

	public override void ControlCharacterDead()
	{
		ToggleEffectTransforms(false);
	}

	protected override void ToggleWeapon(WeaponState weaponState)
	{
		switch (weaponState)
		{
		case WeaponState.TELEPORT_IN:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableCurrentWeapon();
			}
			_busterMesh.enabled = false;
			_saberMesh.enabled = false;
			_saberParticle.Stop();
			break;
		case WeaponState.SKILL_0:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableCurrentWeapon();
			}
			_busterMesh.enabled = true;
			_saberMesh.enabled = false;
			_saberParticle.Stop();
			_refEntity.EnableHandMesh(false);
			break;
		case WeaponState.SKILL_1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableCurrentWeapon();
			}
			_busterMesh.enabled = false;
			_saberMesh.enabled = true;
			_saberParticle.Play();
			break;
		case WeaponState.TELEPORT_OUT:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableCurrentWeapon();
			}
			_busterMesh.enabled = false;
			if (_hasWinPose)
			{
				_saberMesh.enabled = true;
				_saberParticle.Play();
			}
			else
			{
				_saberMesh.enabled = false;
			}
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			_busterMesh.enabled = false;
			_saberMesh.enabled = false;
			_saberParticle.Stop();
			break;
		}
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
				_refEntity.SetStatus(_refEntity.CurMainStatus, _refEntity.CurSubStatus + 1);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SetSkillEnd();
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetStatus(_refEntity.CurMainStatus, _refEntity.CurSubStatus + 1);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				SetSkillEnd();
				break;
			}
			break;
		}
	}

	protected override void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		int nowFrame = GameLogicUpdateManager.GameFrame;
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		PlayerAutoAimSystem playerAutoAimSystem = _refEntity.PlayerAutoAimSystem;
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
				_hasWinPose = true;
				ToggleWeapon(WeaponState.TELEPORT_OUT);
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				ToggleWeapon(WeaponState.TELEPORT_OUT);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			{
				_refEntity.Animator._animator.speed = 0.8f;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[currentActiveSkill];
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				_refEntity.CheckUsePassiveSkill(currentActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
				if (_refEntity.IsCrouching)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
					break;
				}
				if (_refEntity.IsInGround)
				{
					_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
					break;
				}
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				break;
			}
			case OrangeCharacter.SubStatus.SKILL0_1:
				_skillShootFrames.Clear();
				_skillShootFrames.AddRange(FRAMES_SKILL_0_SHOOT.Select((int shootFrame) => nowFrame + shootFrame));
				_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.Animator._animator.speed = 0.8f;
				_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			{
				WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[currentActiveSkill];
				OrangeBattleUtility.UpdateSkillCD(weaponStruct2);
				_refEntity.CheckUsePassiveSkill(currentActiveSkill, weaponStruct2.weaponStatus, _refEntity.ExtraTransforms[0]);
				if (_refEntity.IsCrouching)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)76u);
				}
				else if (_refEntity.IsInGround)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetAnimateId((HumanBase.AnimateId)78u);
				}
				if (playerAutoAimSystem.GetClosestTarget() != null)
				{
					if (_refEntity.IsShootPrev > 0)
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_nightmarezero_001.ToString(), _refEntity._transform.position, (_refEntity.ShootDirection.x > 0f) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
					}
					else
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_nightmarezero_001.ToString(), _refEntity._transform.position, (_refEntity.direction > 0) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
					}
				}
				else
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_nightmarezero_001.ToString(), _refEntity._transform.position, (_refEntity.ShootDirection.x > 0f) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				}
				_skillShootFrames.Clear();
				_skillShootFrames.Add(nowFrame + FRAME_SKILL_1_SHOOT);
				_skillCancelFrame = nowFrame + FRAME_SKILL_1_CANCEL;
				break;
			}
			case OrangeCharacter.SubStatus.SKILL1_1:
				if (_saberParticle.isPlaying)
				{
					_saberParticle.Stop();
				}
				_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
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
			if (CheckCanTriggerSkill(skillId))
			{
				bool isInGround = _refEntity.IsInGround;
				_skillShootIndex = 0;
				PlayerStopDashing();
				SetSkillAndWeapon(skillId);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			}
			break;
		case SkillID.SKILL_1:
			if (CheckCanTriggerSkill(skillId))
			{
				bool isInGround2 = _refEntity.IsInGround;
				_skillShootIndex = 0;
				PlayerStopDashing();
				SetSkillAndWeapon(skillId);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			}
			break;
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
			if (_skillShootIndex < _skillShootFrames.Count && nowFrame >= _skillShootFrames[_skillShootIndex])
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_nightmarebuster_000.ToString(), _refEntity.ExtraTransforms[0].position, (_refEntity.direction > 0) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				int currentActiveSkill = _refEntity.CurrentActiveSkill;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[currentActiveSkill];
				_refEntity.IsShoot = 0;
				_refEntity.StartShootTimer();
				Vector3 shootPosition = _refEntity.ExtraTransforms[0].position + new Vector3((float)_refEntity.direction * SKILL_0_SHIFT_X, UnityEngine.Random.Range(0f - SKILL_0_SHIFT_RANGE_Y, SKILL_0_SHIFT_RANGE_Y), 0f);
				_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, shootPosition, weaponStruct.SkillLV, Vector3.right * _refEntity.direction);
				_skillShootIndex++;
			}
			if (_skillShootIndex >= _skillShootFrames.Count)
			{
				_refEntity.SetStatus(_refEntity.CurMainStatus, _refEntity.CurSubStatus + 1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
			{
				SetSkillEnd();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (_skillShootIndex < _skillShootFrames.Count && nowFrame >= _skillShootFrames[_skillShootIndex])
			{
				int currentActiveSkill2 = _refEntity.CurrentActiveSkill;
				WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[currentActiveSkill2];
				_refEntity.IsShoot = 0;
				_refEntity.StartShootTimer();
				_refEntity.PushBulletDetail(weaponStruct2.BulletData, weaponStruct2.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct2.SkillLV, Vector2.right * _refEntity.direction);
				_skillShootIndex++;
			}
			if (nowFrame >= _skillCancelFrame && ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
			{
				SetSkillEnd();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
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
