using System;
using System.Collections;
using UnityEngine;

public class WantedTargetSelectHelper : OrangeChildUIBase
{
	[SerializeField]
	private WantedTargetSelectUnitHelper[] _targetUnitHelpers;

	private int _selectedIndex;

	private Coroutine _coroutineCountDown;

	public event Action<int, WantedTargetState> OnTargetClickedEvent;

	private void Awake()
	{
		for (int i = 0; i < _targetUnitHelpers.Length; i++)
		{
			_targetUnitHelpers[i].OnSelectButtonClickedEvent += OnWantedTargetClicked;
		}
	}

	private void OnDestroy()
	{
		if (_coroutineCountDown != null)
		{
			StopCoroutine(_coroutineCountDown);
		}
	}

	public override void Setup()
	{
		SetupTargetUnits();
		CheckTargetUnitsCoroutine();
	}

	private void SetupTargetUnits()
	{
		for (int i = 0; i < _targetUnitHelpers.Length; i++)
		{
			WantedTargetSelectUnitHelper obj = _targetUnitHelpers[i];
			WantedTargetInfo wantedTargetInfo = Singleton<WantedSystem>.Instance.WantedTargetInfoCacheList[i];
			obj.Setup(i, wantedTargetInfo.WantedInfo, wantedTargetInfo.WantedAttrData);
		}
	}

	public void CheckTargetUnitsCoroutine()
	{
		if (_coroutineCountDown != null)
		{
			StopCoroutine(_coroutineCountDown);
		}
		if (CheckTargetUnitsState())
		{
			_coroutineCountDown = StartCoroutine(UpdateTargetUnitsRoutine());
		}
	}

	private bool CheckTargetUnitsState()
	{
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		bool flag = false;
		for (int i = 0; i < _targetUnitHelpers.Length; i++)
		{
			WantedTargetSelectUnitHelper wantedTargetSelectUnitHelper = _targetUnitHelpers[i];
			WantedTargetInfo wantedTargetInfo = Singleton<WantedSystem>.Instance.WantedTargetInfoCacheList[i];
			flag |= wantedTargetSelectUnitHelper.CheckTargetState(serverUnixTimeNowUTC, wantedTargetInfo.WantedInfo);
		}
		_targetUnitHelpers[_selectedIndex].OnClickSelectionButton();
		return flag;
	}

	private IEnumerator UpdateTargetUnitsRoutine()
	{
		while (!UpdateTargetUnits())
		{
			yield return CoroutineDefine._1sec;
		}
		yield return CoroutineDefine._1sec;
		CheckTargetUnitsCoroutine();
	}

	private bool UpdateTargetUnits()
	{
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		bool flag = false;
		for (int i = 0; i < _targetUnitHelpers.Length; i++)
		{
			WantedTargetSelectUnitHelper wantedTargetSelectUnitHelper = _targetUnitHelpers[i];
			WantedTargetInfo wantedTargetInfo = Singleton<WantedSystem>.Instance.WantedTargetInfoCacheList[i];
			flag |= wantedTargetSelectUnitHelper.UpdateTime(serverUnixTimeNowUTC, wantedTargetInfo.WantedInfo);
		}
		return flag;
	}

	public void SelectTarget(int index)
	{
		_selectedIndex = index;
		WantedTargetSelectUnitHelper[] targetUnitHelpers = _targetUnitHelpers;
		for (int i = 0; i < targetUnitHelpers.Length; i++)
		{
			targetUnitHelpers[i].IsSelected = false;
		}
		_targetUnitHelpers[index].IsSelected = true;
	}

	private void OnWantedTargetClicked(int index, WantedTargetState targetState)
	{
		Action<int, WantedTargetState> onTargetClickedEvent = this.OnTargetClickedEvent;
		if (onTargetClickedEvent != null)
		{
			onTargetClickedEvent(index, targetState);
		}
	}
}
