using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class TabHelper : MonoBehaviour
{
	private int _tabIndex;

	private Toggle _toggle;

	public event Action<int, bool> OnValueChanged;

	public void OnEnable()
	{
		_toggle = GetComponent<Toggle>();
		_toggle.onValueChanged.AddListener(OnToggleValueChangedEvent);
	}

	public void OnDestroy()
	{
		_toggle.onValueChanged.RemoveAllListeners();
	}

	public void SetTabIndex(int index, Action<int, bool> onValueChanged = null)
	{
		_tabIndex = index;
		OnValueChanged += onValueChanged;
	}

	public void Select()
	{
		_toggle.isOn = true;
	}

	private void OnToggleValueChangedEvent(bool isOn)
	{
		Action<int, bool> onValueChanged = this.OnValueChanged;
		if (onValueChanged != null)
		{
			onValueChanged(_tabIndex, isOn);
		}
	}
}
