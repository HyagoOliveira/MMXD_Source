using CriWare;
using OrangeAudio;

public class AudioData
{
	public AudioChannelType AudioChannelType { get; set; }

	public CriAtomSource CriAtomSource { get; set; }

	public float SettingVol { get; set; }

	public AudioData(AudioChannelType p_audioChannelType, CriAtomSource p_criAtomSource, float p_settingVol)
	{
		AudioChannelType = p_audioChannelType;
		CriAtomSource = p_criAtomSource;
		SettingVol = p_settingVol;
	}
}
