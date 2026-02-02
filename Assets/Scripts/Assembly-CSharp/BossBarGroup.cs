using System;
using System.Collections;
using StageLib;
using UnityEngine;
using UnityEngine.UI;

public class BossBarGroup
{
	public Canvas BossTopBar;

	public Text barcountText;

	public Image[] barnotone;

	public Image[] barnotoneani;

	public Image barone;

	public Image baroneani;

	public StageLoadIcon BossIconImage;

	public StageLoadIcon BuffRefObj;

	private ObjInfoBar.BuffShowIcon[] BuffShowIcons;

	public int nLine = 1;

	public int nNowCount;

	public int nOneLineNum;

	public int nMaxValue = 10;

	public int nNowValue = 10;

	private bool _bFullHpEnd;

	private int nNowBarID;

	private float fTimeLastHit;

	public Coroutine AddHpCoroutine;

	public Action AddHpFullCB;

	private StageObjBase tLinkSOB;

	public bool bDeadHiden = true;

	public bool bFullHpEnd
	{
		get
		{
			return _bFullHpEnd;
		}
		set
		{
			_bFullHpEnd = value;
		}
	}

	public int LinkSOBHp
	{
		get
		{
			if (tLinkSOB != null)
			{
				return tLinkSOB.Hp;
			}
			return nMaxValue;
		}
	}

	public BossBarGroup(Canvas tBossTopBar)
	{
		AutoLinkLib.AutoLinkVar(this, typeof(BossBarGroup), tBossTopBar.transform);
		BossTopBar = tBossTopBar;
		BossTopBar.enabled = false;
		InitBuff();
	}

	public void InitBar(StageObjBase tSOB, int nline, int MaxValue, int NowValue, string sModle = "")
	{
		tLinkSOB = tSOB;
		BossIconImage.CheckLoad(AssetBundleScriptableObject.Instance.m_iconChip, sModle);
		nNowBarID = 0;
		nNowCount = 0;
		barcountText.text = "x" + nNowCount;
		nMaxValue = MaxValue;
		nNowValue = 0;
		nOneLineNum = MaxValue / nline;
		int num = MaxValue % nOneLineNum;
		while (num > 0)
		{
			num -= nline;
			nOneLineNum++;
		}
		float num2 = 1f;
		num2 = 0f;
		for (int i = 0; i < barnotone.Length; i++)
		{
			barnotone[i].fillAmount = 0f;
			barnotoneani[i].fillAmount = 0f;
		}
		barone.fillAmount = 0f;
		baroneani.fillAmount = 0f;
		if (nNowCount > 1)
		{
			for (int j = 0; j < barnotoneani.Length; j++)
			{
				barnotoneani[j].gameObject.SetActive(false);
			}
			barnotoneani[nNowBarID].gameObject.SetActive(true);
			barnotoneani[nNowBarID + 1].gameObject.SetActive(true);
			barone.gameObject.SetActive(false);
			barnotone[nNowBarID].fillAmount = num2;
			barnotone[nNowBarID + 1].fillAmount = 1f;
			barnotoneani[nNowBarID].fillAmount = num2;
			barnotoneani[nNowBarID + 1].fillAmount = 1f;
		}
		else if (nNowCount == 1)
		{
			for (int k = 0; k < barnotoneani.Length; k++)
			{
				barnotoneani[k].gameObject.SetActive(false);
			}
			barnotoneani[nNowBarID].gameObject.SetActive(true);
			baroneani.gameObject.SetActive(true);
			barnotone[nNowBarID].fillAmount = num2;
			barnotoneani[nNowBarID].fillAmount = num2;
			barone.fillAmount = 1f;
			baroneani.fillAmount = 1f;
		}
		else
		{
			for (int l = 0; l < barnotone.Length; l++)
			{
				barnotoneani[l].gameObject.SetActive(false);
			}
			baroneani.gameObject.SetActive(true);
			barone.fillAmount = num2;
			baroneani.fillAmount = num2;
		}
		BossTopBar.enabled = true;
		tSOB.HurtActions += HurtCB;
		tSOB.selfBuffManager.UpdateBuffBar += UpdateBuffCB;
		UpdateBuffCB(tSOB.selfBuffManager);
	}

	public void InitBuff()
	{
		if (BuffShowIcons == null)
		{
			BuffShowIcons = new ObjInfoBar.BuffShowIcon[1];
			BuffShowIcons[0] = new ObjInfoBar.BuffShowIcon();
			BuffShowIcons[0].RefObj = BossTopBar.transform.Find("BuffRoot/Buff").GetComponent<StageLoadIcon>();
			BuffRefObj = BuffShowIcons[0].RefObj;
			BuffShowIcons[0].RefObjMask = BuffShowIcons[0].RefObj.transform.Find("BuffMask").GetComponent<StageLoadIcon>();
			BuffShowIcons[0].CountText = BuffShowIcons[0].RefObj.transform.Find("CountBG/CountText").GetComponent<Text>();
		}
		if (BuffRefObj.transform.parent.childCount > BuffShowIcons.Length)
		{
			ObjInfoBar.BuffShowIcon[] array = new ObjInfoBar.BuffShowIcon[BuffRefObj.transform.parent.childCount];
			for (int i = 0; i < BuffRefObj.transform.parent.childCount; i++)
			{
				array[i] = new ObjInfoBar.BuffShowIcon();
				array[i].RefObj = BuffRefObj.transform.parent.GetChild(i).GetComponent<StageLoadIcon>();
				array[i].RefObjMask = array[i].RefObj.transform.Find("BuffMask").GetComponent<StageLoadIcon>();
				array[i].CountText = array[i].RefObj.transform.Find("CountBG/CountText").GetComponent<Text>();
			}
			for (int j = 0; j < BuffShowIcons.Length; j++)
			{
				if (BuffShowIcons[j].RefObj == BuffRefObj)
				{
					ObjInfoBar.BuffShowIcon buffShowIcon = BuffShowIcons[0];
					BuffShowIcons[0] = BuffShowIcons[j];
					BuffShowIcons[j] = buffShowIcon;
					break;
				}
			}
			BuffShowIcons = array;
		}
		for (int k = 0; k < BuffShowIcons.Length; k++)
		{
			BuffShowIcons[k].RefObj.gameObject.SetActive(false);
		}
	}

	public void CheckAniBar()
	{
		if (!BossTopBar.enabled || !(Time.realtimeSinceStartup - fTimeLastHit > 1f))
		{
			return;
		}
		if (nNowCount > 1)
		{
			if (barnotone[nNowBarID].fillAmount > barnotoneani[nNowBarID].fillAmount)
			{
				barnotoneani[nNowBarID].fillAmount = barnotone[nNowBarID].fillAmount;
			}
			else
			{
				barnotoneani[nNowBarID].fillAmount -= 0.01f;
			}
		}
		else if (nNowCount == 1)
		{
			if (barnotone[nNowBarID].fillAmount > barnotoneani[nNowBarID].fillAmount)
			{
				barnotoneani[nNowBarID].fillAmount = barnotone[nNowBarID].fillAmount;
			}
			else
			{
				barnotoneani[nNowBarID].fillAmount -= 0.01f;
			}
		}
		else if (barone.fillAmount > baroneani.fillAmount)
		{
			baroneani.fillAmount = barone.fillAmount;
		}
		else
		{
			baroneani.fillAmount -= 0.01f;
			baroneani.fillAmount = baroneani.fillAmount;
		}
	}

	public IEnumerator FullBossHpBar(int nTime, int tHP)
	{
		_bFullHpEnd = false;
		nTime = 200;
		float ndivhp = (float)tHP / (float)nTime;
		if (ndivhp <= 0f)
		{
			ndivhp = 1f;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(BattleSE.CRI_BATTLESE_BT_BOSSHP01_LP);
		while (nNowValue < tHP)
		{
			int num = (int)(Time.deltaTime * 100f * ndivhp);
			if (nNowValue + num > tHP)
			{
				num = tHP - nNowValue;
			}
			else if (num <= 0 && tHP > 0 && nNowValue + 1 <= tHP)
			{
				num = 1;
			}
			AddValue(num);
			yield return CoroutineDefine._waitForEndOfFrame;
			while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(BattleSE.CRI_BATTLESE_BT_BOSSHP01_STOP);
		if (tHP > nNowValue)
		{
			AddValue(tHP - nNowValue);
		}
		nNowValue = tHP;
		_bFullHpEnd = true;
		if (AddHpFullCB != null)
		{
			AddHpFullCB();
		}
		for (int i = 0; i < barnotone.Length; i++)
		{
			barnotoneani[i].fillAmount = barnotone[i].fillAmount;
		}
		baroneani.fillAmount = barone.fillAmount;
		AddHpCoroutine = null;
	}

	public void AddValue(int nAddValue)
	{
		nNowValue += nAddValue;
		if (nNowValue <= 0)
		{
			nNowValue = 0;
			nNowCount = 0;
			barcountText.text = "x0";
			if (bDeadHiden)
			{
				BossTopBar.enabled = false;
				if (BattleInfoUI.Instance != null)
				{
					BattleInfoUI.Instance.UpdateBossBarPos();
				}
				return;
			}
			for (int i = 0; i < barnotone.Length; i++)
			{
				barnotoneani[i].gameObject.SetActive(false);
			}
			if (!baroneani.gameObject.activeSelf)
			{
				baroneani.fillAmount = 1f;
			}
			baroneani.gameObject.SetActive(true);
			if (baroneani.fillAmount <= barone.fillAmount)
			{
				fTimeLastHit = Time.realtimeSinceStartup;
			}
			barone.fillAmount = 0f;
			return;
		}
		float fillAmount = 1f;
		if (nAddValue < 0)
		{
			int num = nNowValue - nNowCount * nOneLineNum;
			while (num < 0)
			{
				num += nOneLineNum;
				nNowCount--;
				nNowBarID++;
				if (nNowBarID > barnotone.Length - 2)
				{
					nNowBarID = 0;
				}
				barcountText.text = "x" + nNowCount;
			}
			if (num < nOneLineNum)
			{
				fillAmount = (float)num / (float)nOneLineNum;
			}
		}
		else
		{
			int num2 = nNowValue;
			int num3 = -1;
			while (num2 > 0)
			{
				num2 -= nOneLineNum;
				num3++;
			}
			nNowBarID -= num3 - nNowCount;
			while (nNowBarID < 0)
			{
				nNowBarID += barnotone.Length - 1;
			}
			nNowCount = num3;
			barcountText.text = "x" + nNowCount;
			num2 += nOneLineNum;
			if (num2 < nOneLineNum)
			{
				fillAmount = (float)num2 / (float)nOneLineNum;
			}
			for (int j = 0; j < barnotone.Length; j++)
			{
				barnotone[j].fillAmount = 1f;
			}
		}
		if (nNowCount > 1)
		{
			for (int k = 0; k < barnotone.Length; k++)
			{
				barnotoneani[k].gameObject.SetActive(false);
			}
			barnotoneani[nNowBarID].gameObject.SetActive(true);
			barnotoneani[nNowBarID + 1].gameObject.SetActive(true);
			baroneani.gameObject.SetActive(false);
			if (barnotoneani[nNowBarID].fillAmount <= barnotone[nNowBarID].fillAmount)
			{
				fTimeLastHit = Time.realtimeSinceStartup;
			}
			barnotone[nNowBarID].fillAmount = fillAmount;
			barnotone[nNowBarID + 1].fillAmount = 1f;
		}
		else if (nNowCount == 1)
		{
			for (int l = 0; l < barnotone.Length; l++)
			{
				barnotoneani[l].gameObject.SetActive(false);
			}
			barnotoneani[nNowBarID].gameObject.SetActive(true);
			baroneani.gameObject.SetActive(true);
			if (barnotoneani[nNowBarID].fillAmount <= barnotone[nNowBarID].fillAmount)
			{
				fTimeLastHit = Time.realtimeSinceStartup;
			}
			barnotone[nNowBarID].fillAmount = fillAmount;
			barone.fillAmount = 1f;
		}
		else
		{
			for (int m = 0; m < barnotone.Length; m++)
			{
				barnotoneani[m].gameObject.SetActive(false);
			}
			baroneani.gameObject.SetActive(true);
			if (baroneani.fillAmount <= barone.fillAmount)
			{
				fTimeLastHit = Time.realtimeSinceStartup;
			}
			barone.fillAmount = fillAmount;
		}
	}

	public void HurtCB(StageObjBase tSOB)
	{
		if ((int)tSOB.Hp <= 0)
		{
			tSOB.HurtActions -= HurtCB;
		}
		if (nNowValue != (int)tSOB.Hp)
		{
			AddValue((int)tSOB.Hp - nNowValue);
		}
	}

	public void UpdateBuffCB(PerBuffManager refPBM)
	{
		int count = refPBM.listBuffs.Count;
		if (BuffShowIcons.Length < refPBM.listBuffs.Count)
		{
			ObjInfoBar.BuffShowIcon[] array = new ObjInfoBar.BuffShowIcon[refPBM.listBuffs.Count];
			for (int i = 0; i < BuffShowIcons.Length; i++)
			{
				array[i] = BuffShowIcons[i];
			}
			for (int j = BuffShowIcons.Length; j < refPBM.listBuffs.Count; j++)
			{
				array[j] = new ObjInfoBar.BuffShowIcon();
				array[j].RefObj = UnityEngine.Object.Instantiate(BuffRefObj, Vector3.zero, Quaternion.identity, BuffRefObj.transform.parent);
				array[j].RefObjMask = array[j].RefObj.transform.Find("BuffMask").GetComponent<StageLoadIcon>();
				array[j].CountText = array[j].RefObj.transform.Find("CountBG/CountText").GetComponent<Text>();
			}
			BuffShowIcons = array;
		}
		int num = 0;
		for (int k = 0; k < count; k++)
		{
			if (refPBM.listBuffs[k].refCTable.s_ICON == "null")
			{
				BuffShowIcons[k].RefObj.gameObject.SetActive(false);
				continue;
			}
			BuffShowIcons[k].RefObj.gameObject.SetActive(true);
			BuffShowIcons[k].RefObj.CheckLoad(AssetBundleScriptableObject.Instance.GetIconSkill(refPBM.listBuffs[k].refCTable.s_ICON), refPBM.listBuffs[k].refCTable.s_ICON);
			BuffShowIcons[k].RefObjMask.CheckLoad(AssetBundleScriptableObject.Instance.GetIconSkill(refPBM.listBuffs[k].refCTable.s_ICON), refPBM.listBuffs[k].refCTable.s_ICON);
			BuffShowIcons[k].fDuration = refPBM.listBuffs[k].fDuration;
			BuffShowIcons[k].nDuffID = refPBM.listBuffs[k].refCTable.n_ID;
			BuffShowIcons[k].RefObjMask.fillAmount = ((float)refPBM.listBuffs[k].refCTable.n_DURATION - refPBM.listBuffs[k].fDuration) / (float)refPBM.listBuffs[k].refCTable.n_DURATION;
			BuffShowIcons[k].CountText.text = refPBM.listBuffs[k].nStack.ToString();
			BuffShowIcons[k].RefObj.transform.localPosition = new Vector3(0f, -58f * (float)num);
			num++;
		}
		for (int l = count; l < BuffShowIcons.Length; l++)
		{
			BuffShowIcons[l].RefObj.gameObject.SetActive(false);
		}
	}
}
