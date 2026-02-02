using System.Collections;
using StageLib;
using UnityEngine;
using UnityEngine.UI;

public class ObjInfoBar : PoolBaseObject
{
	public class BuffShowIcon
	{
		public StageLoadIcon RefObj;

		public StageLoadIcon RefObjMask;

		public Text CountText;

		public int nDuffID;

		public float fDuration;
	}

	public enum BAR_COLOR
	{
		RED_BAR = 0,
		GREEN_BAR = 1,
		BLUE_BAR = 2,
		ORANGE_BAR = 3
	}

	public FillSliceImg[] hpbar;

	public FillSliceImg[] hpbarbg;

	public GameObject spbg;

	public FillSliceImg spbar;

	public GameObject angerbg;

	public FillSliceImg angerbar;

	public GameObject[] heart;

	public OrangeText[] NameText;

	public GameObject[] markbg;

	public Text lvText;

	public StageLoadIcon BuffRefObj;

	public Sprite[] MeasureIcon;

	public int nMaxHP;

	public int nHP;

	public int nBackHP;

	public int nWillAdd;

	public int nBackHPBg;

	public int nWillAddBg;

	public int nActiveBar;

	public float fWillDelayTime = 0.3f;

	private float fDelayTime;

	private const float fSX = 130f;

	private const float fSY = -10f;

	private const float fSWD = 55f;

	private Vector3 tSetPosition = new Vector3(9999f, 9999f);

	private BuffShowIcon[] BuffShowIcons;

	private Canvas canvas;

	private bool isBarVisible = true;

	private void Awake()
	{
		canvas = GetComponent<Canvas>();
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_SETTING, UpdateSetting);
		UpdateSetting();
		if (BuffRefObj != null)
		{
			BuffRefObj.gameObject.SetActive(false);
		}
		StartCoroutine(StartStageCoroutine());
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_SETTING, UpdateSetting);
	}

	private void OnEnable()
	{
		if (canvas.worldCamera == null)
		{
			BattleFxCamera battleFxCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.GetBattleFxCamera();
			if ((bool)battleFxCamera)
			{
				canvas.worldCamera = battleFxCamera._camera;
			}
		}
		StartCoroutine(StartStageCoroutine());
	}

	private IEnumerator StartStageCoroutine()
	{
		while (MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera == null)
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		StageUpdate tStageUpdate = StageResManager.GetStageUpdate();
		while (!StageUpdate.gbStageReady && (!(tStageUpdate != null) || !tStageUpdate.IsEnd))
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		RectTransform rectTransform = (RectTransform)base.transform;
		float fCameraHHalf = ManagedSingleton<StageHelper>.Instance.fCameraHHalf;
		float num = ManagedSingleton<StageHelper>.Instance.fCameraWHalf * 2f / rectTransform.rect.width;
		if (num > fCameraHHalf * 2f / rectTransform.rect.height)
		{
			num = fCameraHHalf * 2f / rectTransform.rect.height;
		}
		rectTransform.localScale = new Vector3(num, num, num);
		UpdateCanvas();
	}

	private void LateUpdate()
	{
		if (fDelayTime < Time.realtimeSinceStartup)
		{
			if (nWillAdd < 0 && nBackHP < nHP)
			{
				nBackHP -= nWillAdd;
				if (nBackHP > nHP)
				{
					nWillAdd = 0;
					nBackHP = nHP;
					if (nBackHPBg < nBackHP)
					{
						nBackHPBg = nBackHP;
						hpbarbg[nActiveBar].SetFValue((float)nBackHPBg / (float)nMaxHP);
					}
				}
				hpbar[nActiveBar].SetFValue((float)nBackHP / (float)nMaxHP);
			}
			if (nWillAddBg > 0)
			{
				if (nBackHPBg > nBackHP)
				{
					nBackHPBg -= nWillAddBg;
					if (nBackHPBg < nBackHP)
					{
						nBackHPBg = nBackHP;
						nWillAddBg = 0;
					}
					hpbarbg[nActiveBar].SetFValue((float)nBackHPBg / (float)nMaxHP);
				}
			}
			else if (nWillAddBg < 0 && nBackHPBg < nBackHP)
			{
				nBackHPBg -= nWillAddBg;
				if (nBackHPBg > nBackHP)
				{
					nBackHPBg = nBackHP;
					nWillAddBg = 0;
				}
				hpbar[nActiveBar].SetFValue((float)nBackHPBg / (float)nMaxHP);
				hpbarbg[nActiveBar].SetFValue((float)nBackHPBg / (float)nMaxHP);
			}
		}
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		base.transform.rotation = Quaternion.Euler(0f, 0f, eulerAngles.z);
		base.transform.localPosition = tSetPosition;
	}

	public void SetEnemyBar(int nMaxHp, int nNowHP, string name)
	{
		nMaxHP = nMaxHp;
		nHP = nNowHP;
		nBackHPBg = (nBackHP = nHP);
		base.gameObject.SetActive(true);
		OpenBar(0, string.Empty);
		spbg.SetActive(false);
		angerbg.SetActive(false);
		for (int i = 0; i < heart.Length; i++)
		{
			heart[i].SetActive(false);
		}
		markbg[0].SetActive(false);
		markbg[1].SetActive(true);
	}

	public void SetPlayBar(int nMaxHp, int nNowHP, string name, int nLV, BAR_COLOR barColor = BAR_COLOR.GREEN_BAR)
	{
		nMaxHP = nMaxHp;
		nHP = nNowHP;
		nBackHPBg = (nBackHP = nHP);
		base.gameObject.SetActive(true);
		OpenBar((int)barColor, name);
		spbg.SetActive(false);
		angerbg.SetActive(false);
		for (int i = 0; i < heart.Length; i++)
		{
			heart[i].SetActive(false);
		}
		markbg[0].SetActive(!StageUpdate.gbRegisterPvpPlayer);
		markbg[1].SetActive(false);
		lvText.text = nLV.ToString();
	}

	private void OpenBar(int nI, string name)
	{
		for (int i = 0; i < hpbar.Length; i++)
		{
			if (i == nI)
			{
				hpbarbg[i].gameObject.SetActive(true);
				NameText[i].text = name;
				nActiveBar = i;
				hpbar[i].SetFValue((float)nHP / (float)nMaxHP);
				hpbarbg[i].SetFValue((float)nHP / (float)nMaxHP);
			}
			else
			{
				hpbarbg[i].gameObject.SetActive(false);
				NameText[i].text = string.Empty;
			}
		}
	}

	public void HurtCB(StageObjBase tSOB)
	{
		nMaxHP = tSOB.MaxHp;
		int num = nHP;
		nHP = tSOB.Hp;
		num = nHP - num;
		if (nHP <= 0)
		{
			hpbar[nActiveBar].SetFValue(0f);
			tSOB.HurtActions -= HurtCB;
			tSetPosition = new Vector3(9999f, 9999f);
			tSOB.selfBuffManager.UpdateBuffBar -= UpdateBuffCB;
			base.transform.SetParent(null);
			base.gameObject.SetActive(false);
			StageResManager.RemoveInfoBar(this);
			return;
		}
		fDelayTime = Time.realtimeSinceStartup + fWillDelayTime;
		if (num == 0)
		{
			return;
		}
		if (num < 0)
		{
			hpbar[nActiveBar].SetFValue((float)nHP / (float)nMaxHP);
			nBackHP = nHP;
			nWillAdd = 0;
		}
		else if (nBackHP < nHP)
		{
			nWillAdd = (nBackHP - nHP) / 10;
			fDelayTime = Time.realtimeSinceStartup - 1f;
		}
		if (num < 0)
		{
			nWillAddBg = (nBackHPBg - nHP) / 10;
			if (nWillAddBg < 1)
			{
				nWillAddBg = 1;
			}
		}
		else if (num > 0 && nBackHPBg < nHP)
		{
			nWillAddBg = (nBackHPBg - nHP) / 10;
			if (nWillAddBg > -1)
			{
				nWillAddBg = -1;
			}
		}
	}

	public void RemoveBar(OrangeCharacter tOC)
	{
		if (tOC != null)
		{
			tOC.HurtActions -= HurtCB;
			tOC.selfBuffManager.UpdateBuffBar -= UpdateBuffCB;
		}
	}

	public void RemoveBar(RideArmorController tPlayer)
	{
		if (tPlayer != null)
		{
			tPlayer.HurtActions -= HurtCB;
			tPlayer.selfBuffManager.UpdateBuffBar -= UpdateBuffCB;
		}
	}

	public void InitBuff(Vector3 tSPos)
	{
		if (BuffShowIcons == null)
		{
			BuffShowIcons = new BuffShowIcon[1];
			BuffShowIcons[0] = new BuffShowIcon();
			BuffShowIcons[0].RefObj = BuffRefObj;
			BuffShowIcons[0].RefObjMask = BuffShowIcons[0].RefObj.transform.Find("BuffMask").GetComponent<StageLoadIcon>();
			BuffShowIcons[0].CountText = BuffShowIcons[0].RefObj.transform.Find("CountBG/CountText").GetComponent<Text>();
		}
		if (BuffRefObj.transform.parent.childCount > BuffShowIcons.Length)
		{
			BuffShowIcon[] array = new BuffShowIcon[BuffRefObj.transform.parent.childCount];
			for (int i = 0; i < BuffRefObj.transform.parent.childCount; i++)
			{
				array[i] = new BuffShowIcon();
				array[i].RefObj = BuffRefObj.transform.parent.GetChild(i).GetComponent<StageLoadIcon>();
				array[i].RefObjMask = array[i].RefObj.transform.Find("BuffMask").GetComponent<StageLoadIcon>();
				array[i].CountText = array[i].RefObj.transform.Find("CountBG/CountText").GetComponent<Text>();
			}
			for (int j = 0; j < BuffShowIcons.Length; j++)
			{
				if (BuffShowIcons[j].RefObj == BuffRefObj)
				{
					BuffShowIcon buffShowIcon = BuffShowIcons[0];
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
		tSetPosition = tSPos;
	}

	public void UpdateBuffCB(PerBuffManager refPBM)
	{
		int count = refPBM.listBuffs.Count;
		if (BuffShowIcons.Length < refPBM.listBuffs.Count)
		{
			BuffShowIcon[] array = new BuffShowIcon[refPBM.listBuffs.Count];
			for (int i = 0; i < BuffShowIcons.Length; i++)
			{
				array[i] = BuffShowIcons[i];
			}
			for (int j = BuffShowIcons.Length; j < refPBM.listBuffs.Count; j++)
			{
				array[j] = new BuffShowIcon();
				array[j].RefObj = Object.Instantiate(BuffRefObj, Vector3.zero, Quaternion.identity, BuffRefObj.transform.parent);
				array[j].RefObjMask = array[j].RefObj.transform.Find("BuffMask").GetComponent<StageLoadIcon>();
				array[j].CountText = array[j].RefObj.transform.Find("CountBG/CountText").GetComponent<Text>();
			}
			BuffShowIcons = array;
		}
		int num = 0;
		int num2 = 0;
		bool flag = false;
		float num3 = 0f;
		for (int k = 0; k < count; k++)
		{
			PerBuff perBuff = refPBM.listBuffs[k];
			if (perBuff.bWaitNetDel)
			{
				BuffShowIcons[k].RefObj.gameObject.SetActive(false);
				continue;
			}
			BuffShowIcons[k].fDuration = perBuff.fDuration;
			BuffShowIcons[k].nDuffID = perBuff.refCTable.n_ID;
			num3 = perBuff.refCTable.n_DURATION;
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp && perBuff.refCTable.n_EFFECT == 107)
			{
				num3 = (float)perBuff.refCTable.n_DURATION * (float)OrangeConst.PVP_STUN_MODIFY / 100f;
			}
			BuffShowIcons[k].RefObjMask.fillAmount = (num3 - perBuff.fDuration) / num3;
			if (perBuff.refCTable.s_ICON != null && perBuff.refCTable.s_ICON != "" && perBuff.refCTable.s_ICON != "null")
			{
				BuffShowIcons[k].RefObj.gameObject.SetActive(true);
				BuffShowIcons[k].RefObj.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(perBuff.refCTable.s_ICON), perBuff.refCTable.s_ICON);
				BuffShowIcons[k].RefObjMask.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(perBuff.refCTable.s_ICON), perBuff.refCTable.s_ICON);
				if (perBuff.refCTable.n_EFFECT == 6)
				{
					BuffShowIcons[k].CountText.text = string.Empty;
					flag = true;
				}
				else
				{
					BuffShowIcons[k].CountText.text = perBuff.nStack.ToString();
				}
				BuffShowIcons[k].RefObj.transform.localPosition = new Vector3(130f + 55f * (float)num, -10f - 55f * (float)num2);
				num++;
				if (num == 4)
				{
					num = 0;
					num2++;
				}
			}
			else
			{
				BuffShowIcons[k].RefObj.gameObject.SetActive(false);
			}
		}
		for (int l = count; l < BuffShowIcons.Length; l++)
		{
			BuffShowIcons[l].RefObj.gameObject.SetActive(false);
		}
		float num4 = -31f;
		if (flag)
		{
			spbg.SetActive(true);
			Vector3 localPosition = spbg.transform.localPosition;
			localPosition.y = num4;
			spbg.transform.localPosition = localPosition;
			spbar.SetFValue((float)refPBM.sBuffStatus.nEnergyShield / (float)refPBM.sBuffStatus.nEnergyShieldMax);
			num4 -= 20f;
		}
		else
		{
			spbg.SetActive(false);
		}
		if (refPBM.nMeasureMax > 0)
		{
			angerbg.SetActive(true);
			Vector3 localPosition = angerbg.transform.localPosition;
			localPosition.y = num4;
			angerbg.transform.localPosition = localPosition;
			SKILL_TABLE sklTable;
			if (refPBM.SOB as OrangeCharacter != null && refPBM.sBuffStatus.refPS.IsSpecialCount(out sklTable))
			{
				GameObject[] array2 = heart;
				for (int m = 0; m < array2.Length; m++)
				{
					Image component = array2[m].GetComponent<Image>();
					if ((bool)component)
					{
						component.sprite = MeasureIcon[sklTable.n_TRIGGER_Y];
					}
				}
				angerbar.transform.parent.gameObject.SetActive(false);
				int num5 = 0;
				for (num5 = 0; num5 < refPBM.nMeasureNow && num5 < 5; num5++)
				{
					heart[num5].SetActive(true);
				}
				for (; num5 < 5; num5++)
				{
					heart[num5].SetActive(false);
				}
			}
			else
			{
				angerbar.transform.parent.gameObject.SetActive(true);
				angerbar.SetFValue((float)refPBM.nMeasureNow / (float)refPBM.nMeasureMax);
			}
		}
		else
		{
			angerbg.SetActive(false);
		}
	}

	public void ForceSetNewPosition(Vector3 newPos)
	{
		tSetPosition = newPos;
	}

	public void UpdateSetting()
	{
		isBarVisible = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.HpVisible == 1;
		UpdateCanvas();
	}

	private void UpdateCanvas()
	{
		if (canvas.isActiveAndEnabled != isBarVisible)
		{
			canvas.enabled = isBarVisible;
		}
	}
}
