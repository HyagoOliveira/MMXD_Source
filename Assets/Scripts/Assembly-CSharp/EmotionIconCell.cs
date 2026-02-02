using UnityEngine;
using UnityEngine.UI;

public class EmotionIconCell : ScrollIndexCallback
{
	[SerializeField]
	private MessageDialogUI mdUI;

	private int iconId;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public override void ScrollCellIndex(int p_idx)
	{
		iconId = p_idx + 1;
		string emotionPkgBundle = AssetBundleScriptableObject.Instance.GetEmotionPkgBundle(mdUI.m_currentPkgId);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(emotionPkgBundle, mdUI.iconList[p_idx].s_TEXTURE, delegate(Sprite obj)
		{
			if ((bool)obj)
			{
				GetComponent<Image>().sprite = obj;
			}
		});
		base.gameObject.SetActive(true);
	}

	public void OnClickEmotionIcon()
	{
		mdUI.SelectEmotionIcon(iconId);
		mdUI.ColseEmotionSelect();
	}
}
