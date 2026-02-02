using UnityEngine;
using UnityEngine.UI;

public class Mini01Material : MonoBehaviour
{
	[SerializeField]
	private Minigame01UI m_parentUI;

	[Header("InportMaterial")]
	[SerializeField]
	private Image m_bg;

	[SerializeField]
	private Image m_fg;

	[SerializeField]
	private Image m_icon;

	[SerializeField]
	private Image m_selected;

	[SerializeField]
	private OrangeText m_count;

	[SerializeField]
	private Button m_button;

	[Header("RewardSection")]
	[SerializeField]
	private Image m_bannerBG;

	[SerializeField]
	private Image m_bannerFG;

	[SerializeField]
	private OrangeText m_bannerStr1;

	[SerializeField]
	private OrangeText m_bannerStr2;

	[SerializeField]
	private GameObject m_resultedMask;

	[SerializeField]
	private Image imgCardType;

	public int p_convertItemID;

	public bool Resulted
	{
		get
		{
			return m_resultedMask.activeSelf;
		}
		set
		{
			m_resultedMask.SetActive(value);
		}
	}

	public string ItemName
	{
		set
		{
			m_bannerStr1.text = value;
		}
	}

	public Color ItemNameColor
	{
		set
		{
			m_bannerStr1.color = value;
		}
	}

	public string ItemAmout
	{
		set
		{
			m_bannerStr2.text = value;
		}
	}

	public string Icon
	{
		set
		{
			string iconItem = AssetBundleScriptableObject.Instance.GetIconItem(value);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(iconItem, value, delegate(Sprite obj)
			{
				if ((bool)obj)
				{
					m_icon.sprite = obj;
				}
			});
			Image image = imgCardType;
			if ((object)image != null)
			{
				image.gameObject.SetActive(false);
			}
		}
	}

	public CARD_TABLE CardIcon
	{
		set
		{
			string s_ICON = value.s_ICON;
			string bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(bundleName, s_ICON, delegate(Sprite obj)
			{
				if ((bool)obj)
				{
					m_icon.sprite = obj;
				}
			});
			if (imgCardType != null)
			{
				string cardTypeAssetName = ManagedSingleton<OrangeTableHelper>.Instance.GetCardTypeAssetName(value.n_TYPE);
				imgCardType.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, cardTypeAssetName);
				imgCardType.gameObject.SetActive(true);
			}
		}
	}

	public bool Selected
	{
		get
		{
			return m_selected.transform.gameObject.activeSelf;
		}
		set
		{
			m_selected.transform.gameObject.SetActive(value);
		}
	}

	public int Count
	{
		set
		{
			m_count.text = string.Format("X{0}", value);
		}
	}

	private void Awake()
	{
		m_button.onClick.AddListener(delegate
		{
			OnClickConvertUnit();
		});
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetBanner(Sprite f, Sprite b)
	{
		m_bannerBG.sprite = b;
		m_bannerFG.sprite = f;
	}

	public void SetRera(int rare)
	{
		SetRareInfo(m_bg, AssetBundleScriptableObject.Instance.GetIconRareBgSmall(rare));
		SetRareInfo(m_fg, AssetBundleScriptableObject.Instance.GetIconRareFrameSmall(rare));
	}

	private void SetRareInfo(Image image, string assetName, bool whiteColor = true)
	{
		if (null == image)
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
		{
			if (!(null == image))
			{
				image.sprite = obj;
				image.color = (whiteColor ? Color.white : Color.grey);
			}
		});
	}

	private void OnClickConvertUnit()
	{
		if (p_convertItemID != 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemHowToGet", delegate(ItemHowToGetUI ui)
			{
				ui.Setup(ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[p_convertItemID]);
			});
		}
	}
}
