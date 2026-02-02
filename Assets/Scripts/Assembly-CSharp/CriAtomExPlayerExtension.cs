using System.IO;
using CriWare;
using UnityEngine;

public static class CriAtomExPlayerExtension
{
	public static void PlayHca(this CriAtomExPlayer player, string fileName)
	{
		player.SetFile(null, Path.Combine(Common.streamingAssetsPath, string.Format("hca/{0}.hca", fileName)));
		player.SetFormat(CriAtomEx.Format.HCA);
		float @float = PlayerPrefs.GetFloat("ORANGE_SETTING_SE_VOLUME", 1f);
		player.SetVolume(@float);
		player.Start();
	}
}
