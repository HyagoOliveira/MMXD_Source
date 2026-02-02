using UnityEngine;
using UnityEngine.UI;

public class UIButtonSE : MonoBehaviour
{
	[SerializeField]
	private int SystemSE_CueID;

	private void Start()
	{
		Button button = null;
		Toggle toggle = null;
		if (SystemSE_CueID == 0)
		{
			return;
		}
		button = base.transform.GetComponent<Button>();
		if (button != null)
		{
			button.onClick.AddListener(delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE((SystemSE)SystemSE_CueID);
			});
			return;
		}
		toggle = base.transform.GetComponent<Toggle>();
		if (toggle != null)
		{
			toggle.onValueChanged.AddListener(delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE((SystemSE)SystemSE_CueID);
			});
		}
	}

	private void Update()
	{
	}
}
