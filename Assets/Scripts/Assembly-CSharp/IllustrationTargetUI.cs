using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class IllustrationTargetUI : OrangeUIBase
{
	public class ConditionCell
	{
		public int CardID;

		public List<GALLERY_TABLE> m_galleryList;

		public List<GALLERY_TABLE> m_unlockList;

		public List<GALLERY_TABLE> m_lockList;

		public ConditionCell()
		{
			CardID = 0;
			m_galleryList = new List<GALLERY_TABLE>();
			m_unlockList = new List<GALLERY_TABLE>();
			m_lockList = new List<GALLERY_TABLE>();
		}
	}

	[SerializeField]
	private IllustrColume ScrollColume;

	[SerializeField]
	private LoopVerticalScrollRect LVSR;

	[SerializeField]
	private Button btn;

	[SerializeField]
	private OrangeText btnText;

	public List<GALLERY_TABLE> m_listAll = new List<GALLERY_TABLE>();

	public List<GALLERY_TABLE> m_listUnlock = new List<GALLERY_TABLE>();

	public List<GALLERY_TABLE> m_listLock = new List<GALLERY_TABLE>();

	[HideInInspector]
	public List<ConditionCell> cellList = new List<ConditionCell>();

	private OrangeUIBase parentUI;

	private CharacterInfoBasic cinfobase;

	private WeaponInfoUI winfo;

	private IllustrationUI iUI;

	private CardInfoUI cardUI;

	private OrangeUIBase linkUI;

	private GalleryType tp = GalleryType.Weapon;

	private int tID;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void OnClickCellBtn(GALLERY_TABLE nextTbl)
	{
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		switch ((GalleryCondition)(short)nextTbl.n_CONDITION)
		{
		case GalleryCondition.FirstGet:
			return;
		case GalleryCondition.GetSkillId:
			return;
		case GalleryCondition.UpgradeStar:
			if (lV < OrangeConst.OPENRANK_STARUP || !(iUI != null))
			{
				break;
			}
			if (nextTbl.n_TYPE != 1)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
				{
					ui.nTargetWeaponID = nextTbl.n_MAINID;
					ui.bNeedInitList = true;
					ui.initalization_data();
					ui.OnInfoBtnCB(3);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(RebuildInitData));
					linkUI = ui;
				});
			}
			return;
		case GalleryCondition.CompleteStage:
			return;
		case GalleryCondition.CompleteMultiplay:
			if (lV >= OrangeConst.OPENRANK_PVP)
			{
				return;
			}
			break;
		case GalleryCondition.WeaponUpgradeLevel:
			if (lV >= OrangeConst.OPENRANK_WEAPON_LVUP)
			{
				return;
			}
			break;
		case GalleryCondition.WeaponUpgradeExpert:
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() >= OrangeConst.OPENRANK_WEAPON_UPGRADE)
			{
				return;
			}
			break;
		case GalleryCondition.UnlockPassiveSkill:
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() >= OrangeConst.OPENRANK_SKILL_LVUP)
			{
				return;
			}
			break;
		case GalleryCondition.CharacterSkillAccumulated:
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() >= OrangeConst.OPENRANK_SKILL_LVUP)
			{
				return;
			}
			break;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
		{
			string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SYSTEM_NOT_OEPN");
			tipUI.Setup(str, true);
		});
	}

	public void Setup(OrangeUIBase tUI)
	{
		parentUI = tUI;
		if (cinfobase == null && tUI.transform.GetComponent<CharacterInfoBasic>() != null)
		{
			cinfobase = tUI.transform.GetComponent<CharacterInfoBasic>();
		}
		else if (winfo == null && tUI.transform.GetComponent<WeaponInfoUI>() != null)
		{
			winfo = tUI.transform.GetComponent<WeaponInfoUI>();
		}
		else if (iUI == null && tUI.transform.GetComponent<IllustrationUI>() != null)
		{
			iUI = tUI.transform.GetComponent<IllustrationUI>();
		}
		else if (cardUI == null && tUI.transform.GetComponent<CardInfoUI>() != null)
		{
			cardUI = tUI.transform.GetComponent<CardInfoUI>();
		}
		RebuildInitData();
	}

	public void RebuildInitData()
	{
		cellList.Clear();
		m_listAll.Clear();
		m_listUnlock.Clear();
		m_listLock.Clear();
		btn.onClick.RemoveAllListeners();
		if (cinfobase != null)
		{
			tp = GalleryType.Character;
			tID = cinfobase.characterTable.n_ID;
			ManagedSingleton<GalleryHelper>.Instance.GalleryGetTableAll(tID, tp, out m_listAll, out m_listUnlock, out m_listLock);
			btnText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("HOMETOP_CHARACTER") + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("HOMETOP_GALLERY");
			btn.onClick.AddListener(delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Illustration", delegate(IllustrationUI ui)
				{
					base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK13;
					OnClickCloseBtn();
					ui.Setup();
				});
			});
		}
		else if (winfo != null)
		{
			tID = winfo.nTargetWeaponID;
			tp = GalleryType.Weapon;
			ManagedSingleton<GalleryHelper>.Instance.GalleryGetTableAll(tID, tp, out m_listAll, out m_listUnlock, out m_listLock);
			btnText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("HOMETOP_ITEM") + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("HOMETOP_GALLERY");
			btn.onClick.AddListener(delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Illustration", delegate(IllustrationUI ui)
				{
					base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK13;
					OnClickCloseBtn();
					ui.Setup(GalleryType.Weapon);
				});
			});
		}
		else if (cardUI != null)
		{
			tID = cardUI.CurrentNetCardInfo.CardID;
			tp = GalleryType.Card;
			ManagedSingleton<GalleryHelper>.Instance.GalleryGetCardTableAll(tID, out m_listAll, out m_listUnlock, out m_listLock);
			btnText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("HOMETOP_CARD") + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("HOMETOP_GALLERY");
			btn.onClick.AddListener(delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Illustration", delegate(IllustrationUI ui)
				{
					base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK13;
					OnClickCloseBtn();
					ui.Setup(GalleryType.Card);
				});
			});
		}
		else if (iUI != null)
		{
			GalleryHelper.GalleryCellInfo value = iUI.pCurrentCellInfo;
			if (!ManagedSingleton<GalleryHelper>.Instance.GalleryCellInfos[(int)(value.tType - 1)].TryGetValue(value.m_objID, out value))
			{
				return;
			}
			tID = value.m_objID;
			tp = iUI.m_eCharacter;
			m_listAll = value.m_allGallery;
			m_listUnlock = value.m_unlockGallery;
			m_listLock = value.m_lockGallery;
			btnText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RETURN");
			btn.onClick.AddListener(delegate
			{
				OnClickCloseBtn();
			});
		}
		int i;
		for (i = 1; i <= 10; i++)
		{
			ConditionCell conditionCell = new ConditionCell();
			if (tp == GalleryType.Card)
			{
				conditionCell.CardID = tID;
			}
			conditionCell.m_galleryList = m_listAll.Where((GALLERY_TABLE p) => p.n_CONDITION == i).ToList();
			if (conditionCell.m_galleryList.Count != 0)
			{
				conditionCell.m_unlockList = m_listUnlock.Where((GALLERY_TABLE p) => p.n_CONDITION == i).ToList();
				conditionCell.m_lockList = m_listLock.Where((GALLERY_TABLE p) => p.n_CONDITION == i).ToList();
				conditionCell.m_unlockList.Sort((GALLERY_TABLE p1, GALLERY_TABLE p2) => p1.n_ID - p2.n_ID);
				conditionCell.m_lockList.Sort((GALLERY_TABLE p1, GALLERY_TABLE p2) => p1.n_ID - p2.n_ID);
				cellList.Add(conditionCell);
			}
		}
		OnUpdateScrollRect();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void UpdateComplationRate()
	{
	}

	public void OnUpdateScrollRect()
	{
		int count = cellList.Count;
		int num = (count - count % 2) / 2;
		num += count % 2;
		LVSR.totalCount = num;
		LVSR.RefillCells();
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void Clear(params object[] p_param)
	{
		OnClickCloseBtn();
	}
}
