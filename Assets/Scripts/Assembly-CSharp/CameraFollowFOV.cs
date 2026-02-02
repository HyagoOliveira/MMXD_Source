using UnityEngine;

public class CameraFollowFOV : MonoBehaviour
{
	[SerializeField]
	public Camera MainCamera;

	private Camera camera;

	private void Start()
	{
		camera = base.gameObject.GetComponent<Camera>();
	}

	private void Update()
	{
		camera.fieldOfView = MainCamera.fieldOfView;
	}
}
