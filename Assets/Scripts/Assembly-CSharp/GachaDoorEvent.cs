using System;
using CallbackDefs;
using UnityEngine;

public class GachaDoorEvent : MonoBehaviour
{
	private readonly int i_FlowX = Shader.PropertyToID("_FlowX");

	private readonly int i_DiffuseTex = Shader.PropertyToID("_DiffuseTex");

	private readonly string ANIMATION_NAME_SR = "GachaDoorSR";

	private readonly string ANIMATION_NAME_SSR = "GachaDoorSSR";

	private readonly string ANIMATION_NAME_SSR2 = "GachaDoorSSR2";

	[SerializeField]
	private Renderer[] renderers;

	[SerializeField]
	private Texture[] RareTex;

	private MaterialPropertyBlock mpb;

	private Animation anim;

	private string useAnimationName = string.Empty;

	private SystemSE DoorOpenSE;

	public GachaSceneController GachaSceneController { get; set; }

	private void Awake()
	{
		mpb = new MaterialPropertyBlock();
		anim = GetComponent<Animation>();
	}

	public void SetDoorInfo(int rare, bool special, bool atos = false)
	{
		if (special)
		{
			useAnimationName = ANIMATION_NAME_SSR2;
			mpb.SetTexture(i_DiffuseTex, RareTex[0]);
			mpb.SetFloat(i_FlowX, 0f);
			DoorOpenSE = SystemSE.CRI_SYSTEMSE_SYS_GACHA01_S3;
		}
		else if (rare >= 5)
		{
			useAnimationName = ANIMATION_NAME_SSR;
			mpb.SetTexture(i_DiffuseTex, RareTex[1]);
			mpb.SetFloat(i_FlowX, 0.3f);
			DoorOpenSE = SystemSE.CRI_SYSTEMSE_SYS_GACHA01_S1;
		}
		else
		{
			useAnimationName = ANIMATION_NAME_SR;
			mpb.SetTexture(i_DiffuseTex, RareTex[0]);
			mpb.SetFloat(i_FlowX, 0f);
			if (atos)
			{
				DoorOpenSE = SystemSE.CRI_SYSTEMSE_SYS_GACHA01_S2;
			}
			else
			{
				DoorOpenSE = SystemSE.CRI_SYSTEMSE_SYS_GACHA01_A;
			}
		}
		Renderer[] array = renderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetPropertyBlock(mpb, 1);
		}
	}

	public void PlayAnim()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(DoorOpenSE);
		if ((bool)anim)
		{
			anim.Play(useAnimationName);
		}
		else
		{
			GachaSceneController.SetDoorOpen();
		}
	}

	public void OnAnimationDoorOpen()
	{
		GachaSceneController.SetDoorOpen();
	}

	public void OnDoorRestart()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GachaTap", delegate(GachaTapUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				SetDoorInfo(5, false);
				PlayAnim();
			});
		});
	}
}
