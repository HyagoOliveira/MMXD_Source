using UnityEngine;

public class boxCloudController : MonoBehaviour
{
	public bool ValueOffset;

	public float positionSinSpeed;

	public float positionSinAmplitude;

	public float sizeOffset = 1f;

	public float scaleSinSpeed;

	public float scaleSinAmplitude;

	private Vector3 sinDirection;

	private float offsetRange;

	private Vector3 _initialPosition;

	private void Start()
	{
		switch (Random.Range(0, 3))
		{
		case 0:
			sinDirection = Vector3.up;
			break;
		case 1:
			sinDirection = Vector3.right;
			break;
		case 2:
			sinDirection = Vector3.forward;
			break;
		}
		float num = Random.Range(1f, 10f);
		if (ValueOffset)
		{
			offsetRange = num;
		}
		else
		{
			offsetRange = 0f;
		}
		_initialPosition = base.transform.position;
	}

	private void Update()
	{
		Vector3 vector = sinDirection.normalized * Mathf.Sin(Time.time * 0.1f * positionSinSpeed + offsetRange) * (positionSinAmplitude * 0.1f);
		base.transform.localPosition = _initialPosition + vector;
		float num = Mathf.Abs(Mathf.Sin(Time.time * 0.1f * scaleSinSpeed + offsetRange)) * (scaleSinAmplitude * 1f) + 1.5f;
		base.transform.localScale = new Vector3(sizeOffset, sizeOffset, sizeOffset) * num;
	}
}
