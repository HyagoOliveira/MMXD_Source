#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using DragonBones;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class IllustrationUI : OrangeUIBase
{
	public enum IllustrationType
	{
		Chara = 0,
		Weapon = 1,
		Card = 2
	}

	[SerializeField]
	private LoopVerticalScrollRect tLVCR;

	[SerializeField]
	private GameObject SortPanel;

	[SerializeField]
	private GameObject SortRoot;

	public Button[] WeaponType;

	public Button[] SortType;

	public Button[] GetTypeBtn;

	private Image[] WeaponTypeImg;

	private Image[] SortTypeImg;

	private Image[] GetTypeBtnImg;

	[SerializeField]
	public OrangeText GalleryLevel;

	[SerializeField]
	public Scrollbar ExpBar;

	[SerializeField]
	public OrangeText BattlePower;

	[SerializeField]
	public GameObject InfoBG;

	[SerializeField]
	public OrangeText InfoHP;

	[SerializeField]
	public OrangeText InfoAtk;

	[SerializeField]
	public OrangeText InfoDef;

	[SerializeField]
	public Toggle tCharacterBtn;

	[SerializeField]
	public Image tCharacterImg;

	[SerializeField]
	public GameObject tCharacterTip;

	[SerializeField]
	public Toggle tWeaponBtn;

	[SerializeField]
	public Image tWeaponImg;

	[SerializeField]
	public GameObject tWeaponTip;

	[SerializeField]
	public Toggle tCardBtn;

	[SerializeField]
	public Image tCardImg;

	[SerializeField]
	public GameObject tCardTip;

	[SerializeField]
	public Image[] m_colorBar;

	[SerializeField]
	public UnityArmatureComponent lvUpEff;

	[SerializeField]
	public UnityArmatureComponent lvUpWordEff;

	[SerializeField]
	public UnityArmatureComponent expUpEff;

	[SerializeField]
	public GameObject ScrollContenet;

	[SerializeField]
	public GalleryCell galleryCell;

	[SerializeField]
	public GalleryCardCell galleryCardCell;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_tab;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_unLockSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickIconSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickAddExpIconSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_addExpSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_infoSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_LevelUpSE;

	[HideInInspector]
	public GalleryType m_eCharacter = GalleryType.Character;

	[HideInInspector]
	public UnityEngine.Transform pCurrentCell;

	[HideInInspector]
	public GalleryHelper.GalleryCellInfo pCurrentCellInfo;

	private List<WeaponInfoUI.BtnClickCB> BtnClickCBs = new List<WeaponInfoUI.BtnClickCB>();

	private List<UnityArmatureComponent> effectlist = new List<UnityArmatureComponent>();

	private GalleryCalcResult m_ExpResult = new GalleryCalcResult();

	private EXP_TABLE[,] expTbl = new EXP_TABLE[3, 3];

	private EXP_TABLE cExpTbl = new EXP_TABLE();

	private bool m_bUpdateExpBar;

	private float m_fAddPs;

	private int m_totalAddExp;

	private int m_lvDelExp;

	private int m_currentExp;

	private bool bMaxLV;

	private GalleryType m_currentIllustrationType = GalleryType.Character;

	private bool b_LVUPSEPlayed;

	private bool b_EffectLock;

	public int nColumeCnt = 5;

	private Color[] toggleColor = new Color[2]
	{
		new Color(0.8980392f, 0.8313726f, 0.1843137f, 1f),
		new Color(0.5607843f, 0.6784314f, 0.7607843f, 1f)
	};

	private bool b_GalleryUnlockReqCall;

	private bool b_GalleryLockCell;

	private bool EffectLock
	{
		get
		{
			return b_EffectLock;
		}
		set
		{
			b_EffectLock = value;
			if (b_EffectLock)
			{
				tCharacterBtn.interactable = false;
				tWeaponBtn.interactable = false;
				tCardBtn.interactable = false;
			}
			else
			{
				tCharacterBtn.interactable = true;
				tWeaponBtn.interactable = true;
				tCardBtn.interactable = true;
			}
		}
	}

	public bool CharacterTip
	{
		get
		{
			return tCharacterTip.activeSelf;
		}
		set
		{
			tCharacterTip.SetActive(value);
		}
	}

	public bool WeaponTip
	{
		get
		{
			return tWeaponTip.activeSelf;
		}
		set
		{
			tWeaponTip.SetActive(value);
		}
	}

	public bool CardTip
	{
		get
		{
			return tCardTip.activeSelf;
		}
		set
		{
			tCardTip.SetActive(value);
		}
	}

	public int ToggleINT
	{
		get
		{
			return (int)(m_eCharacter - 1);
		}
	}

	private void Update()
	{
		if (m_bUpdateExpBar)
		{
			m_fAddPs += Time.smoothDeltaTime;
			int num = (int)(m_fAddPs * (float)m_totalAddExp) - m_lvDelExp;
			float num2 = SetExpBarOnly(m_currentExp + num, cExpTbl.n_GALLERYEXP);
			if (m_fAddPs >= 1f)
			{
				m_bUpdateExpBar = false;
				UpdateExpTbl();
				SetExpInfo();
			}
			else if (num2 >= 1f)
			{
				PlayLevelUpEff();
				m_currentExp = 0;
				cExpTbl = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[cExpTbl.n_ID + 1];
				m_lvDelExp = (int)(m_fAddPs * (float)m_totalAddExp);
				GalleryLevel.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GALLERY_LV"), cExpTbl.n_ID);
			}
		}
		for (int i = 0; i < effectlist.Count; i++)
		{
			UnityArmatureComponent unityArmatureComponent = effectlist[i];
			if (!unityArmatureComponent.animation.lastAnimationState.isPlaying)
			{
				UnityEngine.Object.DestroyImmediate(unityArmatureComponent.gameObject);
				effectlist.RemoveAt(i);
				i--;
			}
		}
	}

	private void CloseAllEffect()
	{
	}

	public void Setup(GalleryType tCharacter = GalleryType.Character)
	{
		tCardBtn.gameObject.SetActive(true);
		switch (tCharacter)
		{
		case GalleryType.Character:
			tCharacterBtn.isOn = true;
			tWeaponBtn.isOn = false;
			tCardBtn.isOn = false;
			break;
		case GalleryType.Weapon:
			tCharacterBtn.isOn = false;
			tWeaponBtn.isOn = true;
			tCardBtn.isOn = false;
			break;
		case GalleryType.Card:
			tCharacterBtn.isOn = false;
			tWeaponBtn.isOn = false;
			tCardBtn.isOn = true;
			break;
		}
		m_currentIllustrationType = tCharacter;
		ResetLVCR(tCharacter);
		UpdateExpTbl();
		SetExpInfo();
		tCharacterBtn.onValueChanged.AddListener(OnCharacterChange);
		tWeaponBtn.onValueChanged.AddListener(OnWeaponChange);
		tCardBtn.onValueChanged.AddListener(OnCardChange);
		CharacterTip = ManagedSingleton<GalleryHelper>.Instance.CharacterHint;
		WeaponTip = ManagedSingleton<GalleryHelper>.Instance.WeaponHint;
		CardTip = ManagedSingleton<GalleryHelper>.Instance.CardHint;
	}

	private void ClearList()
	{
	}

	public void OnClickGalleryInfo()
	{
		InfoBG.SetActive(true);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_infoSE);
	}

	public void OnClickGalleryInfoBG()
	{
		InfoBG.SetActive(false);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
	}

	private void ResetLVCR(GalleryType tt)
	{
		m_eCharacter = tt;
		int toggleINT = ToggleINT;
		nColumeCnt = ((m_eCharacter == GalleryType.Card) ? 5 : 6);
		Dictionary<int, GalleryHelper.GalleryCellInfo> obj = ManagedSingleton<GalleryHelper>.Instance.GalleryCellInfos[toggleINT];
		int num = obj.Count / nColumeCnt;
		if (obj.Count % nColumeCnt != 0)
		{
			num++;
		}
		tLVCR.totalCount = num;
		tLVCR.RefillCells();
		tCharacterImg.color = ((m_eCharacter == GalleryType.Character) ? toggleColor[0] : toggleColor[1]);
		tWeaponImg.color = ((m_eCharacter == GalleryType.Weapon) ? toggleColor[0] : toggleColor[1]);
		tCardImg.color = ((m_eCharacter == GalleryType.Card) ? toggleColor[0] : toggleColor[1]);
	}

	public void OnCharacterChange(bool value)
	{
		if (EffectLock || b_GalleryUnlockReqCall)
		{
			if (m_currentIllustrationType == GalleryType.Character)
			{
				tCharacterBtn.isOn = true;
			}
			if (m_currentIllustrationType == GalleryType.Card)
			{
				tCardBtn.isOn = true;
			}
			if (m_currentIllustrationType == GalleryType.Weapon)
			{
				tWeaponBtn.isOn = true;
			}
		}
		else if (m_currentIllustrationType != GalleryType.Character)
		{
			b_GalleryLockCell = true;
			CloseAllEffect();
			ResetLVCR(GalleryType.Character);
			bMaxLV = false;
			UpdateExpTbl();
			SetExpInfo();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_tab);
			m_currentIllustrationType = GalleryType.Character;
			b_GalleryLockCell = false;
		}
	}

	public void OnWeaponChange(bool value)
	{
		if (EffectLock || b_GalleryUnlockReqCall)
		{
			if (m_currentIllustrationType == GalleryType.Character)
			{
				tCharacterBtn.isOn = true;
			}
			if (m_currentIllustrationType == GalleryType.Card)
			{
				tCardBtn.isOn = true;
			}
			if (m_currentIllustrationType == GalleryType.Weapon)
			{
				tWeaponBtn.isOn = true;
			}
		}
		else if (m_currentIllustrationType != GalleryType.Weapon)
		{
			b_GalleryLockCell = true;
			CloseAllEffect();
			ResetLVCR(GalleryType.Weapon);
			bMaxLV = false;
			UpdateExpTbl();
			SetExpInfo();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_tab);
			m_currentIllustrationType = GalleryType.Weapon;
			b_GalleryLockCell = false;
		}
	}

	public void OnCardChange(bool value)
	{
		if (EffectLock || b_GalleryUnlockReqCall)
		{
			if (m_currentIllustrationType == GalleryType.Character)
			{
				tCharacterBtn.isOn = true;
			}
			if (m_currentIllustrationType == GalleryType.Card)
			{
				tCardBtn.isOn = true;
			}
			if (m_currentIllustrationType == GalleryType.Weapon)
			{
				tWeaponBtn.isOn = true;
			}
		}
		else if (m_currentIllustrationType != GalleryType.Card)
		{
			b_GalleryLockCell = true;
			CloseAllEffect();
			ResetLVCR(GalleryType.Card);
			bMaxLV = false;
			UpdateExpTbl();
			SetExpInfo();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_tab);
			m_currentIllustrationType = GalleryType.Card;
			b_GalleryLockCell = false;
		}
	}

	private void UpdateExpTbl()
	{
		int toggleINT = ToggleINT;
		m_ExpResult = ManagedSingleton<GalleryHelper>.Instance.GalleryGetGalleryTypeExp(m_eCharacter);
		expTbl[toggleINT, 0] = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[m_ExpResult.m_lv - 1];
		expTbl[toggleINT, 1] = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[m_ExpResult.m_lv];
		if (m_ExpResult.m_lv + 1 < ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.Count)
		{
			expTbl[toggleINT, 2] = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[m_ExpResult.m_lv + 1];
		}
		else
		{
			bMaxLV = true;
			expTbl[toggleINT, 2] = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[m_ExpResult.m_lv];
		}
		m_ExpResult.m_a = m_ExpResult.m_totalExp - expTbl[toggleINT, 0].n_TOTAL_GALLERYEXP;
		m_ExpResult.m_b = expTbl[toggleINT, 1].n_GALLERYEXP;
	}

	private float SetExpInfo()
	{
		int toggleINT = ToggleINT;
		float num = (float)m_ExpResult.m_a / (float)m_ExpResult.m_b;
		int lv = m_ExpResult.m_lv;
		int num2 = expTbl[toggleINT, 1].n_GALLERY_ATK;
		int num3 = expTbl[toggleINT, 1].n_GALLERY_DEF;
		int num4 = expTbl[toggleINT, 1].n_GALLERY_HP;
		if (toggleINT == 2)
		{
			num2 = expTbl[toggleINT, 1].n_CARDGALLERY_ATK;
			num3 = expTbl[toggleINT, 1].n_CARDGALLERY_DEF;
			num4 = expTbl[toggleINT, 1].n_CARDGALLERY_HP;
		}
		int num5 = num2 * OrangeConst.BP_ATK + num3 * OrangeConst.BP_DEF + num4 * OrangeConst.BP_HP;
		InfoAtk.text = string.Format("+{0}", num2);
		InfoDef.text = string.Format("+{0}", num3);
		InfoHP.text = string.Format("+{0}", num4);
		GalleryLevel.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GALLERY_LV"), lv);
		if (bMaxLV)
		{
			num = 1f;
		}
		ExpBar.size = num;
		BattlePower.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GALLERY_BP"), num5);
		return num;
	}

	private float SetExpBarOnly(int exp, int total)
	{
		float num = (float)exp / (float)total;
		if (bMaxLV)
		{
			num = 1f;
		}
		ExpBar.size = num;
		return num;
	}

	private void PlayLevelUpEff()
	{
		if (!lvUpEff.isActiveAndEnabled && !lvUpWordEff.isActiveAndEnabled)
		{
			EffectLock = true;
			lvUpEff.transform.gameObject.SetActive(true);
			lvUpWordEff.transform.gameObject.SetActive(true);
			lvUpEff.animation.Reset();
			lvUpWordEff.animation.Reset();
			lvUpEff.animation.Play("newAnimation", 1);
			lvUpWordEff.animation.Play("newAnimation", 1);
			LeanTween.delayedCall(lvUpWordEff.animation.GetState("newAnimation").totalTime, (Action)delegate
			{
				lvUpEff.transform.gameObject.SetActive(false);
				lvUpWordEff.transform.gameObject.SetActive(false);
				lvUpEff.animation.Stop();
				lvUpWordEff.animation.Stop();
				EffectLock = false;
			});
			if (!b_LVUPSEPlayed)
			{
				StartCoroutine(LatePlayLevelUpEffSE());
			}
		}
	}

	private IEnumerator LatePlayLevelUpEffSE()
	{
		b_LVUPSEPlayed = true;
		yield return new WaitForSeconds(0.3f);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_LevelUpSE);
	}

	private void calledAction(object obj)
	{
		if (obj != null)
		{
			UnityEngine.Object.DestroyImmediate(obj as GameObject);
		}
	}

	private void PlayGetExpEff(UnityEngine.Transform transf)
	{
		EffectLock = true;
		UnityArmatureComponent unityArmatureComponent = UnityEngine.Object.Instantiate(expUpEff);
		unityArmatureComponent.transform.SetParent(transf);
		unityArmatureComponent.transform.gameObject.SetActive(true);
		unityArmatureComponent.transform.localScale = new Vector3(75f, 75f, 0f);
		unityArmatureComponent.transform.localPosition = new Vector3(10f, -30f, 0f);
		float totalTime = unityArmatureComponent.animation.GetState("newAnimation").totalTime;
		unityArmatureComponent.animation.Reset();
		unityArmatureComponent.animation.Play("newAnimation", 1);
		effectlist.Add(unityArmatureComponent);
	}

	private IEnumerator LatePlayGetExpEffSE()
	{
		b_LVUPSEPlayed = true;
		yield return new WaitForSeconds(0.3f);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addExpSE);
	}

	private IEnumerator WaitEffectPlayEnd(int oldType)
	{
		while (effectlist.Count != 0 || oldType != ToggleINT)
		{
			yield return new WaitForSeconds(0.2f);
		}
		ManagedSingleton<GalleryHelper>.Instance.BuildGalleryInfo();
		CharacterTip = ManagedSingleton<GalleryHelper>.Instance.CharacterHint;
		WeaponTip = ManagedSingleton<GalleryHelper>.Instance.WeaponHint;
		CardTip = ManagedSingleton<GalleryHelper>.Instance.CardHint;
		int toggleINT = ToggleINT;
		Dictionary<int, GalleryHelper.GalleryCellInfo> obj = ManagedSingleton<GalleryHelper>.Instance.GalleryCellInfos[toggleINT];
		int num = obj.Count / nColumeCnt;
		if (obj.Count % nColumeCnt != 0)
		{
			num++;
		}
		tLVCR.totalCount = num;
		tLVCR.RefreshCells();
		b_GalleryUnlockReqCall = false;
		EffectLock = false;
	}

	private void GetExpUnmask(UnityEngine.Transform tf, GalleryHelper.GalleryCellInfo cellinfo)
	{
		m_totalAddExp = 0;
		PlayGetExpEff(tf.gameObject.transform);
		UnlockGetExp(cellinfo);
	}

	private void UnlockGetExp(GalleryHelper.GalleryCellInfo cellInfo)
	{
		List<int> galleryIDs = new List<int>();
		List<GALLERY_TABLE> lockInfo = new List<GALLERY_TABLE>();
		int cardID = 0;
		if (cellInfo.tType == GalleryType.Card)
		{
			cardID = cellInfo.m_objID;
		}
		cellInfo.m_lockGallery.ForEach(delegate(GALLERY_TABLE tbl)
		{
			if (ManagedSingleton<GalleryHelper>.Instance.GalleryCheckUnlock(tbl, cardID))
			{
				galleryIDs.Add(tbl.n_ID);
				m_totalAddExp += tbl.n_EXP;
			}
			else
			{
				lockInfo.Add(tbl);
			}
		});
		if (galleryIDs.Count != 0)
		{
			b_GalleryUnlockReqCall = true;
			if (cellInfo.tType == GalleryType.Card)
			{
				List<NetGalleryMainIdInfo> list = new List<NetGalleryMainIdInfo>();
				NetGalleryMainIdInfo netGalleryMainIdInfo = new NetGalleryMainIdInfo();
				netGalleryMainIdInfo.GalleryMainID = cellInfo.m_objID;
				netGalleryMainIdInfo.GalleryIDList = galleryIDs;
				list.Add(netGalleryMainIdInfo);
				ManagedSingleton<PlayerNetManager>.Instance.GalleryCardUnlockReq(list, delegate
				{
					StartCoroutine(WaitEffectPlayEnd(ToggleINT));
				});
			}
			else
			{
				ManagedSingleton<PlayerNetManager>.Instance.GalleryUnlockReq(galleryIDs, delegate
				{
					StartCoroutine(WaitEffectPlayEnd(ToggleINT));
				});
			}
		}
		if (lockInfo.Count != 0)
		{
			cellInfo.m_lockGallery = lockInfo;
		}
		if (m_totalAddExp != 0)
		{
			int toggleINT = ToggleINT;
			if (expTbl[toggleINT, 1].n_TOTAL_GALLERYEXP == expTbl[toggleINT, 2].n_TOTAL_GALLERYEXP)
			{
				bMaxLV = true;
				return;
			}
			m_currentExp = m_ExpResult.m_a;
			cExpTbl = expTbl[toggleINT, 1];
			m_fAddPs = 0f;
			m_lvDelExp = 0;
			m_bUpdateExpBar = true;
			float num = (float)(m_currentExp + m_totalAddExp) / (float)cExpTbl.n_GALLERYEXP;
		}
	}

	public void onCellClick(UnityEngine.Transform tf, GalleryHelper.GalleryCellInfo cellinfo)
	{
		Debug.Log("cell click");
		if (EffectLock || b_GalleryUnlockReqCall || b_GalleryLockCell)
		{
			return;
		}
		if (cellinfo.m_isCanUnlock)
		{
			PlayUISE(m_unLockSE);
			b_LVUPSEPlayed = false;
			GetExpUnmask(tf, cellinfo);
		}
		else
		{
			if (cellinfo.m_isMask)
			{
				return;
			}
			if (cellinfo.m_isCanGetExp)
			{
				PlayUISE(m_clickAddExpIconSE);
				GetExpUnmask(tf, cellinfo);
				return;
			}
			pCurrentCell = tf;
			pCurrentCellInfo = cellinfo;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_IllustrationTarget", delegate(IllustrationTargetUI ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.closeCB = delegate
				{
					ResetLVCR(m_currentIllustrationType);
					UpdateExpTbl();
					SetExpInfo();
				};
				ui.Setup(this);
			});
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickIconSE);
		}
	}
}
