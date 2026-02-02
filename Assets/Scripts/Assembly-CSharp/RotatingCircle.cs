using UnityEngine;

public class RotatingCircle : MonoBehaviour
{
	[SerializeField]
	[Range(-100f, 100f)]
	private float m_RotationSpeed;

	private void Start()
	{
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		base.transform.Rotate(new Vector3(0f, 0f, 1f) * deltaTime * m_RotationSpeed);
	}
}
