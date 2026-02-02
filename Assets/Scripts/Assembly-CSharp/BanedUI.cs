using NaughtyAttributes;
using UnityEngine;

public class BanedUI : MonoBehaviour
{
	[BoxGroup("BanedUI")]
	[SerializeField]
	private GameObject BanedHint;

	[BoxGroup("BanedUI")]
	[SerializeField]
	private OrangeText BanedText;

	[BoxGroup("BanedUI")]
	[SerializeField]
	private GameObject BtnPVP;

	[BoxGroup("BanedUI")]
	[SerializeField]
	private GameObject BtnMultiplay;

	public EventCell BtnRaid;

	private float nowtime;

	private float starttime;

	private bool isCheat;

	private void Awake()
	{
		isCheat = ManagedSingleton<PlayerHelper>.Instance.GetUseCheatPlugIn();
	}

	private void Start()
	{
		starttime = Time.deltaTime;
		BanedButtons();
	}

	private void Update()
	{
		isCheat = ManagedSingleton<PlayerHelper>.Instance.GetUseCheatPlugIn();
		if (!isCheat)
		{
			BanedButtons();
			return;
		}
		nowtime = Time.deltaTime;
		UpdateTime();
	}

	public void BanedButtons()
	{
		if (!isCheat)
		{
			OrangeGameUtility.DeleteLockObj(BtnPVP.transform);
			OrangeGameUtility.DeleteLockObj(BtnMultiplay.transform);
			if ((bool)BtnRaid)
			{
				BtnRaid.btnIcon.interactable = true;
				OrangeGameUtility.DeleteLockObj(BtnRaid.transform);
			}
			BanedHint.SetActive(false);
		}
		else
		{
			UpdateTime();
			BanedHint.SetActive(true);
			OrangeGameUtility.CreateLockObj(BtnPVP.transform);
			OrangeGameUtility.CreateLockObj(BtnMultiplay.transform, UIOpenChk.ChkBanEnum.OPENBAN_CORP);
		}
	}

	private void UpdateTime()
	{
		float nowtime2 = nowtime;
		float starttime2 = starttime;
		string cheatExpireTime = ManagedSingleton<PlayerHelper>.Instance.GetCheatExpireTime((int)MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC);
		string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PLUGIN_BAN"), cheatExpireTime);
		BanedText.text = text;
	}
}
