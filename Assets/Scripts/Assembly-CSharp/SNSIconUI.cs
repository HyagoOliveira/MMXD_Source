using UnityEngine;
using UnityEngine.UI;

public class SNSIconUI : MonoBehaviour
{
	[SerializeField]
	private GameObject selectionFrame;

	[SerializeField]
	private Button[] socialNetworkButtons;

	private SNS_TYPE currentSNSSelection;

	private void Start()
	{
		if ((bool)selectionFrame)
		{
			selectionFrame.SetActive(false);
		}
		UpdateSelectionFrame(1);
	}

	public void Setup()
	{
	}

	public void OnClickSNSButton(int index)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		UpdateSelectionFrame(index);
	}

	public SNS_TYPE GetSNSSelection()
	{
		return currentSNSSelection;
	}

	private void UpdateSelectionFrame(int index)
	{
		currentSNSSelection = (SNS_TYPE)index;
		selectionFrame.SetActive(true);
		selectionFrame.transform.position = socialNetworkButtons[index].transform.position;
	}
}
