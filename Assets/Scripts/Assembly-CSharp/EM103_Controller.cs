using UnityEngine;

public class EM103_Controller : EM020_Controller
{
	public override void LogicUpdate()
	{
		if (Activate)
		{
			Vector3 localEulerAngles = ModelTransform.localEulerAngles;
			base.LogicUpdate();
			ModelTransform.localEulerAngles = localEulerAngles;
			ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		_transform.position = pos;
	}
}
