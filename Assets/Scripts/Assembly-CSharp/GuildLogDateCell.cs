using System;
using UnityEngine;
using UnityEngine.UI;

public class GuildLogDateCell : MonoBehaviour
{
	[SerializeField]
	private Text _textDate;

	public void Setup(DateTime date)
	{
		_textDate.text = date.ToString("yyyy-MM-dd");
	}
}
