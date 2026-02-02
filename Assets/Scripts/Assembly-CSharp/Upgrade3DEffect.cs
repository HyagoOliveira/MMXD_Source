using DragonBones;
using UnityEngine;

public class Upgrade3DEffect : MonoBehaviour
{
	public void Play(Vector3 position = default(Vector3))
	{
		UnityArmatureComponent component = GetComponent<UnityArmatureComponent>();
		if ((bool)component)
		{
			base.gameObject.SetActive(true);
			base.transform.position = position;
			component.AddEventListener("complete", EffectComplete);
			component.animation.Reset();
			component.animation.Play("newAnimation", 1);
		}
	}

	private void EffectComplete(string type, EventObject eventObject)
	{
		base.gameObject.SetActive(false);
	}
}
