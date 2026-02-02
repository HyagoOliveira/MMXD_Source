using UnityEngine;

public class earth1 : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(new Vector3(0f, 0f, -0.4f));
	}
}
