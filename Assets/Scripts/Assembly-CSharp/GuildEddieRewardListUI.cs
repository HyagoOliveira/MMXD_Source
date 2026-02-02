using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GuildEddieRewardListUI : OrangeUIBase
{
	private const int SCROLL_VISUAL_COUNT = 5;

	[SerializeField]
	private TabGroupHelper _tabHelper;

	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private GuildEddieRewardListCell _scrollCell;

	private int _oldIndex = -1;

	private List<GuildEddieDonateSetting> _donateSettingsCache;

	public bool bFirstSE;

	[HideInInspector]
	public List<BOXGACHACONTENT_TABLE> RewardListCache { get; private set; }

	public void Setup(int rank, int value)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		List<GuildEddieDonateSetting> eddieDonateSettings;
		if (!GuildEddieDonateSetting.TryGetSettingsByGuildRank(rank, out eddieDonateSettings))
		{
			eddieDonateSettings = new List<GuildEddieDonateSetting>();
		}
		_donateSettingsCache = eddieDonateSettings;
		int settingIndex = 0;
		for (int i = 0; i < eddieDonateSettings.Count; i++)
		{
			GuildEddieDonateSetting guildEddieDonateSetting = eddieDonateSettings[i];
			if (value >= guildEddieDonateSetting.Threshold)
			{
				settingIndex = i;
			}
		}
		bFirstSE = true;
		SelectTab(settingIndex);
	}

	private void SelectTab(int settingIndex)
	{
		_tabHelper.SelectTab(settingIndex);
	}

	public void OnTabValueChanged(int tabIndex, bool isOn)
	{
		if (isOn && _oldIndex != tabIndex)
		{
			if (!bFirstSE)
			{
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
			}
			else
			{
				bFirstSE = false;
			}
			GuildEddieDonateSetting setting = _donateSettingsCache[tabIndex];
			_oldIndex = tabIndex;
			RewardListCache = (from attrData in ManagedSingleton<OrangeDataManager>.Instance.BOXGACHACONTENT_TABLE_DICT.Values
				where attrData.n_GROUP == setting.GachaId
				orderby attrData.n_ID
				select attrData).ToList();
			LoopVerticalScrollRect scrollRect = _scrollRect;
			if ((object)scrollRect != null)
			{
				scrollRect.OrangeInit(_scrollCell, 5, RewardListCache.Count);
			}
		}
	}
}
