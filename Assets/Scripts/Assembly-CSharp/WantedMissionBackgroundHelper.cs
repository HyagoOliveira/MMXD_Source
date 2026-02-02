using UnityEngine;
using UnityEngine.UI;

public class WantedMissionBackgroundHelper : MonoBehaviour
{
	[SerializeField]
	private Image _imageBG;

	public void Setup(WANTED_TABLE wantedAttrData)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Sprite>("ui/background/" + wantedAttrData.s_STAGE_ICON, wantedAttrData.s_STAGE_ICON, OnBackgroundLoaded);
	}

	private void OnBackgroundLoaded(Sprite sprite)
	{
		_imageBG.sprite = sprite;
	}
}
