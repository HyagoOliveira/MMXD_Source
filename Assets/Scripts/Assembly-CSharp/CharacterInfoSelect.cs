using System;
using System.Collections;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

internal class CharacterInfoSelect : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private Transform m_sortArrowIcon;

	[SerializeField]
	private Image OnFavoriteImg;

	private CanvasGroup m_canvasGroup;

	private int m_canvasGroupTween;

	private int totalCount = -1;

	private bool refreshMenuFlg;

	private void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_SHOP, UpdateRefreashMenuFlag);
	}

	private void UpdateRefreashMenuFlag()
	{
		refreshMenuFlg = true;
	}

	public void RefreshMenu(bool chkCountChange = false)
	{
		UpdateSortArrow();
		if (TurtorialUI.IsTutorialing())
		{
			scrollRect.vertical = false;
		}
		else if (!refreshMenuFlg && chkCountChange && ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList().Count == totalCount)
		{
			return;
		}
		scrollRect.totalCount = ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList().Count;
		totalCount = scrollRect.totalCount;
		scrollRect.RefillCells();
		StartCoroutine(ResetToTop());
		refreshMenuFlg = false;
	}

	private IEnumerator ResetToTop()
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		scrollRect.SrollToCell(0, 0.1f);
		scrollRect.verticalNormalizedPosition = 0f;
	}

	public void Setup()
	{
		ManagedSingleton<CharacterHelper>.Instance.SortCharacterList();
		m_canvasGroup = GetComponent<CanvasGroup>();
		RefreshMenu();
	}

	public void SetActive(bool bActive)
	{
		if (base.gameObject.activeSelf == bActive)
		{
			return;
		}
		LeanTween.cancel(ref m_canvasGroupTween);
		if (bActive)
		{
			base.gameObject.SetActive(true);
			m_canvasGroup.blocksRaycasts = true;
			RefreshMenu(true);
			m_canvasGroupTween = LeanTween.value(0f, 1f, 0.2f).setOnUpdate(delegate(float val)
			{
				m_canvasGroup.alpha = val;
			}).uniqueId;
		}
		else
		{
			m_canvasGroup.blocksRaycasts = false;
			m_canvasGroupTween = LeanTween.value(1f, 0f, 0.2f).setOnUpdate(delegate(float val)
			{
				m_canvasGroup.alpha = val;
			}).setOnComplete((Action)delegate
			{
				base.gameObject.SetActive(false);
			})
				.uniqueId;
		}
	}

	public void OnClickSortBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Sort", delegate(CharacterInfoSort ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup();
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				RefreshMenu();
			});
		});
	}

	public void OnClickSortDescendBtn()
	{
		bool sortDescend = !ManagedSingleton<CharacterHelper>.Instance.GetCharacterUISortDescend();
		CharacterHelper.SortType characterUISortType = ManagedSingleton<CharacterHelper>.Instance.GetCharacterUISortType();
		CharacterHelper.SortStatus characterUISortStatus = ManagedSingleton<CharacterHelper>.Instance.GetCharacterUISortStatus();
		ManagedSingleton<CharacterHelper>.Instance.SortCharacterList(characterUISortType, characterUISortStatus, sortDescend);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		RefreshMenu();
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}

	private void UpdateSortArrow()
	{
		if (ManagedSingleton<CharacterHelper>.Instance.GetCharacterUISortDescend())
		{
			m_sortArrowIcon.localRotation = new Quaternion(0f, 0f, 180f, 0f);
		}
		else
		{
			m_sortArrowIcon.localRotation = new Quaternion(0f, 0f, 0f, 0f);
		}
	}

	public bool IsSettinfFavorite()
	{
		if (OnFavoriteImg != null)
		{
			return OnFavoriteImg.gameObject.activeSelf;
		}
		return false;
	}

	public void OnSetFavorite()
	{
		if (OnFavoriteImg.gameObject.activeSelf)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
			OnFavoriteImg.gameObject.SetActive(false);
		}
		else
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
			OnFavoriteImg.gameObject.SetActive(true);
		}
		RefreshCells();
	}

	public void RefreshCells()
	{
		scrollRect.RefreshCells();
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref m_canvasGroupTween);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_SHOP, UpdateRefreashMenuFlag);
	}
}
