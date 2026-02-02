#define RELEASE
using UnityEngine;
using UnityEngine.UI;

public class GuildApplyPlayerListChildUI : OrangeChildUIBase
{
	private const int SCROLL_VISUAL_COUNT = 5;

	[SerializeField]
	private Text _textMemberCount;

	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private GuildApplyPlayerCell _scrollCell;

	[SerializeField]
	private GameObject _emptyHint;

	private void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnGetApplyPlayerListEvent += OnGetApplyPlayerListEvent;
		Singleton<GuildSystem>.Instance.OnAgreePlayerApplyEvent += OnAgreePlayerApplyEvent;
		Singleton<GuildSystem>.Instance.OnRefusePlayerApplyEvent += OnRefusePlayerApplyEvent;
	}

	private void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnGetApplyPlayerListEvent -= OnGetApplyPlayerListEvent;
		Singleton<GuildSystem>.Instance.OnAgreePlayerApplyEvent -= OnAgreePlayerApplyEvent;
		Singleton<GuildSystem>.Instance.OnRefusePlayerApplyEvent -= OnRefusePlayerApplyEvent;
	}

	public override void Setup()
	{
		RefreshCells();
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	private void OnGetApplyPlayerListEvent(Code ackCode)
	{
		RefreshCells();
	}

	private void OnAgreePlayerApplyEvent(Code ackCode)
	{
		switch (ackCode)
		{
		case Code.GUILD_AGREE_JOIN_APPLY_SUCCESS:
			RefreshCells();
			break;
		case Code.GUILD_APPLY_NOT_FOUND_DATA:
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_APPLYCANCEL", true, OnPlayerApplyCancel);
			break;
		case Code.GUILD_LEAVE_COOLING_FAIL:
			CommonUIHelper.ShowCommonTipUI(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_SETUP_WARN2", OrangeConst.GUILD_INVITE_TIME), false);
			break;
		case Code.GUILD_MEMBER_MAX:
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_ADDMAX");
			break;
		default:
			Debug.LogWarning(string.Format("Unhandled AckCode : {0}", ackCode));
			RefreshCells();
			break;
		}
	}

	private void OnRefusePlayerApplyEvent(Code ackCode)
	{
		switch (ackCode)
		{
		case Code.GUILD_REFUSE_JOIN_APPLY_SUCCESS:
			RefreshCells();
			break;
		case Code.GUILD_APPLY_NOT_FOUND_DATA:
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_APPLYCANCEL", true, OnPlayerApplyCancel);
			break;
		default:
			Debug.LogWarning(string.Format("Unhandled AckCode : {0}", ackCode));
			RefreshCells();
			break;
		}
	}

	private void OnPlayerApplyCancel()
	{
		Singleton<GuildSystem>.Instance.ReqGetApplyPlayerList();
	}

	private void RefreshCells()
	{
		int count = Singleton<GuildSystem>.Instance.MemberInfoListCache.Count;
		_textMemberCount.text = string.Format("{0}/{1}", count, Singleton<GuildSystem>.Instance.GuildSetting.MemberLimit);
		GameObject emptyHint = _emptyHint;
		if ((object)emptyHint != null)
		{
			emptyHint.SetActive(Singleton<GuildSystem>.Instance.ApplyPlayerListCache.Count == 0);
		}
		Clear();
		LoopVerticalScrollRect scrollRect = _scrollRect;
		if ((object)scrollRect != null)
		{
			scrollRect.OrangeInit(_scrollCell, 5, Singleton<GuildSystem>.Instance.ApplyPlayerListCache.Count);
		}
	}

	public void Clear()
	{
		LoopVerticalScrollRect scrollRect = _scrollRect;
		if ((object)scrollRect != null)
		{
			scrollRect.ClearCells();
		}
	}

	public void OnClickOneAgreeBtn(string playerId)
	{
		if (Singleton<GuildSystem>.Instance.MemberInfoListCache.Count + 1 > Singleton<GuildSystem>.Instance.GuildSetting.MemberLimit)
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_ADDMAX");
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		Singleton<GuildSystem>.Instance.ReqAgreePlayerApply(playerId);
	}

	public void OnClickAgreeAllBtn()
	{
		if (Singleton<GuildSystem>.Instance.MemberInfoListCache.Count + Singleton<GuildSystem>.Instance.ApplyPlayerListCache.Count > Singleton<GuildSystem>.Instance.GuildSetting.MemberLimit)
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_ADDMAX");
		}
		else
		{
			Singleton<GuildSystem>.Instance.ReqAgreePlayerApply();
		}
	}

	public void OnClickOneRefuseBtn(string playerId)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CANCEL);
		Singleton<GuildSystem>.Instance.ReqRefusePlayerApply(playerId);
	}

	public void OnClickRefuseAllBtn()
	{
		Singleton<GuildSystem>.Instance.ReqRefusePlayerApply();
	}
}
