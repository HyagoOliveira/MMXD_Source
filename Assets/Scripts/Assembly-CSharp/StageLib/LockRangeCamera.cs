using System;
using UnityEngine;

namespace StageLib
{
	public class LockRangeCamera : MonoBehaviour
	{
		public Vector2 vLockLR = new Vector2(-9999f, 9999f);

		private float cameraHHalf;

		private float cameraWHalf;

		private void Start()
		{
			Camera component = GetComponent<Camera>();
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.LockRangeParam>(EventManager.ID.LOCK_RANGE, EventLockRange);
			cameraHHalf = Mathf.Tan(0.5f * component.fieldOfView * ((float)Math.PI / 180f)) * Mathf.Abs(base.transform.position.z);
			cameraWHalf = cameraHHalf * component.aspect;
		}

		private void LateUpdate()
		{
			Vector3 position = base.transform.position;
			if (vLockLR.x > position.x - cameraWHalf)
			{
				position.x = vLockLR.x + cameraWHalf;
				base.transform.position = position;
			}
			else if (vLockLR.y < position.x + cameraWHalf)
			{
				position.x = vLockLR.y - cameraWHalf;
				base.transform.position = position;
			}
		}

		private void OnDestroy()
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.LockRangeParam>(EventManager.ID.LOCK_RANGE, EventLockRange);
		}

		private void EventLockRange(EventManager.LockRangeParam tLockRangeParam)
		{
		}
	}
}
