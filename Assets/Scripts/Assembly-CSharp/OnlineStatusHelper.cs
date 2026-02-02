using UnityEngine;
using UnityEngine.UI;

public class OnlineStatusHelper : MonoBehaviour
{
	private static Color STATUS_COLOR_OFFLINE = new Color32(183, 183, 183, byte.MaxValue);

	private static Color STATUS_COLOR_ONLINE = new Color32(107, 194, 222, byte.MaxValue);

	[SerializeField]
	private GameObject _onlineIcon;

	[SerializeField]
	private GameObject _offlineIcon;

	[SerializeField]
	private Text _textStatus;

	public void SetOnlineStatus(int busyState)
	{
		string statusMessage;
		if (ManagedSingleton<PlayerHelper>.Instance.GetOnlineStatus(busyState, out statusMessage))
		{
			_offlineIcon.SetActive(false);
			_onlineIcon.SetActive(true);
			_textStatus.color = STATUS_COLOR_ONLINE;
		}
		else
		{
			_offlineIcon.SetActive(true);
			_onlineIcon.SetActive(false);
			_textStatus.color = STATUS_COLOR_OFFLINE;
		}
		_textStatus.text = statusMessage;
	}
}
