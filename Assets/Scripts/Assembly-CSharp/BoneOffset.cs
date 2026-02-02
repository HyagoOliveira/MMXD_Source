using System;
using DragonBones;
using UnityEngine;

public class BoneOffset : BaseDemo
{
	protected override void OnStart()
	{
		UnityFactory.factory.LoadDragonBonesData("bullet_01/bullet_01_ske");
		UnityFactory.factory.LoadTextureAtlasData("bullet_01/bullet_01_tex");
		for (int i = 0; i < 100; i++)
		{
			UnityArmatureComponent unityArmatureComponent = UnityFactory.factory.BuildArmatureComponent("bullet_01");
			unityArmatureComponent.AddDBEventListener("complete", _OnAnimationHandler);
			_MoveTo(unityArmatureComponent);
		}
	}

	private void _OnAnimationHandler(string type, EventObject eventObject)
	{
		_MoveTo(eventObject.armature.proxy as UnityArmatureComponent);
	}

	private void _MoveTo(UnityArmatureComponent armatureComp)
	{
		float x = armatureComp.transform.localPosition.x;
		float y = armatureComp.transform.localPosition.y;
		float num = UnityEngine.Random.Range(0f, 1f) * (float)Screen.width - (float)Screen.width * 0.5f;
		float num2 = UnityEngine.Random.Range(0f, 1f) * (float)Screen.height - (float)Screen.height * 0.5f;
		float num3 = num - x;
		float num4 = num2 - y;
		Bone bone = armatureComp.armature.GetBone("root");
		Bone bone2 = armatureComp.armature.GetBone("bullet");
		bone.offset.scaleX = Mathf.Sqrt(num3 * num3 + num4 * num4) / 100f;
		bone.offset.rotation = Mathf.Atan2(num4, num3);
		bone.offset.skew = UnityEngine.Random.Range(0f, 1f) * (float)Math.PI - (float)Math.PI / 2f;
		bone2.offset.scaleX = 0.5f + UnityEngine.Random.Range(0f, 1f) * 0.5f;
		bone2.offset.scaleY = 0.5f + UnityEngine.Random.Range(0f, 1f) * 0.5f;
		bone.InvalidUpdate();
		bone2.InvalidUpdate();
		armatureComp.animation.timeScale = 0.5f + UnityEngine.Random.Range(0f, 1f) * 1f;
		armatureComp.animation.Play("idle", 1);
	}
}
