using System;
using UnityEngine;

public class GuildBadgeButtonSwitchHelper : MonoBehaviour
{
	public GameObject SelectionMark;

	private int _index;

	public event Action<int> OnChangeImageEvent;

	private void Start()
	{
		_index = base.transform.GetSiblingIndex();
	}

	private void OnDestroy()
	{
		this.OnChangeImageEvent = null;
	}

	public void ChangeImage()
	{
		Action<int> onChangeImageEvent = this.OnChangeImageEvent;
		if (onChangeImageEvent != null)
		{
			onChangeImageEvent(_index);
		}
	}

	public void ToggleSelectionMark(bool value)
	{
		GameObject selectionMark = SelectionMark;
		if ((object)selectionMark != null)
		{
			selectionMark.SetActive(value);
		}
	}
}
