using UnityEngine;

public class CH113_FxSet : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem[] particleSystems;

	public void FxPlay()
	{
		ParticleSystem[] array = particleSystems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	public void FxStop()
	{
		ParticleSystem[] array = particleSystems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}
}
