using UnityEngine;

public class Scroller : MonoBehaviour
{
	private Vector3 startPosition;

	public float scrollSpeed;

	public float tileSizeZ = 1f;

	private void Start()
	{
		startPosition = base.transform.position;
	}

	private void Update()
	{
		float num = Mathf.Repeat(Time.time * scrollSpeed, tileSizeZ);
		base.transform.position = startPosition + Vector3.up * num;
	}
}
