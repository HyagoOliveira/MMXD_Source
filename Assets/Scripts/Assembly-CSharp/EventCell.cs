using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class EventCell : MonoBehaviour
{
	[SerializeField]
	public Image imgIcon;

	[SerializeField]
	public OrangeText txtEventName;

	[SerializeField]
	public Button btnIcon;

	[SerializeField]
	private UIShadow txtShadow;

	[SerializeField]
	private Image imgBG;

	[SerializeField]
	private Image imgWordBG;

	public bool WordBG
	{
		get
		{
			return imgWordBG.transform.gameObject.activeSelf;
		}
		set
		{
			imgWordBG.transform.gameObject.SetActive(value);
		}
	}

	public void SetType(OrangeTableHelper.EventsDef def)
	{
		if (imgBG != null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_hometop, "UI_Main_event_bg" + def.m_subName, delegate(Sprite s)
			{
				if (s != null)
				{
					imgBG.sprite = s;
				}
			});
		}
		if (imgWordBG != null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_hometop, "UI_Main_event_wordbg_" + def.m_subName, delegate(Sprite s)
			{
				if (s != null)
				{
					imgWordBG.sprite = s;
				}
			});
		}
		txtShadow.effectColor = def.m_color;
	}
}
