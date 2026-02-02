using UnityEngine;

public class KochavaDemo : MonoBehaviour
{
	public void SendEvent()
	{
		Kochava.Tracker.SendEvent(new Kochava.Event(Kochava.EventType.Purchase)
		{
			name = "Gold Token",
			price = 0.99
		});
	}
}
