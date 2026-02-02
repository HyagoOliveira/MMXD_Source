using System.Collections.Generic;
using UnityEngine;

public class SoundChannel : AudioChannelBase
{
	private List<AudioSource> audioSourceList;

	private readonly Queue<AudioSource> audioSourcesQueue = new Queue<AudioSource>();

	private int audioNum;

	private int audioSourceIdx;

	private void Update()
	{
		if (audioSourceList == null)
		{
			return;
		}
		audioNum = audioSourceList.Count;
		if (audioNum == 0)
		{
			return;
		}
		for (audioSourceIdx = audioNum - 1; audioSourceIdx >= 0; audioSourceIdx--)
		{
			AudioSource audioSource = audioSourceList[audioSourceIdx];
			if (audioSource != null)
			{
				if (!audioSource.isPlaying)
				{
					audioSourceList.RemoveAt(audioSourceIdx);
					DeleteAudioSource(audioSource);
				}
			}
			else
			{
				audioSourceList.RemoveAt(audioSourceIdx);
			}
		}
	}

	protected override void OnStopAudioClip(AudioClip clip)
	{
		AudioSource audioSource = FindAudioSource(clip);
		if (audioSource != null)
		{
			audioSourceList.Remove(audioSource);
			DeleteAudioSource(audioSource);
		}
	}

	protected override void OnPlayAudioClip(AudioClip clip, AudioClip clip1)
	{
	}

	protected override void OnPlayAudioClip(AudioClip clip)
	{
		if (audioSourceList == null)
		{
			audioSourceList = new List<AudioSource>();
		}
		AudioSource audioSource = FindAudioSource(clip);
		if (audioSource == null)
		{
			AudioSource audioSource2 = null;
			audioSource2 = ((audioSourcesQueue.Count != 0) ? audioSourcesQueue.Dequeue() : base.gameObject.AddComponent<AudioSource>());
			audioSource2.loop = false;
			audioSource2.clip = clip;
			audioSource2.playOnAwake = false;
			audioSource2.volume = base.Volume;
			audioSource2.mute = base.IsMute;
			audioSourceList.Add(audioSource2);
			audioSource2.Play();
		}
		else
		{
			audioSource.Play();
		}
	}

	private AudioSource FindAudioSource(AudioClip clip)
	{
		if (audioSourceList == null)
		{
			return null;
		}
		foreach (AudioSource audioSource in audioSourceList)
		{
			if (audioSource.clip == clip)
			{
				return audioSource;
			}
		}
		return null;
	}

	protected override void OnVolumeChanged()
	{
		if (audioSourceList == null)
		{
			return;
		}
		foreach (AudioSource audioSource in audioSourceList)
		{
			audioSource.volume = base.Volume;
		}
	}

	protected override void OnMuteChanged()
	{
		if (audioSourceList == null)
		{
			return;
		}
		foreach (AudioSource audioSource in audioSourceList)
		{
			audioSource.mute = base.IsMute;
		}
	}

	protected override void OnClear()
	{
		if (audioSourceList != null)
		{
			foreach (AudioSource audioSource in audioSourceList)
			{
				DeleteAudioSource(audioSource);
			}
			audioSourceList.Clear();
		}
		while (audioSourcesQueue.Count > 0)
		{
			Object.Destroy(audioSourcesQueue.Dequeue());
		}
		audioSourcesQueue.Clear();
	}

	private void DeleteAudioSource(AudioSource source)
	{
		source.Stop();
		source.clip = null;
		audioSourcesQueue.Enqueue(source);
	}
}
