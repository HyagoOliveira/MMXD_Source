using UnityEngine;

public class RotationRateController : MonoBehaviour
{
	public enum EyesUpdateType
	{
		All = 0,
		CLOSE = 1,
		OPEN = 2
	}

	[SerializeField]
	private EyesUpdateType eyesUpdateType;

	[SerializeField]
	private Transform[] arrTrans;

	[SerializeField]
	private float rate;

	private void LateUpdate()
	{
		switch (eyesUpdateType)
		{
		case EyesUpdateType.All:
			UpdateRotation();
			break;
		case EyesUpdateType.CLOSE:
		{
			Animator component2 = GetComponent<Animator>();
			if ((bool)component2 && component2.GetCurrentAnimatorStateInfo(1).IsName("face_2"))
			{
				UpdateRotation();
			}
			break;
		}
		case EyesUpdateType.OPEN:
		{
			Animator component = GetComponent<Animator>();
			if ((bool)component && component.GetCurrentAnimatorStateInfo(1).IsName("face_1"))
			{
				UpdateRotation();
			}
			break;
		}
		}
	}

	private void UpdateRotation()
	{
		Transform[] array = arrTrans;
		foreach (Transform obj in array)
		{
			obj.localRotation = Quaternion.Euler(obj.localRotation.eulerAngles * rate);
		}
	}
}
