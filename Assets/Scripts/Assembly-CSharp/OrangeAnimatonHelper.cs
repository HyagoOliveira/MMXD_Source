#define RELEASE
using System.Collections.Generic;
using UnityEngine;
using enums;

public static class OrangeAnimatonHelper
{
	public static string[] GetStandByClips(string modelName, string animator, WeaponType weaponType, out string bundleName)
	{
		List<string> list = new List<string>();
		string format = "{0}_ui_idle_{1}_loop";
		bundleName = "model/animation/{0}/{1}";
		string empty = string.Empty;
		string arg = string.Empty;
		string empty3 = string.Empty;
		empty = AnimatorShort(animator);
		switch (weaponType)
		{
		case WeaponType.Buster:
			arg = "buster";
			break;
		case WeaponType.Spray:
			arg = "spray";
			break;
		case WeaponType.SprayHeavy:
			arg = "sprayheavy";
			break;
		case WeaponType.Melee:
			arg = "saber";
			break;
		case WeaponType.DualGun:
			arg = "dualgun";
			break;
		case WeaponType.MGun:
			arg = "mgun";
			break;
		case WeaponType.Gatling:
			arg = "gatling";
			break;
		case WeaponType.Launcher:
			arg = "launcher";
			break;
		}
		string empty2 = string.Empty;
		for (int i = 0; i < 3; i++)
		{
			empty2 = string.Format(format, arg, (i + 1).ToString("00"));
			list.Add(empty2);
		}
		bundleName = string.Format(bundleName, arg, empty);
		return list.ToArray();
	}

	public static string[] GetEyesClips(string animator, out string bundleName)
	{
		string[] result = new string[3] { "face_eye_open", "face_eye_wink", "face_eye_close" };
		string text = AnimatorShort(animator);
		bundleName = "model/animation/face/" + text;
		return result;
	}

	public static string[] GetUniqueDebutName(string s_modelName, out string bundleName)
	{
		bundleName = "model/animation/character/" + s_modelName;
		return new string[3]
		{
			s_modelName.Replace("_000", "") + "_ui_debut_start",
			s_modelName.Replace("_000", "") + "_ui_debut_loop",
			s_modelName.Replace("_000", "") + "_ui_debut_loop_egg"
		};
	}

	public static AnimatorOverrideController OverrideRuntimeAnimClip(ref RuntimeAnimatorController runtimeAnimatorController, ref string bundleName, ref string[] clips)
	{
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
		animatorOverrideController.runtimeAnimatorController = runtimeAnimatorController;
		for (int i = 0; i < clips.Length; i++)
		{
			animatorOverrideController[(i + 1).ToString()] = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundleName, clips[i]);
		}
		return animatorOverrideController;
	}

	public static void OverrideRuntimeAnimClip(string clipHeader, ref AnimatorOverrideController overrideController, ref string bundleName, ref string[] clips)
	{
		for (int i = 0; i < clips.Length; i++)
		{
			overrideController[clipHeader + (i + 1)] = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundleName, clips[i]);
		}
	}

	public static string AnimatorShort(string animator)
	{
		if (animator.Contains("classic"))
		{
			return "c";
		}
		if (animator.Contains("female"))
		{
			return "f";
		}
		if (animator.Contains("male"))
		{
			return "m";
		}
		Debug.LogError("未知的animatorType : " + animator);
		return "m";
	}
}
