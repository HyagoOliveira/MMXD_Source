using System.Collections;
using UnityEngine;

public class CharacterAnimatonRandWink : MonoBehaviour
{
	private readonly string triggerName = "wink";

	private Animator animator;

	public void Setup(Animator p_animator)
	{
		animator = p_animator;
		StartCoroutine(OnRandWink());
	}

	private IEnumerator OnRandWink()
	{
		while (true)
		{
			yield return CoroutineDefine._1sec;
			if ((bool)animator && Random.Range(0, 100) >= 66)
			{
				animator.SetTrigger(triggerName);
				yield return CoroutineDefine._1sec;
			}
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}
}
