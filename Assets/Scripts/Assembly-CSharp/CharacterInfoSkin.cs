#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoSkin : OrangeUIBase
{
	[SerializeField]
	private ScrollRect m_scrollView;

	[SerializeField]
	private OrangeText m_HPBonus;

	[SerializeField]
	private OrangeText m_ATKBonus;

	[SerializeField]
	private OrangeText m_DEFBonus;

	[SerializeField]
	private Transform m_selectionFrame;

	[SerializeField]
	private Transform m_unlockInfoPanel;

	[SerializeField]
	private Button m_btnEquip;

	[SerializeField]
	private CommonIconBase m_materialIconRef;

	[SerializeField]
	private Transform m_materialIconPos;

	[SerializeField]
	private Button m_btnUnlock;

	private List<CommonIconBase> m_skinIconList = new List<CommonIconBase>();

	private bool m_bScrollPointerDown;

	private CharacterInfo m_characterInfo;

	private CharacterInfoUI m_characterInfoUI;

	private int m_snapTweenId;

	private int m_previewTweenId;

	private int m_skinCount;

	private float m_originalScale = 0.85f;

	private float m_targetScale = 1.08f;

	private int m_focusIndex = -1;

	private int m_equippedIndex = -1;

	private List<SKIN_TABLE> m_skinTableList;

	private SKIN_TABLE m_currentSkinTable;

	private CommonIconBase m_materialIcon;

	private float m_cellHalfWidth;

	private float m_buttonDownTime;

	private float m_buttonReleaseTime;

	private const float m_buttonClickTime = 0.5f;

	private bool m_bClickScroll;

	private Vector2 m_targetScrollPos = Vector2.zero;

	private GridLayoutGroup m_layoutGroup;

	private float m_minVelocity = 100f;

	private float m_snapSpeed = 0.5f;

	private float m_spacing;

	private bool initSE;

	private int itemOwned;

	private int itemNeeded;

	public override void SetCanvas(bool enable)
	{
		if (enable)
		{
			UpdateMaterialIcon();
		}
		base.SetCanvas(enable);
	}

	public void Setup(CharacterInfo info)
	{
		initSE = false;
		m_characterInfo = info;
		m_characterInfoUI = base.transform.parent.GetComponentInChildren<CharacterInfoUI>();
		m_layoutGroup = m_scrollView.content.GetComponent<GridLayoutGroup>();
		m_focusIndex = -1;
		m_skinTableList = ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.Values.Where((SKIN_TABLE x) => x.n_MAINID == m_characterInfo.netInfo.CharacterID).ToList();
		m_skinCount = m_skinTableList.Count + 1;
		m_spacing = ((m_skinCount == 1) ? 0f : (1f / (float)(m_skinCount - 1)));
		if (m_skinCount > 0)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseBig", "CommonIconBaseBig", delegate(GameObject asset)
			{
				new GameObject().AddComponent<RectTransform>().transform.SetParent(m_scrollView.content.transform);
				int i;
				for (i = 0; i < m_skinCount; i++)
				{
					GameObject obj = UnityEngine.Object.Instantiate(asset, m_scrollView.content.transform);
					obj.transform.localScale = new Vector3(m_originalScale, m_originalScale, 1f);
					CommonIconBase component = obj.GetComponent<CommonIconBase>();
					m_skinIconList.Add(component);
					if (i == 0)
					{
						component.SetupSkin(m_characterInfo, m_characterInfo.netInfo, -1, i);
					}
					else
					{
						NetCharacterInfo netCharacterInfo = new NetCharacterInfo();
						int n_ID = m_skinTableList[i - 1].n_ID;
						netCharacterInfo.CharacterID = m_characterInfo.netInfo.CharacterID;
						component.SetupSkin(m_characterInfo, netCharacterInfo, n_ID, i);
					}
				}
				new GameObject().AddComponent<RectTransform>().transform.SetParent(m_scrollView.content.transform);
				GridLayoutGroup component2 = m_scrollView.content.GetComponent<GridLayoutGroup>();
				m_cellHalfWidth = component2.cellSize.x / 2f * 1.1f;
				m_scrollView.content.sizeDelta = new Vector2(component2.cellSize.x * (float)(i + 2), component2.cellSize.y);
			});
		}
		float x2 = 1f / (float)m_skinCount * ((float)GetEquippedSkinIndex() + 0.5f);
		Vector2 scrollPos = new Vector2(x2, 0f);
		OnScrollUpdate(scrollPos);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void OnClickMaterialIcon(int p_idx)
	{
		ITEM_TABLE itemTable;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(m_currentSkinTable.n_UNLOCK_ID, out itemTable))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemHowToGet", delegate(ItemHowToGetUI ui)
			{
				ui.Setup(itemTable, itemNeeded);
			});
		}
	}

	private void UpdateMaterialIcon()
	{
		itemOwned = 0;
		itemNeeded = 0;
		if (m_currentSkinTable == null)
		{
			return;
		}
		m_btnUnlock.onClick.RemoveAllListeners();
		if (m_materialIcon == null && m_materialIconRef != null)
		{
			m_materialIcon = UnityEngine.Object.Instantiate(m_materialIconRef, m_materialIconPos);
		}
		m_materialIcon.SetupItem(m_currentSkinTable.n_UNLOCK_ID, 0, OnClickMaterialIcon);
		itemNeeded = m_currentSkinTable.n_UNLOCK_COUNT;
		bool flag = false;
		int n_UNLOCK_ID = m_currentSkinTable.n_UNLOCK_ID;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(n_UNLOCK_ID))
		{
			if (n_UNLOCK_ID == OrangeConst.ITEMID_FREE_JEWEL || n_UNLOCK_ID == OrangeConst.ITEMID_JEWEL)
			{
				itemOwned = ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel();
				flag = true;
			}
			else
			{
				itemOwned = ManagedSingleton<PlayerNetManager>.Instance.dicItem[m_currentSkinTable.n_UNLOCK_ID].netItemInfo.Stack;
			}
		}
		m_materialIcon.SetAmount(itemNeeded, itemOwned);
		if (flag)
		{
			m_btnUnlock.onClick.AddListener(OnClickOpenConsumeUI);
			m_btnUnlock.interactable = true;
		}
		else
		{
			m_btnUnlock.onClick.AddListener(OnClickUnlockSkin);
			m_btnUnlock.interactable = itemOwned >= itemNeeded;
		}
	}

	public void OnScrollUpdate(Vector2 scrollPos)
	{
		if (m_bClickScroll)
		{
			scrollPos = m_targetScrollPos;
		}
		float num = ((m_spacing == 0f) ? 0f : ((scrollPos.x % m_spacing >= m_spacing / 2f) ? 1f : 0f));
		float value = ((m_spacing == 0f) ? 0f : ((Mathf.Floor(scrollPos.x / m_spacing) + num) * m_spacing));
		value = Mathf.Clamp01(value);
		int num2 = ((m_spacing != 0f) ? Mathf.RoundToInt(value / m_spacing) : 0);
		float to = value * m_layoutGroup.cellSize.x * (0f - (float)(m_skinCount - 1));
		Canvas.ForceUpdateCanvases();
		IconScaleHelper(m_skinIconList[num2].transform, m_targetScale);
		if (num2 >= 1)
		{
			IconScaleHelper(m_skinIconList[num2 - 1].transform, m_originalScale);
		}
		if (num2 < m_skinCount - 1)
		{
			IconScaleHelper(m_skinIconList[num2 + 1].transform, m_originalScale);
		}
		if (Math.Abs(m_scrollView.velocity.x) <= m_minVelocity && m_snapTweenId == 0 && !m_bScrollPointerDown)
		{
			m_snapTweenId = LeanTween.value(m_scrollView.content.anchoredPosition.x, to, m_snapSpeed).setOnUpdate(delegate(float val)
			{
				m_scrollView.content.anchoredPosition = new Vector2(val, m_scrollView.content.anchoredPosition.y);
			}).setOnComplete((Action)delegate
			{
				initSE = true;
				m_bClickScroll = false;
			})
				.setEase(LeanTweenType.easeOutCubic)
				.uniqueId;
		}
		m_selectionFrame.gameObject.SetActive(true);
		m_selectionFrame.position = m_skinIconList[num2].transform.position;
		m_selectionFrame.localScale = new Vector3(m_targetScale, m_targetScale, 1f);
		if (m_focusIndex == num2)
		{
			return;
		}
		m_focusIndex = num2;
		m_currentSkinTable = ((m_focusIndex == 0) ? null : m_skinTableList[m_focusIndex - 1]);
		UpdateBonusAttributes();
		UpdateUnlockInfo();
		if (initSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
		}
		LeanTween.cancel(ref m_previewTweenId);
		m_previewTweenId = LeanTween.delayedCall(1f, (Action)delegate
		{
			if (!m_characterInfoUI.IsLock)
			{
				m_characterInfoUI.RefreshModel(m_currentSkinTable);
				m_characterInfoUI.RefreshPortrait(m_currentSkinTable);
			}
		}).uniqueId;
	}

	private void IconScaleHelper(Transform targetTransform, float targetScale)
	{
		float x = targetTransform.localScale.x;
		float time = 0.1f;
		LeanTween.value(targetTransform.gameObject, x, targetScale, time).setOnUpdate(delegate(float val)
		{
			targetTransform.localScale = new Vector3(val, val, 1f);
		}).setEase(LeanTweenType.easeOutCubic);
	}

	private void UpdateBonusAttributes()
	{
		m_HPBonus.text = ((m_focusIndex == 0) ? "+0" : string.Format("+{0}", m_currentSkinTable.n_HP));
		m_ATKBonus.text = ((m_focusIndex == 0) ? "+0" : string.Format("+{0}", m_currentSkinTable.n_ATK));
		m_DEFBonus.text = ((m_focusIndex == 0) ? "+0" : string.Format("+{0}", m_currentSkinTable.n_DEF));
	}

	private int GetEquippedSkinIndex()
	{
		for (int i = 0; i < m_skinTableList.Count; i++)
		{
			if (m_skinTableList[i].n_ID == m_characterInfo.netInfo.Skin)
			{
				return i + 1;
			}
		}
		return 0;
	}

	private void UpdateUnlockInfo()
	{
		int skin = m_characterInfo.netInfo.Skin;
		if (m_characterInfo.netInfo.State != 1)
		{
			m_unlockInfoPanel.gameObject.SetActive(false);
			return;
		}
		if (m_focusIndex == 0)
		{
			m_unlockInfoPanel.gameObject.SetActive(false);
			m_btnEquip.gameObject.SetActive(true);
			m_btnEquip.interactable = skin != 0;
			if (skin == 0)
			{
				m_equippedIndex = m_focusIndex;
			}
			return;
		}
		bool flag = m_characterInfo.netSkinList.Contains(m_currentSkinTable.n_ID);
		m_unlockInfoPanel.gameObject.SetActive(!flag);
		m_btnEquip.gameObject.SetActive(flag);
		if (flag)
		{
			m_btnEquip.interactable = skin != m_currentSkinTable.n_ID;
		}
		if (skin == m_currentSkinTable.n_ID)
		{
			m_equippedIndex = m_focusIndex;
		}
		UpdateMaterialIcon();
	}

	private void OnClickSkinIcon(int index)
	{
		Debug.Log("Skin Index = " + index);
	}

	public void OnClickUnlockSkin()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		ManagedSingleton<PlayerNetManager>.Instance.CharacterUnlockSkinReq(m_characterInfo.netInfo.CharacterID, m_currentSkinTable.n_ID, delegate
		{
			m_skinIconList[m_focusIndex].SetOtherSkinInfo(m_characterInfo, m_currentSkinTable.n_ID);
			UpdateUnlockInfo();
			StartCoroutine(ManagedSingleton<CharacterHelper>.Instance.CheckCharacterUpgrades());
			m_characterInfoUI.RefreshSideButtonRedDots();
			m_characterInfoUI.RefreshQuickSelectBar();
			m_skinIconList[m_focusIndex].EnableRedDot(false);
		});
	}

	public void OnClickOpenConsumeUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonConsumeMsg", delegate(CommonConsumeMsgUI ui)
		{
			string itemName = ManagedSingleton<OrangeTableHelper>.Instance.GetItemName(OrangeConst.ITEMID_FREE_JEWEL);
			string desc = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FREE_PAYTOUNLOCK_CONFIRM"), itemName, OrangeConst.RESEARCH_RESIGN_JEWEL);
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), desc, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), itemOwned, itemNeeded, delegate
			{
				if (itemOwned >= itemNeeded)
				{
					OnClickUnlockSkin();
				}
				else
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DIAMOND_OUT"), delegate
					{
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI shopUI)
						{
							shopUI.Setup(ShopTopUI.ShopSelectTab.directproduct);
						});
					}, null);
				}
			});
		});
	}

	public void OnClickEquipSkin()
	{
		int skinID = 0;
		if (m_currentSkinTable != null)
		{
			skinID = m_currentSkinTable.n_ID;
		}
		int index = ((m_characterInfo.netInfo.Skin != 0) ? m_equippedIndex : 0);
		m_skinIconList[index].SetOtherSkinInfo(m_characterInfo, m_characterInfo.netInfo.Skin);
		int index2 = ((skinID != 0) ? m_focusIndex : 0);
		m_skinIconList[index2].SetOtherSkinInfo(m_characterInfo, skinID, true);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
		ManagedSingleton<PlayerNetManager>.Instance.CharacterSkinSetReq(m_characterInfo.netInfo.CharacterID, skinID, delegate
		{
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara == m_characterInfo.netInfo.CharacterID)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_RENDER_CHARACTER, ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara], ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID], m_currentSkinTable);
			}
			m_characterInfo.netInfo.Skin = skinID;
			UpdateUnlockInfo();
			m_characterInfoUI.RefreshQuickSelectBar();
		});
	}

	public void OnScrollViewClick()
	{
		if (!(m_buttonReleaseTime > 0.5f))
		{
			RectTransform component = m_scrollView.GetComponent<RectTransform>();
			Vector3 mousePosition = Input.mousePosition;
			Vector2 localPoint = default(Vector2);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(component, mousePosition, MonoBehaviourSingleton<UIManager>.Instance.UICamera, out localPoint);
			Vector2 vector = new Vector2(1f / (float)m_skinCount, 0f);
			LeanTween.cancel(ref m_snapTweenId);
			if (localPoint.x > m_cellHalfWidth)
			{
				m_targetScrollPos = m_scrollView.normalizedPosition + vector;
				m_bClickScroll = true;
				OnScrollUpdate(m_scrollView.normalizedPosition);
			}
			else if (localPoint.x < 0f - m_cellHalfWidth)
			{
				m_targetScrollPos = m_scrollView.normalizedPosition - vector;
				m_bClickScroll = true;
				OnScrollUpdate(m_scrollView.normalizedPosition);
			}
		}
	}

	public void OnScrollViewPointerDown()
	{
		m_buttonDownTime = Time.time;
		m_bScrollPointerDown = true;
		if (m_snapTweenId != 0)
		{
			LeanTween.cancel(ref m_snapTweenId);
			m_snapTweenId = 0;
		}
	}

	public void OnScrollViewPointerUp()
	{
		m_buttonReleaseTime = Time.time - m_buttonDownTime;
		m_bScrollPointerDown = false;
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref m_snapTweenId);
		LeanTween.cancel(ref m_previewTweenId);
	}

	public void StopLeanTween()
	{
		LeanTween.cancel(ref m_snapTweenId);
		LeanTween.cancel(ref m_previewTweenId);
	}
}
