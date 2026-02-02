using DragonBones;
using UnityEngine;

public class HelloDragonBones : BaseDemo
{
	public UnityDragonBonesData dragonBoneData;

	protected override void OnStart()
	{
		UnityFactory.factory.LoadData(dragonBoneData);
		UnityArmatureComponent unityArmatureComponent = UnityFactory.factory.BuildArmatureComponent("mecha_1002_101d", "mecha_1002_101d_show");
		unityArmatureComponent.animation.Play("idle");
		unityArmatureComponent.name = "dynamic_mecha_1002_101d";
		unityArmatureComponent.transform.localPosition = new Vector3(3f, -1.5f, 1f);
	}
}
