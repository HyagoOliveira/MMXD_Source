#define RELEASE
using System;
using UnityEngine;

public class BGMChannel : AudioChannelBase
{
	private float duration = 0.5f;

	private AudioSource currentAudio;

	private AudioClip nextClip;

	private AudioClip lopClip;

	private LeanTween fadeTweener;

	public string PlayingAudioName
	{
		get
		{
			if (!(currentAudio != null))
			{
				return null;
			}
			return currentAudio.name;
		}
	}

	protected override void OnStopAudioClip(AudioClip clip)
	{
		if (currentAudio != null)
		{
			currentAudio.Stop();
			currentAudio.clip = null;
		}
	}

	private void Update()
	{
		if (currentAudio != null && currentAudio.enabled && !currentAudio.isPlaying && currentAudio.clip != null && nextClip == null)
		{
			if (lopClip != null)
			{
				currentAudio.clip = lopClip;
				lopClip = null;
			}
			currentAudio.Play();
		}
	}

	protected override void OnPlayAudioClip(AudioClip clip, AudioClip clip1)
	{
		if (currentAudio == null)
		{
			currentAudio = base.gameObject.AddComponent<AudioSource>();
		}
		if (clip == currentAudio.clip)
		{
			return;
		}
		nextClip = clip;
		lopClip = clip1;
		StopFadeTweener();
		if (currentAudio.isPlaying && !currentAudio.mute)
		{
			LeanTween.value(base.gameObject, currentAudio.volume, 0f, duration).setOnUpdate(delegate(float val)
			{
				currentAudio.volume = val;
			}).setOnComplete((Action)delegate
			{
				FadeOutEndHandler();
			});
		}
		else
		{
			FadeOutEndHandler();
		}
	}

	protected override void OnPlayAudioClip(AudioClip clip)
	{
		if (currentAudio == null)
		{
			currentAudio = base.gameObject.AddComponent<AudioSource>();
		}
		if (clip == currentAudio.clip)
		{
			return;
		}
		nextClip = clip;
		StopFadeTweener();
		if (currentAudio.isPlaying && !currentAudio.mute)
		{
			LeanTween.value(base.gameObject, currentAudio.volume, 0f, duration).setOnUpdate(delegate(float val)
			{
				currentAudio.volume = val;
			}).setOnComplete((Action)delegate
			{
				FadeOutEndHandler();
			});
		}
		else
		{
			FadeOutEndHandler();
		}
	}

	private void StopFadeTweener()
	{
		if (!(fadeTweener == null))
		{
			fadeTweener.enabled = false;
		}
	}

	private void FadeOutEndHandler()
	{
		if (fadeTweener != null)
		{
			fadeTweener = null;
		}
		currentAudio.Stop();
		if (nextClip != null)
		{
			currentAudio.loop = false;
			currentAudio.clip = nextClip;
			currentAudio.volume = 0.011f;
			currentAudio.playOnAwake = false;
			currentAudio.mute = base.IsMute;
			nextClip = null;
			currentAudio.Play();
			LeanTween.value(base.gameObject, 0f, base.Volume, duration).setOnUpdate(delegate(float val)
			{
				currentAudio.volume = val;
			}).setOnComplete((Action)delegate
			{
				FadeInEndHandler();
			});
		}
	}

	private void FadeInEndHandler()
	{
		StopFadeTweener();
	}

	protected override void OnVolumeChanged()
	{
		if (currentAudio != null)
		{
			currentAudio.volume = base.Volume;
			currentAudio.enabled = base.Volume > 0.01f;
		}
	}

	protected override void OnMuteChanged()
	{
		if (currentAudio != null)
		{
			currentAudio.mute = base.IsMute;
		}
	}

	protected override void OnClear()
	{
		Debug.Log("BGM clear");
		if (fadeTweener != null)
		{
			fadeTweener.enabled = false;
			fadeTweener = null;
		}
		if (currentAudio != null)
		{
			currentAudio.Stop();
			currentAudio.clip = null;
			lopClip = null;
			UnityEngine.Object.Destroy(currentAudio);
			currentAudio = null;
		}
		nextClip = null;
	}
}
