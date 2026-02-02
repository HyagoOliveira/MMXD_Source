using UnityEngine;

public class EM106_Controller : EM027_Controller, IManagedUpdateBehavior
{
	protected override void Awake()
	{
		base.Awake();
		_animationHash[0] = Animator.StringToHash("EM106@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM106@run_loop");
		_animationHash[2] = Animator.StringToHash("EM106@skill_01");
		_animationHash[3] = Animator.StringToHash("EM106@hurt_loop");
		AiTimer.TimerStart();
	}
}
