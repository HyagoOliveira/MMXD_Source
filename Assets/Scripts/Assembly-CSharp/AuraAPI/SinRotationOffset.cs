using UnityEngine;

namespace AuraAPI
{
	public class SinRotationOffset : MonoBehaviour
	{
		private Quaternion _initialRotation;

		public float sinAmplitude = 15f;

		public Vector3 sinDirection = Vector3.up;

		public float sinOffset;

		public float sinSpeed = 1f;

		public Space space = Space.Self;

		private void Start()
		{
			_initialRotation = ((space == Space.Self) ? base.transform.localRotation : base.transform.rotation);
		}

		private void Update()
		{
			Quaternion quaternion = Quaternion.AngleAxis(sinAmplitude * Mathf.Sin(Time.time * sinSpeed + sinOffset), sinDirection);
			if (space == Space.Self)
			{
				base.transform.localRotation = _initialRotation * quaternion;
			}
			else
			{
				base.transform.rotation = _initialRotation * quaternion;
			}
		}
	}
}
