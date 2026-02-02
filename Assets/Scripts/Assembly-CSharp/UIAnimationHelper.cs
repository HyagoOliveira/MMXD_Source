using System;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimationHelper
{
	public static void PlayGroup(OrangeUIAnimation[] arrGroup, bool reverse = false, Callback p_cb = null)
	{
		if (arrGroup != null && arrGroup.Length != 0)
		{
			foreach (OrangeUIAnimation orangeUIAnimation in arrGroup)
			{
				if (orangeUIAnimation.PlayAtStart)
				{
					Play(orangeUIAnimation, reverse, p_cb);
				}
			}
		}
		else
		{
			p_cb.CheckTargetToInvoke();
		}
	}

	public static void Play(OrangeUIAnimation p_Animation, bool reverse = false, Callback p_cb = null)
	{
		foreach (OrangeUIAnimationInfo item in p_Animation.listAnimation)
		{
			if (!item.Rt.gameObject.activeSelf)
			{
				item.Ignore = true;
			}
			else
			{
				item.Ignore = false;
			}
		}
		foreach (OrangeUIAnimationInfo item2 in p_Animation.listAnimation)
		{
			if (!item2.Ignore)
			{
				item2.Rt.gameObject.SetActive(false);
			}
		}
		OnPushAnim(ref p_Animation.listAnimation, reverse, 0, p_cb);
	}

	private static void OnPushAnim(ref List<OrangeUIAnimationInfo> p_listAnimations, bool reverse, int nowIdx, Callback p_cb = null)
	{
		List<OrangeUIAnimationInfo> listAnimations = p_listAnimations;
		int num = listAnimations.Count - 1;
		if (num < nowIdx)
		{
			p_cb.CheckTargetToInvoke();
			return;
		}
		bool flag = true;
		OrangeUIAnimationInfo p_target = listAnimations[nowIdx];
		if (nowIdx + 1 <= num && !listAnimations[nowIdx + 1].waitLast)
		{
			OnPushAnim(ref listAnimations, reverse, ++nowIdx, p_cb);
			flag = false;
		}
		Vector3 originalPos = p_target.OriginalPos;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Callback p_cb2 = null;
		switch (p_target.mAnimationOpt)
		{
		case OrangeUIAnimationInfo.AnimationOpt.MOVE:
			zero = new Vector3(originalPos.x + p_target.animationVal.x, originalPos.y + p_target.animationVal.y, originalPos.z + p_target.animationVal.z);
			zero2 = originalPos;
			if (flag)
			{
				p_cb2 = delegate
				{
					OnPushAnim(ref listAnimations, reverse, ++nowIdx, p_cb);
				};
			}
			if (reverse)
			{
				Vector3 vector2 = zero2;
				zero2 = zero;
				zero = vector2;
			}
			Move(ref p_target, zero, zero2, p_cb2);
			break;
		case OrangeUIAnimationInfo.AnimationOpt.SCALE:
			zero = p_target.animationVal;
			zero2 = p_target.Rt.localScale;
			if (flag)
			{
				p_cb2 = delegate
				{
					OnPushAnim(ref listAnimations, reverse, ++nowIdx, p_cb);
				};
			}
			if (reverse)
			{
				Vector3 vector = zero2;
				zero2 = zero;
				zero = vector;
			}
			else
			{
				Image img = p_target.Rt.GetComponent<Image>();
				if (img != null)
				{
					Color targetColor = img.color;
					img.color = Color.clear;
					LeanTween.value(img.gameObject, 0f, targetColor.a, p_target.PlayTime).setOnUpdate(delegate(float val)
					{
						img.color = new Color(targetColor.r, targetColor.g, targetColor.b, val);
					}).setDelay(p_target.Delay);
				}
			}
			Scale(ref p_target, zero, zero2, p_cb2);
			break;
		}
	}

	public static void Move(ref OrangeUIAnimationInfo p_target, Vector3 from, Vector3 to, Callback p_cb)
	{
		OrangeUIAnimationInfo target = p_target;
		target.Rt.anchoredPosition3D = from;
		if (!target.Ignore)
		{
			target.Rt.gameObject.SetActive(true);
		}
		LTDescr lTDescr = LeanTween.value(target.Rt.gameObject, from, to, target.PlayTime).setOnUpdate(delegate(Vector3 val)
		{
			target.Rt.anchoredPosition3D = val;
		}).setOnComplete((Action)delegate
		{
			p_cb.CheckTargetToInvoke();
		})
			.setEase(target.LeanTweenType)
			.setDelay(target.Delay);
		if (p_target.Loop)
		{
			lTDescr.setLoopPingPong();
		}
	}

	public static void Scale(ref OrangeUIAnimationInfo target, Vector3 from, Vector3 to, Callback p_cb)
	{
		target.Rt.transform.localScale = from;
		if (!target.Ignore)
		{
			target.Rt.gameObject.SetActive(true);
		}
		LTDescr lTDescr = LeanTween.scale(target.Rt, to, target.PlayTime).setOnComplete((Action)delegate
		{
			p_cb.CheckTargetToInvoke();
		}).setEase(target.LeanTweenType)
			.setDelay(target.Delay);
		if (target.Loop)
		{
			lTDescr.setLoopPingPong();
		}
	}
}
