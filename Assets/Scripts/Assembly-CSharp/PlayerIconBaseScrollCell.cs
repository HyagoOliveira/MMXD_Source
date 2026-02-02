using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIconBaseScrollCell : IconBase
{
	[SerializeField]
	private Image ImgFrame;

	[SerializeField]
	private Image ImgFrameSelect;

	[SerializeField]
	private Image ImgFrameUsed;

	[SerializeField]
	private Image ImgIconBase;

	[SerializeField]
	private Image imgRareBg;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickIconSE;

	private PlayerCustomizeUI parentPlayerCustomizeUI;

	private int _id;

	private string assetName;

	private bool bIsGet;

	private string rare_asset_name = "UI_iconsource_{0}_{1}";

	private string frameName = "frame";

	private string bgName = "BG";

	private string small = "_small";

	private string[] strRarity = new string[7] { "Dummy", "D", "C", "B", "A", "S", "SS" };

	private void Update()
	{
		if ((bool)parentPlayerCustomizeUI)
		{
			bool active = parentPlayerCustomizeUI.GetCurrentSelectedIcon() == _id;
			ImgFrameSelect.gameObject.SetActive(active);
			active = parentPlayerCustomizeUI.GetCurrentUsedIcon() == _id;
			ImgFrameUsed.gameObject.SetActive(active);
		}
	}

	public void Setup(int n_ID)
	{
		parentPlayerCustomizeUI = GetComponentInParent<PlayerCustomizeUI>();
		_id = n_ID;
		ImgFrame.gameObject.SetActive(true);
		ImgFrameSelect.gameObject.SetActive(false);
		ImgFrameUsed.gameObject.SetActive(false);
		ImgIconBase.gameObject.SetActive(true);
		bIsGet = parentPlayerCustomizeUI.CheckPlayerIconFlag(n_ID);
		if (bIsGet)
		{
			SetIcon(n_ID);
		}
		else
		{
			imgRareBg.gameObject.SetActive(false);
		}
	}

	public void SetIcon(int n_ID)
	{
		ITEM_TABLE portraitTable = parentPlayerCustomizeUI.GetPortraitTable(n_ID);
		assetName = portraitTable.s_ICON;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconCharacter(assetName), assetName, delegate(Sprite obj)
		{
			if ((bool)obj)
			{
				ImgIconBase.sprite = obj;
			}
		});
		SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[portraitTable.n_RARE] + small));
	}

	public void OnClick()
	{
		if ((bool)parentPlayerCustomizeUI)
		{
			parentPlayerCustomizeUI.SetCurrentSelectedIcon(_id, assetName, ImgIconBase.sprite, imgRareBg.sprite, bIsGet);
		}
	}
}
