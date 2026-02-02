using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GuildEddieRewardUI : OrangeUIBase
{
	[SerializeField]
	private GameObject _panelMVP;

	[SerializeField]
	private GameObject _panelNoMVP;

	[SerializeField]
	private GameObject _mvpIconRoot;

	[SerializeField]
	private Text _mvpPlayerName;

	[SerializeField]
	private GuildEddieRewardProgressHelper _eddieRewardProgressHelper;

	[SerializeField]
	private GuildEddieRewardGroup _eddieRewardGroup;

	[SerializeField]
	private Button _buttonGetReward;

	private int _rank;

	private int _donateValue;

	public void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnGetEddieRewardEvent += OnGetEddieRewardEvent;
	}

	public void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnGetEddieRewardEvent -= OnGetEddieRewardEvent;
	}

	public void Setup(int rank, int donateValue, List<NetEddieBoxGachaRecord> boxGachaRecordList, string mvpPlayerId)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		SetMVPInfo(mvpPlayerId);
		_rank = rank;
		_donateValue = donateValue;
		_buttonGetReward.interactable = boxGachaRecordList.FirstOrDefault((NetEddieBoxGachaRecord record) => ManagedSingleton<PlayerHelper>.Instance.CheckPlayerIsSelf(record.PlayerID) && record.Received == 0) != null;
		List<GuildEddieDonateSetting> eddieDonateSettings;
		if (!GuildEddieDonateSetting.TryGetSettingsByGuildRank(rank, out eddieDonateSettings))
		{
			eddieDonateSettings = new List<GuildEddieDonateSetting>();
		}
		_eddieRewardProgressHelper.Setup(donateValue, eddieDonateSettings);
		_eddieRewardGroup.Setup(rank, donateValue, boxGachaRecordList);
	}

	private void SetMVPInfo(string playerId)
	{
		if (!string.IsNullOrEmpty(playerId))
		{
			_panelMVP.SetActive(true);
			_panelNoMVP.SetActive(false);
			SocketPlayerHUD playerHUD;
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(playerId, out playerHUD))
			{
				_mvpPlayerName.text = playerHUD.m_Name;
				CommonAssetHelper.LoadPlayerIcon(_mvpIconRoot, delegate(PlayerIconBase playerIcon)
				{
					CommonUIHelper.SetPlayerIcon(playerIcon, playerHUD.m_IconNumber);
				});
			}
			else
			{
				_mvpPlayerName.text = "---";
			}
		}
		else
		{
			_panelMVP.SetActive(false);
			_panelNoMVP.SetActive(true);
			_mvpPlayerName.text = string.Empty;
		}
	}

	public void OnClickRewardListBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GuildEddieRewardList", delegate(GuildEddieRewardListUI ui)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(_rank, _donateValue);
		});
	}

	public void OnClickGetRewardBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		Singleton<GuildSystem>.Instance.ReqReceiveEddieReward();
	}

	private void OnGetEddieRewardEvent(Code ackCode, List<NetRewardInfo> rewardList)
	{
		if (ackCode == Code.GUILD_RECEIVE_EDDIE_REWARD_SUCCESS)
		{
			_buttonGetReward.interactable = false;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
			{
				ui.Setup(rewardList);
			});
		}
	}
}
