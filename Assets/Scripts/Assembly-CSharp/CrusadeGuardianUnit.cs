#define RELEASE
using System;
using UnityEngine;
using UnityEngine.UI;

public class CrusadeGuardianUnit : MonoBehaviour
{
	private bool _targetSelected;

	private bool _buffSelected;

	[SerializeField]
	private Image _imageIcon;

	[SerializeField]
	private GameObject _goIconFilter;

	[SerializeField]
	private GameObject _goSelection;

	[SerializeField]
	private GameObject _goCheckmark;

	[SerializeField]
	private GameObject _goTargetSelect;

	[SerializeField]
	private GameObject _goBuffApply;

	[SerializeField]
	private GameObject _goBuffApplied;

	[SerializeField]
	private GameObject _goElectricEffect;

	[SerializeField]
	private GameObject _goElectricCircleEffect;

	[SerializeField]
	private GameObject _goBusted;

	[SerializeField]
	private GameObject _goLifeInfo;

	[SerializeField]
	private Text _textLifeInfo;

	[SerializeField]
	private Text _textResetCD;

	private Action<int> _onTargetSelectEvent;

	private Action<int> _onBuffSelectEvent;

	private NetCrusadeGuardInfo _guardianInfo;

	private bool _isKilled;

	public bool TargetSelected
	{
		get
		{
			return _targetSelected;
		}
		set
		{
			_targetSelected = value;
			_goSelection.SetActive(_targetSelected);
			_goCheckmark.SetActive(_targetSelected);
		}
	}

	public bool BuffSelected
	{
		get
		{
			return _buffSelected;
		}
		set
		{
			_buffSelected = value;
			if (_isKilled)
			{
				_goBuffApply.SetActive(!value);
				_goBuffApplied.SetActive(value);
				_goElectricCircleEffect.SetActive(value);
				_goElectricEffect.SetActive(value);
			}
		}
	}

	public bool isGoSelect
	{
		get
		{
			return _goSelection.activeSelf;
		}
	}

	public int GuardianIndex { get; private set; }

	public bool IsKilled
	{
		get
		{
			return _isKilled;
		}
	}

	public int BustedSkill { get; private set; }

	public bool Setup(int guardianIndex, long datumTime, string iconName, int bustedSkill, NetCrusadeGuardInfo guardInfo, Action<int> onTargetSelect, Action<int> onBuffSelect)
	{
		_guardianInfo = guardInfo;
		GuardianIndex = guardianIndex;
		_onTargetSelectEvent = onTargetSelect;
		_onBuffSelectEvent = onBuffSelect;
		BustedSkill = bustedSkill;
		_isKilled = _guardianInfo.Count >= OrangeConst.GUILD_CRUSADE_KILLNUM;
		_imageIcon.enabled = false;
		if (!iconName.IsNullString())
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_iconChip, iconName, delegate(Sprite sprite)
			{
				_imageIcon.sprite = sprite;
				_imageIcon.enabled = true;
			});
		}
		else
		{
			Debug.LogError(string.Format("Invalid IconName of guardianIndex {0}", guardianIndex));
		}
		_goIconFilter.SetActive(_isKilled);
		_goBusted.SetActive(_isKilled);
		_goLifeInfo.SetActive(!_isKilled);
		_goTargetSelect.SetActive(!_isKilled);
		_goBuffApply.SetActive(false);
		_goBuffApplied.SetActive(false);
		_goElectricCircleEffect.SetActive(false);
		_goElectricEffect.SetActive(false);
		TargetSelected = false;
		BuffSelected = false;
		UpdateInfo(datumTime, guardInfo);
		return _isKilled;
	}

	public bool UpdateInfo(long datumTime, NetCrusadeGuardInfo guardInfo)
	{
		bool result = false;
		_textLifeInfo.text = string.Format("{0}/{1}", guardInfo.Count, OrangeConst.GUILD_CRUSADE_KILLNUM);
		if (guardInfo.Count >= OrangeConst.GUILD_CRUSADE_KILLNUM)
		{
			long num = CapUtility.DateToUnixTime(CapUtility.UnixTimeToDate(guardInfo.KillTime).AddMinutes(OrangeConst.GUILD_CRUSADE_KILLTIME));
			if (num < datumTime)
			{
				result = true;
			}
			_textResetCD.gameObject.SetActive(true);
			TimeSpan timeSpan = TimeSpan.FromSeconds(num - datumTime);
			_textResetCD.text = string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		}
		else
		{
			_textResetCD.gameObject.SetActive(false);
		}
		return result;
	}

	public void OnClickTargetSelectButton()
	{
		Action<int> onTargetSelectEvent = _onTargetSelectEvent;
		if (onTargetSelectEvent != null)
		{
			onTargetSelectEvent(GuardianIndex);
		}
	}

	public void OnClickBuffApplyButton()
	{
		Action<int> onBuffSelectEvent = _onBuffSelectEvent;
		if (onBuffSelectEvent != null)
		{
			onBuffSelectEvent(GuardianIndex);
		}
	}

	public void OnClickTargetInfoButton()
	{
		SKILL_TABLE skillAttrData;
		if (BustedSkill > 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(BustedSkill, out skillAttrData))
		{
			Vector3 tarPos = GetComponent<RectTransform>().position;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonFloatInfo", delegate(CommonFloatInfoUI ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillAttrData.w_TIP), tarPos);
			});
		}
		else
		{
			Debug.LogError(string.Format("Invalid SkillId : {0}", BustedSkill));
		}
	}

	public void OnClickIconButton()
	{
		if (_goSelection.activeSelf)
		{
			return;
		}
		if (_isKilled)
		{
			Action<int> onBuffSelectEvent = _onBuffSelectEvent;
			if (onBuffSelectEvent != null)
			{
				onBuffSelectEvent(GuardianIndex);
			}
		}
		else
		{
			Action<int> onTargetSelectEvent = _onTargetSelectEvent;
			if (onTargetSelectEvent != null)
			{
				onTargetSelectEvent(GuardianIndex);
			}
		}
	}
}
