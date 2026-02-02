using CallbackDefs;
using CriWare;

namespace OrangeAudio
{
	public class AudioLib
	{
		public static string GetVoice(ref CHARACTER_TABLE character)
		{
			return "VOICE_" + character.s_VOICE;
		}

		public static string GetSkillSE(ref CHARACTER_TABLE character)
		{
			return "SkillSE_" + character.s_SE_SKILL;
		}

		public static string GetCharaSE(ref CHARACTER_TABLE character)
		{
			return "CharaSE_" + character.s_SE_CHARA;
		}

		public static void LoadVoice(ref CHARACTER_TABLE character, Callback p_cb)
		{
			if (CriAtom.GetCueSheet(GetVoice(ref character)) != null)
			{
				p_cb.CheckTargetToInvoke();
				return;
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(GetVoice(ref character), 3, delegate
			{
				p_cb.CheckTargetToInvoke();
			});
		}

		public static void LoadCharaSE(ref CHARACTER_TABLE character, Callback p_cb)
		{
			if (CriAtom.GetCueSheet(GetCharaSE(ref character)) != null)
			{
				p_cb.CheckTargetToInvoke();
				return;
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(GetCharaSE(ref character), 2, delegate
			{
				p_cb.CheckTargetToInvoke();
			});
		}

		public static void LoadSkillSE(ref CHARACTER_TABLE character, Callback p_cb)
		{
			if (CriAtom.GetCueSheet(GetSkillSE(ref character)) != null)
			{
				p_cb.CheckTargetToInvoke();
				return;
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(GetSkillSE(ref character), 2, delegate
			{
				p_cb.CheckTargetToInvoke();
			});
		}
	}
}
