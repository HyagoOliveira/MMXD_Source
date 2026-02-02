using System;
using UnityEngine;

[Serializable]
public class OrangeUIAnimationInfo
{
	public enum AnimationOpt
	{
		NONE = 0,
		MOVE = 1,
		SCALE = 2
	}

	public AnimationOpt mAnimationOpt;

	public Vector3 animationVal;

	public RectTransform Rt;

	public bool waitLast;

	public float Delay;

	public float PlayTime;

	public LeanTweenType LeanTweenType = LeanTweenType.linear;

	public bool Loop { get; set; }

	public bool Ignore { get; set; }

	public Vector3 OriginalPos { get; set; }
}
