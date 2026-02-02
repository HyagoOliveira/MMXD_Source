#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS011_MainController : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		RedEye = 1,
		GreenEye = 2,
		BlueEye = 3,
		Wall = 4,
		Dead = 5,
		Hurt = 6
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		Phase6 = 6,
		Phase7 = 7,
		Phase8 = 8,
		Phase9 = 9,
		MAX_SUBSTATUS = 10
	}

	private enum BS011_Parts
	{
		RedEye = 0,
		GreenEye = 1,
		BlueEye = 2,
		Flame = 3,
		Teeth = 4,
		MAX_PARTS = 5
	}

	private enum BS011_Wall
	{
		Left = 0,
		Right = 1,
		MAX_WALL = 2
	}

	private List<MainStatus> MoveList;

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private float _currentFrame;

	private int[] _animationHash;

	private BS011_PartsController[] _partsController;

	private BS011_WallController[] _wallController;

	private BoxCollider2D[] _wallboxcollider;

	private Transform _lEyeTransform;

	private Transform _rEyeTransform;

	private Transform _lAttackTransform;

	private Transform _mAttackTransform;

	private Transform _rAttackTransform;

	private Transform _lFlameTransform;

	private Transform _rFlameTransform;

	private Transform _mFlameTransform;

	private bool _fireBallMoveY;

	private int _fireBallMoveX;

	private int _randomValue;

	private int _eyeBulletCount;

	private int _attackPhase;

	private int _currentTweenId;

	private const float _blueEyeDistance = 5f;

	private const float _redEyeDistance = 2f;

	private const float _flameDistance = 3f;

	private float _tweenTime;

	private Vector3 _eyeBackPos;

	private bool _bDeadCallResult = true;

	[SerializeField]
	private GameObject[] Parts = new GameObject[5];

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Start()
	{
		base.Start();
		DeadCallback = OnDead;
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_lEyeTransform = OrangeBattleUtility.FindChildRecursive(ref target, "L_EyePos", true);
		_rEyeTransform = OrangeBattleUtility.FindChildRecursive(ref target, "R_EyePos", true);
		_lAttackTransform = OrangeBattleUtility.FindChildRecursive(ref target, "L_AttackPos", true);
		_rAttackTransform = OrangeBattleUtility.FindChildRecursive(ref target, "R_AttackPos", true);
		_mAttackTransform = OrangeBattleUtility.FindChildRecursive(ref target, "M_AttackPos", true);
		_lFlameTransform = OrangeBattleUtility.FindChildRecursive(ref target, "L_FlamePos", true);
		_rFlameTransform = OrangeBattleUtility.FindChildRecursive(ref target, "R_FlamePos", true);
		_mFlameTransform = OrangeBattleUtility.FindChildRecursive(ref target, "M_FlamePos", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		_partsController = new BS011_PartsController[5];
		_partsController[0] = Parts[0].GetComponent<BS011_PartsController>();
		_partsController[1] = Parts[1].GetComponent<BS011_PartsController>();
		_partsController[2] = Parts[2].GetComponent<BS011_PartsController>();
		_partsController[3] = Parts[3].GetComponent<BS011_FireBallController>();
		_partsController[4] = Parts[4].GetComponent<BS011_TeethController>();
		for (int i = 0; i < 5; i++)
		{
			_partsController[i].gameObject.AddOrGetComponent<StageObjParam>().nSubPartID = i + 1;
		}
		_currentTweenId = -1;
		_wallController = new BS011_WallController[2];
		_wallController[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L_Door", true).GetComponent<BS011_WallController>();
		_wallController[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R_Door", true).GetComponent<BS011_WallController>();
		_wallboxcollider = new BoxCollider2D[2];
		_wallboxcollider[0] = _wallController[0].gameObject.GetComponent<BoxCollider2D>();
		_wallboxcollider[1] = _wallController[1].gameObject.GetComponent<BoxCollider2D>();
		BS011_WallController[] wallController = _wallController;
		for (int j = 0; j < wallController.Length; j++)
		{
			StageObjParam component = wallController[j].GetComponent<StageObjParam>();
			if ((bool)component)
			{
				component.tLinkSOB = null;
			}
		}
		base.direction = 1;
		MoveList = new List<MainStatus>();
		MoveList.Add(MainStatus.RedEye);
		MoveList.Add(MainStatus.GreenEye);
		MoveList.Add(MainStatus.BlueEye);
		MoveList.Add(MainStatus.Wall);
		CharacterMaterial component2 = GetComponent<CharacterMaterial>();
		if ((bool)component2)
		{
			component2.Appear();
			_wallController[0].gameObject.SetActive(false);
			_wallController[1].gameObject.SetActive(false);
		}
		SetStatus(MainStatus.Idle);
		base.AllowAutoAim = false;
		_bDeadPlayCompleted = false;
	}

	public override Transform GetFXShowTrans()
	{
		switch (_mainStatus)
		{
		case MainStatus.BlueEye:
			return _partsController[2].transform;
		case MainStatus.GreenEye:
			return _partsController[1].transform;
		case MainStatus.RedEye:
			return _partsController[0].transform;
		case MainStatus.Wall:
			return _partsController[3].transform;
		default:
			if (ModelTransform != null)
			{
				return ModelTransform;
			}
			return base.transform;
		}
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		NetSyncData netSyncData = null;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (smsg != null && smsg != "")
		{
			netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
		}
		if (bCanNextStatus)
		{
			if (netSyncData != null)
			{
				UpdateStatusBySend(nSet, netSyncData);
			}
		}
		else
		{
			NexStatusSave nexStatusSave = new NexStatusSave();
			nexStatusSave.MainStatus = nSet;
			nexStatusSave.tSend = netSyncData;
			listNexStatusSave.Add(nexStatusSave);
		}
	}

	private void UpdateStatusBySend(int nSet, NetSyncData tSend)
	{
		if ((nSet < 1 || nSet > 4 || (int)PartHp[nSet - 1] > 0) && tSend != null)
		{
			bCanNextStatus = false;
			TargetPos.x = tSend.TargetPosX;
			TargetPos.y = tSend.TargetPosY;
			TargetPos.z = tSend.TargetPosZ;
			_randomValue = tSend.nParam0;
			SetStatus((MainStatus)nSet, (SubStatus)tSend.nHP);
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Dead:
		{
			_velocity.x = 0;
			OrangeBattleUtility.LockPlayer();
			Vector3 targetTF = new Vector3(_transform.position.x, _transform.position.y + 3f, _transform.position.z);
			if (_bDeadCallResult)
			{
				StartCoroutine(BossDieFlow(targetTF));
			}
			else
			{
				StartCoroutine(BossDieFlow(targetTF, "FX_BOSS_EXPLODE2", false, false));
			}
			break;
		}
		case MainStatus.Wall:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase1)
			{
			}
			break;
		}
		case MainStatus.RedEye:
			switch (_subStatus)
			{
			case SubStatus.Phase2:
				_attackPhase = 0;
				break;
			}
			break;
		case MainStatus.GreenEye:
			switch (_subStatus)
			{
			}
			break;
		case MainStatus.BlueEye:
			switch (_subStatus)
			{
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateCollider();
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		if (!_wallController[0].gameObject.activeSelf)
		{
			_wallController[0].gameObject.SetActive(true);
			_wallController[1].gameObject.SetActive(true);
		}
		IgnoreGravity = true;
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			UpdateRandomState();
			break;
		case MainStatus.Wall:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!_wallController[0].isBusy())
				{
					ToggleWall();
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (!_wallController[0].isBusy())
				{
					_attackPhase = 0;
					_partsController[3].SetVisible();
					_partsController[4].SetVisible();
					_fireBallMoveX = 0;
					_fireBallMoveY = false;
					_eyeBackPos = _mFlameTransform.position;
					if (AiState == AI_STATE.mob_002)
					{
						MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform, 998);
					}
					SetStatus(_mainStatus, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase2:
			{
				if (_currentTweenId != -1)
				{
					break;
				}
				int attackPhase = _attackPhase;
				if (attackPhase == 6)
				{
					_partsController[3].SoundSource.PlaySE("BossSE", 117);
					_tweenTime = Vector3.Distance(_partsController[3]._transform.position, _mFlameTransform.position) / 3f;
					_currentTweenId = LeanTween.value(_partsController[3].gameObject, _partsController[3]._transform.position, _mFlameTransform.position, _tweenTime).setOnUpdate(delegate(Vector3 val)
					{
						_partsController[3]._transform.position = val;
					}).setOnComplete((Action)delegate
					{
						LeanTween.value(base.gameObject, 0f, 1f, 0.5f).setOnComplete((Action)delegate
						{
							_partsController[3].SoundSource.PlaySE("BossSE", 118);
							if ((int)PartHp[3] > 0)
							{
								SetStatus(_mainStatus, SubStatus.Phase3);
							}
							_currentTweenId = -1;
						});
					})
						.uniqueId;
					break;
				}
				_partsController[3].SoundSource.PlaySE("BossSE", 117);
				Vector3 position = _partsController[3]._transform.position;
				Vector3 vec = TargetPos.vec3;
				Vector3 vector5 = position;
				if (Mathf.Abs(position.y - vec.y) > 1f && !_fireBallMoveY)
				{
					vector5.y = vec.y;
					_fireBallMoveY = true;
				}
				else
				{
					if (_fireBallMoveX == 0)
					{
						_fireBallMoveX = Math.Sign(position.x - vec.x);
					}
					else
					{
						_fireBallMoveX = -_fireBallMoveX;
					}
					vector5.x = ((_fireBallMoveX > 0) ? _rFlameTransform.position.x : _lFlameTransform.position.x);
					_fireBallMoveY = false;
				}
				_tweenTime = Vector3.Distance(position, vector5) / 3f;
				_currentTweenId = LeanTween.value(_partsController[3].gameObject, position, vector5, _tweenTime).setOnUpdate(delegate(Vector3 val)
				{
					_partsController[3]._transform.position = val;
				}).setOnComplete((Action)delegate
				{
					LeanTween.value(base.gameObject, 0f, 1f, 1f).setOnComplete((Action)delegate
					{
						_attackPhase++;
						_currentTweenId = -1;
						if ((int)PartHp[3] > 0)
						{
							SetStatus(_mainStatus, SubStatus.Phase5);
						}
					});
				})
					.uniqueId;
				break;
			}
			case SubStatus.Phase3:
				if (!_wallController[0].isBusy())
				{
					ToggleWall();
					_partsController[3].SetVisible(false);
					_partsController[4].SetVisible(false);
					SetStatus(_mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (!_wallController[0].isBusy())
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase5:
				UpdateNextState(_mainStatus, SubStatus.Phase2);
				break;
			}
			break;
		case MainStatus.RedEye:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_partsController[0].SetVisible();
				_attackPhase = 0;
				_partsController[0].transform.position = ((_randomValue == 0) ? _lEyeTransform.position : _rEyeTransform.position);
				if (AiState == AI_STATE.mob_002)
				{
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
				}
				SetStatus(_mainStatus, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (!_partsController[0].IsBusy())
				{
					((BS011_EyeController)_partsController[0]).OpenEye(0.5f);
					_eyeBackPos = _partsController[0].transform.position;
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentTweenId != -1 || _partsController[0].IsBusy())
				{
					break;
				}
				switch (_attackPhase)
				{
				case 0:
				{
					Vector3 vector4 = ((TargetPos.x > Controller.LogicPosition.x) ? _lAttackTransform.position : _rAttackTransform.position);
					_tweenTime = Vector3.Distance(_partsController[0].transform.position, vector4) / 2f;
					_currentTweenId = LeanTween.value(_partsController[0].gameObject, _partsController[0].transform.position, vector4, _tweenTime).setOnUpdate(delegate(Vector3 value)
					{
						_partsController[0].transform.position = value;
					}).setOnComplete((Action)delegate
					{
						LeanTween.value(base.gameObject, 0f, 1f, 0.5f).setOnComplete((Action)delegate
						{
							_currentTweenId = -1;
							_attackPhase++;
							_eyeBulletCount = 0;
						});
					})
						.uniqueId;
					break;
				}
				case 1:
					if (_eyeBulletCount < 290)
					{
						float num = (float)(IntMath.Max(_eyeBulletCount - 50, 0) * 3) * (float)Math.PI / 180f;
						if (_partsController[0].transform.position.x > Controller.LogicPosition.vec3.x)
						{
							num *= -1f;
						}
						Vector2 vector3 = new Vector3((float)base.direction * Mathf.Sin(num), Mathf.Cos(num), 0f);
						BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _partsController[0].ShootPoint, vector3, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _partsController[0].ShootPoint, vector3 * -1f, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						_eyeBulletCount++;
					}
					else
					{
						_attackPhase++;
					}
					break;
				case 2:
					_tweenTime = Vector3.Distance(_partsController[0].transform.position, _eyeBackPos) / 2f;
					_currentTweenId = LeanTween.value(_partsController[0].gameObject, _partsController[0].transform.position, _eyeBackPos, _tweenTime).setOnUpdate(delegate(Vector3 value)
					{
						_partsController[0].transform.position = value;
					}).setOnComplete((Action)delegate
					{
						_currentTweenId = -1;
						_attackPhase++;
					})
						.uniqueId;
					break;
				case 3:
					SetStatus(_mainStatus, SubStatus.Phase3);
					break;
				}
				break;
			case SubStatus.Phase3:
				if (!_partsController[0].IsBusy())
				{
					((BS011_EyeController)_partsController[0]).CloseEye(0.5f);
					SetStatus(_mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (!_partsController[0].IsBusy())
				{
					_partsController[0].SetVisible(false);
					SetStatus(_mainStatus, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (!_partsController[0].IsBusy())
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.GreenEye:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_partsController[1].SetVisible();
				_partsController[1].transform.position = ((_randomValue == 0) ? _lEyeTransform.position : _rEyeTransform.position);
				if (AiState == AI_STATE.mob_002)
				{
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
				}
				SetStatus(_mainStatus, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (!_partsController[1].IsBusy())
				{
					((BS011_EyeController)_partsController[1]).OpenEye(0.5f);
					_eyeBackPos = _partsController[1].transform.position;
					SetStatus(_mainStatus, SubStatus.Phase2);
					_eyeBulletCount = 0;
				}
				break;
			case SubStatus.Phase2:
				if (_partsController[1].IsBusy())
				{
					break;
				}
				if (_eyeBulletCount < 60)
				{
					if (_eyeBulletCount % 2 == 0)
					{
						Vector2 vector2 = (TargetPos.vec3 - _partsController[1].transform.position).normalized;
						BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _partsController[1].ShootPoint, vector2, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
					_eyeBulletCount++;
					if (_eyeBulletCount % 10 == 0)
					{
						SetStatus(_mainStatus, SubStatus.Phase5);
					}
				}
				else
				{
					((BS011_EyeController)_partsController[1]).CloseEye(0.5f);
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (!_partsController[1].IsBusy())
				{
					_partsController[1].SetVisible(false);
					SetStatus(_mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (!_partsController[1].IsBusy())
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase5:
				UpdateNextState(_mainStatus, SubStatus.Phase2);
				break;
			}
			break;
		case MainStatus.BlueEye:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_partsController[2].SetVisible();
				_attackPhase = 0;
				_partsController[2].transform.position = ((_randomValue == 0) ? _lEyeTransform.position : _rEyeTransform.position);
				if (AiState == AI_STATE.mob_002)
				{
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
				}
				SetStatus(_mainStatus, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (!_partsController[2].IsBusy())
				{
					((BS011_EyeController)_partsController[2]).OpenEye(0.5f);
					_eyeBackPos = _partsController[2].transform.position;
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentTweenId != -1 || _partsController[2].IsBusy())
				{
					break;
				}
				if (_attackPhase < 3)
				{
					Vector3 vector = TargetPos.vec3 - _partsController[2].transform.position;
					Vector3 to = _partsController[2].transform.position + vector.normalized * 5f;
					_currentTweenId = LeanTween.value(_partsController[2].gameObject, _partsController[2].transform.position, to, 1f).setOnUpdate(delegate(Vector3 value)
					{
						_partsController[2].transform.position = value;
					}).setOnComplete((Action)delegate
					{
						LeanTween.value(base.gameObject, 0f, 1f, 0.5f).setOnComplete((Action)delegate
						{
							_currentTweenId = -1;
							_attackPhase++;
							if ((int)PartHp[2] > 0)
							{
								SetStatus(_mainStatus, SubStatus.Phase6);
							}
						});
					})
						.uniqueId;
				}
				else if (_attackPhase == 3)
				{
					float time = Vector3.Distance(_partsController[2].transform.position, _eyeBackPos) / 5f;
					_currentTweenId = LeanTween.value(_partsController[2].gameObject, _partsController[2].transform.position, _eyeBackPos, time).setOnUpdate(delegate(Vector3 value)
					{
						_partsController[2].transform.position = value;
					}).setOnComplete((Action)delegate
					{
						_currentTweenId = -1;
						_attackPhase++;
					})
						.uniqueId;
				}
				else
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (!_partsController[2].IsBusy())
				{
					((BS011_EyeController)_partsController[2]).CloseEye(0.5f);
					SetStatus(_mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (!_partsController[2].IsBusy())
				{
					_partsController[2].SetVisible(false);
					SetStatus(_mainStatus, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (!_partsController[2].IsBusy())
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase6:
				UpdateNextState(_mainStatus, SubStatus.Phase2);
				break;
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			if (_mainStatus == MainStatus.Wall && _subStatus == SubStatus.Phase1)
			{
				CheckPlayerStuk();
			}
		}
	}

	private void UpdateRandomState()
	{
		MainStatus nSetKey = MainStatus.Idle;
		bCanNextStatus = true;
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			Target = StageUpdate.GetNearestPlayerByVintPos(Controller.LogicPosition);
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
			}
			if (MoveList.Count > 0)
			{
				nSetKey = MoveList[OrangeBattleUtility.Random(0, MoveList.Count)];
			}
		}
		else if (bWaitNetStatus && listNexStatusSave.Count > 0)
		{
			UpdateStatusBySend(listNexStatusSave[0].MainStatus, listNexStatusSave[0].tSend);
			listNexStatusSave.RemoveAt(0);
		}
		if (StageUpdate.bIsHost)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.TargetPosX = TargetPos.x;
			netSyncData.TargetPosY = TargetPos.y;
			netSyncData.TargetPosZ = TargetPos.z;
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			netSyncData.nParam0 = OrangeBattleUtility.Random(0, 2);
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, (int)nSetKey, JsonConvert.SerializeObject(netSyncData));
		}
	}

	private void UpdateNextState(MainStatus tMainStatus, SubStatus tSubStatus)
	{
		VInt3 vInt = VInt3.zero;
		bCanNextStatus = true;
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			Target = StageUpdate.GetNearestPlayerByVintPos(Controller.LogicPosition);
			vInt = ((!Target) ? new VInt3(_eyeBackPos) : Target.Controller.LogicPosition);
		}
		else if (bWaitNetStatus)
		{
			bWaitNetStatus = false;
			if (listNexStatusSave.Count > 0)
			{
				UpdateStatusBySend(listNexStatusSave[0].MainStatus, listNexStatusSave[0].tSend);
				listNexStatusSave.RemoveAt(0);
			}
		}
		if (StageUpdate.bIsHost)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.TargetPosX = vInt.x;
			netSyncData.TargetPosY = vInt.y;
			netSyncData.TargetPosZ = vInt.z;
			netSyncData.nHP = (int)tSubStatus;
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, (int)tMainStatus, JsonConvert.SerializeObject(netSyncData));
		}
	}

	private void UpdateCollider()
	{
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.Idle || (uint)(mainStatus - 5) <= 1u)
		{
			SetColliderEnable(false);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		SetColliderEnable(false);
		if (isActive)
		{
			_partsController[3].SetVisible();
			LeanTween.value(base.gameObject, 0f, 1f, 0.5f).setOnComplete((Action)delegate
			{
				_partsController[3].SetVisible(false);
			});
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		base.direction = 1;
		base.transform.position = pos;
	}

	public void ToggleWall()
	{
		BS011_WallController[] wallController = _wallController;
		foreach (BS011_WallController bS011_WallController in wallController)
		{
			bS011_WallController.ToggleWall(3f);
			if (AiState == AI_STATE.mob_002)
			{
				bS011_WallController.ActiveBullect(true, friendMask);
			}
			else
			{
				bS011_WallController.ActiveBullect(false, 0);
			}
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		_partsController[0].UpdateBulletData(EnemyWeapons[0].BulletData);
		_partsController[1].UpdateBulletData(EnemyWeapons[0].BulletData);
		_partsController[2].UpdateBulletData(EnemyWeapons[0].BulletData);
		_partsController[3].UpdateBulletData(EnemyWeapons[3].BulletData);
		_partsController[4].UpdateBulletData(EnemyWeapons[4].BulletData);
		PartHp = new ObscuredInt[4];
		int num = (int)MaxHp / 4;
		int num2 = (int)MaxHp % 4;
		for (int i = 0; i < 4; i++)
		{
			PartHp[i] = num;
			if (i == 3)
			{
				ref ObscuredInt reference = ref PartHp[i];
				reference = (int)reference + num2;
			}
		}
		AI_STATE aiState = AI_STATE.mob_001;
		if (EnemyData.s_AI != "null")
		{
			aiState = (AI_STATE)Enum.Parse(typeof(AI_STATE), EnemyData.s_AI);
		}
		AiState = aiState;
		AI_STATE aiState2 = AiState;
		if (aiState2 == AI_STATE.mob_002)
		{
			_bImmunityDeadArea = true;
			_bDeadCallResult = false;
			if (EnemyWeapons.Length > 5)
			{
				_wallController[0].UpdateBulletData(EnemyWeapons[5].BulletData);
				_wallController[1].UpdateBulletData(EnemyWeapons[5].BulletData);
			}
		}
		else
		{
			_bDeadCallResult = true;
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		tHurtPassParam.dmg = selfBuffManager.ReduceDmgByEnergyShild(tHurtPassParam.dmg);
		if (!InGame)
		{
			Debug.LogWarning("[Enemy] InGame Flag is false.");
			return Hp;
		}
		if (tHurtPassParam.nSubPartID == 0)
		{
			return Hp;
		}
		if (tHurtPassParam.nSubPartID >= 1 && tHurtPassParam.nSubPartID <= 4)
		{
			tHurtPassParam.nSubPartID--;
			if ((int)PartHp[tHurtPassParam.nSubPartID] > (int)tHurtPassParam.dmg)
			{
				_partsController[tHurtPassParam.nSubPartID].Hurt();
			}
			else
			{
				tHurtPassParam.dmg = PartHp[tHurtPassParam.nSubPartID];
				_partsController[tHurtPassParam.nSubPartID].SetDestroy();
				PartsExplode(_partsController[tHurtPassParam.nSubPartID].transform);
				PartsDestroy((MainStatus)(1 + tHurtPassParam.nSubPartID));
			}
			OrangeBattleUtility.UpdateEnemyHp(ref PartHp[tHurtPassParam.nSubPartID], ref tHurtPassParam.dmg);
		}
		OrangeBattleUtility.UpdateEnemyHp(ref Hp, ref tHurtPassParam.dmg);
		UpdateHurtAction();
		if ((int)Hp <= 0 && _mainStatus != MainStatus.Dead)
		{
			for (int i = 0; i < 4; i++)
			{
				if (_partsController[i].IsVisible())
				{
					_partsController[i].SetDestroy();
					PartsExplode(_partsController[i].transform);
				}
			}
			StageUpdate.SlowStage();
			SetStatus(MainStatus.Dead);
		}
		return Hp;
	}

	private void PartsDestroy(MainStatus status)
	{
		if (status == _mainStatus)
		{
			_attackPhase = 0;
			switch (status)
			{
			case MainStatus.RedEye:
			case MainStatus.GreenEye:
			case MainStatus.BlueEye:
				SetStatus(MainStatus.Idle);
				break;
			case MainStatus.Wall:
				ToggleWall();
				SetStatus(_mainStatus, SubStatus.Phase4);
				_partsController[4].SetVisible(false);
				break;
			default:
				Debug.Log(string.Concat(status, " can't be removed !"));
				break;
			}
		}
		MoveList.Remove(status);
	}

	public override Vector2 GetDamageTextPos()
	{
		return base.transform.position.xy() + new Vector2(0f, 3.5f);
	}

	private void OnDead()
	{
		LeanTween.cancel(base.gameObject);
	}

	public override void Unlock()
	{
		_unlockReady = true;
		if (InGame && (int)Hp > 0)
		{
			Activate = true;
		}
	}

	private void CheckPlayerStuk()
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < StageUpdate.runPlayers.Count; j++)
			{
				Vector2 zero = Vector2.zero;
				Vector3 position = StageUpdate.runPlayers[j].transform.position;
				Controller2D controller = StageUpdate.runPlayers[j].Controller;
				zero = controller.Collider2D.size;
				zero /= 2f;
				Vector2 vector = new Vector2(_wallboxcollider[i].transform.position.x + _wallboxcollider[i].offset.x - _wallboxcollider[i].size.x / 2f, _wallboxcollider[i].transform.position.y + _wallboxcollider[i].offset.y - _wallboxcollider[i].size.y / 2f);
				Vector2 vector2 = new Vector2(_wallboxcollider[i].transform.position.x + _wallboxcollider[i].offset.x + _wallboxcollider[i].size.x / 2f, _wallboxcollider[i].transform.position.y + _wallboxcollider[i].offset.y + _wallboxcollider[i].size.y / 2f);
				if (position.x >= vector.x - zero.x && position.x <= vector2.x + zero.x && position.y >= vector.y - zero.y && position.y <= vector2.y + zero.y)
				{
					switch (i)
					{
					case 0:
						controller.transform.position = new Vector3(vector.x - zero.x - 0.01f, position.y, 0f);
						controller.LogicPosition = new VInt3(controller.transform.position);
						break;
					case 1:
						controller.transform.position = new Vector3(vector2.x + zero.x + 0.01f, position.y, 0f);
						controller.LogicPosition = new VInt3(controller.transform.position);
						break;
					}
				}
			}
		}
	}
}
