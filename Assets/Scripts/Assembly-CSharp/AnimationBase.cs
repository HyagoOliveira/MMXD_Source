using DragonBones;
using UnityEngine;

public class AnimationBase : BaseDemo
{
	private UnityArmatureComponent _armatureComp;

	protected override void OnStart()
	{
		UnityFactory.factory.LoadDragonBonesData("progress_bar/progress_bar_ske");
		UnityFactory.factory.LoadTextureAtlasData("progress_bar/progress_bar_tex");
		_armatureComp = UnityFactory.factory.BuildArmatureComponent("progress_bar");
		_armatureComp.AddDBEventListener("start", OnAnimationEventHandler);
		_armatureComp.AddDBEventListener("loopComplete", OnAnimationEventHandler);
		_armatureComp.AddDBEventListener("complete", OnAnimationEventHandler);
		_armatureComp.AddDBEventListener("fadeIn", OnAnimationEventHandler);
		_armatureComp.AddDBEventListener("fadeInComplete", OnAnimationEventHandler);
		_armatureComp.AddDBEventListener("fadeOut", OnAnimationEventHandler);
		_armatureComp.AddDBEventListener("fadeOutComplete", OnAnimationEventHandler);
		_armatureComp.AddDBEventListener("frameEvent", OnAnimationEventHandler);
		_armatureComp.animation.Play("idle");
	}

	protected override void OnTouch(TouchType type)
	{
		Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector3 localPosition = _armatureComp.transform.localPosition;
		float a = (vector.x - localPosition.x + 3f) / 6f;
		a = Mathf.Min(Mathf.Max(a, 0f), 1f);
		switch (type)
		{
		case TouchType.TOUCH_BEGIN:
			_armatureComp.animation.GotoAndStopByProgress("idle", a);
			break;
		case TouchType.TOUCH_END:
			_armatureComp.animation.Play();
			break;
		case TouchType.TOUCH_MOVE:
		{
			DragonBones.AnimationState state = _armatureComp.animation.GetState("idle");
			if (state != null)
			{
				state.currentTime = state.totalTime * a;
			}
			break;
		}
		}
	}

	private void OnAnimationEventHandler(string type, EventObject eventObject)
	{
		UnityEngine.Debug.Log(string.Format("animationName:{0},eventType:{1},eventName:{2}", eventObject.animationState.name, type, eventObject.name));
	}
}
