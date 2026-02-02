using DragonBones;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GalleryCell : IconBase
{
	[Header("Gallery Cell")]
	private IllustrationUI parentUI;

	[SerializeField]
	protected Image rareBg;

	[SerializeField]
	protected Image rareFrame;

	[SerializeField]
	private OrangeText GalleryNameText;

	[SerializeField]
	public GameObject UnLockText;

	[SerializeField]
	public GameObject LockMask;

	[SerializeField]
	public GameObject Progress;

	[SerializeField]
	private Image ProgressBar;

	[SerializeField]
	private Scrollbar ProgressScroll;

	[SerializeField]
	private OrangeText ProgressText;

	[SerializeField]
	public GameObject GetExp;

	[SerializeField]
	public GameObject SelectFrame;

	private UnityArmatureComponent GetExpEffect;

	[HideInInspector]
	public GalleryHelper.GalleryCellInfo m_cellInfo;

	public bool ShowGetExp
	{
		get
		{
			return GetExp.activeSelf;
		}
		set
		{
			m_cellInfo.m_isCanGetExp = value;
			SelectFrame.SetActive(value);
			GetExp.SetActive(value);
		}
	}

	public void SetRare(int p_rare)
	{
		SetRareInfo(rareBg, AssetBundleScriptableObject.Instance.GetIconRareBg(p_rare));
		SetRareInfo(rareFrame, AssetBundleScriptableObject.Instance.GetIconRareFrame(p_rare));
		rareBg.type = Image.Type.Sliced;
		rareFrame.type = Image.Type.Sliced;
	}

	private void SetName(string name)
	{
		string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(name, GalleryNameText).Split('\n');
		if (array.Length > 1)
		{
			string text = array[0];
			GalleryNameText.text = text.Substring(0, text.Length - 2) + "...";
		}
		else
		{
			GalleryNameText.text = name;
		}
	}

	public void Setup(GalleryHelper.GalleryCellInfo cellInfo)
	{
		parentUI = MonoBehaviourSingleton<UIManager>.Instance.UI_Parent.Find("UI_Illustration").GetComponent<IllustrationUI>();
		ResetCell();
		m_cellInfo = cellInfo;
		GalleryType num = parentUI.m_eCharacter - 1;
		base.gameObject.SetActive(true);
		if (num == (GalleryType)0)
		{
			CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[cellInfo.m_objID];
			Setup(cellInfo.m_objID, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON);
			SetRare(cHARACTER_TABLE.n_RARITY);
			SetName(ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cHARACTER_TABLE.w_NAME));
		}
		else
		{
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[cellInfo.m_objID];
			Setup(cellInfo.m_objID, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON);
			SetRare(wEAPON_TABLE.n_RARITY);
			SetName(ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(wEAPON_TABLE.w_NAME));
		}
		if (cellInfo.m_isMask)
		{
			LockMask.SetActive(true);
			Progress.SetActive(false);
			if (cellInfo.m_isCanUnlock)
			{
				UnLockText.SetActive(true);
			}
		}
		else
		{
			LockMask.SetActive(false);
			Progress.SetActive(true);
			SetProgressText(cellInfo.m_progress);
		}
		ShowGetExp = cellInfo.m_isCanGetExp;
	}

	public void ResetCell()
	{
		base.gameObject.SetActive(false);
		SelectFrame.SetActive(false);
		LockMask.SetActive(false);
		UnLockText.SetActive(false);
		Progress.SetActive(false);
	}

	public void SetUnlock()
	{
		LockMask.SetActive(false);
		UnLockText.SetActive(false);
		Progress.SetActive(true);
	}

	public void OnCellCkick()
	{
		parentUI.onCellClick(base.transform, m_cellInfo);
	}

	private void SetProgressText(int p)
	{
		ProgressText.text = p + "%";
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
}
