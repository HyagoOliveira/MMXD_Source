using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class TabGroupHelper : MonoBehaviour
{
	[Serializable]
	private class TabValueChangedEvent : UnityEvent<int, bool>
	{
	}

	[SerializeField]
	private TabHelper[] _tabHelpers;

	[SerializeField]
	private TabValueChangedEvent _onTabValueChanged;

	private void OnEnable()
	{
		for (int i = 0; i < _tabHelpers.Length; i++)
		{
			_tabHelpers[i].SetTabIndex(i, OnClickTabEvent);
		}
	}

	public void SelectTab(int tabIndex)
	{
		if (tabIndex >= 0 && tabIndex < _tabHelpers.Length)
		{
			_tabHelpers[tabIndex].Select();
		}
	}

	private void OnClickTabEvent(int tabIndex, bool isOn)
	{
		TabValueChangedEvent onTabValueChanged = _onTabValueChanged;
		if (onTabValueChanged != null)
		{
			onTabValueChanged.Invoke(tabIndex, isOn);
		}
	}
}
