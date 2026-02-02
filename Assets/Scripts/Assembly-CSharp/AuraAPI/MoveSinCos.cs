using UnityEngine;

namespace AuraAPI
{
	public class MoveSinCos : MonoBehaviour
	{
		private Vector3 _initialPosition;

		public float cosAmplitude;

		public Vector3 cosDirection = Vector3.right;

		public float cosOffset;

		public float cosSpeed;

		public float sinAmplitude;

		public Vector3 sinDirection = Vector3.up;

		public float sinOffset;

		public float sinSpeed;

		public Space space = Space.Self;

		private void Start()
		{
			_initialPosition = base.transform.position;
		}

		private void Update()
		{
			Vector3 vector = sinDirection.normalized * Mathf.Sin(Time.time * sinSpeed + sinOffset) * sinAmplitude;
			Vector3 vector2 = cosDirection.normalized * Mathf.Cos(Time.time * cosSpeed + cosOffset) * cosAmplitude;
			vector = ((space == Space.World) ? vector : base.transform.localToWorldMatrix.MultiplyVector(vector));
			vector2 = ((space == Space.World) ? vector2 : base.transform.localToWorldMatrix.MultiplyVector(vector2));
			base.transform.position = _initialPosition + vector + vector2;
		}
	}
}
