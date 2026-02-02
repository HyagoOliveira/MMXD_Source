using StageLib;
using UnityEngine;

public class MoveCylinderElevator : ObjMoveSEBase
{
	[SerializeField]
	protected float disX = 11f;

	private OrangeCharacter oc;

	protected override void Start()
	{
		base.Start();
		base.transform.localPosition = Vector3.zero;
		oc = StageUpdate.GetMainPlayerOC();
	}

	protected override void Update()
	{
		base.Update();
		if (oc != null)
		{
			float num = Mathf.Abs(oc.transform.position.x - base.transform.position.x);
			isForseSE = num < disX;
		}
		else
		{
			oc = StageUpdate.GetMainPlayerOC();
		}
	}
}
