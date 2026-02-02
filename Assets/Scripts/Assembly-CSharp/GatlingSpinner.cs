using UnityEngine;

public class GatlingSpinner : MonoBehaviour
{
	public float SpinSpeed;

	public float MaxSpinSpeed = 30f;

	public float Accelerate = 60f;

	private Transform _transform;

	private Vector3 _angle = Vector3.zero;

	public bool Activate;

	private float forward = 1f;

	private void Start()
	{
		_transform = base.transform;
	}

	private void Update()
	{
		if (Activate)
		{
			if (SpinSpeed < MaxSpinSpeed)
			{
				SpinSpeed = Mathf.Min(SpinSpeed + Time.deltaTime * Accelerate, MaxSpinSpeed);
			}
		}
		else if (SpinSpeed > 0f)
		{
			SpinSpeed = Mathf.Max(SpinSpeed - Time.deltaTime * Accelerate, 0f);
		}
		_angle.z += forward * SpinSpeed;
		_transform.localEulerAngles = _angle;
	}
}
