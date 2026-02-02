using DragonBones;
using UnityEngine;

public class UI_Ready : OrangeUIBase
{
	private readonly string animationName = "newAnimation";

	[SerializeField]
	private UnityArmatureComponent armature;

	public bool Complete { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Complete = false;
	}

	public void Play()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.Play("BattleSE", 1);
		armature.AddEventListener("complete", PlayComplete);
		armature.animation.Play(animationName, 1);
	}

	private void PlayComplete(string type, EventObject eventObject)
	{
		armature.RemoveEventListener("complete", PlayComplete);
		Complete = true;
		OnClickCloseBtn();
	}
}
