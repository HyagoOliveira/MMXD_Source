using System;
using CallbackDefs;
using Coffee.UIExtensions;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class StorageCompUnit : MonoBehaviour
{
	[SerializeField]
	private OrangeText text;

	[SerializeField]
	private Button btn;

	[SerializeField]
	private Image imgBg;

	[SerializeField]
	private Image imgSuggest;

	[SerializeField]
	private Image imgNew;

	[SerializeField]
	private UIShadow uiShadow;

	private StorageInfo storageInfo;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickSE = SystemSE.CRI_SYSTEMSE_SYS_CURSOR01;

	public bool CanPlaySE = true;

	public StorageInfo StorageInfo
	{
		get
		{
			return storageInfo;
		}
	}

	public Button Btn
	{
		get
		{
			return btn;
		}
	}

	public Image ImgBg
	{
		get
		{
			return imgBg;
		}
	}

	public OrangeText Text
	{
		get
		{
			return text;
		}
	}

	public UIShadow UiShadow
	{
		get
		{
			return uiShadow;
		}
	}

    [Obsolete]
    public void SetUnit(StorageInfo p_storageInfo, CallbackObj callbackEx)
	{
		storageInfo = p_storageInfo;
		StorageInfo obj = storageInfo;
		obj.ClickCb = (CallbackObj)Delegate.Combine(obj.ClickCb, callbackEx);
		btn.onClick.RemoveAllListeners();
		imgSuggest.gameObject.SetActive(false);
		imgNew.gameObject.SetActive(storageInfo.IsNew);
		if (storageInfo.Refl10nTable == null)
		{
			text.IsLocalizationText = true;
			text.LocalizationKey = StorageInfo.L10nKey;
			text.UpdateTextImmediate();
		}
		else
		{
			text.text = storageInfo.Refl10nTable.GetL10nValue(storageInfo.L10nKey);
		}
		btn.onClick.AddListener(OnClick);
		OrangeUIAnimation component = GetComponent<OrangeUIAnimation>();
		if ((bool)component)
		{
			component.listAnimation[0].Delay += p_storageInfo.AnimAddDelay;
			component.PlayAnimation();
		}
		UpdateHint();
	}

	public void OnClick()
	{
		if (CanPlaySE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
		}
		else
		{
			CanPlaySE = true;
		}
		storageInfo.ClickCb.CheckTargetToInvoke(storageInfo);
	}

	public void UpdateHint()
	{
		imgSuggest.gameObject.SetActive(storageInfo.UpdateSuggest());
		if (storageInfo.IsNewChecker != null)
		{
			imgNew.gameObject.SetActive(storageInfo.UpdateNew());
		}
	}
}
