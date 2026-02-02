using UnityEngine;

public class CharacterStateLoopBehaviour : StateMachineBehaviour
{
	private bool isTriggerClear;

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!isTriggerClear && stateInfo.normalizedTime >= 0.9f)
		{
			isTriggerClear = true;
			animator.GetComponent<CharacterAnimatonEvent>().IgnoreAnimEvents = true;
		}
	}
}
