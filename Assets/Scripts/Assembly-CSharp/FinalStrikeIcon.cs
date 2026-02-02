using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

internal class FinalStrikeIcon : MonoBehaviour
{
	public enum BUTTON_STATUS
	{
		LOCKED = 0,
		UNLOCKED = 1,
		PRIMARY = 2,
		SECONDARY = 3,
		EMPTY = 4,
		MAX = 5
	}

	[SerializeField]
	private OrangeText m_textLevel;

	[SerializeField]
	private Image m_frameLocked;

	[SerializeField]
	private Image m_frameUnlocked;

	[SerializeField]
	private Image m_framePrimary;

	[SerializeField]
	private Image m_frameSecondary;

	[SerializeField]
	private GameObject m_primaryBadge;

	[SerializeField]
	private GameObject m_secondaryBadge;

	[SerializeField]
	private StarClearComponent m_starComponent;

	[SerializeField]
	private CanvasGroup m_skillImageCanvasGroup;

	[SerializeField]
	private Image m_skillImage;

	[SerializeField]
	private Image m_lockSymbol;

	[SerializeField]
	private Image m_lvlBar;

	[SerializeField]
	private Image m_lvlMaxSymbol;

	[SerializeField]
	private GameObject m_selectedFrame;

	[SerializeField]
	private GameObject m_imgRedDot;

	private FinalStrikeInfo m_FSInfo;

	private List<FS_TABLE> m_fsTableList_fsID = new List<FS_TABLE>();

	private BUTTON_STATUS m_buttonStatus;

	private void Start()
	{
	}

	public void Setup(FinalStrikeInfo fsInfo)
	{
		m_FSInfo = fsInfo;
		List<FS_TABLE> list = null;
		m_fsTableList_fsID = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == m_FSInfo.netFinalStrikeInfo.FinalStrikeID).ToList();
		m_fsTableList_fsID.Sort((FS_TABLE pair1, FS_TABLE pair2) => pair1.n_ID.CompareTo(pair2.n_ID));
		list = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == m_FSInfo.netFinalStrikeInfo.FinalStrikeID && x.n_LV == m_FSInfo.netFinalStrikeInfo.Level).ToList();
		if (list.Count <= 0)
		{
			return;
		}
		FS_TABLE fS_TABLE = list[0];
		bool flag = false;
		SKILL_TABLE value;
		if ((m_FSInfo.netFinalStrikeInfo.Star == 1) ? ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(fS_TABLE.n_SKILL_1, out value) : ((m_FSInfo.netFinalStrikeInfo.Star == 2) ? ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(fS_TABLE.n_SKILL_2, out value) : ((m_FSInfo.netFinalStrikeInfo.Star == 3) ? ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(fS_TABLE.n_SKILL_3, out value) : ((m_FSInfo.netFinalStrikeInfo.Star == 4) ? ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(fS_TABLE.n_SKILL_4, out value) : ((m_FSInfo.netFinalStrikeInfo.Star != 5) ? ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(fS_TABLE.n_SKILL_0, out value) : ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(fS_TABLE.n_SKILL_5, out value))))))
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(value.s_ICON), value.s_ICON, delegate(Sprite obj)
			{
				m_skillImage.sprite = obj;
			});
		}
		SetStarCount(m_FSInfo.netFinalStrikeInfo.Star);
		SetLevel(m_FSInfo.netFinalStrikeInfo.Level);
		SetButtonStatus(BUTTON_STATUS.LOCKED);
		if (m_imgRedDot != null)
		{
			m_imgRedDot.gameObject.SetActive(ManagedSingleton<HintHelper>.Instance.IsFinalStrikeCanUnLock(m_FSInfo.netFinalStrikeInfo.FinalStrikeID) || ManagedSingleton<HintHelper>.Instance.IsFinalStrikeCanStarUp(m_FSInfo.netFinalStrikeInfo.FinalStrikeID));
		}
	}

	private void SetStarCount(int starCount)
	{
		m_starComponent.gameObject.SetActive(true);
		m_starComponent.SetActiveStar(starCount);
	}

	private void SetLevel(int level)
	{
		int num = Mathf.Clamp(level, 0, 999);
		m_textLevel.text = string.Format("Lv{0}", num);
		FS_TABLE fS_TABLE = m_fsTableList_fsID.Last();
		float value = (float)level / (float)fS_TABLE.n_LV;
		value = Mathf.Clamp(value, 0f, 1f);
		m_lvlBar.transform.localScale = new Vector3(value, 1f, 1f);
		m_lvlMaxSymbol.gameObject.SetActive(level >= fS_TABLE.n_LV);
	}

	public bool IsSelected()
	{
		return m_selectedFrame.activeSelf;
	}

	public void SetSelected(bool bSelected)
	{
		m_selectedFrame.SetActive(bSelected);
	}

	public void SetButtonStatus(BUTTON_STATUS status)
	{
		m_frameLocked.gameObject.SetActive(true);
		m_frameUnlocked.gameObject.SetActive(false);
		m_framePrimary.gameObject.SetActive(false);
		m_frameSecondary.gameObject.SetActive(false);
		m_primaryBadge.SetActive(false);
		m_secondaryBadge.SetActive(false);
		m_skillImageCanvasGroup.gameObject.SetActive(true);
		m_skillImageCanvasGroup.alpha = 1f;
		m_lockSymbol.gameObject.SetActive(false);
		SetLevel(m_FSInfo.netFinalStrikeInfo.Level);
		SetStarCount(m_FSInfo.netFinalStrikeInfo.Star);
		m_buttonStatus = status;
		switch (status)
		{
		default:
			m_textLevel.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_LOCKED");
			SetStarCount(0);
			m_skillImageCanvasGroup.alpha = 0.5f;
			m_lockSymbol.gameObject.SetActive(true);
			break;
		case BUTTON_STATUS.UNLOCKED:
			m_frameUnlocked.gameObject.SetActive(true);
			break;
		case BUTTON_STATUS.PRIMARY:
			m_framePrimary.gameObject.SetActive(true);
			m_primaryBadge.SetActive(true);
			break;
		case BUTTON_STATUS.SECONDARY:
			m_frameSecondary.gameObject.SetActive(true);
			m_secondaryBadge.SetActive(true);
			break;
		case BUTTON_STATUS.EMPTY:
			m_skillImageCanvasGroup.gameObject.SetActive(false);
			SetLevel(0);
			m_starComponent.gameObject.SetActive(false);
			break;
		}
	}
}
