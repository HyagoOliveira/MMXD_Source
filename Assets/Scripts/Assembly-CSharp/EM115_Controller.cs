using UnityEngine;

public class EM115_Controller : EM041_Controller
{
	protected override void Awake()
	{
		base.Awake();
		_animationHash = new int[4];
		_animationHash[0] = Animator.StringToHash("EM115@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM115@run_loop");
		_animationHash[2] = Animator.StringToHash("EM115@skill_01");
		_animationHash[3] = Animator.StringToHash("EM115@hurt_loop");
	}
}
