using UnityEngine;

public class ImageCycle : MonoBehaviour
{
	public GameObject[] cycleobjs;

	public float fChangeSpeed = 0.1f;

	public float fChangeWait = 0.5f;

	private float fLeftTime;

	private float fLeftWaitTime;

	private int nNowIndex;

	private void Awake()
	{
		if (cycleobjs != null && cycleobjs.Length != 0)
		{
			for (int i = 1; i < cycleobjs.Length; i++)
			{
				cycleobjs[i].SetActive(false);
			}
			cycleobjs[0].SetActive(true);
		}
	}

	private void Update()
	{
		if (cycleobjs == null || cycleobjs.Length == 0)
		{
			return;
		}
		if (fLeftWaitTime <= 0f)
		{
			fLeftTime += Time.deltaTime;
			if (fLeftTime >= fChangeSpeed)
			{
				fLeftTime -= fChangeSpeed;
				cycleobjs[nNowIndex].SetActive(false);
				nNowIndex++;
				if (nNowIndex >= cycleobjs.Length)
				{
					nNowIndex = 0;
					fLeftWaitTime = fChangeWait;
					cycleobjs[nNowIndex].SetActive(true);
				}
				else
				{
					cycleobjs[nNowIndex].SetActive(true);
				}
			}
		}
		else
		{
			fLeftWaitTime -= Time.deltaTime;
		}
	}
}
