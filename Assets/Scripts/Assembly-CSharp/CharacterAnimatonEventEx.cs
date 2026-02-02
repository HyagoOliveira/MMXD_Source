using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;

public class CharacterAnimatonEventEx : MonoBehaviour
{
	private Queue<Callback> queueCB = new Queue<Callback>();

	public void AddDisableCB(Callback p_cb)
	{
		queueCB.Enqueue(p_cb);
	}

	private void OnDisable()
	{
		while (queueCB.Count > 0)
		{
			queueCB.Dequeue().CheckTargetToInvoke();
		}
	}

	private void OnDestroy()
	{
		queueCB.Clear();
	}
}
