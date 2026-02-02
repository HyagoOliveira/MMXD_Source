using UnityEngine;

public class ForceRotation : MonoBehaviour
{
	private Quaternion _rotation;

	public Vector3 Angle;

	private Transform _transform;

	private void Start()
	{
		_transform = base.transform;
		_rotation = Quaternion.Euler(Angle);
	}

	private void Update()
	{
		_transform.rotation = _rotation;
	}

	public void SetQuaternion(Quaternion pQuaternion)
	{
		_rotation = pQuaternion;
	}

	public void SetQuaternion(Vector3 pVector)
	{
		_rotation = Quaternion.Euler(pVector);
	}
}
