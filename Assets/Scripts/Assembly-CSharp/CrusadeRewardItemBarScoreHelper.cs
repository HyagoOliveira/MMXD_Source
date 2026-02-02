using System;
using UnityEngine;
using UnityEngine.UI;

public class CrusadeRewardItemBarScoreHelper : MonoBehaviour
{
	[SerializeField]
	private GuildBossRewardUI _parentUI;

	[SerializeField]
	private OrangeText _textItemName;

	[SerializeField]
	private OrangeText _textScore;

	[SerializeField]
	private Button _btnGet;

	[SerializeField]
	private Button _btnGot;

	private int _itemID;

	public int ID { get; private set; }

	public bool IsCompleted { get; private set; }

	public bool IsRetrieved { get; private set; }

	public bool IsRetrieving { get; private set; }

	public event Action<int> OnGetOneRewardEvent;

	public void Setup(int itemId, int n_ID, int amount, int score, bool isCompleted, bool isRetrieved)
	{
		_itemID = itemId;
		ID = n_ID;
		IsCompleted = isCompleted;
		IsRetrieved = isRetrieved;
		base.gameObject.SetActive(true);
		_textItemName.text = _parentUI.ItemIconHelper(base.transform, itemId, amount);
		_textScore.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_CONTRIBUTION_POINT"), score);
		if (!isCompleted)
		{
			_btnGot.gameObject.SetActive(false);
			_btnGet.gameObject.SetActive(false);
		}
		else
		{
			_btnGot.gameObject.SetActive(isRetrieved);
			_btnGet.gameObject.SetActive(!isRetrieved);
		}
	}

	public void SetRetrieving()
	{
		IsRetrieving = true;
		_btnGet.gameObject.SetActive(false);
	}

	public void SetRetrived()
	{
		IsRetrieving = false;
		IsRetrieved = true;
		_btnGot.gameObject.SetActive(true);
	}

	public void OnClickGetRewardBtn()
	{
		SetRetrieving();
		Action<int> onGetOneRewardEvent = this.OnGetOneRewardEvent;
		if (onGetOneRewardEvent != null)
		{
			onGetOneRewardEvent(ID);
		}
	}
}
