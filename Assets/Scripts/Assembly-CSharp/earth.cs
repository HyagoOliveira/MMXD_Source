using UnityEngine;

public class earth : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(new Vector3(0f, 0f, 0.2f));
	}
}
