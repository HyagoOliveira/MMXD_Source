#define RELEASE
using UnityEngine;

public class CharacterStateStartBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Debug.Log("[CharacterStateStartBehaviour] OnStateExit");
		animator.GetComponent<CharacterAnimatonEvent>().IgnoreAnimEvents = true;
	}
}
