using UnityEngine;
using UnityEngine.UI;

public class BattleDebug : MonoBehaviour
{
	public GameObject view;

	private float fovRange = 28f;

	[SerializeField]
	private Scrollbar fovScrollbar;

	[SerializeField]
	private Text textFov;

	private CameraControl cameraControl;

	private void Awake()
	{
		Object.Destroy(base.gameObject);
	}

	public void UpdateFOV()
	{
		if (null == cameraControl)
		{
			cameraControl = OrangeSceneManager.FindObjectOfTypeCustom<CameraControl>();
		}
		if (!(null == cameraControl))
		{
			fovRange = Mathf.Clamp(fovScrollbar.value * 56f, 1f, 56f);
			cameraControl.DesignFov = fovRange;
			textFov.text = fovRange.ToString();
			Shader.SetGlobalFloat("cameraFOV", fovRange);
		}
	}
}
