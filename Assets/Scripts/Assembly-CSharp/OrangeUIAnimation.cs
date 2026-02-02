using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using UnityEngine;

public class OrangeUIAnimation : MonoBehaviour
{
	public bool PlayAtStart;

	public bool Loop;

	[BoxGroup("Animation Info")]
	public List<OrangeUIAnimationInfo> listAnimation;

	private bool IsPlaying { get; set; }

	private void Awake()
	{
		foreach (OrangeUIAnimationInfo item in listAnimation)
		{
			item.OriginalPos = item.Rt.anchoredPosition3D;
			item.Loop = Loop;
		}
	}

	private void Start()
	{
		if (PlayAtStart)
		{
			PlayAnimation();
		}
	}

	public void PlayAnimation(Callback p_cb = null)
	{
		UIAnimationHelper.Play(this, false, p_cb);
	}

	public void PlayRevertAnimation(Callback p_cb = null)
	{
		UIAnimationHelper.Play(this, true, p_cb);
	}
}
