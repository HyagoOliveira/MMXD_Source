using UnityEngine;
using UnityEngine.UI;

public class GuildBossMainUI : CrusadeUIBase
{
	[SerializeField]
	private Text _textGuildContributionNum;

	private string[] _preSceneBgm;

	protected override void OnEnable()
	{
		base.OnEnable();
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
	}

	public override void Setup()
	{
		base.Setup();
		_preSceneBgm = new string[2]
		{
			MonoBehaviourSingleton<AudioManager>.Instance.bgmSheet,
			MonoBehaviourSingleton<AudioManager>.Instance.bgmCue
		};
		string[] array = new string[2] { "BGM02", "bgm_sys_guild" };
		int eventID = Singleton<CrusadeSystem>.Instance.EventID;
		EVENT_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT.TryGetValue(eventID, out value) && !string.IsNullOrEmpty(value.s_BGM))
		{
			string[] array2 = value.s_BGM.Split(',');
			if (array2.Length == 2)
			{
				array = array2;
			}
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM(array[0], array[1]);
	}

	protected override void UpdatePersonalEventRanking(CrusadeEventRankingInfo rankingInfo)
	{
		base.UpdatePersonalEventRanking(rankingInfo);
		_textGuildContributionNum.text = rankingInfo.Score.ToString();
	}

	private void PlayPrevBgm()
	{
		if (_preSceneBgm != null && _preSceneBgm.Length == 2)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM(_preSceneBgm[0], _preSceneBgm[1]);
		}
	}

	protected override void OnUILinkPrepareEvent()
	{
		base.OnUILinkPrepareEvent();
		PlayPrevBgm();
	}

	protected override void OnBackToHometop()
	{
		base.OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		PlayPrevBgm();
		base.OnClickCloseBtn();
	}
}
