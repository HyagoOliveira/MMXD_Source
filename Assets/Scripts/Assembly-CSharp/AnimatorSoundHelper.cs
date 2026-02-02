using System;
using StageLib;
using UnityEngine;

public class AnimatorSoundHelper : MonoBehaviour
{
	[Serializable]
	public class CallbackDic : UnitySerializedDictionary<string, Action>
	{
	}

	private CallbackDic dicCallback = new CallbackDic();

	[SerializeField]
	public OrangeCriSource SoundSource;

	public bool visible
	{
		get
		{
			return SoundSource.IsVisiable;
		}
	}

	private void Awake()
	{
	}

	public void PlayBGM(int id)
	{
		SoundSource.PlaySE("BossSE", id);
	}

	public void PlayEnemySE(int id)
	{
		if (visible && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && !TurtorialUI.IsTutorialing() && !StageUpdate.IsRewardUI())
		{
			SoundSource.PlaySE("EnemySE", id);
		}
	}

	public void PlayBossSE(int id)
	{
		if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && !StageUpdate.IsRewardUI())
		{
			SoundSource.PlaySE("BossSE", id);
		}
	}

	public void PlayBossSESP(string param)
	{
		if (!StageUpdate.IsRewardUI())
		{
			string[] array = param.Split(',');
			if (array.Length > 1 && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && !StageUpdate.IsRewardUI())
			{
				SoundSource.PlaySE(array[0], array[1]);
			}
		}
	}

	public void PlayBattleSE(int id)
	{
		if (visible && !StageUpdate.IsRewardUI())
		{
			SoundSource.PlaySE("BattleSE", id);
		}
	}

	public void PlaySE(string param)
	{
		if (StageUpdate.IsRewardUI())
		{
			return;
		}
		string[] SE = param.Split(',');
		if (SE.Length <= 1)
		{
			return;
		}
		if (MonoBehaviourSingleton<AudioManager>.Instance.GetAcb(SE[0], SE[1]) == null)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(SE[0], 3, delegate
			{
				SoundSource.PlaySE(SE[0], SE[1]);
			});
		}
		else
		{
			SoundSource.PlaySE(SE[0], SE[1]);
		}
	}

	public void PlayVoice(string param)
	{
		string[] voice = param.Split(',');
		if (voice.Length > 1)
		{
			int p_channel = (voice[0].StartsWith("VOICE") ? 3 : 2);
			MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(voice[0], p_channel, delegate
			{
				SoundSource.PlaySE(voice[0], voice[1]);
			});
		}
	}

	public void AddCallBack(string key, Action pcb)
	{
		dicCallback.Add(key, pcb);
	}

	public void CallBack(string key)
	{
		if (dicCallback.ContainsKey(key))
		{
			dicCallback[key]();
		}
	}

	public void AnimationEvent_PlaySE(AnimationEvent e)
	{
		string[] array = e.stringParameter.Split(',');
		if (array.Length > 1)
		{
			SoundSource.PlaySE(array[0], array[1]);
		}
	}

	public void AnimationEvent_PlaySE_UI(AnimationEvent e)
	{
		string[] SE = e.stringParameter.Split(',');
		if (SE.Length <= 1)
		{
			return;
		}
		if (SoundSource == null)
		{
			int p_channel = (SE[0].StartsWith("VOICE") ? 3 : 2);
			MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(SE[0], p_channel, delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.Play(SE[0], SE[1]);
			});
		}
		else
		{
			SoundSource.ForcePlaySE(SE[0], SE[1]);
		}
	}
}
