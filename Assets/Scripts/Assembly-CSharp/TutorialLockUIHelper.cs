using UnityEngine;

public class TutorialLockUIHelper : MonoBehaviour
{
	[SerializeField]
	private Camera _camera;

	private string _path = string.Empty;

	public Camera UICamera
	{
		get
		{
			return _camera;
		}
	}

	private void Start()
	{
		if (_camera == null)
		{
			_camera = MonoBehaviourSingleton<UIManager>.Instance.UICamera;
		}
		_path = base.gameObject.GetFullPath(true);
		TurtorialUI.RegisterLockUIName(_path, this);
	}

	private void OnDestroy()
	{
		if (!string.IsNullOrEmpty(_path))
		{
			TurtorialUI.UnregisterLockUIName(_path);
		}
	}
}
