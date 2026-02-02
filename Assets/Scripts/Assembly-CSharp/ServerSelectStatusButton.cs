using UnityEngine;
using UnityEngine.UI;

internal class ServerSelectStatusButton : ScrollIndexCallback
{
	[SerializeField]
	private Image[] statusIndicator;

	[SerializeField]
	private Text zoneName;

	[SerializeField]
	private Text zoneExtra;

	private int m_index;

	private ServerSelectMainUI m_parentServerSelectMainUI;

	public override void ScrollCellIndex(int p_idx)
	{
		m_index = p_idx;
		m_parentServerSelectMainUI = GetComponentInParent<ServerSelectMainUI>();
		if ((bool)m_parentServerSelectMainUI)
		{
			GameServerZoneInfo zoneInfo = m_parentServerSelectMainUI.GetZoneInfo(m_index);
			zoneName.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetNameFromGameServerNameInfo(zoneInfo.Name);
			statusIndicator[0].gameObject.SetActive(false);
			statusIndicator[1].gameObject.SetActive(false);
			statusIndicator[2].gameObject.SetActive(false);
			bool num = ManagedSingleton<ServerStatusHelper>.Instance.IsZoneOffline(zoneInfo.ID);
			bool flag = ManagedSingleton<ServerStatusHelper>.Instance.IsZoneOverload(zoneInfo.ID);
			if (num)
			{
				statusIndicator[2].gameObject.SetActive(true);
			}
			else if (flag)
			{
				statusIndicator[1].gameObject.SetActive(true);
			}
			else
			{
				statusIndicator[0].gameObject.SetActive(true);
			}
			zoneExtra.text = "";
			Button componentInChildren = GetComponentInChildren<Button>();
			componentInChildren.onClick.RemoveAllListeners();
			componentInChildren.onClick.AddListener(delegate
			{
				m_parentServerSelectMainUI.ZoneSelectedCallback(m_index);
			});
		}
	}
}
