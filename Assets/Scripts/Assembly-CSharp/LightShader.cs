using UnityEngine;

public class LightShader : BaseDemo
{
	public Transform target;

	private void Awake()
	{
		_isCreateBackground = false;
	}

	protected override void OnUpdate()
	{
		base.transform.RotateAround(new Vector3(1.2f, 1.2f, -0.5f), Vector3.forward, 4f);
		target.localEulerAngles = new Vector3(Mathf.Sin(Time.realtimeSinceStartup) * 10f + 5f, 0f, 0f);
	}
}
