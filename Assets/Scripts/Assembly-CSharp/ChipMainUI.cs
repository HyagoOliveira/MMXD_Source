using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class ChipMainUI : OrangeUIBase
{
	public LoopVerticalScrollRect tLVSR;

	public GameObject SortRoot;

	public Button[] WeaponType;

	public Button[] SortType;

	public Button[] GetTypeBtn;

	public GameObject SortOrderImg;

	private Image[] WeaponTypeImg;

	private Image[] SortTypeImg;

	private Image[] GetTypeBtnImg;

	private bool bSortOrderDescending = true;

	public Image SortDialogMask;

	private List<WeaponInfoUI.BtnClickCB> BtnClickCBs = new List<WeaponInfoUI.BtnClickCB>();

	[HideInInspector]
	public List<int> listHasChips = new List<int>();

	[HideInInspector]
	public List<int> listFragChips = new List<int>();

	private int nWeaponType;

	private int nSortType;

	private int nGetTypeBtn;

	private int TweenScaleId;

	private void Start()
	{
		Setup();
	}

	private void Setup()
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
			if (i == 5)
			{
				WeaponType[i].gameObject.SetActive(false);
			}
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
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nChipSortKey & (uint)(1 << l)) != 0)
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
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nChipGetKey & (uint)(1 << m)) != 0)
			{
				GetTypeBtnImg[m].gameObject.SetActive(true);
			}
			else
			{
				GetTypeBtnImg[m].gameObject.SetActive(false);
			}
		}
		SortRoot.SetActive(false);
		if (TurtorialUI.IsTutorialing())
		{
			tLVSR.vertical = false;
		}
		SortChips();
		InitLVSR();
		if (ManagedSingleton<EquipHelper>.Instance.bChipSortDescend)
		{
			SortOrderImg.transform.localScale = new Vector3(1f, -1f, 1f);
		}
		else
		{
			SortOrderImg.transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	public void InitLVSR()
	{
		ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.GetEnumerator();
		int num = listHasChips.Count + listFragChips.Count;
		int num2 = (num - num % 6) / 6;
		if (num % 6 > 0)
		{
			num2++;
		}
		tLVSR.totalCount = num2;
		tLVSR.RefillCells();
	}

	public void SortGo()
	{
		StartCoroutine(ObjScaleCoroutine(1f, 0f, 0.2f, SortRoot, delegate
		{
			SortRoot.SetActive(false);
		}));
		SortDialogMaskFade(0.5f, 0f, 0.2f);
		SortChips();
		int num = listHasChips.Count + listFragChips.Count;
		int num2 = (num - num % 6) / 6;
		if (num % 6 > 0)
		{
			num2++;
		}
		tLVSR.totalCount = num2;
		tLVSR.RefillCells();
	}

	public void OnClickSortGo()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		SortGo();
	}

	public void OnSortOrder()
	{
		ManagedSingleton<EquipHelper>.Instance.bChipSortDescend = !ManagedSingleton<EquipHelper>.Instance.bChipSortDescend;
		if (ManagedSingleton<EquipHelper>.Instance.bChipSortDescend)
		{
			SortOrderImg.transform.localScale = new Vector3(1f, -1f, 1f);
		}
		else
		{
			SortOrderImg.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		SortGo();
	}

	public void OpenSort()
	{
		SortRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		StartCoroutine(ObjScaleCoroutine(0f, 1f, 0.2f, SortRoot, null));
		SortRoot.SetActive(true);
		SortDialogMaskFade(0f, 0.5f, 0.2f);
		nWeaponType = (int)ManagedSingleton<EquipHelper>.Instance.nChipSortType;
		nSortType = (int)ManagedSingleton<EquipHelper>.Instance.nChipSortKey;
		nGetTypeBtn = (int)ManagedSingleton<EquipHelper>.Instance.nChipGetKey;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		int[] array = new int[6]
		{
			OrangeConst.WEAPON_FILTER_1,
			OrangeConst.WEAPON_FILTER_2,
			OrangeConst.WEAPON_FILTER_3,
			OrangeConst.WEAPON_FILTER_4,
			OrangeConst.WEAPON_FILTER_5,
			0
		};
		ManagedSingleton<EquipHelper>.Instance.nChipSortType = ManagedSingleton<EquipHelper>.Instance.nChipSortType & (WeaponType)(-17);
		for (int i = 0; i < WeaponType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nChipSortType & (uint)array[i]) == (uint)array[i])
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
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nChipSortKey & (uint)(1 << j)) != 0)
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
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nChipGetKey & (uint)(1 << k)) != 0)
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
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ManagedSingleton<EquipHelper>.Instance.nChipSortType = (WeaponType)nWeaponType;
		ManagedSingleton<EquipHelper>.Instance.nChipSortKey = (EquipHelper.WEAPON_SORT_KEY)nSortType;
		ManagedSingleton<EquipHelper>.Instance.nChipGetKey = (EquipHelper.WEAPON_GET_TYPE)nGetTypeBtn;
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
		if (((uint)ManagedSingleton<EquipHelper>.Instance.nChipSortType & (uint)array[nBID]) == (uint)array[nBID])
		{
			ManagedSingleton<EquipHelper>.Instance.nChipSortType = (WeaponType)((int)ManagedSingleton<EquipHelper>.Instance.nChipSortType & ~array[nBID]);
		}
		else
		{
			ManagedSingleton<EquipHelper>.Instance.nChipSortType = (WeaponType)((int)ManagedSingleton<EquipHelper>.Instance.nChipSortType | array[nBID]);
		}
		for (int i = 0; i < WeaponType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nChipSortType & (uint)array[i]) == (uint)array[i])
			{
				WeaponTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				WeaponTypeImg[i].gameObject.SetActive(false);
			}
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
	}

	public void SetSortType(int nBID)
	{
		int num = 1 << nBID;
		if (ManagedSingleton<EquipHelper>.Instance.nChipSortKey != (EquipHelper.WEAPON_SORT_KEY)num)
		{
			ManagedSingleton<EquipHelper>.Instance.nChipSortKey = (EquipHelper.WEAPON_SORT_KEY)num;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		for (int i = 0; i < SortType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nChipSortKey & (uint)(1 << i)) != 0)
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
		if (ManagedSingleton<EquipHelper>.Instance.nChipGetKey != (EquipHelper.WEAPON_GET_TYPE)num)
		{
			ManagedSingleton<EquipHelper>.Instance.nChipGetKey = (EquipHelper.WEAPON_GET_TYPE)num;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		for (int i = 0; i < GetTypeBtn.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nChipGetKey & (uint)(1 << i)) != 0)
			{
				GetTypeBtnImg[i].gameObject.SetActive(true);
			}
			else
			{
				GetTypeBtnImg[i].gameObject.SetActive(false);
			}
		}
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
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

	public void SortChips()
	{
		ManagedSingleton<EquipHelper>.Instance.SortChipList();
		listHasChips = ManagedSingleton<EquipHelper>.Instance.GetUnlockedChipList();
		listFragChips = ManagedSingleton<EquipHelper>.Instance.GetFragmentChipList();
	}
}
