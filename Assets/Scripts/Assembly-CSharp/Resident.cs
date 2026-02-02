using UnityEngine;

public class Resident : MonoBehaviour
{
	private TopResidentUI topResidentUI;

	public void ActiveTopResidentUI()
	{
		if (topResidentUI != null)
		{
			topResidentUI.SetUIActive(true);
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("UI/UI_TopResident", "UI_TopResident", delegate(GameObject asset)
		{
			GameObject gameObject = Object.Instantiate(asset, base.transform, false);
			topResidentUI = gameObject.GetComponent<TopResidentUI>();
			topResidentUI.SetUIActive(true);
		});
	}

	public void DisableTopResidentUI()
	{
		if (topResidentUI != null)
		{
			topResidentUI.SetUIActive(false);
		}
	}
}
