#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class CardEquipUI : OrangeUIBase
{
	public class BtnClickCB
	{
		public int nBtnID;

		public bool bIsLock;

		public Action<int> action;

		public void OnClick()
		{
			if (!bIsLock && action != null)
			{
				action(nBtnID);
			}
		}
	}

	[SerializeField]
	private GameObject SortRoot;

	[SerializeField]
	private Button[] CardType;

	[SerializeField]
	private Button[] SortType;

	[SerializeField]
	private Button[] GetTypeBtn;

	private Image[] CardTypeImg;

	private Image[] SortTypeImg;

	private Image[] GetTypeBtnImg;

	[SerializeField]
	private Image MaskImage;

	[SerializeField]
	private LoopVerticalScrollRect m_ScrollRect;

	[SerializeField]
	private CardEquipCell m_CardEquipCell;

	[SerializeField]
	private Canvas CurrentCardRoot;

	[SerializeField]
	private Canvas TargetCardRoot;

	[SerializeField]
	private Button EquipBtn;

	[SerializeField]
	private GameObject SkillTooltip;

	[SerializeField]
	private GameObject SkillTooltipRoots;

	[SerializeField]
	private OrangeText SkillNameText;

	[SerializeField]
	private OrangeText SkillInfoText;

	[SerializeField]
	private CardSkillColor[] CardSkillColorRoot11;

	[SerializeField]
	private CardSkillColor[] CardSkillColorRoot12;

	[SerializeField]
	private CardSkillColor[] CardSkillColorRoot21;

	[SerializeField]
	private CardSkillColor[] CardSkillColorRoot22;

	[SerializeField]
	private Transform[] SkillTooltipTransforms;

	[SerializeField]
	private OrangeText[] CardText_ATK;

	[SerializeField]
	private OrangeText[] CardText_MHP;

	[SerializeField]
	private OrangeText[] CardText_DEF;

	[SerializeField]
	private GameObject[] CardSkillRoot1;

	[SerializeField]
	private GameObject[] CardSkillRoot2;

	[SerializeField]
	private GameObject[] CardSkillMaskCharRoots1;

	[SerializeField]
	private GameObject[] CardSkillMaskCharRoots2;

	[SerializeField]
	private GameObject[] CardSkillMaskBasicRoots1;

	[SerializeField]
	private GameObject[] CardSkillMaskBasicRoots2;

	[SerializeField]
	private GameObject[] CardSkillFrameCharRoots1;

	[SerializeField]
	private GameObject[] CardSkillFrameCharRoots2;

	[SerializeField]
	private GameObject[] CardSkillFrameBasicRoots1;

	[SerializeField]
	private GameObject[] CardSkillFrameBasicRoots2;

	[SerializeField]
	private Image[] CardSkillIconRoot1;

	[SerializeField]
	private Image[] CardSkillIconRoot2;

	[SerializeField]
	private GameObject[] CardBaseRoots;

	[SerializeField]
	private CardSkillColor[] CardSkillColorRoot1;

	[SerializeField]
	private CardSkillColor[] CardSkillColorRoot2;

	[SerializeField]
	private GameObject refPrefab;

	[SerializeField]
	private GameObject TooltipSkillConditionRoot;

	[SerializeField]
	private Text TooltipSkillConditionName;

	[SerializeField]
	private CardSkillColor[] TooltipSkillColorRoot;

	public GameObject SortOrderImg;

	public GameObject SortPanel;

	public GameObject SortPanelNew;

	private bool bIsSetDeploy;

	private int CurrentDeployIndex = -1;

	private int CharacterID;

	private int CharacterCardSlot;

	private int CurrentCardSeqID;

	private int TargetCardSeqID;

	private List<NetCharacterCardSlotInfo> CurrentCardSlotList = new List<NetCharacterCardSlotInfo>();

	private List<NetCardDeployInfo> CurrentCardDeployInfoList = new List<NetCardDeployInfo>();

	private List<NetCardInfo> m_listNetCardInfo = new List<NetCardInfo>();

	private List<NetCardInfo> m_listNetCardInfoFiltered = new List<NetCardInfo>();

	private Dictionary<int, NetCardInfo> m_dictSelectedCardInfo = new Dictionary<int, NetCardInfo>();

	private string[] TooltipSkillName = new string[4];

	private string[] TooltipSkillInfo = new string[4];

	private int[] TooltipConditionName = new int[4];

	private string[] TooltipConditionColor = new string[4];

	private List<BtnClickCB> BtnClickCBs = new List<BtnClickCB>();

	private int[] ntyps = new int[6] { 1, 2, 8, 4, 16, 0 };

	private List<NetCharacterCardSlotInfo> tmpNetCharacterCardSlotInfo = new List<NetCharacterCardSlotInfo>();

	private CardColorType tmpCardSortType;

	private EquipHelper.CARD_SORT_KEY tmpCardSortKey;

	private void Start()
	{
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
	}

	public void SetCardType(int nBID)
	{
		if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardSortType & (uint)ntyps[nBID]) == (uint)ntyps[nBID])
		{
			ManagedSingleton<EquipHelper>.Instance.nCardSortType = (CardColorType)((int)ManagedSingleton<EquipHelper>.Instance.nCardSortType & ~ntyps[nBID]);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		else
		{
			ManagedSingleton<EquipHelper>.Instance.nCardSortType = (CardColorType)((int)ManagedSingleton<EquipHelper>.Instance.nCardSortType | ntyps[nBID]);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		for (int i = 0; i < CardType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardSortType & (uint)ntyps[i]) == (uint)ntyps[i])
			{
				CardTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				CardTypeImg[i].gameObject.SetActive(false);
			}
		}
	}

	public void SetSortType(int nBID)
	{
		int num = 1 << nBID;
		if (bIsSetDeploy)
		{
			if (ManagedSingleton<EquipHelper>.Instance.nCardDeploySortKey != (EquipHelper.CARD_SORT_KEY)num)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				ManagedSingleton<EquipHelper>.Instance.nCardDeploySortKey = (EquipHelper.CARD_SORT_KEY)num;
			}
		}
		else if (ManagedSingleton<EquipHelper>.Instance.nCardSortKey != (EquipHelper.CARD_SORT_KEY)num)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			ManagedSingleton<EquipHelper>.Instance.nCardSortKey = (EquipHelper.CARD_SORT_KEY)num;
		}
		for (int i = 0; i < SortType.Length; i++)
		{
			if (bIsSetDeploy)
			{
				if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardDeploySortKey & (uint)(1 << i)) != 0)
				{
					SortTypeImg[i].gameObject.SetActive(true);
				}
				else
				{
					SortTypeImg[i].gameObject.SetActive(false);
				}
			}
			else if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardSortKey & (uint)(1 << i)) != 0)
			{
				SortTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				SortTypeImg[i].gameObject.SetActive(false);
			}
		}
	}

	private void UpdateButtonType()
	{
		for (int i = 0; i < CardType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardSortType & (uint)ntyps[i]) == (uint)ntyps[i])
			{
				CardTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				CardTypeImg[i].gameObject.SetActive(false);
			}
		}
		for (int j = 0; j < SortType.Length; j++)
		{
			if (bIsSetDeploy)
			{
				if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardDeploySortKey & (uint)(1 << j)) != 0)
				{
					SortTypeImg[j].gameObject.SetActive(true);
				}
				else
				{
					SortTypeImg[j].gameObject.SetActive(false);
				}
			}
			else if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardSortKey & (uint)(1 << j)) != 0)
			{
				SortTypeImg[j].gameObject.SetActive(true);
			}
			else
			{
				SortTypeImg[j].gameObject.SetActive(false);
			}
		}
	}

	private void InitSortBtn()
	{
		BtnClickCBs.Clear();
		CardTypeImg = new Image[CardType.Length];
		for (int i = 0; i < CardType.Length; i++)
		{
			BtnClickCB btnClickCB = new BtnClickCB();
			btnClickCB.nBtnID = i;
			btnClickCB.action = (Action<int>)Delegate.Combine(btnClickCB.action, new Action<int>(SetCardType));
			CardType[i].onClick.AddListener(btnClickCB.OnClick);
			BtnClickCBs.Add(btnClickCB);
			CardTypeImg[i] = CardType[i].transform.Find("Image").GetComponent<Image>();
		}
		SortTypeImg = new Image[SortType.Length];
		for (int j = 0; j < SortType.Length; j++)
		{
			SortType[j].gameObject.SetActive(true);
			BtnClickCB btnClickCB2 = new BtnClickCB();
			btnClickCB2.nBtnID = j;
			btnClickCB2.action = (Action<int>)Delegate.Combine(btnClickCB2.action, new Action<int>(SetSortType));
			SortType[j].onClick.AddListener(btnClickCB2.OnClick);
			BtnClickCBs.Add(btnClickCB2);
			SortTypeImg[j] = SortType[j].transform.Find("Image").GetComponent<Image>();
			if (bIsSetDeploy && j + 1 >= SortType.Length)
			{
				SortType[j].gameObject.SetActive(false);
			}
		}
		UpdateButtonType();
		SortRoot.SetActive(false);
	}

	public void Setup(int cid, int slot, int sid, bool bSetDeploy = false, int DeployIndex = -1)
	{
		bIsSetDeploy = bSetDeploy;
		CurrentDeployIndex = DeployIndex;
		SortPanelNew.gameObject.SetActive(true);
		SortPanel.gameObject.SetActive(false);
		if (bIsSetDeploy)
		{
			if (ManagedSingleton<EquipHelper>.Instance.CardDeploySortDescend == 1)
			{
				SortOrderImg.transform.localScale = new Vector3(1f, -1f, 1f);
			}
			else
			{
				SortOrderImg.transform.localScale = new Vector3(1f, 1f, 1f);
			}
		}
		else if (ManagedSingleton<EquipHelper>.Instance.CardSortDescend == 1)
		{
			SortOrderImg.transform.localScale = new Vector3(1f, -1f, 1f);
		}
		else
		{
			SortOrderImg.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		InitSortBtn();
		CharacterID = cid;
		CharacterCardSlot = slot;
		CurrentCardSeqID = sid;
		TargetCardSeqID = sid;
		CurrentCardRoot.enabled = CurrentCardSeqID != 0;
		TargetCardRoot.enabled = CurrentCardSeqID != 0;
		EquipBtn.interactable = false;
		SetCardInfo(0, CurrentCardSeqID);
		SetCardInfo(1, TargetCardSeqID);
		CurrentCardSlotList = new List<NetCharacterCardSlotInfo>();
		InitCardSkillColorRoot();
		m_listNetCardInfo.Clear();
		List<CardInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			m_listNetCardInfo.Add(list[i].netCardInfo);
		}
		if (bIsSetDeploy)
		{
			OnSortGoDeploy();
		}
		else
		{
			OnSortGo();
		}
		base._EscapeEvent = EscapeEvent.CUSTOM;
	}

	private void OnClickCard(int p_idx)
	{
		NetCardInfo netCardInfo = m_listNetCardInfoFiltered[p_idx];
		InitCardSkillColorRoot();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		if (TargetCardSeqID == netCardInfo.CardSeqID)
		{
			TargetCardSeqID = 0;
			TargetCardRoot.enabled = false;
			EquipBtn.interactable = true;
			CardSkillColorRoot2[CharacterCardSlot].SetImage(-1);
			return;
		}
		CARD_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(netCardInfo.CardID, out value);
		if (value != null)
		{
			int n_RARITY = value.n_RARITY;
			if (m_dictSelectedCardInfo.ContainsKey(netCardInfo.CardSeqID))
			{
				m_dictSelectedCardInfo.Remove(netCardInfo.CardSeqID);
			}
			else
			{
				m_dictSelectedCardInfo.Add(netCardInfo.CardSeqID, netCardInfo);
			}
			TargetCardSeqID = netCardInfo.CardSeqID;
			TargetCardRoot.enabled = true;
			EquipBtn.interactable = CurrentCardSeqID != TargetCardSeqID;
			ModifyCardSkillColorRoot(netCardInfo.CardID, netCardInfo.CardSeqID);
			SetCardInfo(1, TargetCardSeqID);
		}
	}

	public void SetCardIcon(CardEquipCell p_unit)
	{
		NetCardInfo netCardInfo = m_listNetCardInfoFiltered[p_unit.NowIdx];
		CARD_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(netCardInfo.CardID, out value);
		if (value != null)
		{
			int cardRank = ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(netCardInfo.Exp);
			p_unit.CardIcon.SetStarAndLv(netCardInfo.Star, cardRank);
			p_unit.CardIcon.SetRare(value.n_RARITY);
			p_unit.CardIcon.SetTypeImage(value.n_TYPE);
			p_unit.CardIcon.SetLockImage(netCardInfo.Protected == 1);
			p_unit.CardIcon.SetFavoriteImage(netCardInfo.Favorite == 1);
			string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
			string s_ICON = value.s_ICON;
			p_unit.CardIcon.Setup(p_unit.NowIdx, p_bundleName, s_ICON, OnClickCard);
			p_unit.SetCardID(netCardInfo.CardSeqID, netCardInfo.CardID);
			p_unit.SetSelection(false);
		}
		else
		{
			p_unit.CardIcon.Clear();
		}
	}

	private Color32 GetValueColor(string str1, string str2)
	{
		Color32[] array = new Color32[3]
		{
			new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
			new Color32(2, 253, 72, byte.MaxValue),
			new Color32(252, 1, 10, byte.MaxValue)
		};
		str1 = str1.Replace("+", "");
		str2 = str2.Replace("+", "");
		int num = int.Parse(str1);
		int num2 = int.Parse(str2);
		if (num2 > num)
		{
			return array[1];
		}
		if (num2 < num)
		{
			return array[2];
		}
		return array[0];
	}

	private void SetCardInfo(int idx, int SeqID)
	{
		CardSkillRoot1[idx].SetActive(false);
		CardSkillRoot2[idx].SetActive(false);
		CardText_ATK[idx].text = "+0";
		CardText_MHP[idx].text = "+0";
		CardText_DEF[idx].text = "+0";
		CardInfo value = null;
		ManagedSingleton<PlayerNetManager>.Instance.dicCard.TryGetValue(SeqID, out value);
		if (value == null)
		{
			return;
		}
		NetCardInfo netCardInfo = value.netCardInfo;
		EXP_TABLE cardExpTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCardExpTable(netCardInfo.Exp);
		CARD_TABLE cARD_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[netCardInfo.CardID];
		CardText_ATK[idx].text = "+" + (int)((float)cardExpTable.n_CARD_ATK * cARD_TABLE.f_PARAM_ATK * (1f + (float)netCardInfo.Star * cARD_TABLE.f_RANKUP));
		CardText_MHP[idx].text = "+" + (int)((float)cardExpTable.n_CARD_HP * cARD_TABLE.f_PARAM_HP * (1f + (float)netCardInfo.Star * cARD_TABLE.f_RANKUP));
		CardText_DEF[idx].text = "+" + (int)((float)cardExpTable.n_CARD_DEF * cARD_TABLE.f_PARAM_DEF * (1f + (float)netCardInfo.Star * cARD_TABLE.f_RANKUP));
		if (idx == 1)
		{
			CardText_ATK[idx].color = GetValueColor(CardText_ATK[0].text, CardText_ATK[idx].text);
			CardText_MHP[idx].color = GetValueColor(CardText_MHP[0].text, CardText_MHP[idx].text);
			CardText_DEF[idx].color = GetValueColor(CardText_DEF[0].text, CardText_DEF[idx].text);
		}
		int[] array = new int[6] { cARD_TABLE.n_SKILL1_RANK0, cARD_TABLE.n_SKILL1_RANK1, cARD_TABLE.n_SKILL1_RANK2, cARD_TABLE.n_SKILL1_RANK3, cARD_TABLE.n_SKILL1_RANK4, cARD_TABLE.n_SKILL1_RANK5 };
		int[] array2 = new int[6] { cARD_TABLE.n_SKILL2_RANK0, cARD_TABLE.n_SKILL2_RANK1, cARD_TABLE.n_SKILL2_RANK2, cARD_TABLE.n_SKILL2_RANK3, cARD_TABLE.n_SKILL2_RANK4, cARD_TABLE.n_SKILL2_RANK5 };
		if (netCardInfo.Star > 5 || netCardInfo.Star < 0)
		{
			netCardInfo.Star = 0;
		}
		int num = array[netCardInfo.Star];
		if (num != 0)
		{
			CardSkillRoot1[idx].SetActive(true);
			SKILL_TABLE skillTbl2 = null;
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(num, out skillTbl2);
			if (skillTbl2 != null)
			{
				TooltipSkillName[idx] = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl2.w_NAME);
				TooltipSkillInfo[idx] = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl2.w_TIP);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(skillTbl2.s_ICON), skillTbl2.s_ICON, delegate(Sprite asset)
				{
					if (asset != null)
					{
						CardSkillIconRoot1[idx].sprite = asset;
					}
					else
					{
						Debug.LogWarning("SkillButton.Setup: unable to load sprite " + skillTbl2.s_ICON);
					}
				});
			}
			if (idx == 0)
			{
				for (int i = 0; i < CardSkillColorRoot11.Length; i++)
				{
					CardSkillColorRoot11[i].SetImage(-1);
				}
			}
			else
			{
				for (int j = 0; j < CardSkillColorRoot21.Length; j++)
				{
					CardSkillColorRoot21[j].SetImage(-1);
				}
			}
			List<NetCharacterCardSlotInfo> tmpList = null;
			if (idx == 1)
			{
				tmpList = tmpNetCharacterCardSlotInfo;
			}
			bool flag = false;
			flag = ((!bIsSetDeploy) ? MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCharactertCardSkillActive(CharacterID, num, value.netCardInfo.CardSeqID, tmpList) : MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCharactertCardSkillActive(0, num, SeqID, tmpList));
			if (cARD_TABLE.n_SKILL1_CHARAID != 0)
			{
				CardSkillMaskCharRoots1[idx].SetActive(!flag);
				CardSkillMaskBasicRoots1[idx].SetActive(false);
				CardSkillFrameCharRoots1[idx].gameObject.SetActive(true);
				CardSkillFrameBasicRoots1[idx].gameObject.SetActive(false);
				TooltipConditionName[idx] = cARD_TABLE.n_SKILL1_CHARAID;
				TooltipConditionColor[idx] = "null";
			}
			else if (cARD_TABLE.s_SKILL1_COMBINATION != "null")
			{
				string[] array3 = cARD_TABLE.s_SKILL1_COMBINATION.Split(',');
				for (int k = 0; k < array3.Length; k++)
				{
					int image = int.Parse(array3[k]);
					if (idx == 0)
					{
						CardSkillColorRoot11[k].SetImage(image);
					}
					else
					{
						CardSkillColorRoot21[k].SetImage(image);
					}
				}
				CardSkillMaskCharRoots1[idx].SetActive(false);
				CardSkillMaskBasicRoots1[idx].SetActive(!flag);
				CardSkillFrameCharRoots1[idx].gameObject.SetActive(false);
				CardSkillFrameBasicRoots1[idx].gameObject.SetActive(true);
				TooltipConditionName[idx] = 0;
				TooltipConditionColor[idx] = cARD_TABLE.s_SKILL1_COMBINATION;
			}
			else
			{
				CardSkillMaskCharRoots1[idx].SetActive(false);
				CardSkillMaskBasicRoots1[idx].SetActive(!flag);
				CardSkillFrameCharRoots1[idx].gameObject.SetActive(false);
				CardSkillFrameBasicRoots1[idx].gameObject.SetActive(true);
				TooltipConditionName[idx] = 0;
				TooltipConditionColor[idx] = "null";
			}
		}
		num = array2[netCardInfo.Star];
		if (num != 0)
		{
			CardSkillRoot2[idx].SetActive(true);
			SKILL_TABLE skillTbl = null;
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(num, out skillTbl);
			if (skillTbl != null)
			{
				TooltipSkillName[idx + 2] = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl.w_NAME);
				TooltipSkillInfo[idx + 2] = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl.w_TIP);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(skillTbl.s_ICON), skillTbl.s_ICON, delegate(Sprite asset)
				{
					if (asset != null)
					{
						CardSkillIconRoot2[idx].sprite = asset;
					}
					else
					{
						Debug.LogWarning("SkillButton.Setup: unable to load sprite " + skillTbl.s_ICON);
					}
				});
			}
			if (idx == 0)
			{
				for (int l = 0; l < CardSkillColorRoot12.Length; l++)
				{
					CardSkillColorRoot12[l].SetImage(-1);
				}
			}
			else
			{
				for (int m = 0; m < CardSkillColorRoot22.Length; m++)
				{
					CardSkillColorRoot22[m].SetImage(-1);
				}
			}
			List<NetCharacterCardSlotInfo> tmpList2 = null;
			if (idx == 1)
			{
				tmpList2 = tmpNetCharacterCardSlotInfo;
			}
			bool flag2 = false;
			flag2 = ((!bIsSetDeploy) ? MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCharactertCardSkillActive(CharacterID, num, value.netCardInfo.CardSeqID, tmpList2) : MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCharactertCardSkillActive(0, num, SeqID, tmpList2));
			if (cARD_TABLE.n_SKILL2_CHARAID != 0)
			{
				CardSkillMaskCharRoots2[idx].SetActive(!flag2);
				CardSkillMaskBasicRoots2[idx].SetActive(false);
				TooltipConditionName[idx + 2] = cARD_TABLE.n_SKILL2_CHARAID;
				TooltipConditionColor[idx + 2] = "null";
			}
			else if (cARD_TABLE.s_SKILL2_COMBINATION != "null")
			{
				string[] array4 = cARD_TABLE.s_SKILL2_COMBINATION.Split(',');
				for (int n = 0; n < array4.Length; n++)
				{
					int image2 = int.Parse(array4[n]);
					if (idx == 0)
					{
						CardSkillColorRoot12[n].SetImage(image2);
					}
					else
					{
						CardSkillColorRoot22[n].SetImage(image2);
					}
				}
				CardSkillMaskCharRoots2[idx].SetActive(false);
				CardSkillMaskBasicRoots2[idx].SetActive(!flag2);
				TooltipConditionName[idx + 2] = 0;
				TooltipConditionColor[idx + 2] = cARD_TABLE.s_SKILL2_COMBINATION;
			}
			else
			{
				CardSkillMaskCharRoots2[idx].SetActive(false);
				CardSkillMaskBasicRoots2[idx].SetActive(!flag2);
				TooltipConditionName[idx + 2] = 0;
				TooltipConditionColor[idx + 2] = "null";
			}
		}
		int childCount = CardBaseRoots[idx].transform.childCount;
		for (int num2 = 0; num2 < childCount; num2++)
		{
			UnityEngine.Object.Destroy(CardBaseRoots[idx].transform.GetChild(num2).gameObject);
		}
		CardBase component = UnityEngine.Object.Instantiate(refPrefab, CardBaseRoots[idx].transform).GetComponent<CardBase>();
		component.CardSetup(netCardInfo);
		component.SetEquipCharImage(netCardInfo.CardSeqID);
	}

	private void InitCardSkillColorRoot()
	{
		for (int i = 0; i < CardSkillColorRoot1.Length; i++)
		{
			CardSkillColorRoot1[i].SetImage(-1);
			CardSkillColorRoot2[i].SetImage(-1);
		}
		if (bIsSetDeploy)
		{
			if (!ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo.ContainsKey(CurrentDeployIndex))
			{
				return;
			}
			CurrentCardDeployInfoList = ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo[CurrentDeployIndex].Values.ToList();
			for (int j = 0; j < CurrentCardDeployInfoList.Count; j++)
			{
				if (ManagedSingleton<PlayerNetManager>.Instance.dicCard.ContainsKey(CurrentCardDeployInfoList[j].CardSeqID))
				{
					CardInfo cardInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCard[CurrentCardDeployInfoList[j].CardSeqID];
					CARD_TABLE cARD_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[cardInfo.netCardInfo.CardID];
					CardSkillColorRoot1[j].SetImage(cARD_TABLE.n_TYPE);
					CardSkillColorRoot2[j].SetImage(cARD_TABLE.n_TYPE);
				}
			}
		}
		else
		{
			if (!ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo.ContainsKey(CharacterID))
			{
				return;
			}
			CurrentCardSlotList = ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo[CharacterID].Values.ToList();
			for (int k = 0; k < CurrentCardSlotList.Count; k++)
			{
				if (ManagedSingleton<PlayerNetManager>.Instance.dicCard.ContainsKey(CurrentCardSlotList[k].CardSeqID))
				{
					CardInfo cardInfo2 = ManagedSingleton<PlayerNetManager>.Instance.dicCard[CurrentCardSlotList[k].CardSeqID];
					CARD_TABLE cARD_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[cardInfo2.netCardInfo.CardID];
					CardSkillColorRoot1[k].SetImage(cARD_TABLE2.n_TYPE);
					CardSkillColorRoot2[k].SetImage(cARD_TABLE2.n_TYPE);
				}
			}
		}
	}

	private void ModifyCardSkillColorRoot(int CardID, int SeqID)
	{
		if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.ContainsKey(CardID))
		{
			CARD_TABLE cARD_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[CardID];
			CardSkillColorRoot2[CharacterCardSlot].SetImage(cARD_TABLE.n_TYPE);
		}
		if (bIsSetDeploy)
		{
			tmpNetCharacterCardSlotInfo.Clear();
			for (int i = 0; i < 3; i++)
			{
				tmpNetCharacterCardSlotInfo.Add(new NetCharacterCardSlotInfo());
			}
			List<NetCardDeployInfo> list = new List<NetCardDeployInfo>();
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo.ContainsKey(CurrentDeployIndex))
			{
				list = ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo[CurrentDeployIndex].Values.ToList();
			}
			for (int j = 0; j < list.Count; j++)
			{
				int num = list[j].CardSlot - 1;
				if (num >= 0 && num < tmpNetCharacterCardSlotInfo.Count)
				{
					tmpNetCharacterCardSlotInfo[num] = new NetCharacterCardSlotInfo();
					tmpNetCharacterCardSlotInfo[num].CharacterID = list[j].DeployId;
					tmpNetCharacterCardSlotInfo[num].CharacterCardSlot = list[j].CardSlot;
					tmpNetCharacterCardSlotInfo[num].CardSeqID = list[j].CardSeqID;
				}
			}
			for (int k = 0; k < CurrentCardDeployInfoList.Count; k++)
			{
				tmpNetCharacterCardSlotInfo[k].CardSeqID = CurrentCardDeployInfoList[k].CardSeqID;
				tmpNetCharacterCardSlotInfo[k].CharacterCardSlot = CurrentCardDeployInfoList[k].CardSlot;
				if (CurrentCardDeployInfoList[k].CardSeqID == SeqID && CurrentCardDeployInfoList[k].CardSeqID != CurrentCardSeqID)
				{
					CardSkillColorRoot2[k].SetImage(-1);
					tmpNetCharacterCardSlotInfo[k].CardSeqID = 0;
					tmpNetCharacterCardSlotInfo[k].CharacterCardSlot = 0;
				}
			}
			tmpNetCharacterCardSlotInfo[CharacterCardSlot].CardSeqID = SeqID;
			tmpNetCharacterCardSlotInfo[CharacterCardSlot].CharacterCardSlot = (sbyte)(CharacterCardSlot + 1);
			return;
		}
		tmpNetCharacterCardSlotInfo.Clear();
		for (int l = 0; l < 3; l++)
		{
			tmpNetCharacterCardSlotInfo.Add(new NetCharacterCardSlotInfo());
		}
		for (int m = 0; m < CurrentCardSlotList.Count; m++)
		{
			tmpNetCharacterCardSlotInfo[m].CardSeqID = CurrentCardSlotList[m].CardSeqID;
			tmpNetCharacterCardSlotInfo[m].CharacterCardSlot = CurrentCardSlotList[m].CharacterCardSlot;
			if (CurrentCardSlotList[m].CardSeqID == SeqID && CurrentCardSlotList[m].CardSeqID != CurrentCardSeqID)
			{
				CardSkillColorRoot2[m].SetImage(-1);
				tmpNetCharacterCardSlotInfo[m].CardSeqID = 0;
				tmpNetCharacterCardSlotInfo[m].CharacterCardSlot = 0;
			}
		}
		tmpNetCharacterCardSlotInfo[CharacterCardSlot].CardSeqID = SeqID;
		tmpNetCharacterCardSlotInfo[CharacterCardSlot].CharacterCardSlot = (sbyte)(CharacterCardSlot + 1);
	}

	public bool IsSelected(int CardSeqID)
	{
		return m_dictSelectedCardInfo.ContainsKey(CardSeqID);
	}

	public int OnGetTargetCardSeqID()
	{
		return TargetCardSeqID;
	}

	public void OnSelectCardEquipIcon(CardEquipCell icon)
	{
		icon.ToggleSelection();
	}

	public void OnClickSortPanelBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		tmpCardSortType = ManagedSingleton<EquipHelper>.Instance.nCardSortType;
		if (bIsSetDeploy)
		{
			tmpCardSortKey = ManagedSingleton<EquipHelper>.Instance.nCardDeploySortKey;
		}
		else
		{
			tmpCardSortKey = ManagedSingleton<EquipHelper>.Instance.nCardSortKey;
		}
		SortRoot.SetActive(true);
		MaskImage.gameObject.SetActive(true);
	}

	public void OnCloseSortRoot()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ManagedSingleton<EquipHelper>.Instance.nCardSortType = tmpCardSortType;
		if (bIsSetDeploy)
		{
			ManagedSingleton<EquipHelper>.Instance.nCardDeploySortKey = tmpCardSortKey;
		}
		else
		{
			ManagedSingleton<EquipHelper>.Instance.nCardSortKey = tmpCardSortKey;
		}
		UpdateButtonType();
		SortRoot.SetActive(false);
		MaskImage.gameObject.SetActive(false);
	}

	public void OnEquipBtn()
	{
		if (bIsSetDeploy)
		{
			CardDeployMain uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardDeployMain>("UI_CardDeployMain");
			if (uI != null)
			{
				uI.OnCurrentDeploySetCard(TargetCardSeqID);
			}
			else
			{
				CharacterInfoCard uI2 = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoCard>("UI_CharacterInfo_Card");
				if (uI2 != null)
				{
					uI2.OnCurrentDeploySetCard(TargetCardSeqID);
				}
				else
				{
					GoCheckUI uI3 = MonoBehaviourSingleton<UIManager>.Instance.GetUI<GoCheckUI>("UI_GoCheck");
					if (uI3 != null)
					{
						uI3.CardDeployScriptRoot.OnCurrentDeploySetCard(TargetCardSeqID);
					}
				}
			}
			base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK08;
			OnClickCloseBtnFix();
		}
		else
		{
			ManagedSingleton<PlayerNetManager>.Instance.EquipCardReq(CharacterID, CharacterCardSlot + 1, TargetCardSeqID, delegate
			{
				base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK08;
				OnClickCloseBtnFix();
			});
		}
	}

	public void OnClickSortGo()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		if (bIsSetDeploy)
		{
			OnSortGoDeploy();
		}
		else
		{
			OnSortGo();
		}
	}

	public void OnSortGo()
	{
		m_dictSelectedCardInfo.Clear();
		m_listNetCardInfoFiltered.Clear();
		foreach (NetCardInfo item in m_listNetCardInfo)
		{
			CARD_TABLE value = null;
			ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(item.CardID, out value);
			if (value != null && ((uint)ManagedSingleton<EquipHelper>.Instance.nCardSortType & (uint)value.n_TYPE) == (uint)value.n_TYPE)
			{
				m_listNetCardInfoFiltered.Add(item);
			}
		}
		m_listNetCardInfoFiltered.Sort((NetCardInfo x, NetCardInfo y) => x.CardID.CompareTo(y.CardID));
		if ((ManagedSingleton<EquipHelper>.Instance.nCardSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_RARITY) == EquipHelper.CARD_SORT_KEY.CARD_SORT_RARITY)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				CARD_TABLE value3 = null;
				CARD_TABLE value4 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(x.CardID, out value3);
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(y.CardID, out value4);
				if (value3 == null || value4 == null)
				{
					return 0;
				}
				int num4 = value3.n_RARITY.CompareTo(value4.n_RARITY);
				if (num4 == 0)
				{
					num4 = x.CardID.CompareTo(y.CardID);
				}
				return num4;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_STAR) == EquipHelper.CARD_SORT_KEY.CARD_SORT_STAR)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num3 = x.Star.CompareTo(y.Star);
				if (num3 == 0)
				{
					num3 = x.CardID.CompareTo(y.CardID);
				}
				return num3;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_LV) == EquipHelper.CARD_SORT_KEY.CARD_SORT_LV)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num2 = x.Exp.CompareTo(y.Exp);
				if (num2 == 0)
				{
					num2 = x.CardID.CompareTo(y.CardID);
				}
				return num2;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_FAVORITE) == EquipHelper.CARD_SORT_KEY.CARD_SORT_FAVORITE)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num = x.Favorite.CompareTo(y.Favorite);
				if (num == 0)
				{
					num = x.CardID.CompareTo(y.CardID);
				}
				return num;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_EXCLUSIVE) == EquipHelper.CARD_SORT_KEY.CARD_SORT_EXCLUSIVE)
		{
			List<NetCardInfo> list = new List<NetCardInfo>();
			List<NetCardInfo> list2 = new List<NetCardInfo>();
			foreach (NetCardInfo item2 in m_listNetCardInfoFiltered)
			{
				CARD_TABLE value2 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(item2.CardID, out value2);
				if (value2 != null)
				{
					if (value2.n_CHARACTER_ID == CharacterID)
					{
						list.Add(item2);
					}
					else
					{
						list2.Add(item2);
					}
				}
			}
			list2.Sort((NetCardInfo x, NetCardInfo y) => x.CardID.CompareTo(y.CardID));
			m_listNetCardInfoFiltered = list2.Concat(list).ToList();
		}
		else
		{
			m_listNetCardInfoFiltered.Sort((NetCardInfo x, NetCardInfo y) => x.CardSeqID.CompareTo(y.CardSeqID));
		}
		if (ManagedSingleton<EquipHelper>.Instance.CardSortDescend == 1)
		{
			m_listNetCardInfoFiltered.Reverse();
		}
		m_ScrollRect.ClearCells();
		m_ScrollRect.OrangeInit(m_CardEquipCell, m_listNetCardInfoFiltered.Count, m_listNetCardInfoFiltered.Count);
		SortRoot.SetActive(false);
		MaskImage.gameObject.SetActive(false);
	}

	public void OnSortGoDeploy()
	{
		m_dictSelectedCardInfo.Clear();
		m_listNetCardInfoFiltered.Clear();
		foreach (NetCardInfo item in m_listNetCardInfo)
		{
			CARD_TABLE value = null;
			ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(item.CardID, out value);
			if (value != null && ((uint)ManagedSingleton<EquipHelper>.Instance.nCardSortType & (uint)value.n_TYPE) == (uint)value.n_TYPE)
			{
				m_listNetCardInfoFiltered.Add(item);
			}
		}
		m_listNetCardInfoFiltered.Sort((NetCardInfo x, NetCardInfo y) => x.CardID.CompareTo(y.CardID));
		if ((ManagedSingleton<EquipHelper>.Instance.nCardDeploySortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_RARITY) == EquipHelper.CARD_SORT_KEY.CARD_SORT_RARITY)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				CARD_TABLE value3 = null;
				CARD_TABLE value4 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(x.CardID, out value3);
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(y.CardID, out value4);
				if (value3 == null || value4 == null)
				{
					return 0;
				}
				int num4 = value3.n_RARITY.CompareTo(value4.n_RARITY);
				if (num4 == 0)
				{
					num4 = x.CardID.CompareTo(y.CardID);
				}
				return num4;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardDeploySortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_STAR) == EquipHelper.CARD_SORT_KEY.CARD_SORT_STAR)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num3 = x.Star.CompareTo(y.Star);
				if (num3 == 0)
				{
					num3 = x.CardID.CompareTo(y.CardID);
				}
				return num3;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardDeploySortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_LV) == EquipHelper.CARD_SORT_KEY.CARD_SORT_LV)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num2 = x.Exp.CompareTo(y.Exp);
				if (num2 == 0)
				{
					num2 = x.CardID.CompareTo(y.CardID);
				}
				return num2;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardDeploySortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_FAVORITE) == EquipHelper.CARD_SORT_KEY.CARD_SORT_FAVORITE)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num = x.Favorite.CompareTo(y.Favorite);
				if (num == 0)
				{
					num = x.CardID.CompareTo(y.CardID);
				}
				return num;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardDeploySortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_EXCLUSIVE) == EquipHelper.CARD_SORT_KEY.CARD_SORT_EXCLUSIVE)
		{
			List<NetCardInfo> list = new List<NetCardInfo>();
			List<NetCardInfo> list2 = new List<NetCardInfo>();
			foreach (NetCardInfo item2 in m_listNetCardInfoFiltered)
			{
				CARD_TABLE value2 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(item2.CardID, out value2);
				if (value2 != null)
				{
					if (value2.n_CHARACTER_ID == CharacterID)
					{
						list.Add(item2);
					}
					else
					{
						list2.Add(item2);
					}
				}
			}
			list2.Sort((NetCardInfo x, NetCardInfo y) => x.CardID.CompareTo(y.CardID));
			m_listNetCardInfoFiltered = list2.Concat(list).ToList();
		}
		else
		{
			m_listNetCardInfoFiltered.Sort((NetCardInfo x, NetCardInfo y) => x.CardSeqID.CompareTo(y.CardSeqID));
		}
		if (ManagedSingleton<EquipHelper>.Instance.CardDeploySortDescend == 1)
		{
			m_listNetCardInfoFiltered.Reverse();
		}
		m_ScrollRect.ClearCells();
		m_ScrollRect.OrangeInit(m_CardEquipCell, m_listNetCardInfoFiltered.Count, m_listNetCardInfoFiltered.Count);
		SortRoot.SetActive(false);
		MaskImage.gameObject.SetActive(false);
	}

	public void OnSortOrder()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		if (bIsSetDeploy)
		{
			ManagedSingleton<EquipHelper>.Instance.CardDeploySortDescend = ((ManagedSingleton<EquipHelper>.Instance.CardDeploySortDescend != 1) ? 1 : 0);
			if (ManagedSingleton<EquipHelper>.Instance.CardDeploySortDescend == 1)
			{
				SortOrderImg.transform.localScale = new Vector3(1f, -1f, 1f);
			}
			else
			{
				SortOrderImg.transform.localScale = new Vector3(1f, 1f, 1f);
			}
			OnSortGoDeploy();
		}
		else
		{
			ManagedSingleton<EquipHelper>.Instance.CardSortDescend = ((ManagedSingleton<EquipHelper>.Instance.CardSortDescend != 1) ? 1 : 0);
			if (ManagedSingleton<EquipHelper>.Instance.CardSortDescend == 1)
			{
				SortOrderImg.transform.localScale = new Vector3(1f, -1f, 1f);
			}
			else
			{
				SortOrderImg.transform.localScale = new Vector3(1f, 1f, 1f);
			}
			OnSortGo();
		}
	}

	public void OnShowSkillTooltip(int idx)
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		SkillTooltipRoots.transform.position = SkillTooltipTransforms[idx].position;
		SkillNameText.text = TooltipSkillName[idx];
		SkillInfoText.text = TooltipSkillInfo[idx];
		SkillTooltip.SetActive(true);
		SkillTooltipRoots.SetActive(true);
		TooltipSkillConditionRoot.SetActive(true);
		TooltipSkillConditionName.gameObject.SetActive(false);
		for (int i = 0; i < TooltipSkillColorRoot.Length; i++)
		{
			TooltipSkillColorRoot[i].SetImage(-1);
		}
		if (TooltipConditionName[idx] != 0)
		{
			TooltipSkillConditionName.gameObject.SetActive(true);
			CHARACTER_TABLE value = null;
			ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(TooltipConditionName[idx], out value);
			if (value != null)
			{
				TooltipSkillConditionName.gameObject.SetActive(true);
				TooltipSkillConditionName.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
			}
		}
		else if (TooltipConditionColor[idx] != "null")
		{
			string[] array = TooltipConditionColor[idx].Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				int image = int.Parse(array[j]);
				TooltipSkillColorRoot[j].SetImage(image);
			}
		}
		else
		{
			TooltipSkillConditionRoot.SetActive(false);
		}
	}

	public void OnHideSkillTooltip()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		SkillTooltip.SetActive(false);
		SkillTooltipRoots.SetActive(false);
	}

	public void OnClickCloseBtnFix()
	{
		CardDeployMain uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardDeployMain>("UI_CardDeployMain");
		if (uI != null)
		{
			uI.OnCardInfoUIClose();
		}
		else
		{
			CharacterInfoCard uI2 = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoCard>("UI_CharacterInfo_Card");
			if (uI2 != null)
			{
				uI2.OnCardInfoUIClose();
			}
			else
			{
				GoCheckUI uI3 = MonoBehaviourSingleton<UIManager>.Instance.GetUI<GoCheckUI>("UI_GoCheck");
				if (uI3 != null)
				{
					uI3.CardDeployScriptRoot.OnCardInfoUIClose();
					uI3.SetMainSubWeapon();
				}
			}
		}
		OnClickCloseBtn();
	}

	protected override void DoCustomEscapeEvent()
	{
		if (SortRoot.activeSelf)
		{
			OnCloseSortRoot();
		}
		else if (SkillTooltip.activeSelf)
		{
			OnHideSkillTooltip();
		}
		else
		{
			OnClickCloseBtn();
		}
	}
}
