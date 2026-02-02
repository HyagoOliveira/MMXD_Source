#define RELEASE
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM074_Controller : EM040_Controller
{
	private bool _patrolIsLoop;

	private Vector3[] _patrolPaths = new Vector3[0];

	private int _patrolIndex;

	private bool _comeBack;

	private bool _endPatrol;

	[SerializeField]
	private float _moveSpeed = 1f;

	private bool _initPatrolPath;

	protected override void Awake()
	{
		base.Awake();
		_patrolIndex = 0;
		_comeBack = false;
		_endPatrol = false;
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_patrolIndex = 0;
			_endPatrol = false;
			_comeBack = false;
		}
		else
		{
			_patrolPaths = new Vector3[0];
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		IgnoreGravity = true;
		BaseLogicUpdate();
		if (!_initPatrolPath)
		{
			_initPatrolPath = true;
			if (_patrolPaths.Length != 0)
			{
				TargetPos = new VInt3(_patrolPaths[0]);
				Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
				_velocity = new VInt3(normalized * _moveSpeed);
			}
		}
		if (_mainStatus == MainStatus.Idle && _patrolPaths.Length != 0 && !_endPatrol)
		{
			float num = Vector3.Distance(TargetPos.vec3, Controller.LogicPosition.vec3);
			if (num < 0.05f || num < distanceDelta * 2f)
			{
				GotoNextPatrolPoint();
			}
			else
			{
				Vector3 normalized2 = (TargetPos - Controller.LogicPosition).vec3.normalized;
				_velocity = new VInt3(normalized2 * _moveSpeed);
			}
		}
		else
		{
			_velocity = VInt3.zero;
		}
		if (bReBullet)
		{
			bReBullet = false;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		if (bNeedRqSyncPos && !MonoBehaviourSingleton<OrangeGameManager>.Instance.bLastGamePause)
		{
			bNeedRqSyncPos = false;
			RqSyncPos();
		}
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		if ((int)Hp <= 0)
		{
			return;
		}
		switch (nSet)
		{
		case 0:
		case 1:
		case 4:
		{
			if (smsg == null || !(smsg != "") || smsg[0] != '{')
			{
				break;
			}
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
			Controller.LogicPosition.x = netSyncData.SelfPosX;
			Controller.LogicPosition.y = netSyncData.SelfPosY;
			Controller.LogicPosition.z = netSyncData.SelfPosZ;
			TargetPos.x = netSyncData.TargetPosX;
			TargetPos.y = netSyncData.TargetPosY;
			TargetPos.z = netSyncData.TargetPosZ;
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
			if (nSet == 1 && netSyncData.sParam0 != null && netSyncData.sParam0 != "")
			{
				OrangeCharacter playerByID = StageUpdate.GetPlayerByID(netSyncData.sParam0);
				if (targetOC != null)
				{
					targetOC.SetStun(false);
				}
				_isCatch = true;
				Target = playerByID;
				targetOC = playerByID;
				playerByID.Controller.LogicPosition = TargetPos;
				playerByID.transform.localPosition = TargetPos.vec3;
				playerByID.vLastMovePt = TargetPos.vec3;
				SetStatus(MainStatus.Suck);
				Debug.LogWarning("Net Suck " + playerByID.name);
			}
			if (nSet == 4)
			{
				_patrolIndex = netSyncData.nParam0;
				Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
				_velocity = new VInt3(normalized * _moveSpeed);
			}
			break;
		}
		case 3:
			RsSyncPos();
			break;
		case 5:
			RotateSpeed += int.Parse(smsg);
			break;
		case 2:
			break;
		}
	}

	protected override void RsSyncPos()
	{
		NetSyncData netSyncData = new NetSyncData();
		netSyncData.TargetPosX = TargetPos.x;
		netSyncData.TargetPosY = TargetPos.y;
		netSyncData.TargetPosZ = TargetPos.z;
		netSyncData.SelfPosX = Controller.LogicPosition.x;
		netSyncData.SelfPosY = Controller.LogicPosition.y;
		netSyncData.SelfPosZ = Controller.LogicPosition.z;
		netSyncData.nParam0 = _patrolIndex;
		StageUpdate.SyncStageObj(1, 0, sNetSerialID + "," + 4 + "," + JsonConvert.SerializeObject(netSyncData), true);
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
		base.SetPatrolPath(isLoop, nMoveSpeed, paths);
		_initPatrolPath = false;
		_patrolIsLoop = isLoop;
		if (nMoveSpeed > 0)
		{
			_moveSpeed = (float)nMoveSpeed * 0.001f;
		}
		_patrolPaths = new Vector3[paths.Length];
		for (int i = 0; i < paths.Length; i++)
		{
			_patrolPaths[i] = paths[i];
		}
	}

	private void GotoNextPatrolPoint()
	{
		if (_patrolPaths.Length == 0)
		{
			TargetPos = Controller.LogicPosition;
			_velocity = VInt3.zero;
		}
		if (_patrolPaths.Length == 1)
		{
			TargetPos = Controller.LogicPosition;
			_velocity = VInt3.zero;
			_endPatrol = true;
		}
		else
		{
			if (_patrolPaths.Length <= 1)
			{
				return;
			}
			if (_patrolIsLoop)
			{
				_patrolIndex++;
				if (_patrolIndex >= _patrolPaths.Length)
				{
					_patrolIndex = 0;
				}
			}
			else
			{
				if ((!_comeBack && _patrolIndex + 1 >= _patrolPaths.Length) || (_comeBack && _patrolIndex == 0))
				{
					_comeBack = !_comeBack;
				}
				_patrolIndex += ((!_comeBack) ? 1 : (-1));
			}
			TargetPos = new VInt3(_patrolPaths[_patrolIndex]);
			Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
			_velocity = new VInt3(normalized * _moveSpeed);
		}
	}
}
