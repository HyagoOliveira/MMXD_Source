#define RELEASE
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GuildListUIBase<U, C> : OrangeUIBase where U : GuildListUIBase<U, C> where C : GuildCell<U>
{
	protected const int SCROLL_VISUAL_COUNT = 3;

	[SerializeField]
	private Button _buttonSearch;

	[SerializeField]
	private Button _buttonRefresh;

	[SerializeField]
	private Text _textSearchCD;

	[SerializeField]
	private Text _textRefreshCD;

	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private C _scrollCell;

	[SerializeField]
	private GameObject _emptyHint;

	[SerializeField]
	private Text _textPage;

	[SerializeField]
	private Button _btnPrevPage;

	[SerializeField]
	private Button _btnNextPage;

	[SerializeField]
	private InputField _inputSearch;

	protected string _inputSearchString = string.Empty;

	private int _coolDownTime;

	private Coroutine _coolDownCoroutine;

	private int _guildIdCache;

	public int CurrentPage { get; protected set; }

	public void PlaySE(SystemSE eCue)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.Play("SystemSE", (int)eCue);
	}

	public void PlaySE(SystemSE02 eCue)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.Play("SystemSE02", (int)eCue);
	}

	protected virtual void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnSearchGuildEvent += OnSearchGuildEvent;
	}

	protected virtual void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnSearchGuildEvent -= OnSearchGuildEvent;
		Singleton<GuildSystem>.Instance.SearchGuildListCache.Clear();
		if (_coolDownCoroutine != null)
		{
			StopCoroutine(_coolDownCoroutine);
		}
	}

	public virtual void Setup()
	{
		Debug.Log("[Setup]");
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_textSearchCD.gameObject.SetActive(false);
		_textRefreshCD.gameObject.SetActive(false);
		ClearSearchGuildList();
		RefreshPageButtonState();
	}

	public void OnClickPrevPageBtn()
	{
		if (CurrentPage > 1)
		{
			PlaySE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
			CurrentPage--;
			RefreshPageButtonState();
			RefreshPage();
		}
	}

	public void OnClickNextPageBtn()
	{
		PlaySE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
		CurrentPage++;
		RefreshPageButtonState();
		if (Singleton<GuildSystem>.Instance.SearchGuildListCache.Count < Singleton<GuildSystem>.Instance.SearchGuildListCount && CurrentPage * 20 > Singleton<GuildSystem>.Instance.SearchGuildListCache.Count)
		{
			SearchGuild();
		}
		else
		{
			RefreshPage();
		}
	}

	public void OnClickGuildInfoBtn(int guildId)
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		_guildIdCache = guildId;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildMemberListUI>("UI_GuildMemberList", OnGuildMemberListUILoaded);
	}

	private void OnGuildMemberListUILoaded(GuildMemberListUI ui)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(_guildIdCache);
	}

	public void OnClickSearchBtn()
	{
		InputField inputSearch = _inputSearch;
		_inputSearchString = (((object)inputSearch != null) ? inputSearch.text : null) ?? string.Empty;
		OnClickRefreshBtn();
	}

	public void OnClickRefreshBtn()
	{
		PlaySE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		ClearSearchGuildList();
		RefreshPageButtonState();
		StartButtonCD();
		SearchGuild();
	}

	protected virtual void SearchGuild()
	{
		Singleton<GuildSystem>.Instance.ReqSearchGuild(_inputSearchString, Singleton<GuildSystem>.Instance.SearchGuildListCache.Count);
	}

	private void ClearSearchGuildList()
	{
		CurrentPage = 1;
		Singleton<GuildSystem>.Instance.SearchGuildListCount = 0;
		Singleton<GuildSystem>.Instance.SearchGuildListCache.Clear();
		_scrollRect.ClearCells();
	}

	protected virtual void RefreshPage()
	{
		RefreshPageButtonState();
		RefreshCells();
	}

	private void RefreshPageButtonState()
	{
		int num = (int)Mathf.Ceil((float)Singleton<GuildSystem>.Instance.SearchGuildListCount / 20f);
		_btnPrevPage.interactable = CurrentPage > 1;
		_btnNextPage.interactable = num > CurrentPage;
		_textPage.text = ((num > 0) ? string.Format("{0}/{1}", CurrentPage, num) : "0/0");
	}

	private void RefreshCells()
	{
		_emptyHint.SetActive(Singleton<GuildSystem>.Instance.SearchGuildListCache.Count == 0);
		_scrollRect.ClearCells();
		_scrollRect.OrangeInit(_scrollCell, 3, Mathf.Min(20, Singleton<GuildSystem>.Instance.SearchGuildListCache.Count - (CurrentPage - 1) * 20));
	}

	private void OnSearchGuildEvent(Code ackCode)
	{
		Debug.Log(string.Format("[{0}] Count = {1}", "OnSearchGuildEvent", Singleton<GuildSystem>.Instance.SearchGuildListCache.Count));
		RefreshPage();
	}

	private void StartButtonCD()
	{
		_buttonSearch.interactable = false;
		_buttonRefresh.interactable = false;
		_textSearchCD.gameObject.SetActive(true);
		_textRefreshCD.gameObject.SetActive(true);
		_coolDownTime = 5;
		_coolDownCoroutine = StartCoroutine(ButtonCDEnumerator());
	}

	private IEnumerator ButtonCDEnumerator()
	{
		while (_coolDownTime > 0)
		{
			_textSearchCD.text = string.Format("{0}", _coolDownTime);
			_textRefreshCD.text = string.Format("{0}", _coolDownTime);
			yield return new WaitForSeconds(1f);
			_coolDownTime--;
		}
		_buttonSearch.interactable = true;
		_buttonRefresh.interactable = true;
		_textSearchCD.gameObject.SetActive(false);
		_textRefreshCD.gameObject.SetActive(false);
	}
}
