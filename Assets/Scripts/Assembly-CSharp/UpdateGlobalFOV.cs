using UnityEngine;

public class UpdateGlobalFOV : MonoBehaviour
{
	private void Start()
	{
		Shader.SetGlobalFloat("cameraFOV", Camera.main.fieldOfView);
		Object.Destroy(this);
	}
}
