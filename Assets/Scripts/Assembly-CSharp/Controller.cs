using UnityEngine;

public class Controller : MonoBehaviour
{
	public float sensitivityX = 45f;

	public Transform cameraTm;

	private bool down;

	private Camera _camera;

	private Vector3 point = Vector3.zero;

	private float rotationX;

	private float speed = 0.05f;

	private void Awake()
	{
		_camera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
	}

    [System.Obsolete]
    private void Update()
	{
		if (Input.touchCount <= 1 && Input.touchCount != 0)
		{
			if (Mathf.Abs(cameraTm.position.x - point.x) < 0.7f && Mathf.Abs(cameraTm.position.y - point.y) < 0.7f)
			{
				down = true;
			}
			else
			{
				down = false;
			}
			if (down)
			{
				Vector2 deltaPosition = Input.GetTouch(0).deltaPosition;
				base.transform.RotateAroundLocal(cameraTm.up, (0f - deltaPosition.x) * speed);
			}
		}
	}
}
