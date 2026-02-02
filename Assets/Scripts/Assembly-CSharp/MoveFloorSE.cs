using UnityEngine;

public class MoveFloorSE : ObjMoveSEBase
{
	protected override void Start()
	{
		base.Start();
		if (base.gameObject.name.Contains("MoveFloorSE"))
		{
			base.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		isForseSE = true;
	}
}
