using System.Collections.Generic;
using Better;
using OrangeAudio;
using UnityEngine;

public abstract class AudioChannelBase : MonoBehaviour
{
	public float defaultVolume = 0.5f;

	private string channelName;

	private string prefix = "";

	private float volume;

	private bool isMute;

	private System.Collections.Generic.Dictionary<string, AudioClip> cache;

	public float Volume
	{
		get
		{
			return volume;
		}
		set
		{
			if (volume != value)
			{
				volume = value;
				PlayerPrefs.SetFloat(channelName, volume);
				OnVolumeChanged();
			}
		}
	}

	public bool IsMute
	{
		get
		{
			return isMute;
		}
		set
		{
			if (isMute != value)
			{
				isMute = value;
				PlayerPrefs.SetInt(channelName, isMute ? 1 : 0);
				OnMuteChanged();
			}
		}
	}

	protected virtual void Awake()
	{
		cache = new Better.Dictionary<string, AudioClip>();
	}

	public void Setup(string sChannelName, AudioChannelType type)
	{
		channelName = sChannelName;
		volume = PlayerPrefs.GetFloat(channelName, defaultVolume);
		isMute = PlayerPrefs.GetInt(channelName, 0) == 1;
		switch (type)
		{
		case AudioChannelType.BGM:
			prefix = AssetBundleScriptableObject.Instance.m_audioBgmPath;
			break;
		default:
			prefix = AssetBundleScriptableObject.Instance.m_audioSePath;
			break;
		case AudioChannelType.Voice:
			prefix = AssetBundleScriptableObject.Instance.m_audioVoicePath;
			break;
		}
	}

	public void Play(string startpath, string looppath)
	{
		AudioClip clip = null;
		AudioClip clip2 = null;
		cache.TryGetValue(startpath, out clip);
		cache.TryGetValue(startpath, out clip2);
		if (clip == null && clip2 == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(prefix + startpath, startpath, delegate(AudioClip asset)
			{
				clip = asset;
				if (clip != null)
				{
					cache[startpath] = clip;
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(prefix + looppath, looppath, delegate(AudioClip asset1)
					{
						clip2 = asset1;
						if (clip2 != null)
						{
							cache[looppath] = clip2;
							OnPlayAudioClip(clip, clip2);
						}
					});
				}
			});
		}
		else if (clip != null && clip2 == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(prefix + looppath, looppath, delegate(AudioClip asset1)
			{
				clip2 = asset1;
				if (clip2 != null)
				{
					cache[looppath] = clip2;
					OnPlayAudioClip(clip, clip2);
				}
			});
		}
		else if (clip == null && clip2 != null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(prefix + startpath, startpath, delegate(AudioClip asset)
			{
				clip = asset;
				if (clip != null)
				{
					cache[startpath] = clip2;
					OnPlayAudioClip(clip, clip2);
				}
			});
		}
		if (clip != null && clip2 != null)
		{
			OnPlayAudioClip(clip, clip2);
		}
	}

	public void Play(string path)
	{
		AudioClip clip = null;
		if (cache.TryGetValue(path, out clip))
		{
			OnPlayAudioClip(clip);
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(prefix + path, path, delegate(AudioClip asset)
		{
			clip = asset;
			if (clip != null)
			{
				cache[path] = clip;
				OnPlayAudioClip(clip);
			}
		});
	}

	public void Play(AudioClip clip)
	{
		if (!(clip == null))
		{
			OnPlayAudioClip(clip);
		}
	}

	private string FindPathByClip(AudioClip clip)
	{
		foreach (KeyValuePair<string, AudioClip> item in cache)
		{
			if (item.Value == clip)
			{
				return item.Key;
			}
		}
		return null;
	}

	public void Stop(AudioClip clip)
	{
		if (!(clip == null))
		{
			OnStopAudioClip(clip);
		}
	}

	public void Stop(string path)
	{
		AudioClip value = null;
		if (cache.TryGetValue(path, out value))
		{
			OnStopAudioClip(value);
		}
	}

	public void Stop()
	{
		foreach (KeyValuePair<string, AudioClip> item in cache)
		{
			OnStopAudioClip(item.Value);
		}
	}

	public void Preload(string path, AssetsBundleManager.OnAsyncLoadAssetComplete<Object> pCb)
	{
		AudioClip clip = null;
		if (!cache.TryGetValue(path, out clip))
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(prefix + path, path, delegate(AudioClip asset)
			{
				clip = asset;
				if (!(clip == null))
				{
					cache[path] = clip;
					pCb(clip);
				}
			});
		}
		else
		{
			pCb(clip);
		}
	}

	public bool GetClip(string path, out AudioClip outClip)
	{
		AudioClip value = null;
		if (cache.TryGetValue(path, out value))
		{
			outClip = value;
			return true;
		}
		outClip = null;
		return false;
	}

	public virtual void Clear()
	{
		Stop();
		OnClear();
		cache = new Better.Dictionary<string, AudioClip>();
	}

	protected abstract void OnStopAudioClip(AudioClip clip);

	protected abstract void OnPlayAudioClip(AudioClip clip);

	protected abstract void OnPlayAudioClip(AudioClip clip, AudioClip clip1);

	protected abstract void OnVolumeChanged();

	protected abstract void OnMuteChanged();

	protected abstract void OnClear();
}
