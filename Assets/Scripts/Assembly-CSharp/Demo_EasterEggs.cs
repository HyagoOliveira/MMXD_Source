using UnityEngine;

public class Demo_EasterEggs : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private int triggerEasterEggCount;

	public bool isStart { get; set; }

	private void LateUpdate()
	{
		if (!animator || !isStart)
		{
			return;
		}
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.IsName("2") && currentAnimatorStateInfo.normalizedTime >= (float)triggerEasterEggCount)
		{
			if ((bool)GetComponent<CharacterAnimatonEvent>())
			{
				GetComponent<CharacterAnimatonEvent>().IgnoreAnimEvents = false;
			}
			animator.Play("3");
			isStart = false;
			base.enabled = false;
		}
	}
}
