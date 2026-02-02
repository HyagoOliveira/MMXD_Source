using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class IllustrTargetCell : MonoBehaviour
{
	[SerializeField]
	private IllustrationTargetUI m_parentUI;

	[SerializeField]
	private Image m_succeed;

	[SerializeField]
	private Image m_characterIcon;

	[SerializeField]
	public Button m_targetBtn;

	[SerializeField]
	private Image[] m_levelStart;

	[SerializeField]
	private Image[] m_levelStartBG;

	[SerializeField]
	private OrangeText m_targetName;

	[SerializeField]
	private RectTransform[] m_color;

	[SerializeField]
	private Scrollbar m_progressBar;

	[SerializeField]
	private OrangeText m_progressText;

	[SerializeField]
	private OrangeText m_targetEXP;

	[SerializeField]
	private OrangeText m_completedText;

	[Header("Users Achieving rate")]
	[SerializeField]
	private OrangeText m_achievingRate;

	private IllustrationTargetUI.ConditionCell m_cell;

	private GALLERY_TABLE nextTbl;

	private OrangeUIBase linkUI;

	private SystemSE goSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;

	private readonly string[] targetNames = new string[11]
	{
		string.Empty,
		"GALLERY_CONDITION_1",
		"GALLERY_CONDITION_2",
		"GALLERY_CONDITION_3",
		"GALLERY_CONDITION_4",
		"GALLERY_CONDITION_5",
		"GALLERY_CONDITION_6",
		"GALLERY_CONDITION_7",
		"GALLERY_CONDITION_8",
		"GALLERY_CONDITION_9",
		"GALLERY_CONDITION_6"
	};

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Setup(IllustrationTargetUI.ConditionCell cell)
	{
		m_cell = cell;
		int count = m_cell.m_galleryList.Count;
		nextTbl = m_cell.m_galleryList[0];
		base.gameObject.SetActive(true);
		SetProgressInfo();
		string iconGallery = AssetBundleScriptableObject.Instance.m_iconGallery;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(iconGallery, nextTbl.s_ICON, delegate(Sprite obj)
		{
			m_characterIcon.sprite = obj;
			if (m_characterIcon.sprite != null)
			{
				m_characterIcon.color = Color.white;
			}
		});
	}

	public void SetComplationRate(int iRate)
	{
		m_achievingRate.text = iRate + "%";
	}

	public void SetProgressInfo()
	{
		string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(targetNames[nextTbl.n_CONDITION]);
		int n_CONDITION_STEP;
		if (m_cell.m_lockList.Count == 0)
		{
			m_completedText.transform.gameObject.SetActive(true);
			m_succeed.transform.gameObject.SetActive(true);
			m_targetEXP.transform.gameObject.SetActive(false);
			nextTbl = m_cell.m_unlockList[m_cell.m_unlockList.Count - 1];
			n_CONDITION_STEP = nextTbl.n_CONDITION_STEP;
		}
		else
		{
			m_completedText.transform.gameObject.SetActive(false);
			m_succeed.transform.gameObject.SetActive(false);
			m_targetEXP.transform.gameObject.SetActive(true);
			nextTbl = m_cell.m_lockList[0];
			m_targetEXP.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GALLERY_ADDEXP"), nextTbl.n_EXP);
			n_CONDITION_STEP = m_cell.m_lockList[m_cell.m_lockList.Count - 1].n_CONDITION_STEP;
		}
		SetStarBG(n_CONDITION_STEP);
		SetStar(m_cell.m_unlockList.Count);
		int a;
		n_CONDITION_STEP = (a = 0);
		switch ((GalleryCondition)(short)nextTbl.n_CONDITION)
		{
		default:
			n_CONDITION_STEP = (a = 1);
			m_targetName.text = str;
			break;
		case GalleryCondition.UpgradeStar:
			if (nextTbl.n_TYPE == 1)
			{
				a = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[nextTbl.n_MAINID].netInfo.Star;
			}
			else if (nextTbl.n_TYPE == 2)
			{
				a = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[nextTbl.n_MAINID].netInfo.Star;
			}
			n_CONDITION_STEP = nextTbl.n_CONDITION_X;
			if (nextTbl.n_TYPE == 3)
			{
				a = m_cell.m_unlockList.Count;
			}
			m_targetName.text = string.Format(str, n_CONDITION_STEP);
			break;
		case GalleryCondition.CompleteStage:
			if (nextTbl.n_TYPE == 1)
			{
				a = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[nextTbl.n_MAINID].netInfo.PveCount;
			}
			else if (nextTbl.n_TYPE == 2)
			{
				a = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[nextTbl.n_MAINID].netInfo.PveCount;
			}
			n_CONDITION_STEP = nextTbl.n_CONDITION_X;
			m_targetName.text = string.Format(str, n_CONDITION_STEP);
			break;
		case GalleryCondition.CompleteMultiplay:
			if (nextTbl.n_TYPE == 1)
			{
				a = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[nextTbl.n_MAINID].netInfo.PvpCount;
			}
			else if (nextTbl.n_TYPE == 2)
			{
				a = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[nextTbl.n_MAINID].netInfo.PvpCount;
			}
			n_CONDITION_STEP = nextTbl.n_CONDITION_X;
			m_targetName.text = string.Format(str, n_CONDITION_STEP);
			break;
		case GalleryCondition.WeaponUpgradeLevel:
			a = ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponRank(ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[nextTbl.n_MAINID].netInfo.Exp);
			n_CONDITION_STEP = nextTbl.n_CONDITION_X;
			m_targetName.text = string.Format(str, n_CONDITION_STEP);
			break;
		case GalleryCondition.WeaponUpgradeExpert:
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[nextTbl.n_MAINID].netExpertInfos != null)
			{
				a = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[nextTbl.n_MAINID].netExpertInfos.Sum((NetWeaponExpertInfo p) => p.ExpertLevel);
			}
			n_CONDITION_STEP = nextTbl.n_CONDITION_X;
			m_targetName.text = string.Format(str, n_CONDITION_STEP);
			break;
		case GalleryCondition.UnlockPassiveSkill:
			n_CONDITION_STEP = nextTbl.n_CONDITION_X;
			a = 0;
			if (nextTbl.n_TYPE == 1)
			{
				a = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[nextTbl.n_MAINID].netSkillDic.Where((KeyValuePair<CharacterSkillSlot, NetCharacterSkillInfo> p) => p.Key >= CharacterSkillSlot.PassiveSkill1 && p.Key <= CharacterSkillSlot.PassiveSkill7).Count();
			}
			else if (nextTbl.n_TYPE == 2 && ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[nextTbl.n_MAINID].netSkillInfos != null)
			{
				a = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[nextTbl.n_MAINID].netSkillInfos.Count;
			}
			m_targetName.text = string.Format(str, n_CONDITION_STEP);
			break;
		case GalleryCondition.CharacterSkillAccumulated:
			a = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[nextTbl.n_MAINID].netSkillDic.Sum((KeyValuePair<CharacterSkillSlot, NetCharacterSkillInfo> s) => s.Value.Level);
			n_CONDITION_STEP = nextTbl.n_CONDITION_X;
			m_targetName.text = string.Format(str, n_CONDITION_STEP);
			break;
		case GalleryCondition.CardLevel:
			n_CONDITION_STEP = nextTbl.n_CONDITION_X;
			a = 0;
			(from p in ManagedSingleton<PlayerNetManager>.Instance.dicCard
				where p.Value.netCardInfo.CardID == m_cell.CardID
				select p into s
				select s.Value).ToList().ForEach(delegate(CardInfo info)
			{
				int cardRank = ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(info.netCardInfo.Exp);
				if (a < cardRank)
				{
					a = cardRank;
				}
			});
			m_targetName.text = string.Format(str, n_CONDITION_STEP);
			break;
		case GalleryCondition.GetSkillId:
			break;
		}
		float num = (float)a / (float)n_CONDITION_STEP;
		if (num > 0.9999f)
		{
			m_color[1].transform.gameObject.SetActive(true);
			m_progressBar.handleRect = m_color[1];
			m_color[0].transform.gameObject.SetActive(false);
		}
		else
		{
			m_color[0].transform.gameObject.SetActive(true);
			m_progressBar.handleRect = m_color[0];
			m_color[1].transform.gameObject.SetActive(false);
		}
		m_progressBar.size = num;
		m_progressText.text = string.Format("{0}/{1}", a, n_CONDITION_STEP);
	}

	private void SetStar(int rera)
	{
		for (int i = 0; i < 5; i++)
		{
			if (i < rera)
			{
				m_levelStart[i].gameObject.SetActive(true);
			}
			else
			{
				m_levelStart[i].gameObject.SetActive(false);
			}
		}
	}

	private void SetStarBG(int rera)
	{
		for (int i = 0; i < 5; i++)
		{
			if (i < rera)
			{
				m_levelStartBG[i].gameObject.SetActive(true);
			}
			else
			{
				m_levelStartBG[i].gameObject.SetActive(false);
			}
		}
	}

	public void ResetActive()
	{
		Image[] levelStart = m_levelStart;
		for (int i = 0; i < levelStart.Length; i++)
		{
			levelStart[i].gameObject.SetActive(false);
		}
		m_succeed.gameObject.SetActive(false);
		base.gameObject.SetActive(false);
	}

	private void GotoUI()
	{
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		if (nextTbl.n_TYPE == 3)
		{
			switch ((GalleryCondition)(short)nextTbl.n_CONDITION)
			{
			case GalleryCondition.UpgradeStar:
				if (lV >= OrangeConst.OPENRANK_STARUP)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(goSE);
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardMain", delegate(CardMainUI ui)
					{
						ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ReflashUIData));
						linkUI = ui;
					});
				}
				break;
			case GalleryCondition.CardLevel:
				if (lV >= OrangeConst.OPENRANK_SKILL_LVUP)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(goSE);
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardMain", delegate(CardMainUI ui)
					{
						ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ReflashUIData));
						linkUI = ui;
					});
				}
				break;
			}
			return;
		}
		int n_MAINID4 = nextTbl.n_MAINID;
		switch ((GalleryCondition)(short)nextTbl.n_CONDITION)
		{
		case GalleryCondition.UpgradeStar:
			if (lV < OrangeConst.OPENRANK_STARUP)
			{
				break;
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(goSE);
			if (nextTbl.n_TYPE == 2)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
				{
					ui.nTargetWeaponID = nextTbl.n_MAINID;
					ui.bNeedInitList = true;
					ui.initalization_data();
					ui.OnInfoBtnCB(3);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ReflashUIData));
					linkUI = ui;
				});
				return;
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("ui/skillbutton", "SkillButton", delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Main", delegate(CharacterInfoUI ui)
				{
					int n_MAINID = nextTbl.n_MAINID;
					List<CharacterInfo> list = ManagedSingleton<CharacterHelper>.Instance.SortCharacterList();
					for (int i = 0; i < list.Count; i++)
					{
						if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[n_MAINID].netInfo.CharacterID == list[i].netInfo.CharacterID)
						{
							ui.Setup(ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[n_MAINID], i, 2);
						}
					}
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ReflashUIData));
					linkUI = ui;
				});
			});
			return;
		case GalleryCondition.CompleteStage:
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(goSE);
			ManagedSingleton<UILinkHelper>.Instance.LoadUI(101);
			return;
		case GalleryCondition.CompleteMultiplay:
			if (lV >= OrangeConst.OPENRANK_PVP)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(goSE);
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpRoomSelect", delegate(PvpRoomSelectUI ui)
				{
					ui.Setup();
				});
				return;
			}
			break;
		case GalleryCondition.WeaponUpgradeLevel:
			if (lV >= OrangeConst.OPENRANK_WEAPON_LVUP)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(goSE);
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
				{
					ui.nTargetWeaponID = nextTbl.n_MAINID;
					ui.bNeedInitList = true;
					ui.initalization_data();
					ui.OnInfoBtnCB(1);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ReflashUIData));
					linkUI = ui;
				});
				return;
			}
			break;
		case GalleryCondition.WeaponUpgradeExpert:
			if (lV >= OrangeConst.OPENRANK_WEAPON_UPGRADE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(goSE);
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
				{
					ui.nTargetWeaponID = nextTbl.n_MAINID;
					ui.bNeedInitList = true;
					ui.initalization_data();
					ui.OnInfoBtnCB(2);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ReflashUIData));
					linkUI = ui;
				});
				return;
			}
			break;
		case GalleryCondition.UnlockPassiveSkill:
			if (lV < OrangeConst.OPENRANK_SKILL_LVUP)
			{
				break;
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(goSE);
			if (nextTbl.n_TYPE == 2)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
				{
					ui.nTargetWeaponID = nextTbl.n_MAINID;
					ui.bNeedInitList = true;
					ui.initalization_data();
					ui.OnInfoBtnCB(4);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ReflashUIData));
					linkUI = ui;
				});
				return;
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("ui/skillbutton", "SkillButton", delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Main", delegate(CharacterInfoUI ui)
				{
					int n_MAINID2 = nextTbl.n_MAINID;
					List<CharacterInfo> sortedCharacterList = ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList();
					for (int j = 0; j < sortedCharacterList.Count; j++)
					{
						if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[n_MAINID2].netInfo.CharacterID == sortedCharacterList[j].netInfo.CharacterID)
						{
							ui.Setup(ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[n_MAINID2], j, 1);
							LeanTween.delayedCall(1.3f, (Action)delegate
							{
								if (ui != null && !ui.IsLock)
								{
									ui.RefreshMenuSkillPassive();
								}
							});
						}
					}
					CharacterInfoUI characterInfoUI = ui;
					characterInfoUI.closeCB = (Callback)Delegate.Combine(characterInfoUI.closeCB, new Callback(ReflashUIData));
					linkUI = ui;
				});
			});
			return;
		case GalleryCondition.CharacterSkillAccumulated:
			if (lV < OrangeConst.OPENRANK_SKILL_LVUP)
			{
				break;
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(goSE);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("ui/skillbutton", "SkillButton", delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Main", delegate(CharacterInfoUI ui)
				{
					int n_MAINID3 = nextTbl.n_MAINID;
					List<CharacterInfo> sortedCharacterList2 = ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList();
					for (int k = 0; k < sortedCharacterList2.Count; k++)
					{
						if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[n_MAINID3].netInfo.CharacterID == sortedCharacterList2[k].netInfo.CharacterID)
						{
							ui.Setup(ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[n_MAINID3], k, 1);
						}
					}
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ReflashUIData));
					linkUI = ui;
				});
			});
			return;
		}
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("SYSTEM_NOT_OEPN");
	}

	private void ReflashUIData()
	{
		ManagedSingleton<GalleryHelper>.Instance.BuildGalleryInfo();
		IllustrationTargetUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<IllustrationTargetUI>("UI_IllustrationTarget");
		if (!(uI == null))
		{
			uI.RebuildInitData();
		}
	}

	public void OnClick()
	{
		if (!m_completedText.gameObject.activeInHierarchy && !(MonoBehaviourSingleton<UIManager>.Instance.GetUI<IllustrationUI>("UI_Illustration") == null))
		{
			GotoUI();
		}
	}

	public string GetGalleryTargetFormatText(GALLERY_TABLE tbl)
	{
		string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(targetNames[tbl.n_CONDITION]);
		GalleryCondition galleryCondition = (GalleryCondition)tbl.n_CONDITION;
		if (galleryCondition != GalleryCondition.FirstGet && (uint)(galleryCondition - 2) <= 7u)
		{
			return string.Format(str, tbl.n_CONDITION_X);
		}
		return str;
	}
}
