using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class WeaponMainUI : OrangeUIBase
{
	public LoopVerticalScrollRect tLVCR;

	public GameObject SortRoot;

	public Button[] WeaponType;

	public Button[] SortType;

	public Button[] GetTypeBtn;

	public GameObject SortOrderImg;

	private Image[] WeaponTypeImg;

	private Image[] SortTypeImg;

	private Image[] GetTypeBtnImg;

	public Image SortDialogMask;

	public Image OnFavoriteImg;

	private List<WeaponInfoUI.BtnClickCB> BtnClickCBs = new List<WeaponInfoUI.BtnClickCB>();

	[HideInInspector]
	public List<int> listHasWeapons = new List<int>();

	[HideInInspector]
	public List<int> listFragWeapons = new List<int>();

	private int TweenScaleId;

	private int nWeaponType;

	private int nSortType;

	private int nGetTypeBtn;

	private void Start()
	{
		BtnClickCBs.Clear();
		WeaponTypeImg = new Image[WeaponType.Length];
		for (int i = 0; i < WeaponType.Length; i++)
		{
			WeaponInfoUI.BtnClickCB btnClickCB = new WeaponInfoUI.BtnClickCB();
			btnClickCB.nBtnID = i;
			btnClickCB.action = (Action<int>)Delegate.Combine(btnClickCB.action, new Action<int>(SetWeaponType));
			WeaponType[i].onClick.AddListener(btnClickCB.OnClick);
			BtnClickCBs.Add(btnClickCB);
			WeaponTypeImg[i] = WeaponType[i].transform.Find("Image").GetComponent<Image>();
		}
		SortTypeImg = new Image[SortType.Length];
		for (int j = 0; j < SortType.Length; j++)
		{
			WeaponInfoUI.BtnClickCB btnClickCB2 = new WeaponInfoUI.BtnClickCB();
			btnClickCB2.nBtnID = j;
			btnClickCB2.action = (Action<int>)Delegate.Combine(btnClickCB2.action, new Action<int>(SetSortType));
			SortType[j].onClick.AddListener(btnClickCB2.OnClick);
			BtnClickCBs.Add(btnClickCB2);
			SortTypeImg[j] = SortType[j].transform.Find("Image").GetComponent<Image>();
		}
		GetTypeBtnImg = new Image[GetTypeBtn.Length];
		for (int k = 0; k < GetTypeBtn.Length; k++)
		{
			WeaponInfoUI.BtnClickCB btnClickCB3 = new WeaponInfoUI.BtnClickCB();
			btnClickCB3.nBtnID = k;
			btnClickCB3.action = (Action<int>)Delegate.Combine(btnClickCB3.action, new Action<int>(SetGetBtnType));
			GetTypeBtn[k].onClick.AddListener(btnClickCB3.OnClick);
			BtnClickCBs.Add(btnClickCB3);
			GetTypeBtnImg[k] = GetTypeBtn[k].transform.Find("Image").GetComponent<Image>();
		}
		for (int l = 0; l < SortType.Length; l++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nWeaponSortKey & (uint)(1 << l)) != 0)
			{
				SortTypeImg[l].gameObject.SetActive(true);
			}
			else
			{
				SortTypeImg[l].gameObject.SetActive(false);
			}
		}
		for (int m = 0; m < GetTypeBtn.Length; m++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nWeaponGetKey & (uint)(1 << m)) != 0)
			{
				GetTypeBtnImg[m].gameObject.SetActive(true);
			}
			else
			{
				GetTypeBtnImg[m].gameObject.SetActive(false);
			}
		}
		SortRoot.SetActive(false);
		if (OnFavoriteImg != null)
		{
			OnFavoriteImg.gameObject.SetActive(false);
		}
		if (TurtorialUI.IsTutorialing())
		{
			ManagedSingleton<EquipHelper>.Instance.WeaponSortDescend = 1;
			ManagedSingleton<EquipHelper>.Instance.nWeaponGetKey = EquipHelper.WEAPON_GET_TYPE.WEAPON_GETED;
		}
		if (TurtorialUI.IsTutorialing())
		{
			tLVCR.vertical = false;
		}
		ManagedSingleton<EquipHelper>.Instance.SortWeaponList();
		listHasWeapons = ManagedSingleton<EquipHelper>.Instance.GetUnlockedWeaponList();
		listFragWeapons = ManagedSingleton<EquipHelper>.Instance.GetFragmentWeaponList();
		int num = listHasWeapons.Count + listFragWeapons.Count;
		int num2 = (num - num % 6) / 6;
		if (num % 6 > 0)
		{
			num2++;
		}
		tLVCR.totalCount = num2;
		tLVCR.RefillCells();
		if (ManagedSingleton<EquipHelper>.Instance.WeaponSortDescend == 1)
		{
			SortOrderImg.transform.localScale = new Vector3(1f, -1f, 1f);
		}
		else
		{
			SortOrderImg.transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	public void OnClickSortGo()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		OnSortGo();
	}

	public void OnSortGo()
	{
		StartCoroutine(ObjScaleCoroutine(1f, 0f, 0.2f, SortRoot, delegate
		{
			SortRoot.SetActive(false);
		}));
		SortDialogMaskFade(0.5f, 0f, 0.2f);
		ManagedSingleton<EquipHelper>.Instance.SortWeaponList();
		listHasWeapons = ManagedSingleton<EquipHelper>.Instance.GetUnlockedWeaponList();
		listFragWeapons = ManagedSingleton<EquipHelper>.Instance.GetFragmentWeaponList();
		int num = listHasWeapons.Count + listFragWeapons.Count;
		int num2 = (num - num % 6) / 6;
		if (num % 6 > 0)
		{
			num2++;
		}
		tLVCR.totalCount = num2;
		tLVCR.RefillCells();
	}

	public void OnSortOrder()
	{
		ManagedSingleton<EquipHelper>.Instance.WeaponSortDescend = ((ManagedSingleton<EquipHelper>.Instance.WeaponSortDescend != 1) ? 1 : 0);
		if (ManagedSingleton<EquipHelper>.Instance.WeaponSortDescend == 1)
		{
			SortOrderImg.transform.localScale = new Vector3(1f, -1f, 1f);
		}
		else
		{
			SortOrderImg.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		OnSortGo();
	}

	public void OpenSort()
	{
		SortRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		StartCoroutine(ObjScaleCoroutine(0f, 1f, 0.2f, SortRoot, null));
		SortRoot.SetActive(true);
		SortDialogMaskFade(0f, 0.5f, 0.2f);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		nWeaponType = (int)ManagedSingleton<EquipHelper>.Instance.nWeaponSortType;
		nSortType = (int)ManagedSingleton<EquipHelper>.Instance.nWeaponSortKey;
		nGetTypeBtn = (int)ManagedSingleton<EquipHelper>.Instance.nWeaponGetKey;
		int[] array = new int[6]
		{
			OrangeConst.WEAPON_FILTER_1,
			OrangeConst.WEAPON_FILTER_2,
			OrangeConst.WEAPON_FILTER_3,
			OrangeConst.WEAPON_FILTER_4,
			OrangeConst.WEAPON_FILTER_5,
			0
		};
		for (int i = 0; i < WeaponType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nWeaponSortType & (uint)array[i]) == (uint)array[i])
			{
				WeaponTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				WeaponTypeImg[i].gameObject.SetActive(false);
			}
		}
		for (int j = 0; j < SortType.Length; j++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nWeaponSortKey & (uint)(1 << j)) != 0)
			{
				SortTypeImg[j].gameObject.SetActive(true);
			}
			else
			{
				SortTypeImg[j].gameObject.SetActive(false);
			}
		}
		for (int k = 0; k < GetTypeBtn.Length; k++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nWeaponGetKey & (uint)(1 << k)) != 0)
			{
				GetTypeBtnImg[k].gameObject.SetActive(true);
			}
			else
			{
				GetTypeBtnImg[k].gameObject.SetActive(false);
			}
		}
	}

	public void CloseSort()
	{
		ManagedSingleton<EquipHelper>.Instance.nWeaponSortType = (WeaponType)nWeaponType;
		ManagedSingleton<EquipHelper>.Instance.nWeaponSortKey = (EquipHelper.WEAPON_SORT_KEY)nSortType;
		ManagedSingleton<EquipHelper>.Instance.nWeaponGetKey = (EquipHelper.WEAPON_GET_TYPE)nGetTypeBtn;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		StartCoroutine(ObjScaleCoroutine(1f, 0f, 0.2f, SortRoot, delegate
		{
			SortRoot.SetActive(false);
		}));
		SortDialogMaskFade(0.5f, 0f, 0.2f);
	}

	public void SetWeaponType(int nBID)
	{
		int[] array = new int[6]
		{
			OrangeConst.WEAPON_FILTER_1,
			OrangeConst.WEAPON_FILTER_2,
			OrangeConst.WEAPON_FILTER_3,
			OrangeConst.WEAPON_FILTER_4,
			OrangeConst.WEAPON_FILTER_5,
			0
		};
		if (((uint)ManagedSingleton<EquipHelper>.Instance.nWeaponSortType & (uint)array[nBID]) == (uint)array[nBID])
		{
			ManagedSingleton<EquipHelper>.Instance.nWeaponSortType = (WeaponType)((int)ManagedSingleton<EquipHelper>.Instance.nWeaponSortType & ~array[nBID]);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		else
		{
			ManagedSingleton<EquipHelper>.Instance.nWeaponSortType = (WeaponType)((int)ManagedSingleton<EquipHelper>.Instance.nWeaponSortType | array[nBID]);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		for (int i = 0; i < WeaponType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nWeaponSortType & (uint)array[i]) == (uint)array[i])
			{
				WeaponTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				WeaponTypeImg[i].gameObject.SetActive(false);
			}
		}
	}

	public void SetSortType(int nBID)
	{
		int num = 1 << nBID;
		if (ManagedSingleton<EquipHelper>.Instance.nWeaponSortKey != (EquipHelper.WEAPON_SORT_KEY)num)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			ManagedSingleton<EquipHelper>.Instance.nWeaponSortKey = (EquipHelper.WEAPON_SORT_KEY)num;
		}
		for (int i = 0; i < SortType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nWeaponSortKey & (uint)(1 << i)) != 0)
			{
				SortTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				SortTypeImg[i].gameObject.SetActive(false);
			}
		}
	}

	public void SetGetBtnType(int nBID)
	{
		int num = 1 << nBID;
		if (ManagedSingleton<EquipHelper>.Instance.nWeaponGetKey != (EquipHelper.WEAPON_GET_TYPE)num)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			ManagedSingleton<EquipHelper>.Instance.nWeaponGetKey = (EquipHelper.WEAPON_GET_TYPE)num;
		}
		for (int i = 0; i < GetTypeBtn.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nWeaponGetKey & (uint)(1 << i)) != 0)
			{
				GetTypeBtnImg[i].gameObject.SetActive(true);
			}
			else
			{
				GetTypeBtnImg[i].gameObject.SetActive(false);
			}
		}
	}

	public void SortAtkPower()
	{
	}

	public bool IsSettingFavorite()
	{
		if (OnFavoriteImg != null)
		{
			return OnFavoriteImg.gameObject.activeSelf;
		}
		return false;
	}

	public void ReFresh()
	{
		tLVCR.RefreshCells();
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
		ReFresh();
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
	}

	public override void OnClickCloseBtn()
	{
		LeanTween.cancel(ref TweenScaleId);
		base.OnClickCloseBtn();
	}

	private void SortDialogMaskFade(float fStart, float fEnd, float fTime)
	{
		LeanTween.cancel(ref TweenScaleId);
		SortDialogMask.gameObject.SetActive(true);
		TweenScaleId = LeanTween.value(SortDialogMask.gameObject, fStart, fEnd, fTime).setOnUpdate(delegate(float alpha)
		{
			SortDialogMask.color = new Color(0f, 0f, 0f, alpha);
		}).setOnComplete((Action)delegate
		{
			TweenScaleId = -1;
			if (fEnd == 0f)
			{
				SortDialogMask.gameObject.SetActive(false);
			}
		})
			.uniqueId;
	}

	private IEnumerator ObjScaleCoroutine(float fStart, float fEnd, float fTime, GameObject tObj, Action endcb)
	{
		float fNowValue = fStart;
		float fLeftTime = fTime;
		float fD = (fEnd - fStart) / fTime;
		Vector3 nowScale = new Vector3(fNowValue, fNowValue, 1f);
		tObj.transform.localScale = nowScale;
		while (fLeftTime > 0f)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			float deltaTime = Time.deltaTime;
			fLeftTime -= deltaTime;
			fNowValue = (nowScale.y = (nowScale.x = fNowValue + fD * deltaTime));
			tObj.transform.localScale = nowScale;
		}
		nowScale.x = fEnd;
		nowScale.y = fEnd;
		tObj.transform.localScale = nowScale;
		if (endcb != null)
		{
			endcb();
		}
	}
}
