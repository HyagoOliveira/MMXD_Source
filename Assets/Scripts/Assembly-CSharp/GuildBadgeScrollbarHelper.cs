using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Scrollbar))]
public class GuildBadgeScrollbarHelper : MonoBehaviour
{
	private Scrollbar _scrollbar;

	public event Action<float> OnChangeValueEvent;

	private void Awake()
	{
		_scrollbar = GetComponent<Scrollbar>();
	}

	private void OnDisable()
	{
		this.OnChangeValueEvent = null;
	}

	public void ChangeColor()
	{
		Action<float> onChangeValueEvent = this.OnChangeValueEvent;
		if (onChangeValueEvent != null)
		{
			onChangeValueEvent(_scrollbar.value);
		}
	}
}
