using UnityEngine;

public class StarAni : MonoBehaviour
{
	private float fScale = 1f;

	private float fRotate;

	private int nTick;

	private void Start()
	{
		nTick = Random.Range(30, 70);
	}

	private void Update()
	{
		fRotate += 1f;
		base.transform.localRotation = new Quaternion(0f, 0f, fRotate, 1f);
		if (fRotate > 90f)
		{
			fRotate = 0f;
		}
		if (nTick > 0)
		{
			fScale -= 0.01f;
			nTick--;
		}
		else
		{
			fScale += 0.01f;
			if (fScale == 1f)
			{
				nTick = Random.Range(30, 70);
			}
		}
		base.transform.localScale = new Vector3(fScale, fScale, fScale);
	}
}
