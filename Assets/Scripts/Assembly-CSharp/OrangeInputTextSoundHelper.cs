using UnityEngine;

public class OrangeInputTextSoundHelper : MonoBehaviour
{
	private bool canPlayClickInputSE = true;

	public void OnClickInputArea()
	{
		if (canPlayClickInputSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			canPlayClickInputSE = false;
		}
	}

	public void OnEndEditInput()
	{
		canPlayClickInputSE = true;
	}
}
