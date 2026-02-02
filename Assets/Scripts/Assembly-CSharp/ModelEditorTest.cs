using System.Collections.Generic;
using UnityEngine;

public class ModelEditorTest : MonoBehaviour
{
	private Camera _camera;

	public int searchIdx;

	private List<string[]> listEnemy = new List<string[]>();

	private GameObject newObj;

	private void Start()
	{
	}

	public void GetNextEnemy(int add)
	{
	}

	public void Rotate()
	{
		if (newObj != null)
		{
			newObj.transform.Rotate(new Vector3(0f, 90f, 0f));
		}
	}

	private void UpdateCameraZ(float add)
	{
		Vector3 position = _camera.transform.position;
		_camera.transform.position = new Vector3(position.x, position.y, position.z + add);
	}

	private void UpdateCameraX(float add)
	{
		Vector3 position = _camera.transform.position;
		_camera.transform.position = new Vector3(position.x + add, position.y, position.z);
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.UpArrow))
		{
			UpdateCameraZ(0.2f);
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			UpdateCameraZ(-0.2f);
		}
		else if (Input.GetKeyDown(KeyCode.Space))
		{
			_camera.transform.position = new Vector3(-0.23f, 2.92f, -18f);
		}
		else if (Input.GetKey(KeyCode.LeftArrow))
		{
			UpdateCameraX(-0.2f);
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			UpdateCameraX(0.2f);
		}
	}
}
