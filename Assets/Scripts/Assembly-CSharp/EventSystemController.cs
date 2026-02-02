using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventSystem))]
public class EventSystemController : MonoBehaviour
{
	private int tweenUid = -1;

	private EventSystem eventSystem;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		eventSystem = GetComponent<EventSystem>();
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			LeanTween.cancel(base.gameObject, tweenUid);
			eventSystem.enabled = false;
			tweenUid = LeanTween.delayedCall(base.gameObject, 0.3f, SetEventSystemEnable).uniqueId;
		}
	}

	private void SetEventSystemEnable()
	{
		tweenUid = -1;
		eventSystem.enabled = true;
	}
}
