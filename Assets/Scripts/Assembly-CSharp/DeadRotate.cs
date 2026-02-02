using UnityEngine;
using UnityEngine.UI;

public class DeadRotate : MonoBehaviour
{
	public Image[] fill;

	public float fChangeWaitTIme = 0.3f;

	private float fTimeChange;

	private int nNowIndex;

	private void Start()
	{
		fTimeChange = fChangeWaitTIme;
		UpdateIndex();
	}

	private void Update()
	{
		fTimeChange -= Time.deltaTime;
		if (fTimeChange < 0f)
		{
			fTimeChange += fChangeWaitTIme;
			nNowIndex++;
			if (nNowIndex >= fill.Length)
			{
				nNowIndex = 0;
			}
			UpdateIndex();
		}
	}

	public void ReStart()
	{
		nNowIndex = 0;
		UpdateIndex();
		fTimeChange = fChangeWaitTIme;
	}

	public void UpdateIndex()
	{
		Color color = new Color(1f, 1f, 1f, 0f);
		Color color2 = new Color(1f, 1f, 1f, 0.5f);
		for (int i = 0; i < fill.Length; i++)
		{
			fill[i].color = color;
		}
		if (nNowIndex == 0)
		{
			fill[nNowIndex].color = Color.white;
			fill[fill.Length - 1].color = color2;
			fill[nNowIndex + 1].color = color2;
		}
		else if (nNowIndex == fill.Length - 1)
		{
			fill[nNowIndex].color = Color.white;
			fill[nNowIndex - 1].color = color2;
			fill[0].color = color2;
		}
		else
		{
			fill[nNowIndex].color = Color.white;
			fill[nNowIndex - 1].color = color2;
			fill[nNowIndex + 1].color = color2;
		}
	}
}
