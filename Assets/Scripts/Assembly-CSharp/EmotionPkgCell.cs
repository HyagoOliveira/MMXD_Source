using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class EmotionPkgCell : ScrollIndexCallback
{
	[SerializeField]
	private MessageDialogUI mdUI;

	[SerializeField]
	private Image mIcon;

	public int pkgID;

	private UIEffect eff;

	private void SetImage(string name)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetEmotionPkgBundle(pkgID), name, delegate(Sprite obj)
		{
			mIcon.sprite = obj;
		});
	}

	public override void BackToPool()
	{
		base.gameObject.SetActive(false);
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
	}

	public override void ScrollCellIndex(int p_idx)
	{
		pkgID = p_idx + 1;
		string image = string.Format(AssetBundleScriptableObject.Instance.m_chat_pkg_icon_format, pkgID);
		SetImage(image);
		base.gameObject.SetActive(true);
		eff = base.gameObject.GetComponent<UIEffect>();
		SetCellGray(mdUI.m_currentPkgId != pkgID);
	}

	public void OnClickPkgIcon()
	{
		mdUI.SelectPkgIcon(this);
	}

	public void SetCellGray(bool boolen)
	{
		eff.effectFactor = (boolen ? 0.8f : 0f);
	}
}
