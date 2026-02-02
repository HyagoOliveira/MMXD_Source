using UnityEngine;

public class EM002_Controller : Em001Controller
{
	protected override void Awake()
	{
		base.Awake();
		HASH_IDLE_LOOP = Animator.StringToHash("EM002@idle_loop");
		HASH_HIDE_START = Animator.StringToHash("EM002@crouch_start");
		HASH_HIDE_LOOP = Animator.StringToHash("EM002@crouch_loop");
		HASH_HIDE_END = Animator.StringToHash("EM002@crouch_end");
		HASH_HURT_LOOP = Animator.StringToHash("EM002@hurt_loop");
		HASH_RUN_LOOP = Animator.StringToHash("EM002@run_loop");
		HASH_SK001 = Animator.StringToHash("EM002@skill_01");
	}
}
