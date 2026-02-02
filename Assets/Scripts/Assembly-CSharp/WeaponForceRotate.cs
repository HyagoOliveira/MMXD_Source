using UnityEngine;

public class WeaponForceRotate : MonoBehaviour
{
	[SerializeField]
	private Transform[] rotateTransform;

	[SerializeField]
	private Vector3[] angles;

	public void TriggerRotate()
	{
		if (rotateTransform != null && angles != null && rotateTransform.Length == angles.Length)
		{
			for (int i = 0; i < rotateTransform.Length; i++)
			{
				rotateTransform[i].localRotation = Quaternion.Euler(angles[i]);
			}
		}
	}
}
