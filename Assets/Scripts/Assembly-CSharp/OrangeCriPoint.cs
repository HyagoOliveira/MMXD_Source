using System;
using System.Collections.Generic;
using CriWare;
using OrangeAudio;
using UnityEngine;

[Serializable]
public class OrangeCriPoint
{
	public enum LOOPTYPE
	{
		NONE = 0,
		SE_LOOP = 1,
		PG_LOOP = 2,
		REMOVED = 3
	}

	[SerializeField]
	public AudioChannelType s_Channel = AudioChannelType.Sound;

	[SerializeField]
	public string m_acb = "";

	[SerializeField]
	public string m_cue = "";

	[SerializeField]
	public LOOPTYPE e_LoopType;

	[SerializeField]
	public long LoopTime = 500L;

	[SerializeField]
	public long DelayTime;

	[SerializeField]
	public OrangeTimer Timer;

	[SerializeField]
	public CueInfo CueInfo = new CueInfo();

	[SerializeField]
	private OrangeCriSource _SoundSource;

	public CriAtomExPlayer Player = new CriAtomExPlayer();

	public CriAtomExPlayback playerpb;

	private float currentVol = 1f;

	private Dictionary<LOOPTYPE, Action<OrangeCriPoint, bool>> PauseFuncs = new Dictionary<LOOPTYPE, Action<OrangeCriPoint, bool>>
	{
		{
			LOOPTYPE.NONE,
			PauseLoopSE
		},
		{
			LOOPTYPE.SE_LOOP,
			PauseLoopSE
		},
		{
			LOOPTYPE.PG_LOOP,
			PauseLoopPG
		},
		{
			LOOPTYPE.REMOVED,
			PauseLoopSE
		}
	};

	private Dictionary<LOOPTYPE, Action<OrangeCriPoint>> StopFuncs = new Dictionary<LOOPTYPE, Action<OrangeCriPoint>>
	{
		{
			LOOPTYPE.NONE,
			StopLoopSE
		},
		{
			LOOPTYPE.SE_LOOP,
			StopLoopSE
		},
		{
			LOOPTYPE.PG_LOOP,
			StopLoopPG
		},
		{
			LOOPTYPE.REMOVED,
			StopLoopSE
		}
	};

	private Dictionary<LOOPTYPE, Action<OrangeCriPoint, float>> UpdateFuncs = new Dictionary<LOOPTYPE, Action<OrangeCriPoint, float>>
	{
		{
			LOOPTYPE.NONE,
			UpdateNONE
		},
		{
			LOOPTYPE.SE_LOOP,
			UpdateLoopSE
		},
		{
			LOOPTYPE.PG_LOOP,
			UpdateLoopPG
		},
		{
			LOOPTYPE.REMOVED,
			UpdateLoopSE
		}
	};

	public OrangeCriSource sourceObj
	{
		get
		{
			return _SoundSource;
		}
		set
		{
			_SoundSource = value;
		}
	}

	public bool Pause
	{
		get
		{
			return playerpb.IsPaused();
		}
		set
		{
			PauseFuncs[e_LoopType](this, value);
		}
	}

	public float ChannelVolum
	{
		get
		{
			return MonoBehaviourSingleton<AudioManager>.Instance.GetVol((int)s_Channel);
		}
	}

	~OrangeCriPoint()
	{
		if (Player != null)
		{
			Player.Dispose();
		}
	}

	public void Reset()
	{
		sourceObj = null;
		if (Timer != null)
		{
			OrangeTimerManager.ReturnTimer(Timer);
		}
		Timer = null;
		s_Channel = AudioChannelType.Sound;
		m_acb = "";
		m_cue = "";
		e_LoopType = LOOPTYPE.NONE;
		LoopTime = 500L;
		if (Player != null)
		{
			Player.Pause(false);
			Player.Stop();
		}
		else
		{
			Player = new CriAtomExPlayer();
		}
	}

	public void Update(float volume = 0f)
	{
		if (sourceObj != null)
		{
			volume = sourceObj.f_vol;
		}
		currentVol = MonoBehaviourSingleton<AudioManager>.Instance.GetVol((int)s_Channel);
		UpdateFuncs[e_LoopType](this, volume * currentVol);
	}

	public void Stop()
	{
		StopFuncs[e_LoopType](this);
	}

	public CriAtomExPlayback Play(float maxdis, Transform trans, string s_acb, string cue)
	{
		CriAtomExAcb acb = MonoBehaviourSingleton<AudioManager>.Instance.GetAcb(s_acb, cue);
		if (acb == null)
		{
			return playerpb;
		}
		s_Channel = (AudioChannelType)MonoBehaviourSingleton<AudioManager>.Instance.GetChannel(s_acb);
		currentVol = MonoBehaviourSingleton<AudioManager>.Instance.GetVol((int)s_Channel);
		float num = OrangeCriSource.CalcVolume2MainOC(maxdis, trans) * currentVol;
		if (num <= 0f)
		{
			return playerpb;
		}
		m_acb = s_acb;
		m_cue = cue;
		Player.SetCue(acb, cue);
		Player.SetVolume(num);
		Player.Pause(false);
		playerpb = Player.Start();
		return playerpb;
	}

	public CriAtomExPlayback Play(string s_acb, string cuename)
	{
		s_Channel = (AudioChannelType)MonoBehaviourSingleton<AudioManager>.Instance.GetChannel(s_acb);
		currentVol = ChannelVolum;
		m_acb = s_acb;
		m_cue = cuename;
		Player.SetVolume(sourceObj.f_vol * currentVol);
		Player.Pause(false);
		playerpb = Player.Start();
		return playerpb;
	}

	private static void PauseLoopSE(OrangeCriPoint pt, bool sw)
	{
		pt.Player.Pause(sw);
	}

	private static void PauseLoopPG(OrangeCriPoint pt, bool sw)
	{
		if (sw)
		{
			pt.Timer.TimerPause();
		}
		else
		{
			pt.Timer.TimerResume();
		}
		pt.Player.Pause(sw);
	}

	private static void StopLoopSE(OrangeCriPoint pt)
	{
		pt.Player.Stop();
		pt.Reset();
	}

	private static void StopLoopPG(OrangeCriPoint pt)
	{
		pt.Player.Stop();
		pt.Reset();
	}

	private static void UpdateNONE(OrangeCriPoint pt, float v)
	{
	}

	private static void UpdateLoopSE(OrangeCriPoint pt, float v)
	{
		if (pt.playerpb.status == CriAtomExPlayback.Status.Removed)
		{
			pt.Player.Stop();
			pt.Reset();
		}
		else
		{
			pt.Player.SetVolume(v);
			pt.Player.Update(pt.playerpb);
		}
	}

	private static void UpdateLoopPG(OrangeCriPoint pt, float v)
	{
		if (pt.DelayTime != 0L)
		{
			if (pt.Timer.GetMillisecond() > pt.DelayTime)
			{
				pt.DelayTime = 0L;
				pt.Player.SetCue(MonoBehaviourSingleton<AudioManager>.Instance.GetAcb(pt.m_acb, pt.m_cue), pt.m_cue);
				pt.Player.SetVolume(v);
				if (v != 0f)
				{
					pt.playerpb = pt.Player.Start();
				}
				pt.Timer.TimerStart();
			}
		}
		else if (pt.Timer.GetMillisecond() > pt.LoopTime)
		{
			pt.playerpb.Stop(true);
			pt.Player.Stop();
			pt.Player.Update(pt.playerpb);
			pt.Player.SetCue(MonoBehaviourSingleton<AudioManager>.Instance.GetAcb(pt.m_acb, pt.m_cue), pt.m_cue);
			pt.Player.SetVolume(v);
			pt.Timer.TimerStart();
			if (v != 0f)
			{
				pt.playerpb = pt.Player.Start();
			}
		}
		else
		{
			pt.Player.SetVolume(v);
			pt.Player.Update(pt.playerpb);
		}
	}
}
