using System;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class RewardPopupUIUnit : MonoBehaviour
{
	[SerializeField]
	private ItemIconBase itemIconBase;

	[SerializeField]
	private Text textAmount;

	[SerializeField]
	private Image imgPiece;

	[SerializeField]
	private Image imgCardType;

	[SerializeField]
	private Canvas rareRoot;

	private int idx;

	private string bundleName = string.Empty;

	private string assetName = string.Empty;

	private int rare;

	private int amout;

	private int tweenUid = -1;

	private int tweenAnimUid = -1;

	private bool isCancel;
    [Obsolete]
    private CallbackIdx m_cb;

	[SerializeField]
	private Image imgConvert;

	private bool canPlayConvertAnim;

	private CanvasGroup canvasGroupConvert;

    [Obsolete]
    public void Setup(int p_idx, string p_bundleName, string p_assetName, int p_rare, int p_amount, CallbackIdx p_cb)
	{
		idx = p_idx;
		bundleName = p_bundleName;
		assetName = p_assetName;
		rare = p_rare;
		amout = p_amount;
		m_cb = p_cb;
		itemIconBase.Setup(idx, bundleName, assetName, m_cb);
		itemIconBase.SetRare(rare);
		SetTextAmount("x" + p_amount);
		if ((bool)rareRoot)
		{
			rareRoot.enabled = ((rare >= 5) ? true : false);
		}
		if (!isCancel)
		{
			tweenUid = LeanTween.value(1.1f, 0.9f, 0.4f).setOnUpdate(delegate(float val)
			{
				base.transform.localScale = new Vector3(val, val, 1f);
			}).setOnComplete((Action)delegate
			{
				tweenUid = -1;
			})
				.uniqueId;
		}
	}

	public void SetPieceActive(bool active)
	{
		imgPiece.color = (active ? Color.white : Color.clear);
	}

	public void SetCardType(bool atv, string bundleName = "", string assetName = "")
	{
		if (!(null == imgCardType))
		{
			imgCardType.color = (atv ? Color.white : Color.clear);
			if (atv)
			{
				imgCardType.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName);
			}
		}
	}

	public void IgonreTween()
	{
		isCancel = true;
		LeanTween.cancel(ref tweenUid);
	}

	public void SetConvertAnim(int p_convertItemID)
	{
		canvasGroupConvert = imgConvert.GetComponent<CanvasGroup>();
		switch (p_convertItemID)
		{
		case -1:
			canvasGroupConvert.alpha = 0f;
			return;
		case 0:
			canvasGroupConvert.alpha = 1f;
			return;
		}
		canPlayConvertAnim = true;
		ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[p_convertItemID];
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(iTEM_TABLE.s_ICON), iTEM_TABLE.s_ICON, delegate(Sprite obj)
		{
			imgConvert.sprite = obj;
		});
	}

	public void PlayConvertAnim()
	{
		if (canPlayConvertAnim)
		{
			imgConvert.color = Color.white;
			tweenAnimUid = LeanTween.value(canvasGroupConvert.gameObject, 1f, 0f, 1f).setOnUpdate(delegate(float val)
			{
				canvasGroupConvert.alpha = val;
			}).setEaseInOutQuint()
				.setLoopPingPong()
				.uniqueId;
		}
	}

	private void OnDestroy()
	{
		if (tweenUid != -1)
		{
			LeanTween.cancel(ref tweenUid);
		}
		if (tweenAnimUid != -1)
		{
			LeanTween.cancel(ref tweenAnimUid);
		}
	}

	public void SetTextAmount(string str)
	{
		textAmount.text = str;
	}
}
