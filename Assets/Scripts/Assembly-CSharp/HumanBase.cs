#define RELEASE
using System;
using StageLib;
using UnityEngine;
using enums;

public class HumanBase
{
	public enum AnimateId : uint
	{
		ANI_STAND = 0u,
		ANI_RIDEARMOR = 1u,
		ANI_WALK = 2u,
		ANI_WALKBACK = 3u,
		ANI_DASH = 4u,
		ANI_DASH_END = 5u,
		ANI_SLIDE = 6u,
		ANI_SLIDE_END = 7u,
		ANI_JUMP = 8u,
		ANI_FALL = 9u,
		ANI_LAND = 10u,
		ANI_WALLGRAB_BEGIN = 11u,
		ANI_WALLGRAB = 12u,
		ANI_WALLGRAB_END = 13u,
		ANI_WALLGRAB_SLASH = 14u,
		ANI_WALLGRAB_SLASH_END = 15u,
		ANI_WALLKICK = 16u,
		ANI_WALLKICK_END = 17u,
		ANI_CROUCH = 18u,
		ANI_CROUCH_END = 19u,
		ANI_CROUCH_UP = 20u,
		ANI_AIRDASH_END = 21u,
		ANI_STEP = 22u,
		ANI_HURT_BEGIN = 23u,
		ANI_HURT_LOOP = 24u,
		ANI_HURT_END = 25u,
		ANI_CLASSIC_SLASH1 = 26u,
		ANI_CLASSIC_SLASH2 = 27u,
		ANI_CLASSIC_SLASH3 = 28u,
		ANI_SLASH1 = 29u,
		ANI_SLASH2 = 30u,
		ANI_SLASH3 = 31u,
		ANI_SLASH4 = 32u,
		ANI_SLASH5 = 33u,
		ANI_WALKSLASH1 = 34u,
		ANI_WALKSLASH2 = 35u,
		ANI_WALKSLASH1_END = 36u,
		ANI_WALKSLASH2_END = 37u,
		ANI_CLASSIC_SLASH1_END = 38u,
		ANI_CLASSIC_SLASH2_END = 39u,
		ANI_CLASSIC_SLASH3_END = 40u,
		ANI_SLASH1_END = 41u,
		ANI_SLASH2_END = 42u,
		ANI_SLASH3_END = 43u,
		ANI_SLASH4_END = 44u,
		ANI_SLASH5_END = 45u,
		ANI_JUMPSLASH = 46u,
		ANI_DASHSLASH1 = 47u,
		ANI_DASHSLASH2 = 48u,
		ANI_DASHSLASH1_END = 49u,
		ANI_DASHSLASH2_END = 50u,
		ANI_CROUCHSLASH1 = 51u,
		ANI_CROUCHSLASH1_END = 52u,
		ANI_TELEPORT_IN_POSE = 53u,
		ANI_WIN_POSE = 54u,
		ANI_TELEPORT_OUT_POSE = 55u,
		ANI_GIGA_STAND_START = 56u,
		ANI_GIGA_STAND_END = 57u,
		ANI_GIGA_JUMP_START = 58u,
		ANI_GIGA_JUMP_END = 59u,
		ANI_GIGA_CROUCH_START = 60u,
		ANI_GIGA_CROUCH_END = 61u,
		ANI_SUMMON0 = 62u,
		ANI_SUMMON1 = 63u,
		ANI_SUMMON2 = 64u,
		ANI_SKILL_START = 65u,
		ANI_SKILL_END = 115u,
		ANI_BLEND_SKILL_START = 116u,
		ANI_BLEND_SKILL_END = 126u,
		ANI_BTSKILL_START = 127u,
		ANI_BTSKILL_END = 142u,
		ANI_STAND_SKILL = 143u,
		ANI_LOGOUT2 = 144u,
		MAX_ANI = 145u
	}

	public delegate void OutAction<T1, T2>(out T1 arg1, out T2 arg2);

	public enum UniqueBattlePose
	{
		login = 0,
		logout = 1,
		win = 2,
		dive_trigger_stand_start = 3,
		dive_trigger_stand_end = 4,
		dive_trigger_jump_start = 5,
		dive_trigger_jump_end = 6,
		dive_trigger_crouch_start = 7,
		dive_trigger_crouch_end = 8,
		logout2 = 9
	}

	public static readonly string[] commonMotionList = new string[7] { "_damage_armor_end", "_damage_armor_loop", "_damage_armor_start", "_damage_end", "_damage_loop", "_damage_start", "_ride_loop" };

	public static readonly string[] classicMotionList = new string[12]
	{
		"_slide_atk_down_end", "_slide_atk_down_loop", "_slide_atk_down_start", "_slide_atk_mid_end", "_slide_atk_mid_loop", "_slide_atk_mid_start", "_slide_atk_up_end", "_slide_atk_up_loop", "_slide_atk_up_start", "_slide_end",
		"_slide_loop", "_slide_start"
	};

	public static readonly string[] motionList = new string[86]
	{
		"_backward_atk_down_loop", "_backward_atk_mid_loop", "_backward_atk_up_loop", "_crouch_atk_down", "_crouch_atk_down_end", "_crouch_atk_down_start", "_crouch_atk_mid", "_crouch_atk_mid_end", "_crouch_atk_mid_start", "_crouch_atk_up",
		"_crouch_atk_up_end", "_crouch_atk_up_start", "_crouch_end", "_crouch_loop", "_crouch_start", "_dash_atk_down_end", "_dash_atk_down_loop", "_dash_atk_down_start", "_dash_atk_mid_end", "_dash_atk_mid_loop",
		"_dash_atk_mid_start", "_dash_atk_up_end", "_dash_atk_up_loop", "_dash_atk_up_start", "_dash_end", "_dash_loop", "_dash_start", "_fall_atk_down", "_fall_atk_mid", "_fall_atk_up",
		"_fall_loop", "_jump_atk_down", "_jump_atk_down_start", "_jump_atk_mid", "_jump_atk_mid_start", "_jump_atk_up", "_jump_atk_up_start", "_jump_loop", "_jump_start", "_jump_to_fall",
		"_jump_to_fall_atk_down", "_jump_to_fall_atk_mid", "_jump_to_fall_atk_up", "_landing", "_landing_atk_down", "_landing_atk_mid", "_landing_atk_up", "_run_atk_down_loop", "_run_atk_down_start", "_run_atk_mid_loop",
		"_run_atk_mid_start", "_run_atk_up_loop", "_run_atk_up_start", "_run_loop", "_run_start", "_stand_atk_down", "_stand_atk_mid", "_stand_atk_up", "_stand_loop", "_stand_weak_loop",
		"_ui_idle_01_loop", "_ui_idle_02_loop", "_ui_idle_03_loop", "_wallgrab_atk_down", "_wallgrab_atk_down_start", "_wallgrab_atk_down_step", "_wallgrab_atk_mid", "_wallgrab_atk_mid_start", "_wallgrab_atk_mid_step", "_wallgrab_atk_up",
		"_wallgrab_atk_up_start", "_wallgrab_atk_up_step", "_wallgrab_loop", "_wallgrab_start", "_wallgrab_step", "_walljump_atk_down", "_walljump_atk_down_start", "_walljump_atk_mid", "_walljump_atk_mid_start", "_walljump_atk_up",
		"_walljump_atk_up_start", "_walljump_loop", "_walljump_start", "_enemy_summon_0", "_enemy_summon_1", "_enemy_summon_2"
	};

	public static readonly string[] classicMeleeMotionList = new string[6] { "_slide_atk_end", "_slide_atk_loop", "_slide_atk_start", "_slide_end", "_slide_loop", "_slide_start" };

	public static readonly string[] meleeMotionList = new string[51]
	{
		"_crouch_atk_end", "_crouch_atk_start", "_crouch_end", "_crouch_loop", "_crouch_start", "_dash_atk_end", "_dash_atk_loop", "_dash_atk_start", "_dash_end", "_dash_loop",
		"_dash_start", "_fall_loop", "_jump_atk_end", "_jump_atk_loop", "_jump_atk_start", "_jump_loop", "_jump_start", "_jump_to_fall", "_landing", "_run_atk_1",
		"_run_atk_2", "_run_loop", "_run_start", "_stand_atk1_end", "_stand_atk1_start", "_stand_atk2_end", "_stand_atk2_start", "_stand_atk3_end", "_stand_atk3_start", "_stand_atk4_end",
		"_stand_atk4_start", "_stand_atk5_end", "_stand_atk5_start", "_stand_classic_atk1_end", "_stand_classic_atk1_start", "_stand_classic_atk2_end", "_stand_classic_atk2_start", "_stand_classic_atk3_end", "_stand_classic_atk3_start", "_stand_loop",
		"_stand_weak_loop", "_ui_idle_01_loop", "_ui_idle_02_loop", "_ui_idle_03_loop", "_wallgrab_atk_end", "_wallgrab_atk_start", "_wallgrab_loop", "_wallgrab_start", "_wallgrab_step", "_walljump_loop",
		"_walljump_start"
	};

	public static string GetWeaponTypeName(WeaponType weaponType)
	{
		switch (weaponType)
		{
		case WeaponType.Dummy:
			return "normal";
		case WeaponType.Buster:
			return "buster";
		case WeaponType.Spray:
			return "spray";
		case WeaponType.SprayHeavy:
			return "sprayheavy";
		case WeaponType.Melee:
			return "saber";
		case WeaponType.DualGun:
			return "dualgun";
		case WeaponType.MGun:
			return "mgun";
		case WeaponType.Gatling:
			return "gatling";
		case WeaponType.Launcher:
			return "launcher";
		default:
			Debug.LogError("發現未知的武器類型 " + weaponType);
			return "dummy";
		}
	}

	public static string GetWeaponMotionBundlePath(string animatorType, WeaponType weaponType)
	{
		if (animatorType.Contains("classic"))
		{
			animatorType = "c";
		}
		else if (animatorType.Contains("female"))
		{
			animatorType = "f";
		}
		else if (animatorType.Contains("male"))
		{
			animatorType = "m";
		}
		else
		{
			animatorType = "m";
			Debug.LogError("未知的animatorType : " + animatorType);
		}
		string weaponTypeName = GetWeaponTypeName(weaponType);
		return string.Format("model/animation/{0}/{1}", weaponTypeName, animatorType);
	}

	public static void LoadMotion(StageUpdate.LoadCallBackObj tNewLoad, string animatorType, WeaponType weaponType)
	{
		string weaponMotionBundlePath = GetWeaponMotionBundlePath(animatorType, weaponType);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(new string[1] { weaponMotionBundlePath }, tNewLoad.LoadCBNoParam, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
	}

	public static void SetMotion(ref AnimatorOverrideController animator, string animatorType, WeaponType weaponType)
	{
		string weaponMotionBundlePath = GetWeaponMotionBundlePath(animatorType, weaponType);
		string weaponTypeName = GetWeaponTypeName(weaponType);
		string[] array;
		switch (weaponType)
		{
		case WeaponType.Dummy:
			array = commonMotionList;
			foreach (string text5 in array)
			{
				string text6 = weaponTypeName + text5;
				animator[text6] = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(weaponMotionBundlePath, text6);
			}
			return;
		case WeaponType.Melee:
			array = meleeMotionList;
			foreach (string text in array)
			{
				string text2 = weaponTypeName + text;
				animator[text2] = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(weaponMotionBundlePath, text2);
			}
			if (animatorType.Contains("classic"))
			{
				array = classicMeleeMotionList;
				foreach (string text3 in array)
				{
					string text4 = weaponTypeName + text3;
					animator[text4] = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(weaponMotionBundlePath, text4);
				}
			}
			return;
		}
		array = motionList;
		foreach (string text7 in array)
		{
			string text8 = weaponTypeName + text7;
			animator[text8] = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(weaponMotionBundlePath, text8);
		}
		if (animatorType.Contains("classic"))
		{
			array = classicMotionList;
			foreach (string text9 in array)
			{
				string text10 = weaponTypeName + text9;
				animator[text10] = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(weaponMotionBundlePath, text10);
			}
		}
	}

	public static string[] GetUniqueBattlePose(string s_modelName, out string bundleName, out string[] motionName)
	{
		motionName = Enum.GetNames(typeof(UniqueBattlePose));
		string[] array = new string[motionName.Length];
		bundleName = "model/animation/character/" + s_modelName;
		for (int i = 0; i < motionName.Length; i++)
		{
			array[i] = s_modelName.Replace("_000", "") + "_" + motionName[i];
		}
		return array;
	}

	public static void SetUniqueMotion(ref AnimatorOverrideController overrideController, string bundle, OutAction<string[], string[]> GetUniqueMotion)
	{
		string[] arg;
		string[] arg2;
		GetUniqueMotion(out arg, out arg2);
		if (arg.Length == 0 || !(arg[0] == "null"))
		{
			for (int i = 0; i < arg2.Length; i++)
			{
				overrideController[arg[i]] = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundle, arg2[i]);
			}
		}
	}

	public static void SetUniqueWeaponMotion(ref AnimatorOverrideController overrideController, string animatorType, WeaponType weaponType, OutAction<string[], string[]> GetUniqueWeaponMotion)
	{
		string[] arg;
		string[] arg2;
		GetUniqueWeaponMotion(out arg, out arg2);
		if (arg.Length != 0 && arg[0] == "null")
		{
			return;
		}
		string weaponMotionBundlePath = GetWeaponMotionBundlePath(animatorType, weaponType);
		string weaponTypeName = GetWeaponTypeName(weaponType);
		for (int i = 0; i < arg2.Length; i++)
		{
			string text = weaponTypeName + arg2[i];
			AnimationClip assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(weaponMotionBundlePath, text);
			if (assstSync != null)
			{
				overrideController[weaponTypeName + arg[i]] = assstSync;
			}
			else
			{
				Debug.LogError(text + " in " + weaponMotionBundlePath + " is null");
			}
		}
	}
}
