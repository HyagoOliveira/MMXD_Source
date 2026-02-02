using UnityEngine;

public class PausableFx : MonoBehaviour
{
	private ParticleSystem[] fxs;

	private bool isPausing;

	private void Awake()
	{
		fxs = GetComponentsInChildren<ParticleSystem>();
		isPausing = false;
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.GAME_PAUSE, OnPause);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.GAME_PAUSE, OnPause);
	}

	public void OnPause(bool isPause)
	{
		if (isPause)
		{
			for (int i = 0; i < fxs.Length; i++)
			{
				if (fxs[i].gameObject.activeInHierarchy)
				{
					if (fxs[i].isPlaying && !isPausing)
					{
						isPausing = true;
					}
					fxs[i].Pause();
				}
			}
		}
		else
		{
			if (!isPausing)
			{
				return;
			}
			for (int j = 0; j < fxs.Length; j++)
			{
				if (fxs[j].gameObject.activeInHierarchy)
				{
					fxs[j].Play();
				}
			}
			isPausing = false;
		}
	}
}
