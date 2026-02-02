using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM094_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Move = 1,
		Swing = 2,
		Thunder = 3,
		Die = 4,
		IdleWaitNet = 5
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		MAX_SUBSTATUS = 5
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private float _currentFrame;

	private Vector3 lastpos;

	private Transform BoxCol2D;

	private Transform HandModel;

	private int MoveSpeed = 500;

	[SerializeField]
	private float Distance = 4f;

	private float NextXPos;

	private OrangeTimer MoveTimer;

	private Vector3 HandRotation;

	private Transform SwingTarget;

	private SpriteRenderer PredictSpriteRenderer;

	private OrangeTimer SkillTimer;

	private CollideBullet SwingCollide;

	private Vector3 SwingStart;

	private Vector2 SwingEnd;

	private float LeftRange;

	private float RightRange;

	private CollideBullet ThunderCollider;

	private Vector3 StartPos;

	private float Vibration = 0.06f;

	private int VDirection = 1;

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
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		SwingCollide = OrangeBattleUtility.FindChildRecursive(ref target, "SwingCollider").gameObject.AddOrGetComponent<CollideBullet>();
		ThunderCollider = OrangeBattleUtility.FindChildRecursive(ref target, "ThunderCollider").gameObject.AddOrGetComponent<CollideBullet>();
		BoxCol2D = OrangeBattleUtility.FindChildRecursive(ref target, "BlockCollider");
		HandModel = OrangeBattleUtility.FindChildRecursive(ref target, "bs_041_Right_Hand");
		SwingTarget = OrangeBattleUtility.FindChildRecursive(ref target, "SwingTarget");
		PredictSpriteRenderer = SwingTarget.GetComponent<SpriteRenderer>();
		PredictSpriteRenderer.enabled = false;
		MoveTimer = OrangeTimerManager.GetTimer();
		SkillTimer = OrangeTimerManager.GetTimer();
		_mainStatus = MainStatus.Idle;
		base.AimPoint = new Vector3(0f, 0f, 0f);
		base.AllowAutoAim = false;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(8f, 5f);
		if ((bool)_characterMaterial)
		{
			_characterMaterial.Appear(delegate
			{
				OnToggleCharacterMaterial(true);
			});
		}
		else
		{
			OnToggleCharacterMaterial(true);
		}
		StageObjParam[] componentsInChildren = GetComponentsInChildren<StageObjParam>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].tLinkSOB = null;
		}
		FallDownSE = new string[2] { "BattleSE", "bt_ridearmor02" };
	}

	protected override void Start()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuseTarget", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_wolf-sigma_above_hand_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_wolf-sigma_under_hand_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_wolf-sigma_hitground_000", 2);
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (smsg != null && smsg != "")
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
			Controller.LogicPosition.x = netSyncData.SelfPosX;
			Controller.LogicPosition.y = netSyncData.SelfPosY;
			Controller.LogicPosition.z = netSyncData.SelfPosZ;
			TargetPos.x = netSyncData.TargetPosX;
			TargetPos.y = netSyncData.TargetPosY;
			TargetPos.z = netSyncData.TargetPosZ;
			MoveSpeed = netSyncData.nParam0 * 500;
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
		}
		SetStatus((MainStatus)nSet);
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (TargetPos.x > Controller.LogicPosition.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity.x = 0;
			_velocity.y = 0;
			break;
		case MainStatus.Move:
			_velocity.x = MoveSpeed * base.direction;
			break;
		case MainStatus.Swing:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.y = 12000;
				_velocity.x = 0;
				SwingStart = Controller.LogicPosition.vec3;
				base.SoundSource.PlaySE("BossSE02", "bs017_wfsig07");
				break;
			case SubStatus.Phase1:
				SwingEnd.x = TargetPos.vec3.x;
				SkillTimer.TimerStart();
				SwingTarget.position = new Vector3(TargetPos.vec3.x, SwingEnd.y + 0.1f, 0f);
				SwingTarget.localRotation = Quaternion.Euler(90f, 90f, 0f);
				PredictSpriteRenderer.enabled = true;
				break;
			case SubStatus.Phase2:
				_velocity.x = (int)((float)(TargetPos.x - Controller.LogicPosition.x) * 3.6f);
				_velocity.y = -15000;
				IgnoreGravity = false;
				SkillTimer.TimerStop();
				break;
			case SubStatus.Phase3:
			{
				base.SoundSource.PlaySE("BossSE02", "bs017_wfsig04");
				IgnoreGravity = true;
				SkillTimer.TimerStart();
				for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
				{
					Vector2 zero = Vector2.zero;
					Vector3 position2 = StageUpdate.runPlayers[i].transform.position;
					Controller2D controller = StageUpdate.runPlayers[i].Controller;
					zero = controller.Collider2D.size;
					zero /= 2f;
					Vector2 vector = new Vector2(_transform.position.x + Controller.Collider2D.offset.x - Controller.Collider2D.size.x / 2f - 0.1f, _transform.position.y + Controller.Collider2D.offset.y - Controller.Collider2D.size.y / 2f - 0.1f);
					Vector2 vector2 = new Vector2(_transform.position.x + Controller.Collider2D.offset.x + Controller.Collider2D.size.x / 2f + 0.1f, _transform.position.y + Controller.Collider2D.offset.y + 0.1f);
					if (position2.x >= vector.x - zero.x && position2.x <= vector2.x + zero.x && position2.y >= vector.y - zero.y && position2.y <= vector2.y + zero.y)
					{
						if (position2.x <= _transform.position.x)
						{
							controller.transform.position = new Vector3(vector.x - zero.x - 0.01f, position2.y, 0f);
							controller.LogicPosition = new VInt3(controller.transform.position);
						}
						else
						{
							controller.transform.position = new Vector3(vector2.x + zero.x + 0.01f, position2.y, 0f);
							controller.LogicPosition = new VInt3(controller.transform.position);
						}
					}
				}
				break;
			}
			case SubStatus.Phase4:
				_velocity.x = 0;
				_velocity.y = 3000;
				break;
			}
			break;
		case MainStatus.Thunder:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.x = 0;
				SkillTimer.TimerStart();
				break;
			case SubStatus.Phase1:
				base.SoundSource.PlaySE("BossSE02", "bs017_wfsig02_lp");
				SkillTimer.TimerStart();
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_wolf-sigma_above_hand_000", _transform.position + new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), new object[1]
				{
					new Vector3(1f, 1f, 1f)
				});
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_wolf-sigma_under_hand_000", _transform.position + new Vector3(0f, -1f, 0f), Quaternion.Euler(0f, 0f, 0f), new object[1]
				{
					new Vector3(1f, 1f, 1f)
				});
				break;
			case SubStatus.Phase2:
				SkillTimer.TimerStart();
				break;
			}
			break;
		case MainStatus.Die:
		{
			Vector3 position = new Vector3(_transform.position.x, _transform.position.y + 1000f, _transform.position.z);
			Controller.LogicPosition = new VInt3(position);
			_transform.position = position;
			_velocity.x = 0;
			_velocity.y = 0;
			SwingCollide.BackToPool();
			ThunderCollider.BackToPool();
			base.SoundSource.PlaySE("BossSE02", "bs017_wfsig02_stop");
			break;
		}
		}
		AiTimer.TimerStart();
	}

	private void UpdateRandomState(MainStatus Default = MainStatus.Idle)
	{
		base.AllowAutoAim = false;
		MainStatus mainStatus = Default;
		if (Default == MainStatus.Idle)
		{
			switch (_mainStatus)
			{
			case MainStatus.Idle:
				mainStatus = MainStatus.Move;
				break;
			case MainStatus.Move:
				mainStatus = ((OrangeBattleUtility.Random(0, 4) != 3) ? MainStatus.Swing : MainStatus.Thunder);
				break;
			}
		}
		if (mainStatus == MainStatus.Move && !MoveTimer.IsStarted())
		{
			MoveTimer.TimerStart();
		}
		switch (mainStatus)
		{
		case MainStatus.Swing:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (Target == null)
			{
				mainStatus = MainStatus.Thunder;
			}
			else if (Target.transform.position.x < LeftRange || Target.transform.position.x > RightRange)
			{
				mainStatus = MainStatus.Thunder;
			}
			break;
		case MainStatus.Idle:
			return;
		}
		if (CheckHost())
		{
			UploadEnemyStatus((int)mainStatus, false, new object[1] { OrangeBattleUtility.Random(6, 12) });
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			UpdateRandomState();
			break;
		case MainStatus.Move:
			if (MoveTimer.GetMillisecond() > 4000)
			{
				MoveTimer.TimerStop();
				if (SkillTimer.IsStarted())
				{
					SkillTimer.TimerStop();
				}
				UpdateRandomState();
			}
			break;
		case MainStatus.Swing:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_velocity.y < 0)
				{
					_velocity.y = 0;
					SetStatus(MainStatus.Swing, SubStatus.Phase1);
				}
				else
				{
					_velocity.y -= 800;
				}
				break;
			case SubStatus.Phase1:
				if (SkillTimer.GetMillisecond() > 1000)
				{
					PredictSpriteRenderer.enabled = false;
					SetStatus(MainStatus.Swing, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_transform.position.y < SwingStart.y - 1f && !SwingCollide.IsActivate)
				{
					SwingCollide.Active(targetMask);
				}
				if (_transform.position.y < SwingEnd.y + 4f && BoxCol2D.gameObject.activeSelf)
				{
					BoxCol2D.gameObject.SetActive(false);
				}
				if (_transform.position.y < SwingEnd.y + 2f)
				{
					_velocity = VInt3.zero;
					Vector3 vector = new Vector3(_transform.position.x, SwingEnd.y + Controller.Collider2D.size.y / 2f, 0f);
					Controller.LogicPosition = new VInt3(vector);
					_transform.position = vector;
					SwingCollide.BackToPool();
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
					SetStatus(MainStatus.Swing, SubStatus.Phase3);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_wolf-sigma_hitground_000", vector, Quaternion.Euler(0f, 0f, 0f), new object[1]
					{
						new Vector3(1f, 1f, 1f)
					});
				}
				else
				{
					_velocity.y -= 2400;
				}
				break;
			case SubStatus.Phase3:
				if (!BoxCol2D.gameObject.activeSelf)
				{
					BoxCol2D.gameObject.SetActive(true);
				}
				if (SkillTimer.GetMillisecond() > 1000)
				{
					SetStatus(MainStatus.Swing, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_transform.position.y > SwingStart.y)
				{
					_velocity.y = 0;
					Vector3 position = new Vector3(_transform.position.x, SwingStart.y, 0f);
					Controller.LogicPosition = new VInt3(position);
					_transform.position = position;
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Thunder:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (SkillTimer.GetMillisecond() > 300)
				{
					SkillTimer.TimerStop();
					SetStatus(MainStatus.Thunder, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (SkillTimer.GetMillisecond() > 2400)
				{
					ThunderCollider.BackToPool();
					SkillTimer.TimerStop();
					base.SoundSource.PlaySE("BossSE02", "bs017_wfsig02_stop");
					SetStatus(MainStatus.Idle);
				}
				else if (SkillTimer.GetMillisecond() > 1000 && !ThunderCollider.IsActivate)
				{
					ThunderCollider.Active(targetMask);
				}
				break;
			case SubStatus.Phase2:
				if (SkillTimer.GetMillisecond() > 600)
				{
					SkillTimer.TimerStop();
					base.SoundSource.PlaySE("BossSE02", "bs017_wfsig02_stop");
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Die:
			if (_transform.position.y < SwingEnd.y - 2f)
			{
				BackToPool();
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (!Activate)
		{
			return;
		}
		lastpos = _transform.localPosition;
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		switch (_mainStatus)
		{
		case MainStatus.Move:
			MovePlayer();
			if ((_transform.position.x < NextXPos && base.direction == -1) || (_transform.position.x > NextXPos && base.direction == 1))
			{
				base.direction *= -1;
				NextXPos += Distance * (float)base.direction;
				_velocity.x = 0;
				UpdateRandomState(MainStatus.Move);
			}
			break;
		case MainStatus.Swing:
			if (_subStatus == SubStatus.Phase0 || _subStatus == SubStatus.Phase4)
			{
				for (int j = 0; j < StageUpdate.runPlayers.Count; j++)
				{
					Vector2 zero2 = Vector2.zero;
					Vector3 position2 = StageUpdate.runPlayers[j].transform.position;
					Controller2D controller2 = StageUpdate.runPlayers[j].Controller;
					zero2 = controller2.Collider2D.size;
					zero2 /= 2f;
					Vector2 vector4 = new Vector2(_transform.position.x + Controller.Collider2D.offset.x - Controller.Collider2D.size.x / 2f - 0.1f, _transform.position.y + Controller.Collider2D.offset.y - Controller.Collider2D.size.y / 2f - 0.1f);
					Vector2 vector5 = new Vector2(_transform.position.x + Controller.Collider2D.offset.x + Controller.Collider2D.size.x / 2f + 0.1f, _transform.position.y + Controller.Collider2D.offset.y + Controller.Collider2D.size.y / 2f + 0.1f);
					if (position2.x >= vector4.x - zero2.x && position2.x <= vector5.x + zero2.x && position2.y >= vector4.y - zero2.y && position2.y <= vector5.y + zero2.y)
					{
						Vector3 vector6 = base.transform.position - lastpos;
						controller2.transform.localPosition += vector6;
						controller2.LogicPosition = new VInt3(controller2.LogicPosition.vec3 + vector6);
					}
				}
			}
			else
			{
				if (_subStatus != SubStatus.Phase2 || !BoxCol2D.gameObject.activeSelf)
				{
					break;
				}
				for (int k = 0; k < StageUpdate.runPlayers.Count; k++)
				{
					Vector2 zero3 = Vector2.zero;
					Vector3 position3 = StageUpdate.runPlayers[k].transform.position;
					Controller2D controller3 = StageUpdate.runPlayers[k].Controller;
					zero3 = controller3.Collider2D.size;
					zero3 /= 2f;
					Vector2 vector7 = new Vector2(_transform.position.x + Controller.Collider2D.offset.x - Controller.Collider2D.size.x / 2f - 0.1f, _transform.position.y + Controller.Collider2D.offset.y - Controller.Collider2D.size.y / 2f - 0.1f);
					Vector2 vector8 = new Vector2(_transform.position.x + Controller.Collider2D.offset.x + Controller.Collider2D.size.x / 2f + 0.1f, _transform.position.y + Controller.Collider2D.offset.y + Controller.Collider2D.size.y / 2f + 0.1f);
					if (position3.x >= vector7.x - zero3.x && position3.x <= vector8.x + zero3.x && position3.y >= vector7.y - zero3.y && position3.y <= vector8.y + zero3.y)
					{
						Vector3 vector9 = base.transform.position - lastpos;
						controller3.transform.localPosition += vector9;
						controller3.LogicPosition = new VInt3(controller3.LogicPosition.vec3 + vector9);
					}
				}
			}
			break;
		case MainStatus.Thunder:
		{
			if (_subStatus != SubStatus.Phase1 || SkillTimer.GetMillisecond() >= 1000)
			{
				break;
			}
			ModelTransform.position = new Vector3((float)VDirection * Vibration, 0f, 0f) + ModelTransform.position;
			VDirection *= -1;
			for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
			{
				Vector2 zero = Vector2.zero;
				Vector3 position = StageUpdate.runPlayers[i].transform.position;
				Controller2D controller = StageUpdate.runPlayers[i].Controller;
				zero = controller.Collider2D.size;
				zero /= 2f;
				Vector2 vector = new Vector2(_transform.position.x + Controller.Collider2D.offset.x - Controller.Collider2D.size.x / 2f - 0.1f, _transform.position.y + Controller.Collider2D.offset.y - Controller.Collider2D.size.y / 2f - 0.1f);
				Vector2 vector2 = new Vector2(_transform.position.x + Controller.Collider2D.offset.x + Controller.Collider2D.size.x / 2f + 0.1f, _transform.position.y + Controller.Collider2D.offset.y + Controller.Collider2D.size.y / 2f + 0.1f);
				if (position.x >= vector.x - zero.x && position.x <= vector2.x + zero.x && position.y >= vector.y - zero.y && position.y <= vector2.y + zero.y)
				{
					Vector3 vector3 = base.transform.position - lastpos;
					controller.transform.localPosition += vector3;
					controller.LogicPosition = new VInt3(controller.LogicPosition.vec3 + vector3);
				}
			}
			break;
		}
		case MainStatus.Idle:
			break;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.AllowAutoAim = false;
		base.SetActive(isActive);
		if (isActive)
		{
			IsInvincible = true;
			IgnoreGravity = true;
			SwingStart = _transform.position;
			SwingCollide.UpdateBulletData(EnemyWeapons[1].BulletData);
			SwingCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			ThunderCollider.UpdateBulletData(EnemyWeapons[0].BulletData);
			ThunderCollider.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(_transform.position, Vector2.down, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
			if ((bool)raycastHit2D)
			{
				SwingEnd.y = raycastHit2D.point.y;
				SwingTarget.position = new Vector3(SwingTarget.position.x, raycastHit2D.transform.position.y, 0f);
			}
			NextXPos = _transform.position.x + 4f * (float)base.direction;
			if (base.direction == 1)
			{
				RightRange = NextXPos + 1.8f * (float)base.direction;
				LeftRange = NextXPos + (Distance + 1.2f) * (float)(-base.direction);
			}
			else
			{
				LeftRange = NextXPos + 1.8f * (float)base.direction;
				RightRange = NextXPos + (Distance + 1.2f) * (float)(-base.direction);
			}
		}
		else
		{
			MoveTimer.TimerStop();
			SwingCollide.BackToPool();
			ThunderCollider.BackToPool();
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		_transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z);
		_transform.position = pos;
	}

	protected override void UpdateGravity()
	{
		if (!IgnoreGravity)
		{
			if ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above))
			{
				_velocity.y = 0;
			}
			_velocity.y += OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(_maxGravity.i * 3));
		}
	}

	protected void MovePlayer()
	{
		for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
		{
			Vector2 zero = Vector2.zero;
			Vector3 position = StageUpdate.runPlayers[i].transform.position;
			Controller2D controller = StageUpdate.runPlayers[i].Controller;
			zero = controller.Collider2D.size;
			zero /= 2f;
			Vector2 vector = new Vector2(_transform.position.x + Controller.Collider2D.offset.x - Controller.Collider2D.size.x / 2f - 0.1f, _transform.position.y + Controller.Collider2D.offset.y - Controller.Collider2D.size.y / 2f - 0.1f);
			Vector2 vector2 = new Vector2(_transform.position.x + Controller.Collider2D.offset.x + Controller.Collider2D.size.x / 2f + 0.1f, _transform.position.y + Controller.Collider2D.offset.y + Controller.Collider2D.size.y / 2f + 0.1f);
			if (position.x >= vector.x - zero.x && position.x <= vector2.x + zero.x && position.y >= vector.y - zero.y && position.y <= vector2.y + zero.y)
			{
				Vector3 vector3 = base.transform.position - lastpos;
				controller.transform.localPosition += vector3;
				controller.LogicPosition = new VInt3(controller.LogicPosition.vec3 + vector3);
				if (!SkillTimer.IsStarted())
				{
					SkillTimer.TimerStart();
				}
				if (SkillTimer.GetMillisecond() > 3000)
				{
					MoveTimer.TimerStop();
					SkillTimer.TimerStop();
					SetStatus(MainStatus.Thunder);
				}
			}
			else
			{
				SkillTimer.TimerStop();
			}
		}
	}

	public void SetDead()
	{
		SetStatus(MainStatus.Die);
	}
}
