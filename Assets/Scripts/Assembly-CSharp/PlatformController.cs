using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController, IManagedUpdateBehavior
{
	private struct PassengerMovement
	{
		public Transform transform;

		public Vector3 velocity;

		public bool standingOnPlatform;

		public bool moveBeforePlatform;

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
		{
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	}

	private Transform _transform;

	public LayerMask passengerMask;

	private readonly List<PassengerMovement> passengerMovement = new List<PassengerMovement>();

	private readonly HashSet<Transform> movedPassengers = new HashSet<Transform>();

	private readonly Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

	public Vector3 vMove = Vector3.zero;

	public void PushMove(Vector3 moveDis)
	{
		vMove += moveDis;
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	public override void Start()
	{
		base.Start();
		_transform = base.transform;
		passengerMask = (int)BulletScriptableObject.Instance.BulletLayerMaskEnemy + (int)BulletScriptableObject.Instance.BulletLayerMaskPlayer;
	}

	public void UpdateFunc()
	{
		if (!(vMove == Vector3.zero))
		{
			UpdateRaycastOrigins();
			CalculatePassengerMovement(vMove);
			MovePassengers(true);
			base.transform.Translate(vMove);
			MovePassengers(false);
			vMove = Vector3.zero;
		}
	}

	public void ManualUpdatePhase1(Vector3 _velocity, bool moveSelf = true)
	{
		UpdateRaycastOrigins();
		CalculatePassengerMovement(_velocity);
		MovePassengers(true);
	}

	public void ManualUpdatePhase2()
	{
		MovePassengers(false);
	}

	private void MovePassengers(bool beforeMovePlatform)
	{
		foreach (PassengerMovement item in passengerMovement)
		{
			if (!passengerDictionary.ContainsKey(item.transform))
			{
				Controller2D controller2D = item.transform.GetComponentInParent<Controller2D>();
				if (!controller2D)
				{
					controller2D = item.transform.GetComponentInChildren<Controller2D>();
				}
				passengerDictionary.Add(item.transform, controller2D);
			}
			if (item.moveBeforePlatform == beforeMovePlatform)
			{
				passengerDictionary[item.transform].Move(item.velocity, item.standingOnPlatform);
			}
		}
	}

	private void CalculatePassengerMovement(Vector3 velocity)
	{
		movedPassengers.Clear();
		passengerMovement.Clear();
		float num = Mathf.Sign(velocity.x);
		float num2 = Mathf.Sign(velocity.y);
		if (velocity.y != 0f)
		{
			float distance = Mathf.Abs(velocity.y) + 0.015f;
			for (int i = 0; i < verticalRayCount; i++)
			{
				RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(((num2 == -1f) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft) + Vector2.right * (verticalRaySpacing * (float)i), Vector2.up * num2, distance, passengerMask, _transform);
				if ((bool)raycastHit2D && !movedPassengers.Contains(raycastHit2D.transform))
				{
					movedPassengers.Add(raycastHit2D.transform);
					float x = ((num2 == 1f) ? velocity.x : 0f);
					float y = velocity.y - (raycastHit2D.distance - 0.015f) * num2;
					passengerMovement.Add(new PassengerMovement(raycastHit2D.transform, new Vector3(x, y), num2 == 1f, true));
				}
			}
		}
		if (velocity.x != 0f)
		{
			float distance2 = Mathf.Abs(velocity.x) + 0.015f;
			for (int j = 0; j < horizontalRayCount; j++)
			{
				RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(((num == -1f) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * (horizontalRaySpacing * (float)j), Vector2.right * num, distance2, passengerMask, _transform);
				if ((bool)raycastHit2D2 && !movedPassengers.Contains(raycastHit2D2.transform))
				{
					movedPassengers.Add(raycastHit2D2.transform);
					float x2 = velocity.x - (raycastHit2D2.distance - 0.015f) * num;
					float y2 = -0.015f;
					passengerMovement.Add(new PassengerMovement(raycastHit2D2.transform, new Vector3(x2, y2), false, true));
				}
			}
		}
		if (num2 != -1f && (velocity.y != 0f || velocity.x == 0f))
		{
			return;
		}
		float distance3 = 0.03f;
		for (int k = 0; k < verticalRayCount; k++)
		{
			RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * (float)k), Vector2.up, distance3, passengerMask, _transform);
			if ((bool)raycastHit2D3 && !movedPassengers.Contains(raycastHit2D3.transform))
			{
				movedPassengers.Add(raycastHit2D3.transform);
				float x3 = velocity.x;
				float y3 = velocity.y;
				passengerMovement.Add(new PassengerMovement(raycastHit2D3.transform, new Vector3(x3, y3), true, false));
			}
		}
	}
}
