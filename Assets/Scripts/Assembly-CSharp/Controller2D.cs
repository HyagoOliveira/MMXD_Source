#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using StageLib;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
	public struct RaycastOrigins
	{
		public Vector2 topLeft;

		public Vector2 topRight;

		public Vector2 bottomLeft;

		public Vector2 bottomRight;
	}

	public struct CollisionInfo
	{
		public bool above;

		public bool below;

		public bool left;

		public bool right;

		public bool JSB_below;

		public bool climbingSlope;

		public bool descendingSlope;

		public float slopeAngle;

		public float slopeAngleOld;

		public VInt3 velocityOld;

		public void Reset()
		{
			above = (below = false);
			left = (right = false);
			climbingSlope = false;
			descendingSlope = false;
			JSB_below = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0f;
		}
	}

	public LayerMask collisionMask;

	public LayerMask wallkickMask;

	public LayerMask collisionMaskThrough;

	public bool UseIgnoreSelf;

	public bool UseIgnoreHurtObject;

	public bool UseVerticalShiftOnHorizontalCollisions;

	public bool UseSticky;

	public static float SkinWidth = 0.015f;

	public int HorizontalRayCount = 4;

	public int VerticalRayCount = 4;

	public readonly int MaxBypassHeight = 300;

	private float _maxClimbAngle = 60f;

	private float _maxDescendAngle = 60f;

	public float _horizontalRaySpacing;

	public float _verticalRaySpacing;

	public float _extendClimbingUp;

	[HideInInspector]
	public BoxCollider2D Collider2D;

	public RaycastOrigins _raycastOrigins;

	public CollisionInfo Collisions;

	public CollisionInfo CollisionsOld;

	public VInt3 LogicPosition;

	protected float DistanceDelta;

	private Transform _transform;

	private Bounds _bounds;

	private readonly RaycastHit2D _emptyHit2D;

	private RaycastHit2D[] raycastHit2Ds = new RaycastHit2D[10];

	public Callback HitWallCallbackAbove;

	public Callback HitWallCallbackBelow;

	public Callback HitWallCallbackLeft;

	public Callback HitWallCallbackRight;

	private int _directionX;

	private float _rayLengthX;

	private bool _bypassChecked;

	private int _bypassHeight;

	private int _directionY;

	private float _rayLengthY;

	private RaycastHit2D _insideJumpThroughBlockOld;

	private RaycastHit2D _insideJumpThroughBlock;

	public bool JumpThrough { get; set; }

	public bool JumpUPThrough { get; set; }

	public RaycastHit2D BelowInBypassRange { get; private set; }

	private bool _bypass
	{
		get
		{
			if (_bypassChecked)
			{
				return _bypassHeight <= MaxBypassHeight;
			}
			return false;
		}
	}

	public event Action<Controller2D> MoveEndCall;

	private void Awake()
	{
		_transform = base.transform;
		Collider2D = GetComponent<BoxCollider2D>();
	}

	private void Start()
	{
		LogicPosition = new VInt3(_transform.localPosition);
		UpdateRaycastOrigins();
		CalculateRaySpacing();
	}

	private void OnDestroy()
	{
	}

	public void Reset()
	{
		Collisions.Reset();
		CollisionsOld.Reset();
	}

	public float Move(Vector3 velocity, bool standingOnPlatform = false)
	{
		Move(new VInt3(velocity), standingOnPlatform);
		DistanceDelta = Vector3.Distance(base.transform.localPosition, LogicPosition.vec3);
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, LogicPosition.vec3, DistanceDelta);
		return DistanceDelta;
	}

	public void Move(VInt3 p_velocity, bool standingOnPlatform = false)
	{
		VInt3 velocity = p_velocity;
		UpdateRaycastOrigins();
		if (!CollisionsOld.above && Collisions.above && HitWallCallbackAbove != null)
		{
			HitWallCallbackAbove();
			HitWallCallbackAbove = null;
		}
		if (!CollisionsOld.below && Collisions.below && HitWallCallbackBelow != null)
		{
			HitWallCallbackBelow();
			HitWallCallbackBelow = null;
		}
		if (!CollisionsOld.left && Collisions.left && HitWallCallbackLeft != null)
		{
			HitWallCallbackLeft();
			HitWallCallbackLeft = null;
		}
		if (!CollisionsOld.right && Collisions.right && HitWallCallbackRight != null)
		{
			HitWallCallbackRight();
			HitWallCallbackRight = null;
		}
		CollisionsOld = Collisions;
		Collisions.Reset();
		Collisions.velocityOld = velocity;
		if (velocity.y < 0)
		{
			DescendSlope(ref velocity);
		}
		if (velocity.x != 0)
		{
			HorizontalCollisions(ref velocity);
		}
		if (velocity.y != 0)
		{
			VerticalCollisions(ref velocity);
		}
		if (standingOnPlatform)
		{
			Collisions.below = true;
		}
		BelowInBypassRange = CheckBypassBelowRange();
		LogicPosition += velocity;
		if (this.MoveEndCall != null)
		{
			this.MoveEndCall(this);
		}
	}

	public void RunMoveEndCall()
	{
		if (this.MoveEndCall != null)
		{
			this.MoveEndCall(this);
		}
	}

	public void SetLogicPosition(VInt3 pos)
	{
		LogicPosition = pos;
		_transform.position = new Vector3(pos.x / 1000, pos.y / 1000);
	}

	public void SetLogicPosition(int posX, int posY)
	{
		LogicPosition.x = posX;
		LogicPosition.y = posY;
		_transform.position = new Vector3(posX / 1000, posY / 1000);
	}

	public void AddLogicPosition(VInt3 pos)
	{
		LogicPosition += pos;
	}

	private void HorizontalCollisions(ref VInt3 velocity)
	{
		_bypassChecked = false;
		_directionX = Math.Sign(velocity.x);
		_rayLengthX = Mathf.Abs((float)velocity.x * 0.001f + SkinWidth * (float)_directionX);
		for (int i = 0; i < HorizontalRayCount; i++)
		{
			Vector2 vector = ((_directionX == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight);
			vector += Vector2.up * (_horizontalRaySpacing * (float)i);
			if (i == 0 && UseVerticalShiftOnHorizontalCollisions)
			{
				vector.y += 0.075f;
			}
			RaycastHit2D raycastHit2D = ((!UseIgnoreSelf) ? ((JumpThrough || !BelowInBypassRange) ? Physics2D.Raycast(vector, Vector2.right * _directionX, _rayLengthX, collisionMask) : Physics2D.Raycast(vector, Vector2.right * _directionX, _rayLengthX, (int)collisionMask | (int)collisionMaskThrough)) : ((JumpThrough || !BelowInBypassRange) ? OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector2.right * _directionX, _rayLengthX, collisionMask, _transform) : OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector2.right * _directionX, _rayLengthX, (int)collisionMask | (int)collisionMaskThrough, _transform)));
			if (!raycastHit2D)
			{
				continue;
			}
			int layer = raycastHit2D.transform.gameObject.layer;
			if (((1 << layer) & (int)collisionMaskThrough) != 0)
			{
				if ((bool)(UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(vector + Vector2.left * SkinWidth * _directionX, Vector2.left * _directionX, 0.01f, collisionMaskThrough, _transform) : Physics2D.Raycast(vector + Vector2.left * SkinWidth * _directionX, Vector2.left * _directionX, 0.01f, collisionMaskThrough)))
				{
					continue;
				}
			}
			else if (layer == ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer)
			{
				StageBlockWall component = raycastHit2D.collider.GetComponent<StageBlockWall>();
				if ((bool)component && !component.isAllBlock() && ((_directionX == -1 && component.isBlockLeft()) || (_directionX == 1 && component.isBlockRight()) || Mathf.Approximately(raycastHit2D.distance, 0f)))
				{
					continue;
				}
			}
			else if (UseIgnoreHurtObject && (bool)raycastHit2D.collider.GetComponent<StageHurtObj>())
			{
				continue;
			}
			float num = Vector2.Angle(raycastHit2D.normal, Vector2.up);
			if (num > _maxClimbAngle)
			{
				_bypassHeight = GetBypassHeight(ref velocity);
			}
			if (i == 0 && (num <= _maxClimbAngle || _bypass))
			{
				if (Collisions.descendingSlope)
				{
					Collisions.descendingSlope = false;
					velocity = Collisions.velocityOld;
				}
				float num2 = 0f;
				if (num != Collisions.slopeAngleOld)
				{
					num2 = raycastHit2D.distance - SkinWidth;
					velocity.x -= Mathf.RoundToInt(1000f * num2 * (float)_directionX);
				}
				ClimbSlope(ref velocity, num);
				velocity.x += Mathf.RoundToInt(1000f * num2 * (float)_directionX);
			}
			if (!Collisions.climbingSlope || (!(num <= _maxClimbAngle) && !_bypass))
			{
				velocity.x = Mathf.RoundToInt(1000f * (raycastHit2D.distance - SkinWidth) * (float)_directionX);
				_rayLengthX = raycastHit2D.distance;
				if (Collisions.climbingSlope)
				{
					velocity.y = Mathf.RoundToInt(Mathf.Tan(Collisions.slopeAngle * ((float)Math.PI / 180f)) * (float)Math.Abs(velocity.x));
				}
				Collisions.left = _directionX == -1;
				Collisions.right = _directionX == 1;
			}
		}
	}

	private void VerticalCollisions(ref VInt3 velocity)
	{
		_directionY = Math.Sign(velocity.y);
		_rayLengthY = Mathf.Abs((float)velocity.y * 0.001f + SkinWidth * (float)_directionY);
		LayerMask layerMask = collisionMask;
		if (_directionY < 0 && !JumpThrough)
		{
			layerMask = (int)layerMask | (int)collisionMaskThrough;
		}
		if (JumpThrough)
		{
			_insideJumpThroughBlock = _emptyHit2D;
		}
		bool flag = false;
		for (int i = 0; i < VerticalRayCount; i++)
		{
			Vector2 vector = ((_directionY == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft);
			vector += Vector2.right * (_verticalRaySpacing * (float)i + (float)velocity.x * 0.001f);
			if (Collisions.climbingSlope)
			{
				vector.y += _extendClimbingUp;
			}
			RaycastHit2D raycastHit2D = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector2.up * _directionY, _rayLengthY, layerMask, _transform) : Physics2D.Raycast(vector, Vector2.up * _directionY, _rayLengthY, layerMask));
			if ((bool)raycastHit2D)
			{
				int layer = raycastHit2D.transform.gameObject.layer;
				if (((1 << layer) & (int)collisionMaskThrough) != 0)
				{
					if ((bool)(UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(vector + Vector2.down * SkinWidth * _directionY, Vector2.down * _directionY, 0.01f, collisionMaskThrough, _transform) : Physics2D.Raycast(vector + Vector2.down * SkinWidth * _directionY, Vector2.down * _directionY, 0.01f, collisionMaskThrough)))
					{
						if (i == 0)
						{
							flag = true;
						}
						continue;
					}
				}
				else if (layer == ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer)
				{
					StageBlockWall component = raycastHit2D.collider.GetComponent<StageBlockWall>();
					if ((bool)component && !component.isAllBlock())
					{
						bool flag2 = true;
						if (_directionY == -1 && !JumpThrough)
						{
							RaycastHit2D raycastHit2D2 = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(vector + Vector2.up * SkinWidth * _directionY, Vector2.up * _directionY, _rayLengthY, collisionMaskThrough, _transform) : Physics2D.Raycast(vector + Vector2.up * SkinWidth * _directionY, Vector2.up * _directionY, _rayLengthY, collisionMaskThrough));
							if ((bool)raycastHit2D2)
							{
								raycastHit2D = raycastHit2D2;
								layer = raycastHit2D.transform.gameObject.layer;
								flag2 = false;
							}
						}
						if (flag2)
						{
							if (JumpThrough && !_insideJumpThroughBlock)
							{
								_insideJumpThroughBlock = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector2.up * _directionY, _rayLengthY, collisionMaskThrough, _transform) : Physics2D.Raycast(vector, Vector2.up * _directionY, _rayLengthY, collisionMaskThrough));
							}
							continue;
						}
					}
				}
				else if (UseIgnoreHurtObject && (bool)raycastHit2D.collider.GetComponent<StageHurtObj>())
				{
					continue;
				}
				velocity.y = Mathf.RoundToInt(1000f * (raycastHit2D.distance - SkinWidth) * (float)_directionY);
				_rayLengthY = raycastHit2D.distance;
				if (Collisions.climbingSlope)
				{
					velocity.x = Mathf.RoundToInt((float)velocity.y / Mathf.Tan(Collisions.slopeAngle * ((float)Math.PI / 180f)) * Mathf.Sign(velocity.x));
				}
				Collisions.JSB_below = _directionY == -1 && ((1 << layer) & (int)collisionMaskThrough) != 0;
				Collisions.below = _directionY == -1;
				Collisions.above = _directionY == 1;
			}
			if (JumpThrough && !_insideJumpThroughBlock)
			{
				_insideJumpThroughBlock = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector2.up * _directionY, _rayLengthY, collisionMaskThrough, _transform) : Physics2D.Raycast(vector, Vector2.up * _directionY, _rayLengthY, collisionMaskThrough));
			}
		}
		if (JumpThrough)
		{
			if ((!_insideJumpThroughBlock && (bool)_insideJumpThroughBlockOld) || Collisions.below)
			{
				JumpThrough = false;
			}
			_insideJumpThroughBlockOld = _insideJumpThroughBlock;
		}
		if (Collisions.climbingSlope)
		{
			int num = Math.Sign(velocity.x);
			_rayLengthY = Mathf.Abs((float)velocity.x * 0.001f + SkinWidth * (float)num);
			Vector2 origin = ((num == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight) + Vector2.up * ((float)velocity.y / 1000f);
			RaycastHit2D raycastHit2D3 = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(origin, Vector2.right * num, _rayLengthY, layerMask, _transform) : Physics2D.Raycast(origin, Vector2.right * num, _rayLengthY, layerMask));
			float num2 = Vector2.Angle(raycastHit2D3.normal, Vector2.up);
			if (!raycastHit2D3 || num2 == Collisions.slopeAngle)
			{
				return;
			}
			velocity.x = Mathf.RoundToInt((raycastHit2D3.distance - SkinWidth) * (float)num * 1000f);
			Collisions.slopeAngle = num2;
		}
		if (!Collisions.JSB_below || velocity.y <= 0)
		{
			return;
		}
		Vector2 origin2 = (flag ? _raycastOrigins.topLeft : _raycastOrigins.topRight);
		RaycastHit2D raycastHit2D4 = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(origin2, Vector2.down, _bounds.size.y, collisionMaskThrough, _transform) : Physics2D.Raycast(origin2, Vector2.down, _bounds.size.y, collisionMaskThrough));
		if ((bool)raycastHit2D4)
		{
			float num3 = _bounds.size.y - raycastHit2D4.distance;
			if (num3 > 0f)
			{
				velocity.y = Mathf.RoundToInt(1000f * (num3 + SkinWidth * 3f));
				Collisions.left = false;
				Collisions.right = false;
			}
		}
	}

	private void ClimbSlope(ref VInt3 velocity, float slopeAngle)
	{
		int num = Mathf.Abs(velocity.x);
		int num2 = ((!_bypass) ? Mathf.RoundToInt(Mathf.Sin(slopeAngle * ((float)Math.PI / 180f)) * (float)num) : _bypassHeight);
		if (velocity.y <= num2)
		{
			velocity.y = num2;
			if (!_bypass)
			{
				velocity.x = Mathf.RoundToInt(Mathf.Cos(slopeAngle * ((float)Math.PI / 180f)) * (float)velocity.x);
			}
			Collisions.below = true;
			Collisions.climbingSlope = true;
			Collisions.slopeAngle = slopeAngle;
		}
	}

	private void DescendSlope(ref VInt3 velocity)
	{
		int num = Math.Sign(velocity.x);
		Vector2 origin = ((num == -1) ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft);
		RaycastHit2D raycastHit2D = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(origin, Vector2.down, float.PositiveInfinity, collisionMask, _transform) : Physics2D.Raycast(origin, Vector2.down, float.PositiveInfinity, collisionMask));
		if ((bool)raycastHit2D)
		{
			float num2 = Vector2.Angle(raycastHit2D.normal, Vector2.up);
			if (num2 != 0f && !(num2 > _maxDescendAngle) && Mathf.Sign(raycastHit2D.normal.x) == (float)num && !(raycastHit2D.distance - SkinWidth > Mathf.Tan(num2 * ((float)Math.PI / 180f)) * Mathf.Abs((float)velocity.x * 0.001f)))
			{
				int num3 = Mathf.Abs(velocity.x);
				int num4 = Mathf.RoundToInt(Mathf.Sin(num2 * ((float)Math.PI / 180f)) * (float)num3);
				velocity.x = Mathf.RoundToInt(Mathf.Cos(num2 * ((float)Math.PI / 180f)) * (float)velocity.x);
				velocity.y -= num4;
				Collisions.slopeAngle = num2;
				Collisions.descendingSlope = true;
				Collisions.below = true;
			}
		}
	}

	private RaycastHit2D CheckBypassBelowRange()
	{
		Vector2 bottomLeft = _raycastOrigins.bottomLeft;
		RaycastHit2D raycastHit2D = _emptyHit2D;
		float num = 0f;
		for (int i = 0; i < VerticalRayCount; i++)
		{
			RaycastHit2D raycastHit2D2 = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(bottomLeft + Vector2.right * _verticalRaySpacing * i, Vector2.down, (float)MaxBypassHeight * 0.001f, (int)collisionMask | (int)collisionMaskThrough, _transform) : Physics2D.Raycast(bottomLeft + Vector2.right * _verticalRaySpacing * i, Vector2.down, (float)MaxBypassHeight * 0.001f, (int)collisionMask | (int)collisionMaskThrough));
			if (!raycastHit2D2)
			{
				continue;
			}
			if (!raycastHit2D)
			{
				raycastHit2D = raycastHit2D2;
				num = Vector2.Angle(raycastHit2D.normal, Vector2.up);
				continue;
			}
			float num2 = Vector2.Angle(raycastHit2D2.normal, Vector2.up);
			if (raycastHit2D.distance > raycastHit2D2.distance || num > num2)
			{
				raycastHit2D = raycastHit2D2;
				num = num2;
			}
		}
		return raycastHit2D;
	}

	public RaycastHit2D SolidMeeting(float x, float y)
	{
		return ObjectMeeting(x, y, collisionMask);
	}

	public RaycastHit2D ObjectMeeting(float x, float y, LayerMask mask)
	{
		if (x == 0f && y == 0f)
		{
			Vector2 vector = _raycastOrigins.topLeft + new Vector2(x, y);
			for (int i = 0; i < HorizontalRayCount; i++)
			{
				RaycastHit2D raycastHit2D = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(vector + Vector2.down * _horizontalRaySpacing * i, Vector2.right, _bounds.size.x, mask, _transform) : Physics2D.Raycast(vector + Vector2.down * _horizontalRaySpacing * i, Vector2.right, _bounds.size.x, mask));
				if ((bool)raycastHit2D)
				{
					return raycastHit2D;
				}
			}
		}
		if (x != 0f)
		{
			Vector2 origin = ((x > 0f) ? _raycastOrigins.topRight : _raycastOrigins.topLeft);
			Vector2 direction = ((x > 0f) ? Vector2.right : Vector2.left);
			Vector2 vector2 = Vector2.down * _horizontalRaySpacing;
			float distance = Mathf.Abs(x);
			for (int j = 0; j < HorizontalRayCount; j++)
			{
				RaycastHit2D raycastHit2D2 = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(origin, direction, distance, mask, _transform) : Physics2D.Raycast(origin, direction, distance, mask));
				if ((bool)raycastHit2D2)
				{
					if (raycastHit2D2.collider.transform.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer)
					{
						return raycastHit2D2;
					}
					StageBlockWall component = raycastHit2D2.collider.GetComponent<StageBlockWall>();
					if ((bool)component)
					{
						if (x > 0f && _raycastOrigins.topRight.x < raycastHit2D2.point.x && component.isBlockLeft())
						{
							return raycastHit2D2;
						}
						if (x < 0f && _raycastOrigins.topLeft.x > raycastHit2D2.point.x && component.isBlockRight())
						{
							return raycastHit2D2;
						}
					}
				}
				origin += vector2;
			}
		}
		if (y != 0f)
		{
			Vector2 origin2 = ((y > 0f) ? _raycastOrigins.topLeft : _raycastOrigins.bottomLeft);
			Vector2 direction2 = ((y > 0f) ? Vector2.up : Vector2.down);
			Vector2 vector3 = Vector2.right * _verticalRaySpacing;
			float distance2 = Mathf.Abs(y);
			for (int k = 0; k < VerticalRayCount; k++)
			{
				RaycastHit2D raycastHit2D3 = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(origin2, direction2, distance2, mask, _transform) : Physics2D.Raycast(origin2, direction2, distance2, mask));
				if ((bool)raycastHit2D3)
				{
					return raycastHit2D3;
				}
				RaycastHit2D raycastHit2D4 = Physics2D.Raycast(origin2, direction2, 3f, collisionMaskThrough);
				if ((bool)raycastHit2D4 && raycastHit2D4.collider.transform.name != "a_colliderObj")
				{
					JumpUPThrough = true;
				}
				origin2 += vector3;
			}
		}
		return _emptyHit2D;
	}

	public RaycastHit2D ObjectMeetingIgnoreSideStageBlockWall(LayerMask mask)
	{
		Vector2 topLeft = _raycastOrigins.topLeft;
		for (int i = 0; i < HorizontalRayCount; i++)
		{
			RaycastHit2D raycastHit2D = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(topLeft + Vector2.down * _horizontalRaySpacing * i, Vector2.right, _bounds.size.x, mask, _transform) : Physics2D.Raycast(topLeft + Vector2.down * _horizontalRaySpacing * i, Vector2.right, _bounds.size.x, mask));
			if (!raycastHit2D)
			{
				continue;
			}
			if (raycastHit2D.collider.transform.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer)
			{
				StageBlockWall component = raycastHit2D.collider.GetComponent<StageBlockWall>();
				if ((bool)component && (component.isBlockLeft() || component.isBlockRight()))
				{
					continue;
				}
			}
			return raycastHit2D;
		}
		return _emptyHit2D;
	}

	public RaycastOrigins GetRaycastOrigins()
	{
		return _raycastOrigins;
	}

	public void UpdateRaycastOrigins()
	{
		Vector3 vector = LogicPosition.vec3 - _transform.localPosition;
		_bounds = Collider2D.bounds;
		_bounds.SetMinMax(_bounds.min + vector, _bounds.max + vector);
		_bounds.Expand(SkinWidth * -2f);
		_raycastOrigins.bottomLeft = new Vector2(_bounds.min.x, _bounds.min.y);
		_raycastOrigins.bottomRight = new Vector2(_bounds.max.x, _bounds.min.y);
		_raycastOrigins.topLeft = new Vector2(_bounds.min.x, _bounds.max.y);
		_raycastOrigins.topRight = new Vector2(_bounds.max.x, _bounds.max.y);
	}

	private void CalculateRaySpacing()
	{
		HorizontalRayCount = Mathf.Clamp(HorizontalRayCount, 2, int.MaxValue);
		VerticalRayCount = Mathf.Clamp(VerticalRayCount, 2, int.MaxValue);
		_horizontalRaySpacing = _bounds.size.y / (float)(HorizontalRayCount - 1);
		_verticalRaySpacing = _bounds.size.x / (float)(VerticalRayCount - 1);
	}

	public void SetColliderBox(ref Vector2 offset, ref Vector2 size)
	{
		bool flag = false;
		float y = _bounds.min.y;
		float y2 = Collider2D.size.y;
		Collider2D.offset = offset;
		Collider2D.size = size;
		UpdateRaycastOrigins();
		CalculateRaySpacing();
		if (Mathf.Abs(_bounds.min.y - y) < 0.001f)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		Vector2 bottomLeft = _raycastOrigins.bottomLeft;
		for (int i = 0; i < VerticalRayCount; i++)
		{
			RaycastHit2D raycastHit2D = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(bottomLeft, Vector2.up, size.y, collisionMask, _transform) : Physics2D.Raycast(bottomLeft, Vector2.up, size.y, collisionMask));
			Debug.DrawRay(bottomLeft, Vector2.up * size.y, Color.red);
			bottomLeft += Vector2.right * _verticalRaySpacing;
			if ((bool)raycastHit2D && !(raycastHit2D.distance >= size.y) && !_bypass && !(raycastHit2D.distance <= y2))
			{
				OrangeCharacter component = base.transform.GetComponent<OrangeCharacter>();
				if (component != null)
				{
					component.transform.localPosition = component.transform.localPosition + new Vector3(0f, raycastHit2D.distance - size.y, 0f);
					component.Controller.LogicPosition = new VInt3(component.transform.localPosition);
				}
				break;
			}
		}
	}

	private int GetBypassHeight(ref VInt3 velocity)
	{
		float y = _bounds.size.y;
		float num = Mathf.Abs(velocity.vec3.x);
		int num2 = Math.Sign(velocity.vec3.x);
		_bypassChecked = true;
		Vector2 vector = ((num2 == -1) ? _raycastOrigins.topLeft : _raycastOrigins.topRight);
		if ((bool)(UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector2.right * num2, num, collisionMask, _transform) : Physics2D.Raycast(vector, Vector2.right * num2, num, collisionMask)))
		{
			return int.MaxValue;
		}
		Vector2 vector2 = vector + Vector2.right * num2 * num;
		RaycastHit2D raycastHit2D = (UseIgnoreSelf ? OrangeBattleUtility.RaycastIgnoreSelf(vector2, Vector2.down, y, collisionMask, _transform) : Physics2D.Raycast(vector2, Vector2.down, y, collisionMask));
		if ((bool)raycastHit2D)
		{
			Debug.DrawLine(vector, vector2, Color.cyan, 0.5f);
			Debug.DrawLine(vector2, raycastHit2D.point, Color.cyan, 0.5f);
			return Mathf.RoundToInt(1000f * (y - raycastHit2D.distance + SkinWidth));
		}
		return int.MaxValue;
	}

	public Bounds GetBounds()
	{
		return _bounds;
	}

	public Bounds GetNewNowBounds()
	{
		Vector3 vector = LogicPosition.vec3 - _transform.localPosition;
		Bounds bounds = Collider2D.bounds;
		bounds.SetMinMax(bounds.min + vector, bounds.max + vector);
		bounds.Expand(SkinWidth * -2f);
		return bounds;
	}

	public Vector3 GetCenterPos()
	{
		Vector3 position = base.transform.position;
		position.x += Collider2D.offset.x * base.transform.localScale.x;
		position.y = position.y + Collider2D.offset.y * base.transform.localScale.y + Collider2D.bounds.extents.y;
		return position;
	}

	public Vector3 GetRealCenterPos()
	{
		Vector3 position = base.transform.position;
		position.x += Collider2D.offset.x * base.transform.localScale.x;
		position.y += Collider2D.offset.y * base.transform.localScale.y;
		return position;
	}

	public bool CheckCenterToShotPos(Vector2 vShotPos, List<Transform> listIgnoreTrans = null)
	{
		Vector2 vector = GetBounds().center;
		Vector2 direction = vShotPos;
		direction -= vector;
		float magnitude = direction.magnitude;
		direction /= magnitude;
		int num = Physics2D.RaycastNonAlloc(vector, direction, raycastHit2Ds, magnitude, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		if (listIgnoreTrans != null)
		{
			for (int i = 0; i < num; i++)
			{
				if (!listIgnoreTrans.Contains(raycastHit2Ds[i].collider.transform))
				{
					return true;
				}
			}
		}
		else if (num > 0)
		{
			return true;
		}
		return false;
	}

	public bool CheckCanMovePositionX(BoxCollider2D tB2D)
	{
		float y = Collider2D.bounds.size.y;
		bool flag = false;
		Vector3 vec = LogicPosition.vec3;
		Vector3 position = base.transform.position;
		float[] flyXDis = GetFlyXDis(tB2D);
		for (int i = 0; i < flyXDis.Length; i++)
		{
			LogicPosition = new VInt3(vec + new Vector3(flyXDis[i], 0f, 0f));
			UpdateRaycastOrigins();
			Vector2 bottomLeft = _raycastOrigins.bottomLeft;
			Vector2 up = Vector2.up;
			Vector2 vector = Vector2.right * _verticalRaySpacing;
			flag = true;
			for (int j = 0; j < VerticalRayCount; j++)
			{
				if ((bool)Physics2D.Raycast(bottomLeft, up, y, (int)collisionMask | (int)collisionMaskThrough))
				{
					flag = false;
					break;
				}
				bottomLeft += vector;
			}
			if (flag)
			{
				vec.x += flyXDis[i];
				position.x += flyXDis[i];
				base.transform.position = position;
				LogicPosition = new VInt3(vec);
				return true;
			}
		}
		return false;
	}

	public bool CheckCanMovePositionY(BoxCollider2D tB2D)
	{
		float x = Collider2D.bounds.size.x;
		bool flag = false;
		Vector3 vec = LogicPosition.vec3;
		Vector3 position = base.transform.position;
		float[] flyYDis = GetFlyYDis(tB2D);
		for (int i = 0; i < flyYDis.Length; i++)
		{
			LogicPosition = new VInt3(vec + new Vector3(0f, flyYDis[i], 0f));
			UpdateRaycastOrigins();
			Vector2 bottomLeft = _raycastOrigins.bottomLeft;
			Vector2 right = Vector2.right;
			Vector2 vector = Vector2.up * _horizontalRaySpacing;
			flag = true;
			for (int j = 0; j < HorizontalRayCount; j++)
			{
				if ((bool)Physics2D.Raycast(bottomLeft, right, x, (int)collisionMask | (int)collisionMaskThrough))
				{
					flag = false;
					break;
				}
				bottomLeft += vector;
			}
			if (flag)
			{
				vec.y += flyYDis[i];
				position.y += flyYDis[i];
				base.transform.position = position;
				LogicPosition = new VInt3(vec);
				return true;
			}
		}
		return false;
	}

	private float[] GetFlyXDis(BoxCollider2D tBoxCollider2D)
	{
		float num = 0f;
		float num2 = 0f;
		float[] array = new float[2];
		if (tBoxCollider2D != null)
		{
			num = tBoxCollider2D.bounds.min.x - Collider2D.bounds.max.x;
			if (num > 0f)
			{
				num = 0f;
			}
			num2 = tBoxCollider2D.bounds.max.x - Collider2D.bounds.min.x;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
		}
		if (Mathf.Abs(num2) > Mathf.Abs(num))
		{
			array[0] = num;
			array[1] = num2;
		}
		else
		{
			array[0] = num2;
			array[1] = num;
		}
		return array;
	}

	private float[] GetFlyYDis(BoxCollider2D tBoxCollider2D)
	{
		float num = 0f;
		float num2 = 0f;
		float[] array = new float[2];
		if (tBoxCollider2D != null)
		{
			num = tBoxCollider2D.bounds.min.y - Collider2D.bounds.max.y;
			if (num > 0f)
			{
				num = 0f;
			}
			num2 = tBoxCollider2D.bounds.max.y - Collider2D.bounds.min.y;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
		}
		if (Mathf.Abs(num2) > Mathf.Abs(num))
		{
			array[0] = num;
			array[1] = num2;
		}
		else
		{
			array[0] = num2;
			array[1] = num;
		}
		return array;
	}
}
