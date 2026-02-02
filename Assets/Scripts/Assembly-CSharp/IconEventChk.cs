using System.Collections.Generic;
using UnityEngine;
using enums;

public class IconEventChk : MonoBehaviour
{
	[SerializeField]
	private bool isAny;

	[SerializeField]
	private StageType chkType;

	public void Awake()
	{
		if (isAny)
		{
			long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			List<EVENT_TABLE> eventTableByType = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_BONUS, serverUnixTimeNowUTC);
			base.gameObject.SetActive(eventTableByType != null && eventTableByType.Count > 0);
		}
		else
		{
			base.gameObject.SetActive(ManagedSingleton<OrangeTableHelper>.Instance.IsAnyEventBonusByType(chkType));
		}
	}
}
