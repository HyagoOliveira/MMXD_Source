using UnityEngine;

public class StageAnimationUpdateCall : StateMachineBehaviour
{
	public string sName = "NowStatus";

	public int nValue;

	public bool bIsCallFunction;

	public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!bIsCallFunction)
		{
			animator.SetInteger(sName, nValue);
			return;
		}
		StageAnimationEvent component = animator.transform.GetComponent<StageAnimationEvent>();
		if (!(component == null))
		{
			component.SendMessageToCtrl(sName);
		}
	}
}
