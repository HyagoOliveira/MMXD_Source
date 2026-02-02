using CallbackDefs;
using UnityEngine;

public class CollideBulletExit : MonoBehaviour
{
	private Callback<Transform> exitCB;

	public void Clear()
	{
		exitCB = null;
	}

	public void Setup(Callback<Transform> p_exitCB)
	{
		Clear();
		exitCB = p_exitCB;
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		exitCB.CheckTargetToInvoke(col.transform);
	}
}
