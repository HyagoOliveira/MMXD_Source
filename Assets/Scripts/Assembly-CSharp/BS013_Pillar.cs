using UnityEngine;

public class BS013_Pillar : MonoBehaviour
{
	public int nSetID;

	private void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
	}

	public void CallOnByID(EventManager.StageEventCall tStageEventCall)
	{
		int nID = tStageEventCall.nID;
		if (nSetID == nID)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
