using System;
using UnityEngine;

internal class GyroCamera : MonoBehaviour
{
	[Serializable]
	public class GryoInfo
	{
		public Transform _Transform;

		[Range(0f, 50f)]
		public float OffsetMaxX = 5f;

		[Range(0f, 50f)]
		public float OffsetMaxY = 5f;

		[Range(0f, 50f)]
		public float OffsetMaxZ = 5f;

		[Range(0.01f, 0.4f)]
		public float GyroFactor = 0.05f;

		[HideInInspector]
		public float[] EulerValue = new float[3];

		[HideInInspector]
		public Vector3 InitLocalEuler = Vector3.zero;

		[HideInInspector]
		public Vector3 LocalEuler = Vector3.zero;

		[HideInInspector]
		public ushort UnmovedCount;

		[HideInInspector]
		public int tweenUid = -1;
	}

	private Gyroscope gyro;

	public float UpateInterval = 0.02f;

	public GryoInfo[] gryoInfoArray;

	private Vector3 gyroInput = Vector3.zero;

	private Vector3 tGyroInput = Vector3.zero;

	public bool Active = true;

	private int i;

	private float sqrMin = 0.2f;

	private int UnmovedCountMax = 100;

	private void Awake()
	{
		if (!Active)
		{
			UnityEngine.Object.Destroy(this);
		}
		for (i = 0; i < gryoInfoArray.Length; i++)
		{
		}
	}

	private void Start()
	{
		gyro = Input.gyro;
		gyro.enabled = true;
		gyro.updateInterval = UpateInterval;
	}

	private void Update()
	{
		for (i = 0; i < gryoInfoArray.Length; i++)
		{
			UpdateGyro(ref gryoInfoArray[i]);
		}
	}

	private void UpdateGyro(ref GryoInfo gyroInfo)
	{
		if (gyroInfo.tweenUid != -1)
		{
			return;
		}
		if (Vector3.SqrMagnitude(gyro.rotationRateUnbiased) < sqrMin)
		{
			gyroInfo.UnmovedCount++;
			if (gyroInfo.UnmovedCount < UnmovedCountMax)
			{
				return;
			}
			gyroInfo.UnmovedCount = 0;
			if (gyroInfo.tweenUid == -1)
			{
				GryoInfo tweenInfo = gyroInfo;
				gyroInfo.tweenUid = LeanTween.rotateLocal(gyroInfo._Transform.gameObject, gyroInfo.InitLocalEuler, 1f).setOnComplete((Action)delegate
				{
					tweenInfo.LocalEuler = tweenInfo.InitLocalEuler;
					tweenInfo.tweenUid = -1;
				}).uniqueId;
			}
		}
		else
		{
			gyroInfo.UnmovedCount = 0;
			gyroInput = gyro.rotationRateUnbiased * gyroInfo.GyroFactor;
			tGyroInput = new Vector3(gyroInput.x, gyroInput.y, gyroInput.z);
			gyroInfo.LocalEuler += tGyroInput;
			gyroInfo.EulerValue[0] = ClampEulerOffset(gyroInfo.LocalEuler.x, gyroInfo.InitLocalEuler.x, gyroInfo.OffsetMaxX);
			gyroInfo.EulerValue[1] = ClampEulerOffset(gyroInfo.LocalEuler.y, gyroInfo.InitLocalEuler.y, gyroInfo.OffsetMaxY);
			gyroInfo.EulerValue[2] = ClampEulerOffset(gyroInfo.LocalEuler.z, gyroInfo.InitLocalEuler.z, gyroInfo.OffsetMaxZ);
			gyroInfo.LocalEuler = new Vector3(gyroInfo.EulerValue[0], gyroInfo.EulerValue[1], gyroInfo.EulerValue[2]);
			gyroInfo._Transform.localEulerAngles = gyroInfo.LocalEuler;
		}
	}

	private float ClampEulerOffset(float p_float, float p_init, float p_offset)
	{
		if (p_float > p_init + p_offset)
		{
			p_float = p_init + p_offset;
		}
		else if (p_float < p_init - p_offset)
		{
			p_float = p_init - p_offset;
		}
		return p_float;
	}
}
