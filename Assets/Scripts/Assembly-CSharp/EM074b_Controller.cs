#define RELEASE
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM074b_Controller : EM040_Controller
{
	private bool _patrolIsLoop;

	private Vector3[] _patrolPaths = new Vector3[0];

	private int _patrolIndex;

	private bool _comeBack;

	private bool _endPatrol;

	[SerializeField]
	private float _moveSpeed = 1f;

	private bool _initPatrolPath;

	public Color mMain_Color = new Color(1f, 1f, 1f);

	public Color mEye_Color = new Color(1f, 1f, 1f);

	private float fTmpDis;

	protected override void Awake()
	{
		base.Awake();
		_patrolIndex = 0;
		_comeBack = false;
		_endPatrol = false;
		Change_color();
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_patrolIndex = 0;
			_endPatrol = false;
			_comeBack = false;
			TargetPos = Controller.LogicPosition;
		}
		else
		{
			_patrolPaths = new Vector3[0];
		}
	}

	private void Change_color()
	{
		for (int i = 0; i < _mesh.materials.Length; i++)
		{
			_mesh.materials[i].SetColor("_BodyColor", mMain_Color);
			_mesh.materials[i].SetColor("_EyeColor", mEye_Color);
		}
		ParticleSystem[] componentsInChildren = _fxLoop.transform.GetComponentsInChildren<ParticleSystem>();
		if (componentsInChildren != null)
		{
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				ParticleSystem.MainModule main = componentsInChildren[j].main;
				main.startColor = mMain_Color;
			}
		}
		ParticleSystem[] componentsInChildren2 = _fxEnd.transform.GetComponentsInChildren<ParticleSystem>();
		if (componentsInChildren2 != null)
		{
			for (int k = 0; k < componentsInChildren2.Length; k++)
			{
				ParticleSystem.MainModule main2 = componentsInChildren2[k].main;
				main2.startColor = mMain_Color;
			}
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		if (!_initPatrolPath)
		{
			_initPatrolPath = true;
			IgnoreGravity = true;
			if (_patrolPaths.Length != 0)
			{
				TargetPos = new VInt3(_patrolPaths[0]);
				Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
				_velocity = new VInt3(normalized * _moveSpeed);
			}
			else
			{
				_velocity = VInt3.zero;
			}
		}
		else if (_mainStatus == MainStatus.Idle && _patrolPaths.Length != 0 && !_endPatrol)
		{
			Vector3 normalized2 = (TargetPos - Controller.LogicPosition).vec3.normalized;
			_velocity = new VInt3(normalized2 * _moveSpeed);
		}
		else
		{
			_velocity = VInt3.zero;
		}
		BaseLogicUpdate();
		if (_mainStatus == MainStatus.Suck && _subStatus == SubStatus.Phase1 && (bool)Target)
		{
			Controller.LogicPosition = new VInt3(Target.Controller.LogicPosition.x, Target.Controller.LogicPosition.y + 500, Target.Controller.LogicPosition.z);
			_transform.position = Controller.LogicPosition.vec3;
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

	public override void UpdateFunc()
	{
		if (Activate)
		{
			_modelEulerAngles.y += (float)(RotateSpeed * 100) * Time.deltaTime;
			ModelTransform.localEulerAngles = _modelEulerAngles;
			fTmpDis = (TargetPos.vec3 - base.transform.position).magnitude;
			if (distanceDelta > fTmpDis)
			{
				base.transform.position = TargetPos.vec3;
				fTmpDis = 0f;
			}
			else
			{
				base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
				fTmpDis -= distanceDelta;
			}
			if (_mainStatus == MainStatus.Idle && _patrolPaths.Length != 0 && !_endPatrol && _initPatrolPath && fTmpDis < 0.01f)
			{
				base.transform.position = TargetPos.vec3;
				GotoNextPatrolPoint();
			}
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
		if (_patrolIsLoop && (paths[0] - paths[paths.Length - 1]).sqrMagnitude < 0.0015f)
		{
			_patrolPaths = new Vector3[paths.Length - 1];
		}
		else
		{
			_patrolPaths = new Vector3[paths.Length];
		}
		for (int i = 0; i < _patrolPaths.Length; i++)
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
					_velocity = VInt3.zero;
					_endPatrol = true;
					return;
				}
				_patrolIndex += ((!_comeBack) ? 1 : (-1));
			}
			TargetPos = new VInt3(_patrolPaths[_patrolIndex]);
			Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
			_velocity = new VInt3(normalized * _moveSpeed);
			if (StageUpdate.gbIsNetGame && StageUpdate.bIsHost)
			{
				RsSyncPos();
			}
		}
	}
}
