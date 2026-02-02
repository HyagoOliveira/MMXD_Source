using DragonBones;
using UnityEngine;

public class OrangeDBSoundEvent : MonoBehaviour
{
	[SerializeField]
	private DBSoundEvent[] dbEvents = new DBSoundEvent[0];

	private UnityArmatureComponent armature;

	private bool visible;

	private void Awake()
	{
		armature = GetComponent<UnityArmatureComponent>();
		if (dbEvents.Length != 0 && armature != null)
		{
			armature.AddEventListener("frameEvent", FrameEvent);
		}
	}

	private void FrameEvent(string type, EventObject e)
	{
		if (!visible)
		{
			return;
		}
		DBSoundEvent[] array = dbEvents;
		foreach (DBSoundEvent dBSoundEvent in array)
		{
			if (e.name.Equals(dBSoundEvent.EventName))
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE((SystemSE)dBSoundEvent.SeCudID);
				break;
			}
		}
	}

	private void OnEnable()
	{
		visible = true;
	}

	private void OnDisable()
	{
		visible = false;
	}
}
