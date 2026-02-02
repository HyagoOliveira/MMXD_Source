using UnityEngine;

public class OrangeCamera : MonoBehaviour
{
	public Camera _camera;

	protected int cameraCullingMask = -1;

	public virtual void Awake()
	{
		cameraCullingMask = _camera.cullingMask;
	}

	public virtual void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.SCENE_INIT, CleraOtherCameraLayer);
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.SCENE_INIT, CleraOtherCameraLayer);
	}

	protected void CleraOtherCameraLayer()
	{
		Camera[] array = OrangeSceneManager.FindObjectsOfTypeCustom<Camera>();
		foreach (Camera camera in array)
		{
			if (!camera.GetComponent<OrangeCamera>())
			{
				int cullingMask = camera.cullingMask;
				camera.cullingMask = cullingMask & (cullingMask ^ cameraCullingMask);
			}
		}
	}
}
