#define RELEASE
using System;
using CallbackDefs;
using DragonBones;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GalleryCardCell : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private GameObject SelectFrame;

	[SerializeField]
	private GameObject[] RareBGs;

	[SerializeField]
	private GameObject[] RareFrames;

	[SerializeField]
	private GameObject[] Balls;

	[SerializeField]
	private Image CardIcon;

	[SerializeField]
	private OrangeText CardName;

	[SerializeField]
	private OrangeText LVNum;

	[SerializeField]
	private GameObject Progress;

	[SerializeField]
	private Image ProgressBar;

	[SerializeField]
	private Scrollbar ProgressScroll;

	[SerializeField]
	private OrangeText ProgressNum;

	[SerializeField]
	private GameObject ExpImg;

	[SerializeField]
	private GameObject Mask;

	[SerializeField]
	private GameObject UnLockText;

	private IllustrationUI parentUI;

	private UnityArmatureComponent GetExpEffect;

	[HideInInspector]
	public GalleryHelper.GalleryCellInfo m_cellInfo;

	private CARD_TABLE tCardTable;

	public bool ShowGetExp
	{
		get
		{
			return ExpImg.activeSelf;
		}
		set
		{
			m_cellInfo.m_isCanGetExp = value;
			ExpImg.SetActive(value);
			SelectFrame.SetActive(value);
		}
	}

	[Obsolete]
	public event CallbackIdx callback;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			this.callback.CheckTargetToInvoke(tCardTable.n_ID);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Setup(GalleryHelper.GalleryCellInfo cellInfo)
	{
		tCardTable = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[cellInfo.m_objID];
		parentUI = MonoBehaviourSingleton<UIManager>.Instance.UI_Parent.Find("UI_Illustration").GetComponent<IllustrationUI>();
		ResetCell();
		m_cellInfo = cellInfo;
		base.gameObject.SetActive(true);
		SetCardImage();
		SetRare();
		SetType();
		SetName();
		if (cellInfo.m_isMask)
		{
			Mask.SetActive(true);
			Progress.SetActive(false);
			if (cellInfo.m_isCanUnlock)
			{
				UnLockText.SetActive(true);
			}
		}
		else
		{
			Mask.SetActive(false);
			Progress.SetActive(true);
			SetProgressText(cellInfo.m_progress);
		}
		ShowGetExp = cellInfo.m_isCanGetExp;
	}

	public void ResetCell()
	{
		base.gameObject.SetActive(false);
		Mask.SetActive(true);
		UnLockText.SetActive(false);
		Progress.SetActive(false);
		ExpImg.SetActive(false);
	}

	public void SetRare()
	{
		for (int i = 0; i < 5; i++)
		{
			bool active = i + 1 == tCardTable.n_RARITY;
			RareBGs[i].SetActive(active);
			RareFrames[i].SetActive(active);
		}
	}

	public void SetType()
	{
		int num = (int)Math.Log(tCardTable.n_TYPE, 2.0);
		for (int i = 0; i < 5; i++)
		{
			bool active = i == num;
			Balls[i].SetActive(active);
		}
	}

	public void SetMask(bool sw)
	{
		Mask.SetActive(sw);
		Progress.SetActive(!sw);
	}

	public void Unlock()
	{
		Mask.SetActive(false);
		UnLockText.SetActive(false);
		Progress.SetActive(true);
	}

	private void SetName()
	{
		string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CARDTEXT_TABLE_DICT.GetL10nValue(tCardTable.w_NAME);
		string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, CardName).Split('\n');
		if (array.Length > 1)
		{
			string text = array[0];
			CardName.text = text.Substring(0, text.Length - 2) + "...";
		}
		else
		{
			CardName.text = l10nValue;
		}
	}

	private void SetProgressText(int p)
	{
		ProgressNum.text = p + "%";
		if (p < 33)
		{
			ProgressBar.sprite = parentUI.m_colorBar[0].sprite;
		}
		else if (p < 66)
		{
			ProgressBar.sprite = parentUI.m_colorBar[1].sprite;
		}
		else if (p < 99)
		{
			ProgressBar.sprite = parentUI.m_colorBar[2].sprite;
		}
		else
		{
			ProgressBar.sprite = parentUI.m_colorBar[3].sprite;
		}
		float size = (float)p / 100f;
		ProgressScroll.size = size;
	}

	public void SetCardImage()
	{
		string text = "";
		text = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_m_format, tCardTable.n_PATCH);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(text, tCardTable.s_CARDIMG, delegate(Sprite asset)
		{
			if (asset != null)
			{
				CardIcon.sprite = asset;
			}
			else
			{
				Debug.LogWarning("CardTable: unable to load sprite " + tCardTable.s_ICON);
			}
		});
	}

	public void OnCellCkick()
	{
		parentUI.onCellClick(base.transform, m_cellInfo);
	}
}
