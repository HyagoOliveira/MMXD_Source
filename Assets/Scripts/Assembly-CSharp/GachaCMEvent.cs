using Cinemachine;
using UnityEngine;

public class GachaCMEvent : MonoBehaviour
{
	[SerializeField]
	private CinemachineVirtualCamera[] virtualCameras;

	[SerializeField]
	private Animation anim;

	[SerializeField]
	private GachaSceneController controller;

	[SerializeField]
	private GachaCapsule gachaCapsule;

	private const int DesignWidth = 1920;

	private const int DesignHeight = 1080;

	private void Awake()
	{
		CinemachineVirtualCamera[] array = virtualCameras;
		for (int i = 0; i < array.Length && OrangeGameUtility.SetNewFov(ref array[i].m_Lens.FieldOfView); i++)
		{
		}
	}

	public void PlayCM()
	{
		anim.Play("GachaCM_001");
	}

	public void PlayWhiteShiny()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShinyEffect", delegate(ShinyEffectUI ui)
		{
			ui.Setup();
		});
	}

	public void PlayRareFx()
	{
		gachaCapsule.PlayRareFx();
	}

	public void PlayComplete()
	{
		controller.OnDoorAnimStopped();
	}
}
