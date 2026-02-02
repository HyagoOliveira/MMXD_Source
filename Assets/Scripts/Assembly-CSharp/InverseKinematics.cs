using System;
using DragonBones;
using UnityEngine;

public class InverseKinematics : BaseDemo
{
	private UnityArmatureComponent _armatureComp;

	private UnityArmatureComponent _floorBoardComp;

	private Bone _chestBone;

	private Bone _leftFootBone;

	private Bone _rightFootBone;

	private Bone _circleBone;

	private Bone _floorBoardBone;

	private DragonBones.AnimationState _aimState;

	private float _offsetRotation;

	private int _faceDir;

	private float _aimRadian;

	protected override void OnStart()
	{
		UnityFactory.factory.LoadDragonBonesData("mecha_1406/mecha_1406_ske");
		UnityFactory.factory.LoadTextureAtlasData("mecha_1406/mecha_1406_tex");
		UnityFactory.factory.LoadDragonBonesData("floor_board/floor_board_ske");
		UnityFactory.factory.LoadTextureAtlasData("floor_board/floor_board_tex");
		_armatureComp = UnityFactory.factory.BuildArmatureComponent("mecha_1406");
		_floorBoardComp = UnityFactory.factory.BuildArmatureComponent("floor_board");
		_chestBone = _armatureComp.armature.GetBone("chest");
		_leftFootBone = _armatureComp.armature.GetBone("foot_l");
		_rightFootBone = _armatureComp.armature.GetBone("foot_r");
		_circleBone = _floorBoardComp.armature.GetBone("circle");
		_floorBoardBone = _floorBoardComp.armature.GetBone("floor_board");
		_armatureComp.animation.Play("idle");
		_aimState = _armatureComp.animation.FadeIn("aim", 0.1f, 1, 0, "aimGroup");
		_aimState.resetToPose = false;
		_aimState.Stop();
		_floorBoardComp.animation.Play("idle");
		_floorBoardComp.armature.GetSlot("player").display = _armatureComp.gameObject;
		_armatureComp.transform.localPosition = Vector3.zero;
		_floorBoardComp.transform.localPosition = new Vector4(0f, -0.25f, 0f);
		_floorBoardComp.CloseCombineMeshs();
		EnableDrag(_floorBoardComp.armature.GetSlot("circle").display as GameObject);
	}

	protected override void OnUpdate()
	{
		_UpdateAim();
		_UpdateFoot();
	}

	private void _UpdateAim()
	{
		Vector3 vector = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane));
		Vector3 localPosition = _armatureComp.transform.localPosition;
		float num = _chestBone.global.y * base.transform.localScale.y;
		_faceDir = ((vector.x > 0f) ? 1 : (-1));
		_armatureComp.armature.flipX = _faceDir < 0;
		if (_faceDir > 0)
		{
			_aimRadian = Mathf.Atan2(0f - (vector.y - localPosition.y - num), vector.x - localPosition.x);
		}
		else
		{
			_aimRadian = (float)Math.PI - Mathf.Atan2(0f - (vector.y - localPosition.y - num), vector.x - localPosition.x);
			if (_aimRadian > (float)Math.PI)
			{
				_aimRadian -= (float)Math.PI * 2f;
			}
		}
		float num2 = Mathf.Abs((_aimRadian + (float)Math.PI / 2f) / (float)Math.PI);
		_aimState.currentTime = num2 * _aimState.totalTime;
	}

	private void _UpdateFoot()
	{
		float b = -0.43633232f;
		float b2 = 0.43633232f;
		float num = Mathf.Atan2(0f - _circleBone.global.y, _circleBone.global.x);
		if ((double)_circleBone.global.x < 0.0)
		{
			num = DragonBones.Transform.NormalizeRadian(num + (float)Math.PI);
		}
		_offsetRotation = Mathf.Min(Mathf.Max(num, b), b2);
		_floorBoardBone.offset.rotation = _offsetRotation;
		_floorBoardBone.InvalidUpdate();
		float num2 = Mathf.Tan(_offsetRotation);
		float num3 = 1f / Mathf.Sin((float)Math.PI / 2f - _offsetRotation) - 1f;
		_leftFootBone.offset.y = num2 * _leftFootBone.global.x + _leftFootBone.origin.y * num3;
		_leftFootBone.offset.rotation = _offsetRotation * (float)_faceDir;
		_leftFootBone.InvalidUpdate();
		_rightFootBone.offset.y = num2 * _rightFootBone.global.x + _rightFootBone.origin.y * num3;
		_rightFootBone.offset.rotation = _offsetRotation * (float)_faceDir;
		_rightFootBone.InvalidUpdate();
	}
}
