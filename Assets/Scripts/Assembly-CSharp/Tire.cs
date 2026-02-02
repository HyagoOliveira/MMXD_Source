using UnityEngine;

public class Tire : MonoBehaviour
{
	public Transform[] TireList;

	public float rotX;

	public float rotY;

	public float rotZ;

	private void Start()
	{
	}

	private void Update()
	{
		for (int i = 0; i < TireList.Length; i++)
		{
			TireList[i].Rotate(new Vector3(rotX, rotY, rotZ));
		}
	}
}
