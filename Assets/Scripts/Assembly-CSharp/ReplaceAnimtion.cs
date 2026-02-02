using System.Collections.Generic;
using DragonBones;
using UnityEngine;

public class ReplaceAnimtion : BaseDemo
{
	private UnityArmatureComponent _armatureCompA;

	private UnityArmatureComponent _armatureCompB;

	private UnityArmatureComponent _armatureCompC;

	private UnityArmatureComponent _armatureCompD;

	protected override void OnStart()
	{
		UnityFactory.factory.LoadDragonBonesData("mecha_2903/mecha_2903_ske");
		UnityFactory.factory.LoadTextureAtlasData("mecha_2903/mecha_2903_tex");
		_armatureCompA = UnityFactory.factory.BuildArmatureComponent("mecha_2903");
		_armatureCompB = UnityFactory.factory.BuildArmatureComponent("mecha_2903b");
		_armatureCompC = UnityFactory.factory.BuildArmatureComponent("mecha_2903c");
		_armatureCompD = UnityFactory.factory.BuildArmatureComponent("mecha_2903d");
		ArmatureData armatureData = UnityFactory.factory.GetArmatureData("mecha_2903d");
		UnityFactory.factory.ReplaceAnimation(_armatureCompA.armature, armatureData);
		UnityFactory.factory.ReplaceAnimation(_armatureCompB.armature, armatureData);
		UnityFactory.factory.ReplaceAnimation(_armatureCompC.armature, armatureData);
		_armatureCompA.transform.localPosition = new Vector3(-4f, -3f, 0f);
		_armatureCompB.transform.localPosition = new Vector3(0f, -3f, 0f);
		_armatureCompC.transform.localPosition = new Vector3(4f, -3f, 0f);
		_armatureCompD.transform.localPosition = new Vector3(0f, 0f, 0f);
	}

	protected override void OnUpdate()
	{
		if (Input.GetMouseButtonDown(0))
		{
			ChangeAnimtion();
		}
	}

	private void ChangeAnimtion()
	{
		string lastAnimationName = _armatureCompD.animation.lastAnimationName;
		if (!string.IsNullOrEmpty(lastAnimationName))
		{
			List<string> animationNames = _armatureCompD.animation.animationNames;
			int index = (animationNames.IndexOf(lastAnimationName) + 1) % animationNames.Count;
			_armatureCompD.animation.Play(animationNames[index]);
		}
		else
		{
			_armatureCompD.animation.Play();
		}
		lastAnimationName = _armatureCompD.animation.lastAnimationName;
		_armatureCompA.animation.Play(lastAnimationName);
		_armatureCompB.animation.Play(lastAnimationName);
		_armatureCompC.animation.Play(lastAnimationName);
	}
}
