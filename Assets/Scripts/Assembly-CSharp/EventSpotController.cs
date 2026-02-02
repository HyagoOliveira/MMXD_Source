using UnityEngine;

public class EventSpotController : MonoBehaviour
{
	private Transform _transform;

	private void Start()
	{
		_transform = base.transform;
		FreeAutoAimSystem.AddEventSpot(ref _transform);
	}

	private void OnDestroy()
	{
		FreeAutoAimSystem.RemoveEventSpot(ref _transform);
	}
}
