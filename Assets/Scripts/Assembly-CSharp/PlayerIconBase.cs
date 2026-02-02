using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIconBase : IconBase
{
	private string rare_asset_name = "UI_iconsource_{0}_{1}";

	private string bgName = "BG";

	private string frameName = "frame";

	private string small = "_small";

	private string weaponLvlName = "UI_iconsource_powerupicon_Lv{0}";

	private string rarityImgName = "UI_Common_word_{0}";

	private string tempAssetName = "";

	private string[] strRarity = new string[7] { "Dummy", "D", "C", "B", "A", "S", "SS" };

	[SerializeField]
	private Image IconImage;

	[SerializeField]
	private Image RareImage;

	private int n_ID;

	private Callback cb;

	private new void SetRareInfo(Image image, string assetName, bool whiteColor = true)
	{
		if (null == image)
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
		{
			if (!(null == image))
			{
				image.gameObject.SetActive(true);
				image.sprite = obj;
				image.color = (whiteColor ? white : grey);
			}
		});
	}

	public void SetIcon()
	{
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.ContainsKey(n_ID))
		{
			n_ID = 900001;
		}
		ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[n_ID];
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconCharacter(iTEM_TABLE.s_ICON), iTEM_TABLE.s_ICON, delegate(Sprite sprite)
		{
			if ((bool)sprite)
			{
				IconImage.sprite = sprite;
			}
		});
		SetRareInfo(RareImage, string.Format(rare_asset_name, bgName, strRarity[iTEM_TABLE.n_RARE] + small));
	}

	public void UpdatePlayerIcon()
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PortraitID != n_ID)
		{
			n_ID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PortraitID;
			SetIcon();
		}
	}

	public void Setup(int _id, bool bOwn = false, Callback _cb = null)
	{
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.ContainsKey(_id) || _id <= 900000)
		{
			_id = 900001;
		}
		n_ID = _id;
		cb = _cb;
		SetIcon();
		if (bOwn)
		{
			InvokeRepeating("UpdatePlayerIcon", 1f, 1f);
		}
	}

	public void SetupByUnlockID(int _id)
	{
		n_ID = _id;
		SetIcon();
	}

	public void OnClickIcon()
	{
		if (cb != null)
		{
			cb.CheckTargetToInvoke();
		}
	}
}
