using System;
using System.Collections.Generic;
using DragonBones;
using UnityEngine;

public class Mecha
{
	private const float JUMP_SPEED = -0.2f;

	private const float NORMALIZE_MOVE_SPEED = 0.03f;

	private const float MAX_MOVE_SPEED_FRONT = 0.042f;

	private const float MAX_MOVE_SPEED_BACK = 0.03f;

	private const string NORMAL_ANIMATION_GROUP = "normal";

	private const string AIM_ANIMATION_GROUP = "aim";

	private const string ATTACK_ANIMATION_GROUP = "attack";

	private static readonly string[] WEAPON_L_LIST = new string[6] { "weapon_1502b_l", "weapon_1005", "weapon_1005b", "weapon_1005c", "weapon_1005d", "weapon_1005e" };

	private static readonly string[] WEAPON_R_LIST = new string[5] { "weapon_1502b_r", "weapon_1005", "weapon_1005b", "weapon_1005c", "weapon_1005d" };

	private static readonly string[] SKINS = new string[4] { "mecha_1502b", "skin_a", "skin_b", "skin_c" };

	private bool _isJumpingA;

	private bool _isJumpingB;

	private bool _isSquating;

	private bool _isAttackingA;

	private bool _isAttackingB;

	private int _weaponRIndex;

	private int _weaponLIndex;

	private int _skinIndex;

	private int _faceDir = 1;

	private int _aimDir;

	private int _moveDir;

	private float _aimRadian;

	private float _speedX;

	private float _speedY;

	private Armature _armature;

	private UnityArmatureComponent _armatureComponent;

	private Armature _weaponL;

	private Armature _weaponR;

	private DragonBones.AnimationState _aimState;

	private DragonBones.AnimationState _walkState;

	private DragonBones.AnimationState _attackState;

	private Vector2 _target = Vector2.zero;

	public Mecha()
	{
		_armatureComponent = UnityFactory.factory.BuildArmatureComponent("mecha_1502b");
		_armature = _armatureComponent.armature;
		_armatureComponent.transform.localPosition = new Vector3(0f, 0f, 0f);
		_armatureComponent.AddDBEventListener("fadeInComplete", _OnAnimationEventHandler);
		_armatureComponent.AddDBEventListener("fadeOutComplete", _OnAnimationEventHandler);
		_armatureComponent.AddDBEventListener("complete", _OnAnimationEventHandler);
		_weaponL = _armature.GetSlot("weapon_l").childArmature;
		_weaponR = _armature.GetSlot("weapon_r").childArmature;
		_weaponL.eventDispatcher.AddDBEventListener("frameEvent", _OnFrameEventHandler);
		_weaponR.eventDispatcher.AddDBEventListener("frameEvent", _OnFrameEventHandler);
		_UpdateAnimation();
	}

	public void Move(int dir)
	{
		if (_moveDir != dir)
		{
			_moveDir = dir;
			_UpdateAnimation();
		}
	}

	public void Jump()
	{
		if (!_isJumpingA)
		{
			_isJumpingA = true;
			_armature.animation.FadeIn("jump_1", -1f, -1, 0, "normal").resetToPose = false;
			_walkState = null;
		}
	}

	public void Squat(bool isSquating)
	{
		if (_isSquating != isSquating)
		{
			_isSquating = isSquating;
			_UpdateAnimation();
		}
	}

	public void Attack(bool isAttacking)
	{
		if (_isAttackingA != isAttacking)
		{
			_isAttackingA = isAttacking;
		}
	}

	public void SwitchWeaponL()
	{
		_weaponL.eventDispatcher.RemoveDBEventListener("frameEvent", _OnFrameEventHandler);
		_weaponLIndex++;
		_weaponLIndex %= WEAPON_L_LIST.Length;
		string armatureName = WEAPON_L_LIST[_weaponLIndex];
		_weaponL = UnityFactory.factory.BuildArmature(armatureName);
		_armature.GetSlot("weapon_l").childArmature = _weaponL;
		_weaponL.eventDispatcher.AddDBEventListener("frameEvent", _OnFrameEventHandler);
	}

	public void SwitchWeaponR()
	{
		_weaponR.eventDispatcher.RemoveDBEventListener("frameEvent", _OnFrameEventHandler);
		_weaponRIndex++;
		_weaponRIndex %= WEAPON_R_LIST.Length;
		string armatureName = WEAPON_R_LIST[_weaponRIndex];
		_weaponR = UnityFactory.factory.BuildArmature(armatureName);
		_armature.GetSlot("weapon_r").childArmature = _weaponR;
		_weaponR.eventDispatcher.AddDBEventListener("frameEvent", _OnFrameEventHandler);
	}

	public void SwitchSkin()
	{
		_skinIndex++;
		_skinIndex %= SKINS.Length;
		string name = SKINS[_skinIndex];
		SkinData defaultSkin = UnityFactory.factory.GetArmatureData(name).defaultSkin;
		List<string> list = new List<string>();
		list.Add("weapon_l");
		list.Add("weapon_r");
		UnityFactory.factory.ReplaceSkin(_armature, defaultSkin, false, list);
	}

	public void Aim(float x, float y)
	{
		_target.x = x;
		_target.y = y;
	}

	public void Update()
	{
		_UpdatePosition();
		_UpdateAim();
		_UpdateAttack();
	}

	private void _UpdatePosition()
	{
		if (_speedX == 0f && !_isJumpingB)
		{
			return;
		}
		Vector3 localPosition = _armatureComponent.transform.localPosition;
		if (_speedX != 0f)
		{
			localPosition.x += _speedX * _armatureComponent.animation.timeScale;
			if (localPosition.x < -4f)
			{
				localPosition.x = -4f;
			}
			else if (localPosition.x > 4f)
			{
				localPosition.x = 4f;
			}
		}
		if (_isJumpingB)
		{
			if (_speedY > -0.05f && _speedY + -0.005f <= -0.05f)
			{
				_armatureComponent.animation.FadeIn("jump_3", -1f, -1, 0, "normal").resetToPose = false;
			}
			_speedY += -0.005f;
			localPosition.y += _speedY * _armatureComponent.animation.timeScale;
			if (localPosition.y < 0f)
			{
				localPosition.y = 0f;
				_isJumpingA = false;
				_isJumpingB = false;
				_speedX = 0f;
				_speedY = 0f;
				_armatureComponent.animation.FadeIn("jump_4", -1f, -1, 0, "normal").resetToPose = false;
			}
		}
		_armatureComponent.transform.localPosition = localPosition;
	}

	private void _UpdateAim()
	{
		Vector3 localPosition = _armatureComponent.transform.localPosition;
		_faceDir = ((_target.x > localPosition.x) ? 1 : (-1));
		if (((float)_faceDir < 0f) ? (!_armatureComponent.armature.flipX) : _armatureComponent.armature.flipX)
		{
			_armatureComponent.armature.flipX = !_armatureComponent.armature.flipX;
			if (_moveDir != 0)
			{
				_UpdateAnimation();
			}
		}
		float num = _armatureComponent.armature.GetBone("chest").global.y * _armatureComponent.transform.localScale.y;
		if (_faceDir > 0)
		{
			_aimRadian = Mathf.Atan2(0f - (_target.y - localPosition.y - num), _target.x - localPosition.x);
		}
		else
		{
			_aimRadian = (float)Math.PI - Mathf.Atan2(0f - (_target.y - localPosition.y - num), _target.x - localPosition.x);
			if (_aimRadian > (float)Math.PI)
			{
				_aimRadian -= (float)Math.PI * 2f;
			}
		}
		int num2 = 0;
		num2 = ((!(_aimRadian > 0f)) ? 1 : (-1));
		if (_aimState == null || _aimDir != num2)
		{
			_aimDir = num2;
			if (_aimDir >= 0)
			{
				_aimState = _armatureComponent.animation.FadeIn("aim_up", -1f, 1, 0, "aim", AnimationFadeOutMode.SameGroup);
			}
			else
			{
				_aimState = _armatureComponent.animation.FadeIn("aim_down", -1f, 1, 0, "aim", AnimationFadeOutMode.SameGroup);
			}
			_aimState.resetToPose = false;
		}
		_aimState.weight = Mathf.Abs(_aimRadian / (float)Math.PI * 2f);
		_armatureComponent.armature.InvalidUpdate();
	}

	private void _UpdateAttack()
	{
		if (_isAttackingA && !_isAttackingB)
		{
			_isAttackingB = true;
			_attackState = _armature.animation.FadeIn("attack_01", -1f, -1, 0, "attack");
			_attackState.resetToPose = false;
			_attackState.autoFadeOutTime = _attackState.fadeTotalTime;
		}
	}

	private void _UpdateAnimation()
	{
		if (_isJumpingA)
		{
			return;
		}
		if (_isSquating)
		{
			_speedX = 0f;
			_armature.animation.FadeIn("squat", -1f, -1, 0, "normal").resetToPose = false;
			_walkState = null;
			return;
		}
		if (_moveDir == 0)
		{
			_speedX = 0f;
			_armature.animation.FadeIn("idle", -1f, -1, 0, "normal").resetToPose = false;
			_walkState = null;
			return;
		}
		if (_walkState == null)
		{
			_walkState = _armature.animation.FadeIn("walk", -1f, -1, 0, "normal");
			_walkState.resetToPose = false;
		}
		if (_moveDir * _faceDir > 0)
		{
			_walkState.timeScale = 1.4f;
		}
		else
		{
			_walkState.timeScale = -1f;
		}
		if (_moveDir * _faceDir > 0)
		{
			_speedX = 0.042f * (float)_faceDir;
		}
		else
		{
			_speedX = -0.03f * (float)_faceDir;
		}
	}

	private void _Fire(Vector3 firePoint)
	{
		firePoint.x += UnityEngine.Random.Range(-0.01f, 0.01f);
		firePoint.y += UnityEngine.Random.Range(-0.01f, 0.01f);
		firePoint.z = -0.2f;
		UnityArmatureComponent unityArmatureComponent = UnityFactory.factory.BuildArmatureComponent("bullet_01");
		Bullet bullet = unityArmatureComponent.gameObject.AddComponent<Bullet>();
		float num = ((_faceDir < 0) ? ((float)Math.PI - _aimRadian) : _aimRadian);
		unityArmatureComponent.animation.timeScale = _armatureComponent.animation.timeScale;
		bullet.transform.position = firePoint;
		bullet.Init("fire_effect_01", num + UnityEngine.Random.Range(-0.01f, 0.01f), 0.4f);
	}

	private void _OnAnimationEventHandler(string type, EventObject evt)
	{
		switch (evt.type)
		{
		case "fadeInComplete":
			if (!(evt.animationState.name == "jump_1"))
			{
				break;
			}
			_isJumpingB = true;
			_speedY = 0.2f;
			if (_moveDir != 0)
			{
				if (_moveDir * _faceDir > 0)
				{
					_speedX = 0.042f * (float)_faceDir;
				}
				else
				{
					_speedX = -0.03f * (float)_faceDir;
				}
			}
			_armature.animation.FadeIn("jump_2", -1f, -1, 0, "normal").resetToPose = false;
			break;
		case "fadeOutComplete":
			if (evt.animationState.name == "attack_01")
			{
				_isAttackingB = false;
				_attackState = null;
			}
			break;
		case "complete":
			if (evt.animationState.name == "jump_4")
			{
				_isJumpingA = false;
				_isJumpingB = false;
				_UpdateAnimation();
			}
			break;
		}
	}

	private void _OnFrameEventHandler(string type, EventObject eventObject)
	{
		if (eventObject.name == "fire")
		{
			UnityEngine.Transform transform = (eventObject.armature.display as GameObject).transform;
			Vector3 point = new Vector3(eventObject.bone.global.x, 0f - eventObject.bone.global.y, 0f);
			Vector3 firePoint = transform.worldToLocalMatrix.inverse.MultiplyPoint(point);
			_Fire(firePoint);
		}
	}
}
