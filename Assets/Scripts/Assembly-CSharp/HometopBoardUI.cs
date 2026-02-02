using UnityEngine;

public class HometopBoardUI : MonoBehaviour
{
	public GameObject[] ButtonGroupOld;

	public GameObject[] ButtonGroupNew;

	public void Start()
	{
		bool isNewButtonUI = false;
		isNewButtonUI = true;
		ButtonGroupOld.ForEach(delegate(GameObject btn)
		{
			btn.SetActive(!isNewButtonUI);
		});
		ButtonGroupNew.ForEach(delegate(GameObject btn)
		{
			btn.SetActive(isNewButtonUI);
		});
	}
}
