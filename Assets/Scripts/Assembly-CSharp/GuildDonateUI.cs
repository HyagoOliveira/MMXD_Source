public class GuildDonateUI : GuildDonateUIBase
{
	private void Start()
	{
		Singleton<GuildSystem>.Instance.OnDonateEvent += OnDonateEvent;
	}

	private void OnDestroy()
	{
		Singleton<GuildSystem>.Instance.OnDonateEvent -= OnDonateEvent;
	}

	public void Setup(int maxValue)
	{
		bMuteSE = true;
		SliderDonate.maxValue = maxValue;
		bMuteSE = true;
		OnSliderValueChangedEvent(SliderDonate.value);
	}

	public void OnClickConfirmBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		Singleton<GuildSystem>.Instance.ReqDonate((int)SliderDonate.value);
	}

	private void OnDonateEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		OnClickCloseBtn();
	}
}
