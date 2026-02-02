using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountDownBar : MonoBehaviour
{
	public Image cdbar;

	private void OnEnable()
	{
		StartCoroutine(StartStageCoroutine());
	}

	public void SetFValue(float fV)
	{
		cdbar.fillAmount = fV;
	}

	private IEnumerator StartStageCoroutine()
	{
		while (MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera == null)
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		RectTransform rectTransform = (RectTransform)base.transform;
		float fCameraHHalf = ManagedSingleton<StageHelper>.Instance.fCameraHHalf;
		float num = ManagedSingleton<StageHelper>.Instance.fCameraWHalf * 2f / rectTransform.rect.width;
		if (num > fCameraHHalf * 2f / rectTransform.rect.height)
		{
			num = fCameraHHalf * 2f / rectTransform.rect.height;
		}
		rectTransform.localScale = new Vector3(num, num, num);
	}
}
