using System;
using System.Collections;
using System.Linq;
using CallbackDefs;
using NaughtyAttributes;
using OrangeAudio;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class Topbar : MonoBehaviour
{
	[SerializeField]
	private Button closeBtn;

	[SerializeField]
	private OrangeText uiName;

	[SerializeField]
	protected OrangeText textEnergyTime;

	[SerializeField]
	private Image imgEnergyBar;

	[SerializeField]
	private Button addMoneyBtn;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickHomeSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private BGM01 m_hometopBGM;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_addJewelSE;

	[BoxGroup("Sound")]
	[SerializeField]
	public SystemSE clickBackSE;

	public Button btnHometop;

	public Text Energy;

	public Text Money;

	public Text JEWEL;

	[HideInInspector]
	public OrangeUIBase Parent;

	public Callback TopOpenUICloseCB;

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

	private void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_TOPBAR_DATA, UpdateTopBarData);
		UpdateTopBarData();
		if (addMoneyBtn != null)
		{
			addMoneyBtn.gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		recoverTrigger = 60 * OrangeConst.AP_RECOVER_TIME;
		StartCoroutine(OnStartUpdateEnergyTime());
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_TOPBAR_DATA, UpdateTopBarData);
	}

	public void Init(OrangeUIBase uiParent)
	{
		Parent = uiParent;
		base.transform.SetParent(uiParent.ResidentParent, false);
		if (!string.IsNullOrEmpty(uiParent.UIName))
		{
			LOCALIZATION_TABLE value;
			if (ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.TryGetValue(uiParent.UIName, out value))
			{
				RefreashName(value.w_KEY);
			}
			else
			{
				value = ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.Values.FirstOrDefault((LOCALIZATION_TABLE x) => x.w_CHT == uiParent.UIName);
				if (value != null)
				{
					RefreashName(value.w_KEY);
				}
				else
				{
					uiName.text = uiParent.UIName;
				}
			}
		}
		OrangeUIBase parent = Parent;
		parent.closeCB = (Callback)Delegate.Combine(parent.closeCB, (Callback)delegate
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_BACK_BGM);
		});
		closeBtn.onClick.AddListener(delegate
		{
			Parent.OnClickCloseBtn();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(clickBackSE);
		});
		TopOpenUICloseCB = uiParent.TopOpenUICloseCB;
		uiParent.TopbarInitComplete(this);
	}

	public void UpdateTopBarData()
	{
		UpdateEnergyVal();
		JEWEL.text = ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel().ToString("#,0");
		Money.text = ManagedSingleton<PlayerHelper>.Instance.GetZenny().ToString("#,0");
	}

	public void OnAddEnergy()
	{
		if (Parent.CanBuy())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChargeStamina", delegate(ChargeStaminaUI ui)
			{
				ui.Setup(ChargeType.ActionPoint);
			});
		}
	}

	public void OnAddMoney()
	{
	}

	public void OnAddJEWEL()
	{
		if (!Parent.CanBuy())
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
		MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Voice);
		ShopTopUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<ShopTopUI>("UI_ShopTop");
		if (uI != null)
		{
			uI.GoShop(ShopTopUI.ShopSelectTab.directproduct);
			uI.closeCB = (Callback)Delegate.Combine(uI.closeCB, TopOpenUICloseCB);
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addJewelSE);
			ui.Setup(ShopTopUI.ShopSelectTab.directproduct);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, TopOpenUICloseCB);
		});
	}

	public void OnClickBackToHometop()
	{
		if (Parent.CanBackToHometop())
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Voice);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickHomeSE);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_HOME_BGM);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BACK_TO_HOMETOP);
			MonoBehaviourSingleton<UIManager>.Instance.BackToHometop();
		}
	}

	private IEnumerator OnStartUpdateEnergyTime()
	{
		while (true)
		{
			int energyRecoverDiff = HometopResidentUI.EnergyRecoverDiff;
			if (energyRecoverDiff <= 0)
			{
				textEnergyTime.text = string.Empty;
				yield return CoroutineDefine._1sec;
				continue;
			}
			hour = (int)((float)energyRecoverDiff / 3600f) % 60;
			minutes = (int)((float)energyRecoverDiff / 60f) % 60;
			seconds = (int)((float)energyRecoverDiff % 60f);
			textEnergyTime.text = hour.ToString(tFormat2) + tFormat1 + minutes.ToString(tFormat2) + tFormat1 + seconds.ToString(tFormat2);
			yield return CoroutineDefine._1sec;
			accumuler++;
			if (accumuler >= recoverTrigger)
			{
				UpdateEnergyVal();
			}
		}
	}

	private void UpdateEnergyVal()
	{
		accumuler = 0;
		staminaNow = ManagedSingleton<PlayerHelper>.Instance.GetStamina();
		staminaLimit = ManagedSingleton<PlayerHelper>.Instance.GetStaminaLimit();
		imgEnergyBar.fillAmount = Mathf.Clamp01((float)staminaNow / (float)staminaLimit);
		if (staminaNow <= staminaLimit)
		{
			Energy.text = string.Format(arrEnergyFormat[0], staminaNow, staminaLimit);
		}
		else
		{
			Energy.text = string.Format(arrEnergyFormat[1], staminaNow, staminaLimit);
		}
	}

	public void RefreashName(string key)
	{
		uiName.IsLocalizationText = true;
		uiName.LocalizationKey = key;
		uiName.UpdateTextImmediate();
	}
}
