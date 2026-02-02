using UnityEngine;

public class SineEffect : MonoBehaviour
{
	private Transform _transform;

	private Vector3 _tempPosition;

	public float ShiftY = 0.25f;

	public float TimeMultiplier = 20f;

	public float StartTime;

	private void Start()
	{
		_transform = base.transform;
	}

	private void Update()
	{
		_tempPosition = _transform.localPosition;
		_tempPosition.y = Mathf.Cos((Time.timeSinceLevelLoad - StartTime) * TimeMultiplier) * ShiftY;
		_transform.localPosition = _tempPosition;
	}
}
