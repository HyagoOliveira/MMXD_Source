#define RELEASE
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GuildLogCell : MonoBehaviour
{
	[SerializeField]
	private Text _log;

	[SerializeField]
	private Text _logTime;

	public void Setup(NetGuildLog logInfo)
	{
		GuildLogType logType = (GuildLogType)logInfo.LogType;
		SocketPlayerHUD value;
		string text = (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(logInfo.PlayerID, out value) ? value.m_Name : "---");
		_logTime.text = DateTimeHelper.FromEpochLocalTime(logInfo.LogTime).ToString("HH:mm:ss");
		_log.alignByGeometry = false;
		switch (logType)
		{
		case GuildLogType.GuildLogAdd:
			_log.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LOG_ADD", text);
			break;
		case GuildLogType.GuildLogLeave:
			_log.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LOG_LEAVE", text);
			break;
		case GuildLogType.GuildLogKickout:
			_log.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LOG_KICKOUT", text);
			break;
		case GuildLogType.GuildLogMoney:
			_log.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LOG_MONEY", text, logInfo.Money.ToString("#,0"));
			break;
		case GuildLogType.GuildLogWanted:
			_log.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LOG_WANTED", text, logInfo.Score.ToString("#,0"));
			break;
		case GuildLogType.GuildLogChangeLeader:
			_log.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LOG_CHAIRMAN", text);
			break;
		case GuildLogType.GuildLogPillarOpen:
		{
			ORE_TABLE value2;
			SKILL_TABLE value3;
			string text2 = ((ManagedSingleton<OrangeDataManager>.Instance.ORE_TABLE_DICT.TryGetValue(logInfo.EventID, out value2) && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(value2.n_ORE_SKILL_1, out value3)) ? ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value3.w_NAME) : "---");
			_log.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LOG_POWEROPEN", text, text2, logInfo.Money.ToString("#,0"));
			break;
		}
		case GuildLogType.GuildLogPillarClose:
			_log.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LOG_POWERCLOSE", text);
			break;
		default:
			_log.text = string.Empty;
			Debug.LogError("Unhandled Log : " + JsonHelper.Serialize(logInfo));
			break;
		}
	}
}
