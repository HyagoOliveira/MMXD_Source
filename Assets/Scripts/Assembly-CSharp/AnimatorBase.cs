using System;
using UnityEngine;
using enums;

public class AnimatorBase : MonoBehaviour
{
	public Vector3 _modelShift = Vector3.zero;

	public float _defaultModelshiftY;

	public readonly int hashDirection = Animator.StringToHash("fDirection");

	public readonly int hashEquip = Animator.StringToHash("fEquip");

	public readonly int hashVelocityX = Animator.StringToHash("fVelocityX");

	public readonly int hashVelocityY = Animator.StringToHash("fVelocityY");

	public readonly int hashSpeedMultiplier = Animator.StringToHash("fSpeedMultiplier");

	public bool RealRotate;

	public Animator _animator;

	public Transform AimTarget;

	public int LastUpdateFrame;

	private OrangeTimer _animationTimer;

	public Transform ModelTransform;

	private int _direction = 1;

	private Vector3 _shootDirection;

	private short _shootLevel;

	private static int[] _animateStatus;

	private static int _idleChargeShootHash;

	private static int _idleShootHash;

	private static int _stepShootHash;

	private static int _runShootHash;

	private static int _dashShootHash;

	private static int _slideShootHash;

	private static int _crouchShootHash;

	private static int _crouchEndShootHash;

	private static int _crouchUpShootHash;

	private static int _wallgrabStepShootHash;

	private static int _wallgrabStartShootHash;

	private static int _wallgrabEndShootHash;

	private static int _walljumpStartShootHash;

	private static int _walljumpEndShootHash;

	private static int _dashShootEndHash;

	private static int _slideShootEndHash;

	private static int _jumpShootHash;

	private static int _fallShootHash;

	private static int _landShootHash;

	private static int _currentUpperAnimeId;

	public bool IsDefaultAnimator
	{
		get
		{
			if (_animator != null)
			{
				return _animator.runtimeAnimatorController.name == "NewEmptyController";
			}
			return false;
		}
	}

	private void Start()
	{
		_animator = GetComponentInChildren<Animator>();
		ModelTransform = OrangeBattleUtility.FindChildRecursive(base.transform, "model");
		_animationTimer = OrangeTimerManager.GetTimer();
		_animationTimer.TimerStart();
		InitAnimator(_animateStatus);
	}

	private void InitAnimator(int[] p_animateStatus)
	{
		if (p_animateStatus == null)
		{
			_animateStatus = InitAnimator();
			_idleChargeShootHash = Animator.StringToHash("stand_charge_atk");
			_idleShootHash = Animator.StringToHash("stand_atk");
			_stepShootHash = Animator.StringToHash("run_atk_start");
			_runShootHash = Animator.StringToHash("run_atk_loop");
			_dashShootHash = Animator.StringToHash("dash_atk_loop");
			_dashShootEndHash = Animator.StringToHash("dash_atk_end");
			_slideShootHash = Animator.StringToHash("slide_atk_loop");
			_slideShootEndHash = Animator.StringToHash("slide_atk_end");
			_crouchShootHash = Animator.StringToHash("crouch_atk_start");
			_crouchUpShootHash = Animator.StringToHash("crouch_atk_end");
			_crouchEndShootHash = Animator.StringToHash("crouch_atk");
			_wallgrabEndShootHash = Animator.StringToHash("wallgrab_atk");
			_jumpShootHash = Animator.StringToHash("jump_atk_start");
			_fallShootHash = Animator.StringToHash("fall_atk");
			_landShootHash = Animator.StringToHash("landing_atk");
			_walljumpStartShootHash = Animator.StringToHash("walljump_atk_start");
			_walljumpEndShootHash = Animator.StringToHash("fall_atk");
		}
	}

	public int[] InitAnimator()
	{
		int[] array = new int[145];
		for (int i = 0; i < 16; i++)
		{
			array[i] = Animator.StringToHash("placeHolder");
		}
		array[1] = Animator.StringToHash("ride_loop");
		array[0] = Animator.StringToHash("stand_loop");
		array[8] = Animator.StringToHash("jump_start");
		array[9] = Animator.StringToHash("fall_loop");
		array[10] = Animator.StringToHash("landing");
		array[22] = Animator.StringToHash("run_start");
		array[2] = Animator.StringToHash("run_loop");
		array[3] = Animator.StringToHash("backward_atk");
		array[4] = Animator.StringToHash("dash_start");
		array[5] = Animator.StringToHash("dash_end");
		array[6] = Animator.StringToHash("slide_start");
		array[7] = Animator.StringToHash("slide_end");
		array[11] = Animator.StringToHash("wallgrab_step");
		array[12] = Animator.StringToHash("wallgrab_start");
		array[13] = Animator.StringToHash("wallgrab_loop");
		array[16] = Animator.StringToHash("walljump_start");
		array[17] = Animator.StringToHash("fall_loop");
		array[18] = Animator.StringToHash("crouch_start");
		array[19] = Animator.StringToHash("crouch_loop");
		array[20] = Animator.StringToHash("crouch_end");
		array[23] = Animator.StringToHash("damage_start");
		array[24] = Animator.StringToHash("damage_loop");
		array[21] = Animator.StringToHash("airdash_end");
		array[26] = Animator.StringToHash("melee_stand_classic_atk1_start");
		array[27] = Animator.StringToHash("melee_stand_classic_atk2_start");
		array[28] = Animator.StringToHash("melee_stand_classic_atk3_start");
		array[38] = Animator.StringToHash("melee_stand_classic_atk1_end");
		array[39] = Animator.StringToHash("melee_stand_classic_atk2_end");
		array[40] = Animator.StringToHash("melee_stand_classic_atk3_end");
		array[29] = Animator.StringToHash("melee_stand_atk1_start");
		array[30] = Animator.StringToHash("melee_stand_atk2_start");
		array[31] = Animator.StringToHash("melee_stand_atk3_start");
		array[32] = Animator.StringToHash("melee_stand_atk4_start");
		array[33] = Animator.StringToHash("melee_stand_atk5_start");
		array[41] = Animator.StringToHash("melee_stand_atk1_end");
		array[42] = Animator.StringToHash("melee_stand_atk2_end");
		array[43] = Animator.StringToHash("melee_stand_atk3_end");
		array[44] = Animator.StringToHash("melee_stand_atk4_end");
		array[45] = Animator.StringToHash("melee_stand_atk5_end");
		array[46] = Animator.StringToHash("melee_jump_atk_loop");
		array[47] = Animator.StringToHash("melee_dash_atk_loop");
		array[49] = Animator.StringToHash("melee_dash_atk_end");
		array[51] = Animator.StringToHash("melee_crouch_atk_start");
		array[52] = Animator.StringToHash("melee_crouch_atk_end");
		array[14] = Animator.StringToHash("wallgrab_slash");
		array[15] = Animator.StringToHash("wallgrab_slash_end");
		array[53] = Animator.StringToHash("login");
		array[54] = Animator.StringToHash("win");
		array[55] = Animator.StringToHash("logout");
		array[56] = Animator.StringToHash("dive_trigger_stand_start");
		array[57] = Animator.StringToHash("dive_trigger_stand_end");
		array[58] = Animator.StringToHash("dive_trigger_jump_start");
		array[59] = Animator.StringToHash("dive_trigger_jump_end");
		array[60] = Animator.StringToHash("dive_trigger_crouch_start");
		array[61] = Animator.StringToHash("dive_trigger_crouch_end");
		for (int j = 0; j < 50; j++)
		{
			array[65 + j] = Animator.StringToHash("skillclip" + j);
		}
		for (int k = 0; k < 10; k++)
		{
			array[116 + k] = Animator.StringToHash("blendskill" + k);
		}
		for (int l = 0; l < 15; l++)
		{
			array[127 + l] = Animator.StringToHash("btskillclip" + l);
		}
		array[143] = Animator.StringToHash("stand_special");
		array[144] = Animator.StringToHash("logout2");
		array[34] = Animator.StringToHash("melee_run_atk1");
		array[36] = Animator.StringToHash("melee_run_atk2");
		array[35] = Animator.StringToHash("melee_run_atk2");
		array[48] = Animator.StringToHash("dash_slash2");
		array[50] = Animator.StringToHash("dash_slash2_end");
		return array;
	}

	public void SetAnimatorParameters(AnimationParameters animationParams)
	{
		if (animationParams.AnimateUpperID == 12 || animationParams.AnimateUpperID == 13 || animationParams.AnimateUpperID == 14 || animationParams.AnimateUpperID == 15)
		{
			_modelShift.x = (float)_direction * -0.18f;
		}
		else
		{
			_modelShift.x = 0f;
		}
		ModelTransform.localPosition = _modelShift;
		_shootLevel = animationParams.IsShoot;
		float timeFrame = (animationParams.AnimateUpperKeepFlag ? _animator.GetCurrentAnimatorStateInfo(0).normalizedTime : 0f);
		_currentUpperAnimeId = _animateStatus[animationParams.AnimateUpperID];
		if (_shootLevel != 0)
		{
			switch ((HumanBase.AnimateId)animationParams.AnimateUpperID)
			{
			case HumanBase.AnimateId.ANI_STEP:
				_currentUpperAnimeId = _stepShootHash;
				break;
			case HumanBase.AnimateId.ANI_STAND:
			case HumanBase.AnimateId.ANI_STAND_SKILL:
				_currentUpperAnimeId = ((_shootLevel < 3) ? _idleShootHash : _idleChargeShootHash);
				break;
			case HumanBase.AnimateId.ANI_JUMP:
				_currentUpperAnimeId = _jumpShootHash;
				break;
			case HumanBase.AnimateId.ANI_FALL:
				_currentUpperAnimeId = _fallShootHash;
				break;
			case HumanBase.AnimateId.ANI_LAND:
				_currentUpperAnimeId = _landShootHash;
				break;
			case HumanBase.AnimateId.ANI_WALK:
				_currentUpperAnimeId = _runShootHash;
				break;
			case HumanBase.AnimateId.ANI_CROUCH:
				_currentUpperAnimeId = _crouchShootHash;
				break;
			case HumanBase.AnimateId.ANI_CROUCH_END:
				_currentUpperAnimeId = _crouchEndShootHash;
				break;
			case HumanBase.AnimateId.ANI_CROUCH_UP:
				_currentUpperAnimeId = _crouchUpShootHash;
				break;
			case HumanBase.AnimateId.ANI_DASH:
				_currentUpperAnimeId = _dashShootHash;
				break;
			case HumanBase.AnimateId.ANI_SLIDE:
				_currentUpperAnimeId = _slideShootHash;
				break;
			case HumanBase.AnimateId.ANI_DASH_END:
				_currentUpperAnimeId = _dashShootEndHash;
				break;
			case HumanBase.AnimateId.ANI_SLIDE_END:
				_currentUpperAnimeId = _slideShootEndHash;
				break;
			case HumanBase.AnimateId.ANI_WALLKICK:
				_currentUpperAnimeId = _walljumpStartShootHash;
				break;
			case HumanBase.AnimateId.ANI_WALLKICK_END:
				_currentUpperAnimeId = _walljumpEndShootHash;
				break;
			case HumanBase.AnimateId.ANI_WALLGRAB_END:
				_currentUpperAnimeId = _wallgrabEndShootHash;
				break;
			}
		}
		if (_idleChargeShootHash == _currentUpperAnimeId)
		{
			if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _currentUpperAnimeId)
			{
				float value = Mathf.Abs(Vector2.SignedAngle(Vector2.up, _shootDirection)) / 180f;
				_animator.SetFloat(hashDirection, value);
			}
		}
		else
		{
			float value2 = Mathf.Abs(Vector2.SignedAngle(Vector2.up, _shootDirection)) / 180f;
			_animator.SetFloat(hashDirection, value2);
		}
		if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _currentUpperAnimeId || !animationParams.AnimateUpperKeepFlag)
		{
			PlayAnimation(_currentUpperAnimeId, 0, timeFrame);
		}
	}

	public void PlayAnimation(HumanBase.AnimateId id, float timeFrame)
	{
		PlayAnimation(_animateStatus[(uint)id], 0, timeFrame);
	}

	public void PlayAnimation(int id, int layer, float timeFrame)
	{
		_animator.Play(id, 0, timeFrame);
		_animator.Update(0f);
		_animationTimer.TimerStart();
		LastUpdateFrame = GameLogicUpdateManager.GameFrame;
	}

	public void SetAttackLayerActive(Vector3 pShootDirection)
	{
		_shootDirection = pShootDirection;
		AimTarget.localPosition = _shootDirection * 3f;
	}

	public void UpdateDirection(int pDirection)
	{
		if (RealRotate)
		{
			ModelTransform.eulerAngles = new Vector3(0f, 90 * pDirection, 0f);
		}
		else
		{
			ModelTransform.localScale = new Vector3(1f, 1f, pDirection);
		}
		_direction = pDirection;
		if (_shootLevel != 0)
		{
			AimTarget.localPosition = new Vector3((float)_direction * Mathf.Abs(AimTarget.localPosition.x), AimTarget.localPosition.y, 0f);
		}
		else
		{
			AimTarget.localPosition = new Vector3(_direction, 0f, 0f);
		}
	}

	public void SetVelocity(Vector3 velocity)
	{
		_animator.SetFloat(hashVelocityX, velocity.x);
		_animator.SetFloat(hashVelocityY, velocity.y);
	}

	public void SetSpeedMultiplier(float speed)
	{
		_animator.SetFloat(hashSpeedMultiplier, speed);
	}

	public void SetAnimatorEquip(int equipType)
	{
		switch ((WeaponType)(short)equipType)
		{
		case WeaponType.Dummy:
			equipType = 0;
			break;
		case WeaponType.Buster:
			equipType = 1;
			break;
		case WeaponType.Spray:
			equipType = 2;
			break;
		case WeaponType.SprayHeavy:
			equipType = 3;
			break;
		case WeaponType.Melee:
			equipType = 4;
			break;
		case WeaponType.DualGun:
			equipType = 5;
			break;
		case WeaponType.MGun:
			equipType = 6;
			break;
		case WeaponType.Gatling:
			equipType = 7;
			break;
		case WeaponType.Launcher:
			equipType = 8;
			break;
		default:
			throw new ArgumentOutOfRangeException("equipType", equipType, null);
		}
		_animator.SetFloat(hashEquip, equipType);
	}
}
