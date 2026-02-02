using UnityEngine;

public class Fxlooptheloop : FxBase
{
	public static string CLIP_NAME = "p_looptheloop_000_Anim";

	[SerializeField]
	private GameObject[] activeObject = new GameObject[4];

	[SerializeField]
	private Animation animation;

	public override void Active(params object[] p_params)
	{
		base.Active(p_params);
		GameObject[] array = activeObject;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(true);
		}
		float time = (float)p_params[0];
		animation[CLIP_NAME].time = time;
	}

	public void SetInactive(int index)
	{
		if (index < activeObject.Length)
		{
			activeObject[index].SetActive(false);
		}
	}
}
