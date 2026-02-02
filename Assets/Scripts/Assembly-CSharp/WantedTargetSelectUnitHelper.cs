#define RELEASE
using System;
using UnityEngine;
using UnityEngine.UI;

public class WantedTargetSelectUnitHelper : MonoBehaviour
{
	[SerializeField]
	private Color _colorLevelIconOn;

	[SerializeField]
	private Color _colorLevelIconOff;

	[SerializeField]
	private Color _colorTimeStarted;

	[SerializeField]
	private Color _colorTimeReceived;

	[SerializeField]
	private GameObject _goSelection;

	[SerializeField]
	private Image _imageIcon;

	[SerializeField]
	private Material _materialIconClear;

	[SerializeField]
	private GameObject _goReceive;

	[SerializeField]
	private GameObject _goClear;

	[SerializeField]
	private Image[] _imageLevelIcons;

	[SerializeField]
	private GameObject _goTimeHint;

	[SerializeField]
	private Image _imageTimeIcon;

	[SerializeField]
	private Text _textTimeHint;

	private bool _isSelected;

	private int _index;

	private WantedTargetState _targetState;

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			_goSelection.SetActive(_isSelected);
		}
	}

	public event Action<int, WantedTargetState> OnSelectButtonClickedEvent;

	public void Setup(int index, NetWantedInfo wantedInfo, WANTED_TABLE wantedAttrData)
	{
		_index = index;
		if (!wantedAttrData.s_ICON.IsNullString())
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Sprite>(AssetBundleScriptableObject.Instance.m_iconChip, wantedAttrData.s_ICON, OnIconLoaded);
		}
		else
		{
			Debug.LogError(string.Format("Invalid IconName of {0} : {1}", "WANTED_TABLE", wantedAttrData.n_ID));
		}
		SetLevelIcons(wantedAttrData);
	}

	private void SetLevelIcons(WANTED_TABLE wantedAttrData)
	{
		Image[] imageLevelIcons = _imageLevelIcons;
		for (int i = 0; i < imageLevelIcons.Length; i++)
		{
			imageLevelIcons[i].color = _colorLevelIconOff;
		}
		for (int j = 0; j < wantedAttrData.n_STAR; j++)
		{
			_imageLevelIcons[j].color = _colorLevelIconOn;
		}
	}

	public bool CheckTargetState(long datumTime, NetWantedInfo wantedInfo)
	{
		_goReceive.SetActive(false);
		_goClear.SetActive(false);
		_imageIcon.material = null;
		_goTimeHint.SetActive(false);
		if (wantedInfo.Received > 0)
		{
			_goClear.SetActive(true);
			_goTimeHint.SetActive(true);
			_imageTimeIcon.color = _colorTimeReceived;
			_textTimeHint.color = _colorTimeReceived;
			_imageIcon.material = _materialIconClear;
			_targetState = WantedTargetState.Received;
		}
		else if (wantedInfo.StartTime > 0)
		{
			if (wantedInfo.FinishTime <= datumTime)
			{
				_goReceive.SetActive(true);
				_targetState = WantedTargetState.Finished;
			}
			else
			{
				_goTimeHint.SetActive(true);
				_imageTimeIcon.color = _colorTimeStarted;
				_textTimeHint.color = _colorTimeStarted;
				_targetState = WantedTargetState.Started;
			}
		}
		else
		{
			_targetState = WantedTargetState.Normal;
		}
		if (_targetState != WantedTargetState.Started)
		{
			return _targetState == WantedTargetState.Received;
		}
		return true;
	}

	public bool UpdateTime(long datumTime, NetWantedInfo wantedInfo)
	{
		switch (_targetState)
		{
		case WantedTargetState.Started:
		{
			TimeSpan timeSpan2 = CapUtility.UnixTimeToDate(wantedInfo.FinishTime) - CapUtility.UnixTimeToDate(datumTime);
			_textTimeHint.text = string.Format("{0:00}:{1:00}:{2:00}", timeSpan2.Hours, timeSpan2.Minutes, timeSpan2.Seconds);
			return wantedInfo.FinishTime <= datumTime;
		}
		case WantedTargetState.Received:
		{
			int currentResetTime = MonoBehaviourSingleton<OrangeGameManager>.Instance.serverInfo.DailyResetInfo.CurrentResetTime;
			TimeSpan timeSpan = CapUtility.UnixTimeToDate(currentResetTime) - CapUtility.UnixTimeToDate(datumTime);
			_textTimeHint.text = string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
			return currentResetTime <= datumTime;
		}
		default:
			return false;
		}
	}

	public void OnClickSelectionButton()
	{
		Action<int, WantedTargetState> onSelectButtonClickedEvent = this.OnSelectButtonClickedEvent;
		if (onSelectButtonClickedEvent != null)
		{
			onSelectButtonClickedEvent(_index, _targetState);
		}
	}

	private void OnIconLoaded(Sprite sprite)
	{
		_imageIcon.sprite = sprite;
		_imageIcon.enabled = true;
	}
}
