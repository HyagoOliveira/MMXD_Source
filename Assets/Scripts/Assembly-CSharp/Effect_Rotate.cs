using UnityEngine;

public class Effect_Rotate : MonoBehaviour
{
	public enum QueueType
	{
		Space_World = 0,
		Space_Local = 1
	}

	public float RotateEffectX;

	public float RotateEffectY;

	public float RotateEffectZ;

	public float SinRotateEffectX;

	public float SinRotateEffectY;

	public float SinRotateEffectZ;

	public float SinSpeed;

	public QueueType Space_Type = QueueType.Space_Local;

	private void Update()
	{
		if (Space_Type == QueueType.Space_Local)
		{
			base.transform.Rotate(Time.deltaTime * RotateEffectX, Time.deltaTime * RotateEffectY, Time.deltaTime * RotateEffectZ);
		}
		else
		{
			base.transform.Rotate(Time.deltaTime * RotateEffectX, Time.deltaTime * RotateEffectY, Time.deltaTime * RotateEffectZ, Space.World);
		}
	}
}
