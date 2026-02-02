using CallbackDefs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IconBase : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	protected Image imgIcon;

	private int idx;

	protected Color white = Color.white;

	protected Color grey = Color.grey;

	protected Color clear = Color.clear;

	private string sLastLoadingName = "";

    [System.Obsolete]
    public event CallbackIdx callback;

	private void Awake()
	{
	}

    [System.Obsolete]
    public virtual void Setup(int p_idx, string p_bundleName, string p_assetName, CallbackIdx clickCB = null, bool whiteColor = true)
	{
		idx = p_idx;
		imgIcon.color = clear;
		if (p_bundleName != "" && p_assetName != "")
		{
			sLastLoadingName = p_bundleName + p_assetName;
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(p_bundleName, p_assetName, delegate(Sprite obj)
			{
				if (!(sLastLoadingName != p_bundleName + p_assetName))
				{
					imgIcon.sprite = obj;
					imgIcon.color = (whiteColor ? white : grey);
				}
			});
		}
		else
		{
			imgIcon.color = clear;
		}
		this.callback = clickCB;
		imgIcon.raycastTarget = ((this.callback != null) ? true : false);
	}

	public virtual void Clear()
	{
		imgIcon.color = clear;
		imgIcon.raycastTarget = false;
		this.callback = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			this.callback.CheckTargetToInvoke(idx);
		}
	}

	protected void SetRareInfo(Image image, string assetName, bool whiteColor = true)
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
				image.color = (whiteColor ? white : grey);
			}
		});
	}
}
