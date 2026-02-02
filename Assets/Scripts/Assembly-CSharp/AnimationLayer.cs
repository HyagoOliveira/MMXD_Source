using DragonBones;
using UnityEngine;

public class AnimationLayer : BaseDemo
{
	private UnityArmatureComponent _mechaArmatureComp;

	protected override void OnStart()
	{
		UnityFactory.factory.LoadDragonBonesData("mecha_1004d/mecha_1004d_ske");
		UnityFactory.factory.LoadTextureAtlasData("mecha_1004d/mecha_1004d_tex");
		_mechaArmatureComp = UnityFactory.factory.BuildArmatureComponent("mecha_1004d");
		_mechaArmatureComp.AddDBEventListener("loopComplete", OnAnimationEventHandler);
		_mechaArmatureComp.animation.Play("walk");
		_mechaArmatureComp.transform.localPosition = new Vector3(0f, -2f, 0f);
	}

	private void OnAnimationEventHandler(string type, EventObject eventObject)
	{
		if (_mechaArmatureComp.animation.GetState("attack_01") == null)
		{
			DragonBones.AnimationState animationState = _mechaArmatureComp.animation.FadeIn("attack_01", 0.2f, 1, 1);
			animationState.resetToPose = true;
			animationState.autoFadeOutTime = 0.1f;
			animationState.AddBoneMask("chest");
			animationState.AddBoneMask("effect_l");
			animationState.AddBoneMask("effect_r");
		}
	}
}
