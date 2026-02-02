using System.Collections.Generic;
using UnityEngine;

public class FreeAutoAimSystem : MonoBehaviourSingleton<FreeAutoAimSystem>
{
	private static List<Transform> EventSpots;

	public static Transform GetTarget(string str)
	{
		GameObject gameObject = GameObject.Find(str);
		if (!gameObject)
		{
			return null;
		}
		return gameObject.transform;
	}

	public static Transform GetClosestEventSpot(Transform source)
	{
		if (EventSpots.Count == 0)
		{
			return null;
		}
		Transform transform = EventSpots[0];
		foreach (Transform eventSpot in EventSpots)
		{
			if (Mathf.Abs(eventSpot.position.y - source.position.y) < Mathf.Abs(transform.position.y - source.position.y))
			{
				transform = eventSpot;
			}
		}
		return transform;
	}

	public void Init()
	{
		EventSpots = new List<Transform>();
	}

	private void Awake()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.SCENE_INIT, Init);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.SCENE_INIT, Init);
		EventSpots.Clear();
	}

	public static void AddEventSpot(ref Transform target)
	{
		EventSpots.Add(target);
	}

	public static void RemoveEventSpot(ref Transform target)
	{
		EventSpots.Remove(target);
	}
}
