using UnityEngine;

public class GachaCapsuleEft : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem[] arrayPs;

	public Color NowColor { get; set; }

	public void Play(bool p_loop = false)
	{
		ParticleSystem[] array = arrayPs;
		foreach (ParticleSystem obj in array)
		{
			ParticleSystem.MainModule main = obj.main;
			main.startColor = NowColor;
			main.loop = p_loop;
			obj.Play();
		}
	}

	public void Stop()
	{
		ParticleSystem[] array = arrayPs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
	}
}
