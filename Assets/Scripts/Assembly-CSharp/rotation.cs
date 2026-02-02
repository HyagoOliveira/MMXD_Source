using UnityEngine;

public class rotation : MonoBehaviour
{
	public float xRotation;

	public float yRotation;

	public float zRotation;

	private void OnEnable()
	{
		InvokeRepeating("rotate", 0f, 0.0167f);
	}

	private void OnDisable()
	{
		CancelInvoke();
	}

	public void clickOn()
	{
		InvokeRepeating("rotate", 0f, 0.0167f);
	}

	public void clickOff()
	{
		CancelInvoke();
	}

	private void rotate()
	{
		base.transform.localEulerAngles += new Vector3(xRotation, yRotation, zRotation);
	}
}
