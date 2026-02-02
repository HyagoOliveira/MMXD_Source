using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HometopResidentUI : MonoBehaviour
{
	[SerializeField]
	protected OrangeText textEnergy;

	[SerializeField]
	protected OrangeText textEnergyTime;

	[SerializeField]
	protected OrangeText textGmoney;

	[SerializeField]
	protected OrangeText textMoney;

	[SerializeField]
	private OrangeText textLv;

	[SerializeField]
	private Image imgExp;

	[SerializeField]
	private Image imgEnergyBar;

	public static int EnergyRecoverDiff;

	private int staminaNow;

	private int staminaLimit;

	private int recoverTrigger;

	private int accumuler;

	private int hour;

	private int minutes;

	private int seconds;

	private string tFormat1 = ":";

	private string tFormat2 = "D2";

	private string[] arrEnergyFormat = new string[2] { "<color=#FFFFFF>{0}/{1}</color>", "<color=#5DDEF4>{0}</color>/{1}" };

	private void OnEnable()
	{
		recoverTrigger = 60 * OrangeConst.AP_RECOVER_TIME;
		StartCoroutine(OnStartUpdateEnergyTime());
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_TOPBAR_DATA, UpdateValue);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_TOPBAR_DATA, UpdateValue);
	}

	public void UpdateValue()
	{
		UpdateEnergyVal();
		textMoney.text = ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel().ToString("#,0");
		textGmoney.text = ManagedSingleton<PlayerHelper>.Instance.GetZenny().ToString("#,0");
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		int exp = ManagedSingleton<PlayerHelper>.Instance.GetExp();
		textLv.text = lV.ToString();
		EXP_TABLE value = null;
		EXP_TABLE value2 = null;
		ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(lV - 1, out value2);
		ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(lV, out value);
		if (value == null)
		{
			imgExp.fillAmount = 1f;
			return;
		}
		float fillAmount = (float)(exp - value2.n_TOTAL_RANKEXP) / (float)value.n_RANKEXP;
		imgExp.fillAmount = fillAmount;
	}

	private IEnumerator OnStartUpdateEnergyTime()
	{
		while (true)
		{
			if (EnergyRecoverDiff <= 0)
			{
				textEnergyTime.text = string.Empty;
				yield return CoroutineDefine._1sec;
				continue;
			}
			hour = (int)((float)EnergyRecoverDiff / 3600f) % 60;
			minutes = (int)((float)EnergyRecoverDiff / 60f) % 60;
			seconds = (int)((float)EnergyRecoverDiff % 60f);
			textEnergyTime.text = hour.ToString(tFormat2) + tFormat1 + minutes.ToString(tFormat2) + tFormat1 + seconds.ToString(tFormat2);
			yield return CoroutineDefine._1sec;
			EnergyRecoverDiff--;
			accumuler++;
			if (accumuler >= recoverTrigger)
			{
				UpdateEnergyVal();
			}
		}
	}

	private void UpdateEnergyVal()
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
		{
			accumuler = 0;
			staminaNow = ManagedSingleton<PlayerHelper>.Instance.GetStamina();
			staminaLimit = ManagedSingleton<PlayerHelper>.Instance.GetStaminaLimit();
			EnergyRecoverDiff = (staminaLimit - staminaNow) * recoverTrigger - (int)(MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC - ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.ActionPointTimer);
			if (staminaLimit > staminaNow)
			{
				EnergyRecoverDiff = Mathf.Abs(EnergyRecoverDiff);
			}
			imgEnergyBar.fillAmount = Mathf.Clamp01((float)staminaNow / (float)staminaLimit);
			if (staminaNow < staminaLimit)
			{
				textEnergy.text = string.Format(arrEnergyFormat[0], staminaNow, staminaLimit);
			}
			else if (staminaNow == staminaLimit)
			{
				EnergyRecoverDiff = 0;
				textEnergy.text = string.Format(arrEnergyFormat[0], staminaNow, staminaLimit);
			}
			else
			{
				EnergyRecoverDiff = 0;
				textEnergy.text = string.Format(arrEnergyFormat[1], staminaNow, staminaLimit);
			}
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			UpdateEnergyVal();
		}
	}
}
