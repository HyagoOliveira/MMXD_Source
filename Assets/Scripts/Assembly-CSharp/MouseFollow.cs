using UnityEngine;

public class MouseFollow : MonoBehaviour
{
	[SerializeField]
	private Camera _Camera;

	[SerializeField]
	private ParticleSystem[] tapEffects = new ParticleSystem[2];

	public float distanceCamera = 10f;

	private Vector3 mousePosition = new Vector3(0f, 0f, 0f);

	private int maxIdx;

	private int nowIdx;

	private int i;

	private void Awake()
	{
		maxIdx = tapEffects.Length;
	}

	private void Update()
	{
		if (Input.touchCount <= 0)
		{
			return;
		}
		for (i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			if (touch.phase == TouchPhase.Began)
			{
				TriggerEffect(touch.position);
			}
		}
	}

	private void TriggerEffect(Vector3 position)
	{
		mousePosition = position;
		mousePosition.z = distanceCamera;
		Vector3 position2 = _Camera.ScreenToWorldPoint(mousePosition);
		int effectIdx = GetEffectIdx();
		tapEffects[effectIdx].gameObject.transform.position = position2;
		tapEffects[effectIdx].Play(true);
	}

	private int GetEffectIdx()
	{
		if (nowIdx >= maxIdx)
		{
			nowIdx = 0;
		}
		return nowIdx++;
	}
}
