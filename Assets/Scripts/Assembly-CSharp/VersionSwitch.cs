using UnityEngine;

public class VersionSwitch : MonoBehaviour
{
	public enum VERSION_ENUM
	{
		UD_ALL = 0,
		UD_11_3 = 1
	}

	public VERSION_ENUM eVersion;

	public bool bOpen = true;

	private void Awake()
	{
		VERSION_ENUM vERSION_ENUM = eVersion;
		if (vERSION_ENUM != 0 && vERSION_ENUM == VERSION_ENUM.UD_11_3)
		{
			base.gameObject.SetActive(bOpen);
		}
	}
}
