using System.Collections.Generic;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class BS044_SubHeadController : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum MainStatus
	{
		Idle = 0,
		Move = 1,
		MoveBack = 2,
		Skill = 3,
		WaitNet = 4
	}

	protected enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		MAX_SUBSTATUS = 6
	}

	public enum Direction
	{
		Front = 0,
		Left = 1,
		Right = 2
	}

	[SerializeField]
	protected Transform modelTransform;

	[SerializeField]
	protected Transform shootPointTransform;

	[SerializeField]
	protected BoxCollider2D blockCollider;

	private Vector3 lastPosition;

	private Vector2 movePosStart;

	private Vector2 movePosEnd;

	private float moveSpeed;

	private bool isMoveLoop;

	private float moveDistance;

	private List<OrangeCharacter> moveLinkPlayers = new List<OrangeCharacter>();

	[SerializeField]
	protected Renderer thornLeftMesh;

	[SerializeField]
	protected Renderer thornRightMesh;

	[SerializeField]
	protected CollideBullet thornLeftCollider;

	[SerializeField]
	protected CollideBullet thornRightCollider;

	protected MainStatus mainStatus;

	protected SubStatus subStatus;

	protected MainStatus cacheStatus;

	protected float currentDirection;

	protected bool isBlockActive = true;

	private bool activeLeftThorn;

	private bool activeRightThorn;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		ModelTransform = modelTransform;
		mainStatus = MainStatus.Idle;
		subStatus = SubStatus.Phase0;
		IgnoreGravity = true;
		IgnoreGlobalVelocity = true;
		base.AllowAutoAim = false;
		AiTimer.TimerStart();
		StageObjParam[] componentsInChildren = GetComponentsInChildren<StageObjParam>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].tLinkSOB = null;
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_sigma_mode3_000", 10);
	}

	public override void LogicUpdate()
	{
		if (Activate)
		{
			BaseLogicUpdate();
			UpdateStatusLogic();
			UpdateWaitNetStatus();
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			lastPosition = _transform.position;
			if (modelTransform.eulerAngles.y > currentDirection)
			{
				float value = modelTransform.eulerAngles.y - Time.deltaTime / GameLogicUpdateManager.m_fFrameLen * 45f;
				value = Mathf.Clamp(value, 90f, 270f);
				modelTransform.localRotation = Quaternion.Euler(0f, value, 0f);
			}
			else if (modelTransform.eulerAngles.y < currentDirection)
			{
				float value2 = modelTransform.eulerAngles.y + Time.deltaTime / GameLogicUpdateManager.m_fFrameLen * 45f;
				value2 = Mathf.Clamp(value2, 90f, 270f);
				modelTransform.localRotation = Quaternion.Euler(0f, value2, 0f);
			}
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			if (mainStatus != 0)
			{
				MovingPlayer();
			}
			DetectColliderOverlap();
		}
	}

	public override void SetActive(bool _isActive)
	{
		base.SetActive(_isActive);
		if (_isActive)
		{
			DetectColliderOverlap();
			_characterMaterial.Appear(AppearCallback);
			mainStatus = MainStatus.Idle;
			subStatus = SubStatus.Phase0;
			isBlockActive = true;
			blockCollider.gameObject.SetActive(true);
			currentDirection = 180f;
			modelTransform.localRotation = Quaternion.Euler(0f, currentDirection, 0f);
			thornLeftCollider.BackToPool();
			thornRightCollider.BackToPool();
		}
		else
		{
			thornLeftCollider.BackToPool();
			thornRightCollider.BackToPool();
		}
	}

	public void AppearCallback()
	{
		if (thornLeftCollider != null)
		{
			if (activeLeftThorn)
			{
				thornLeftCollider.UpdateBulletData(EnemyWeapons[0].BulletData);
				thornLeftCollider.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				thornLeftCollider.Active(targetMask);
				thornLeftCollider._transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			else
			{
				thornLeftCollider.BackToPool();
			}
		}
		if (thornRightCollider != null)
		{
			if (activeRightThorn)
			{
				thornRightCollider.UpdateBulletData(EnemyWeapons[0].BulletData);
				thornRightCollider.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				thornRightCollider.Active(targetMask);
				thornRightCollider._transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			else
			{
				thornRightCollider.BackToPool();
			}
		}
	}

	public void SetThornActive(bool activeLeft, bool activeRight)
	{
		activeLeftThorn = activeLeft;
		activeRightThorn = activeRight;
		thornLeftMesh.enabled = activeLeftThorn;
		thornRightMesh.enabled = activeRightThorn;
	}

	public void StartMove(Vector2 _posStart, Vector2 _posEnd, float _moveSpeed, bool _isMoveLoop = false)
	{
		movePosStart = _posStart;
		movePosEnd = _posEnd;
		moveSpeed = _moveSpeed;
		isMoveLoop = _isMoveLoop;
		moveDistance = Vector2.Distance(_posStart, _posEnd);
		RegisterStatus(MainStatus.Move);
	}

	public void StartUseSkill()
	{
		RegisterStatus(MainStatus.Skill);
	}

	public void StopAndHide()
	{
		RegisterStatus(MainStatus.Idle);
		isBlockActive = false;
		blockCollider.gameObject.SetActive(false);
		_characterMaterial.Disappear();
		thornLeftCollider.BackToPool();
		thornRightCollider.BackToPool();
	}

	protected void UpdateWaitNetStatus()
	{
		if (bWaitNetStatus && (!StageUpdate.bIsHost || !StageUpdate.gbIsNetGame))
		{
			bWaitNetStatus = false;
			SetStatus(cacheStatus);
		}
	}

	protected void RegisterStatus(MainStatus _mainStatus)
	{
		cacheStatus = _mainStatus;
		if (StageUpdate.gbIsNetGame)
		{
			if (!bWaitNetStatus && StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)_mainStatus);
				mainStatus = MainStatus.WaitNet;
				bWaitNetStatus = true;
			}
		}
		else
		{
			if (bWaitNetStatus)
			{
				bWaitNetStatus = false;
			}
			SetStatus(_mainStatus);
		}
	}

	public override void UpdateEnemyID(int _id)
	{
		base.UpdateEnemyID(_id);
		if (_enemyAutoAimSystem == null)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
	}

	public override void UpdateStatus(int _nSet, string _smsg, Callback _callback = null)
	{
		SetStatus((MainStatus)_nSet);
	}

	public override void SetPositionAndRotation(Vector3 _pos, bool _back)
	{
		Controller.LogicPosition = new VInt3(_pos);
		base.transform.position = _pos;
	}

	protected void SetFacingDirection(Direction _direction)
	{
		switch (_direction)
		{
		case Direction.Front:
			currentDirection = 180f;
			break;
		case Direction.Left:
			currentDirection = 270f;
			break;
		case Direction.Right:
			currentDirection = 90f;
			break;
		}
	}

	protected void MovingPlayer()
	{
		if (!isBlockActive)
		{
			return;
		}
		for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
		{
			OrangeCharacter orangeCharacter = StageUpdate.runPlayers[i];
			if (!moveLinkPlayers.Contains(orangeCharacter))
			{
				if (DetectPlayerStandOnBlock(orangeCharacter))
				{
					moveLinkPlayers.Add(orangeCharacter);
				}
			}
			else if (DetectPlayerStandOnBlock(orangeCharacter))
			{
				VInt3 vInt = new VInt3(_transform.position - lastPosition);
				orangeCharacter.Controller.LogicPosition.x += vInt.x;
			}
			else
			{
				moveLinkPlayers.RemoveAt(i);
			}
		}
		lastPosition = _transform.localPosition;
	}

	protected bool DetectPlayerStandOnBlock(OrangeCharacter _player)
	{
		if (!isBlockActive)
		{
			return false;
		}
		if (Mathf.Abs(_player._transform.position.y - (blockCollider.transform.position.y + blockCollider.size.y * 0.5f)) < 0.04f)
		{
			float num = Mathf.Abs(_player._transform.position.x - blockCollider.transform.position.x);
			float num2 = blockCollider.size.x * 0.5f;
			if (num < num2 + _player.Controller.Collider2D.size.x * 0.5f)
			{
				return true;
			}
		}
		return false;
	}

	protected void DetectColliderOverlap()
	{
		if (!isBlockActive)
		{
			return;
		}
		blockCollider.gameObject.SetActive(false);
		float num = Physics2D.defaultContactOffset * 2f + 0.001f;
		Vector3 position = blockCollider.transform.position;
		Collider2D[] array = Physics2D.OverlapBoxAll(size: new Vector2(blockCollider.size.x - num * 2f, blockCollider.size.y - num * 2f), point: position, angle: 0f, layerMask: 1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer);
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = 0; j < StageUpdate.runPlayers.Count; j++)
			{
				OrangeCharacter orangeCharacter = StageUpdate.runPlayers[j];
				if (orangeCharacter.transform == array[i].transform)
				{
					PushPlayer(position, orangeCharacter);
				}
			}
		}
		blockCollider.gameObject.SetActive(true);
	}

	private void PushPlayer(Vector3 _center, OrangeCharacter _player)
	{
		float num = Physics2D.defaultContactOffset * 2f + 0.02f;
		Vector2 vector = _player.Controller.Collider2D.offset * _player.transform.localScale;
		Vector2 vector2 = new Vector2(_player._transform.position.x, _player._transform.position.y) + vector;
		Vector2 size = _player.Controller.Collider2D.size;
		float num2 = (blockCollider.size.x + size.x) * 0.5f;
		float num3 = (blockCollider.size.y + size.y) * 0.5f;
		float num4 = ((_center.x < vector2.x) ? 1 : (-1));
		float num5 = ((_center.y < vector2.y) ? 1 : (-1));
		Vector2 vector3 = vector2 - new Vector2(_center.x, _center.y);
		float num6 = (size.x + blockCollider.size.x) / (size.y + blockCollider.size.y);
		vector3.y *= num6;
		byte b = 0;
		Vector3 vector4 = _player._transform.position;
		if (Mathf.Abs(vector3.x) > Mathf.Abs(vector3.y))
		{
			RaycastHit2D raycastHit2D = Physics2D.Raycast(_center, Vector3.right * num4, 100f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
			if (raycastHit2D.transform != null)
			{
				if (Vector2.Distance(_center, raycastHit2D.point) < blockCollider.size.x * 0.5f + size.x + 0.01f)
				{
					float num7 = (size.x * 0.5f + num) * num4;
					float num8 = (blockCollider.size.y * 0.5f + size.y * 0.5f + num) * num5;
					Vector2 point = raycastHit2D.point + new Vector2(0f - num7, num8);
					if (Physics2D.OverlapBoxAll(point, size, 0f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer).Length != 0)
					{
						point = raycastHit2D.point + new Vector2(0f - num7, 0f - num8);
						if (Physics2D.OverlapBoxAll(point, size, 0f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer).Length == 0)
						{
							HurtPlayer(_player);
							vector4 = new Vector3(point.x, point.y, _player._transform.position.z);
						}
					}
					else
					{
						HurtPlayer(_player);
						vector4 = new Vector3(point.x - vector.x, point.y - vector.y, _player._transform.position.z);
					}
				}
				else
				{
					b = 1;
					vector4 = new Vector3(_center.x + num2 * num4 - vector.x, _player._transform.position.y, _player._transform.position.z);
				}
			}
		}
		else
		{
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(_center, Vector3.up * num5, 100f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
			if (raycastHit2D2.transform != null)
			{
				if (Vector2.Distance(_center, raycastHit2D2.point) < blockCollider.size.y * 0.5f + size.y + 0.01f)
				{
					float num9 = (blockCollider.size.x * 0.5f + size.x * 0.5f + num) * num4;
					float num10 = (size.y * 0.5f + num) * num5;
					Vector2 point2 = raycastHit2D2.point + new Vector2(num9, 0f - num10);
					if (Physics2D.OverlapBoxAll(point2, size, 0f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer).Length != 0)
					{
						point2 = raycastHit2D2.point + new Vector2(0f - num9, 0f - num10);
						if (Physics2D.OverlapBoxAll(point2, size, 0f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer).Length == 0)
						{
							HurtPlayer(_player);
							vector4 = new Vector3(point2.x - vector.x, point2.y - vector.y, _player._transform.position.z);
						}
					}
					else
					{
						HurtPlayer(_player);
						vector4 = new Vector3(point2.x - vector.x, point2.y - vector.y, _player._transform.position.z);
					}
				}
				else
				{
					b = 2;
					vector4 = new Vector3(_player._transform.position.x, _center.y + num3 * num5 - vector.y, _player._transform.position.z);
				}
			}
		}
		switch (b)
		{
		case 1:
			_player.Controller.LogicPosition.x = new VInt3(_player._transform.position).x;
			_player.Controller.AddLogicPosition(new VInt3(vector4 - _player._transform.position));
			break;
		case 2:
			_player.Controller.LogicPosition.y = new VInt3(_player._transform.position).y;
			_player.Controller.AddLogicPosition(new VInt3(vector4 - _player._transform.position));
			break;
		default:
			_player.Controller.LogicPosition = new VInt3(vector4);
			_player._transform.position = _player.Controller.LogicPosition.vec3;
			break;
		}
	}

	public void HurtPlayer(OrangeCharacter _player)
	{
		EventManager.StageSkillAtkTargetParam stageSkillAtkTargetParam = new EventManager.StageSkillAtkTargetParam();
		stageSkillAtkTargetParam.nSkillID = EnemyWeapons[2].BulletData.n_ID;
		stageSkillAtkTargetParam.bAtkNoCast = true;
		stageSkillAtkTargetParam.tPos = Vector3.zero;
		stageSkillAtkTargetParam.tDir = Vector3.zero;
		stageSkillAtkTargetParam.bBuff = false;
		stageSkillAtkTargetParam.tTrans = _player._transform;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_REBORNEVENT, _player, _player._transform.position);
	}

	protected void SetStatus(MainStatus _mainStatus, SubStatus _subStatus = SubStatus.Phase0)
	{
		mainStatus = _mainStatus;
		subStatus = _subStatus;
		switch (mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Move:
			if (subStatus == SubStatus.Phase0)
			{
				_velocity = new VInt3((movePosEnd - movePosStart).normalized * moveSpeed);
			}
			break;
		case MainStatus.MoveBack:
			if (subStatus == SubStatus.Phase0)
			{
				_velocity = new VInt3((movePosStart - movePosEnd).normalized * moveSpeed);
			}
			break;
		case MainStatus.Skill:
			SetStatusOfSkill();
			break;
		}
		AiTimer.TimerStart();
	}

	protected virtual void SetStatusOfSkill()
	{
	}

	private void UpdateStatusLogic()
	{
		switch (mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Move:
			if (subStatus == SubStatus.Phase0)
			{
				float currentDistance2 = Vector2.Distance(_transform.position, movePosStart);
				Moving(currentDistance2, movePosEnd);
			}
			break;
		case MainStatus.MoveBack:
			if (subStatus == SubStatus.Phase0)
			{
				float currentDistance = Vector2.Distance(_transform.position, movePosEnd);
				Moving(currentDistance, movePosStart);
			}
			break;
		case MainStatus.Skill:
			UpdateStatusOfSkill();
			break;
		}
	}

	private void Moving(float _currentDistance, Vector2 _finalPos)
	{
		if (!(moveDistance - _currentDistance < 0.1f) && !(_currentDistance > moveDistance))
		{
			return;
		}
		Controller.LogicPosition = new VInt3(_finalPos);
		if (isMoveLoop)
		{
			if (mainStatus == MainStatus.Move)
			{
				RegisterStatus(MainStatus.MoveBack);
			}
			else
			{
				RegisterStatus(MainStatus.Move);
			}
		}
		else
		{
			RegisterStatus(MainStatus.Idle);
		}
	}

	protected virtual void UpdateStatusOfSkill()
	{
	}
}
