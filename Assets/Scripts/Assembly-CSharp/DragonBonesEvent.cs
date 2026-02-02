using DragonBones;
using UnityEngine;

public class DragonBonesEvent : BaseDemo
{
	private UnityArmatureComponent _mechaArmatureComp;

	[SerializeField]
	private AudioSource _sound;

	protected override void OnStart()
	{
		UnityFactory.factory.LoadDragonBonesData("mecha_1004d/mecha_1004d_ske");
		UnityFactory.factory.LoadTextureAtlasData("mecha_1004d/mecha_1004d_tex");
		_mechaArmatureComp = UnityFactory.factory.BuildArmatureComponent("mecha_1004d");
		_mechaArmatureComp.transform.localPosition = new Vector3(0f, -2f, 0f);
		_mechaArmatureComp.AddDBEventListener("complete", OnAnimationEventHandler);
		UnityFactory.factory.soundEventManager.AddDBEventListener("soundEvent", OnSoundEventHandler);
		_mechaArmatureComp.animation.Play("walk");
	}

	protected override void OnUpdate()
	{
		if (Input.GetMouseButtonDown(0))
		{
			_mechaArmatureComp.animation.FadeIn("skill_03", 0.2f);
		}
	}

	private void OnSoundEventHandler(string type, EventObject eventObject)
	{
		UnityEngine.Debug.Log(eventObject.name);
		if (eventObject.name == "footstep")
		{
			_sound.Play();
		}
	}

	private void OnAnimationEventHandler(string type, EventObject eventObject)
	{
		if (eventObject.animationState.name == "skill_03")
		{
			_mechaArmatureComp.animation.FadeIn("walk", 0.2f);
		}
	}
}
