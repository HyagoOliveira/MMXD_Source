using System;
using System.Collections.Generic;
using UnityEngine;

public class GuildLogChildUI : OrangeChildUIBase
{
	[SerializeField]
	private Transform _content;

	[SerializeField]
	private GuildLogDateCell _logDateCell;

	[SerializeField]
	private GuildLogCell _logCell;

	private List<GameObject> _cellObjects = new List<GameObject>();

	public override void Setup()
	{
		_logDateCell.gameObject.SetActive(false);
		_logCell.gameObject.SetActive(false);
		DateTime date = DateTime.MaxValue;
		foreach (NetGuildLog item in Singleton<GuildSystem>.Instance.LogListCache)
		{
			DateTime dateTime = DateTimeHelper.FromEpochLocalTime(item.LogTime);
			if (dateTime.Date < date.Date)
			{
				date = dateTime.Date;
				GuildLogDateCell guildLogDateCell = UnityEngine.Object.Instantiate(_logDateCell);
				guildLogDateCell.transform.SetParent(_content, false);
				guildLogDateCell.gameObject.SetActive(true);
				guildLogDateCell.Setup(date);
				_cellObjects.Add(guildLogDateCell.gameObject);
			}
			GuildLogCell guildLogCell = UnityEngine.Object.Instantiate(_logCell);
			guildLogCell.transform.SetParent(_content, false);
			guildLogCell.gameObject.SetActive(true);
			guildLogCell.Setup(item);
			_cellObjects.Add(guildLogCell.gameObject);
		}
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	public void Clear()
	{
		foreach (GameObject cellObject in _cellObjects)
		{
			UnityEngine.Object.DestroyImmediate(cellObject);
		}
		_cellObjects.Clear();
	}
}
