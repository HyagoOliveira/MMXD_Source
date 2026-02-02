using UnityEngine;
using UnityEngine.UI;

public class FullLoadingUI : MonoBehaviour
{
	[SerializeField]
	private Text textLoadingProgress;

	[SerializeField]
	private Image imgFill;

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<int, int>(EventManager.ID.UPDATE_FULL_LOADING_PROGRESS, UpdateProgress);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<int, int>(EventManager.ID.UPDATE_FULL_LOADING_PROGRESS, UpdateProgress);
	}

	public void UpdateProgress(int nowValue, int maxValue)
	{
		textLoadingProgress.text = nowValue + "/" + maxValue;
		imgFill.fillAmount = Mathf.Clamp01((float)nowValue / (float)maxValue);
	}
}
