#define RELEASE
using System;
using System.Collections;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;
using enums;

public class EM040_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum MainStatus
	{
		Idle = 0,
		Suck = 1,
		Dead = 2,
		RqSync = 3,
		Sync = 4,
		SyncRotateSpeed = 5
	}

	protected enum SubStatus
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

	protected MeshRenderer _mesh;

	protected ParticleSystem _fxLoop;

	protected ParticleSystem _fxEnd;

	public int RotateSpeed;

	protected bool _isCatch;

	protected OrangeCharacter targetOC;

	protected MainStatus _mainStatus;

	protected SubStatus _subStatus;

	private Coroutine AttackSEPlayer;

	protected Vector3 _modelEulerAngles;

	protected bool bReBullet;

	protected bool bNeedRqSyncPos;

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_mesh = OrangeBattleUtility.FindChildRecursive(ref target, "Em040_02", true).GetComponent<MeshRenderer>();
		_fxLoop = OrangeBattleUtility.FindChildRecursive(ref target, "fx_loop", true).GetComponent<ParticleSystem>();
		_fxEnd = OrangeBattleUtility.FindChildRecursive(ref target, "fx_end", true).GetComponent<ParticleSystem>();
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Em040_02", true);
		_modelEulerAngles = ModelTransform.localEulerAngles;
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "CollideBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		_collideBullet.HitCallback = HitPlayer;
		base.AllowAutoAim = false;
		SetStatus(MainStatus.Idle);
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			bReBullet = true;
			bNeedRqSyncPos = true;
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

	private void UpdateSuck()
	{
		if (!(targetOC == null) && !targetOC.IsDead())
		{
			TargetPos = targetOC.Controller.LogicPosition;
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.TargetPosX = TargetPos.x;
			netSyncData.TargetPosY = TargetPos.y;
			netSyncData.TargetPosZ = TargetPos.z;
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			netSyncData.sParam0 = targetOC.sNetSerialID;
			StageUpdate.SyncStageObj(1, 0, sNetSerialID + "," + 1 + "," + JsonConvert.SerializeObject(netSyncData), true);
		}
	}

	protected void RqSyncPos()
	{
		StageUpdate.SyncStageObj(1, 0, sNetSerialID + "," + 3 + ",", true);
	}

	protected virtual void RsSyncPos()
	{
		NetSyncData netSyncData = new NetSyncData();
		netSyncData.TargetPosX = TargetPos.x;
		netSyncData.TargetPosY = TargetPos.y;
		netSyncData.TargetPosZ = TargetPos.z;
		netSyncData.SelfPosX = Controller.LogicPosition.x;
		netSyncData.SelfPosY = Controller.LogicPosition.y;
		netSyncData.SelfPosZ = Controller.LogicPosition.z;
		StageUpdate.SyncStageObj(1, 0, sNetSerialID + "," + 4 + "," + JsonConvert.SerializeObject(netSyncData), true);
	}

	public override void LogicUpdate()
	{
		if (Activate)
		{
			IgnoreGravity = true;
			base.LogicUpdate();
			_velocity.x = RotateSpeed * 300;
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
			_modelEulerAngles.y += (float)(3 * RotateSpeed * 100) * Time.deltaTime;
		}
	}

	public virtual void UpdateFunc()
	{
		if (Activate)
		{
			ModelTransform.localEulerAngles = Vector3.MoveTowards(ModelTransform.localEulerAngles, _modelEulerAngles, (float)(RotateSpeed * 100) * Time.deltaTime);
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if ((int)Hp <= 0)
		{
			SetStatus(MainStatus.Dead);
			return 0;
		}
		if (_mainStatus != 0)
		{
			return Hp;
		}
		if (tHurtPassParam.vBulletDis != Vector2.zero)
		{
			if (StageUpdate.gbIsNetGame)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, 5, Math.Sign(tHurtPassParam.vBulletDis.x).ToString(), true);
			}
			else
			{
				RotateSpeed += Math.Sign(tHurtPassParam.vBulletDis.x);
			}
		}
		if (!tHurtPassParam.IsBreak && tHurtPassParam.wpnType != WeaponType.Melee)
		{
			return Hp;
		}
		tHurtPassParam.dmg = selfBuffManager.ReduceDmgByEnergyShild(tHurtPassParam.dmg);
		OrangeBattleUtility.UpdateEnemyHp(ref Hp, ref tHurtPassParam.dmg);
		if (!InGame)
		{
			Debug.LogWarning("[Enemy] InGame Flag is false.");
			return Hp;
		}
		UpdateHurtAction();
		if ((int)Hp > 0)
		{
			_characterMaterial.Hurt();
		}
		else
		{
			SetStatus(MainStatus.Dead);
		}
		return Hp;
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
		_transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * -1f);
		_transform.position = pos;
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	protected void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Suck:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (targetOC == null)
				{
					Debug.LogWarning("EM040: 目標消失了!?");
					SetStatus(MainStatus.Idle);
					break;
				}
				RotateSpeed = 0;
				targetOC.SetStun(true);
				StartCoroutine(StageResManager.TweenVc3Coroutine(_transform.position, targetOC.GetTargetPoint(), 0.5f, delegate(Vector3 v)
				{
					Controller.LogicPosition = new VInt3(v);
				}, delegate
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}));
				break;
			case SubStatus.Phase1:
				_mesh.enabled = false;
				_fxLoop.Play(true);
				StartCoroutine(StageResManager.TweenFloatCoroutine(0f, 1f, 3f, null, delegate
				{
					SetStatus(MainStatus.Dead);
					if (AttackSEPlayer != null)
					{
						StopCoroutine(AttackSEPlayer);
						AttackSEPlayer = null;
					}
				}));
				if (AttackSEPlayer == null)
				{
					AttackSEPlayer = StartCoroutine(AttackLoopSE());
				}
				break;
			}
			break;
		case MainStatus.Dead:
			if ((bool)targetOC)
			{
				targetOC.SetStun(false);
			}
			targetOC = null;
			RotateSpeed = 0;
			_mesh.enabled = false;
			_fxLoop.Stop(true);
			_fxEnd.Play(true);
			_collideBullet.BackToPool();
			StartCoroutine(StageResManager.TweenFloatCoroutine(0f, 1f, 1f, null, BackToPool));
			PlaySE("HitSE", 102);
			break;
		case MainStatus.Idle:
			break;
		}
	}

	private void HitPlayer(object obj)
	{
		Collider2D collider2D = obj as Collider2D;
		if (collider2D == null)
		{
			return;
		}
		StageObjParam component = collider2D.transform.GetComponent<StageObjParam>();
		if ((bool)component)
		{
			OrangeCharacter orangeCharacter = component.tLinkSOB as OrangeCharacter;
			if (!(orangeCharacter == null) && !(orangeCharacter.sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify) && !_isCatch)
			{
				_isCatch = true;
				Target = orangeCharacter;
				targetOC = Target;
				UpdateSuck();
				SetStatus(MainStatus.Suck);
			}
		}
	}

	public override void BackToPool()
	{
		if (_isCatch && (bool)targetOC)
		{
			targetOC.SetStun(false);
		}
		base.BackToPool();
		_isCatch = false;
		_fxLoop.Stop(true);
		_fxEnd.Stop(true);
		_mesh.enabled = true;
		_modelEulerAngles.y = 0f;
		ModelTransform.localEulerAngles = _modelEulerAngles;
		SetStatus(MainStatus.Idle);
		RotateSpeed = 0;
		Target = null;
		targetOC = null;
	}

	private IEnumerator AttackLoopSE()
	{
		while (true)
		{
			PlaySE("EnemySE", 53);
			yield return new WaitForSeconds(0.5f);
		}
	}
}
