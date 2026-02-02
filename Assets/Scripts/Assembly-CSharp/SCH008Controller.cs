using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class SCH008Controller : PetControllerBase
{
	private long _nLifeTime;

	private OrangeTimer _lifeTimer;

	private Transform _shootPoint;

	private Transform _shootPointUp;

	private BulletBase _beam;

	private int _oldDirection;

	private bool _bTraceType;

	private float _moveSpeed = 10f;

	private IAimTarget _assignTarget;

	private VInt3 _traceTargetPos = VInt3.zero;

	private bool bSetTracePos;

	private const int nAtkMaxDistanceX = 400;

	private const int nAtkMaxDistanceY = 1000;

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_shootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint", true);
		_shootPointUp = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPointUp", true);
		StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
		{
			BeamBullet component = ((GameObject)asset).GetComponent<BeamBullet>();
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BeamBullet>(UnityEngine.Object.Instantiate(component), "p_waterbeam_000", 5);
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/bullet/p_waterbeam_000", "p_waterbeam_000", loadCallBackObj.LoadCB);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_summer-penguin_000", 2);
		_lifeTimer = OrangeTimerManager.GetTimer();
	}

	protected override void Start()
	{
		base.Start();
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_velocity = VInt3.zero;
		if (isActive)
		{
			_lifeTimer.TimerStart();
			_oldDirection = direction;
			for (int i = 0; i < EquippedWeaponNum; i++)
			{
				PetWeapons[i].LastUseTimer.TimerStart();
				PetWeapons[i].MagazineRemain = PetWeapons[i].BulletData.n_MAGAZINE;
			}
			return;
		}
		_autoAim.SetEnable(false);
		_autoAim.ClearTargetList();
		SetFollowEnabled(false);
		bSetTracePos = false;
		if ((bool)_beam)
		{
			_beam.BackToPool();
		}
	}

	public void FollowPlayerDead()
	{
		SetActive(false);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_summer-penguin_000", base.transform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
		switch (nSet)
		{
		case 1:
			if (!_follow_Player.IsLocalPlayer)
			{
				Controller.LogicPosition.x = netSyncData.SelfPosX;
				Controller.LogicPosition.y = netSyncData.SelfPosY;
				Controller.LogicPosition.z = netSyncData.SelfPosZ;
			}
			_traceTargetPos.x = netSyncData.TargetPosX;
			_traceTargetPos.y = netSyncData.TargetPosY;
			_traceTargetPos.z = netSyncData.TargetPosZ;
			bSetTracePos = true;
			direction = netSyncData.nParam0;
			if (ModelTransform != null)
			{
				ModelTransform.localScale = new Vector3(1f, 1f, direction);
			}
			break;
		case 2:
			if (!_follow_Player.IsLocalPlayer)
			{
				Controller.LogicPosition.x = netSyncData.SelfPosX;
				Controller.LogicPosition.y = netSyncData.SelfPosY;
				Controller.LogicPosition.z = netSyncData.SelfPosZ;
				if (_bTraceType)
				{
					_transform.localPosition = Controller.LogicPosition.vec3;
				}
			}
			bSetTracePos = false;
			direction = netSyncData.nParam0;
			if (ModelTransform != null)
			{
				ModelTransform.localScale = new Vector3(1f, 1f, direction);
			}
			if (_beam == null)
			{
				UpdateSkillCD(0);
				CreateSkillBullet();
			}
			if (_bTraceType)
			{
				_velocity = VInt3.zero;
			}
			break;
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		if (!_bTraceType)
		{
			if (_follow_Player != null && _follow_Player.IsLocalPlayer)
			{
				if (_beam == null && isSkillAvailable(0))
				{
					UpdateSkillCD(0);
					CreateSkillBullet();
					NetSyncData netSyncData = new NetSyncData();
					netSyncData.SelfPosX = Controller.LogicPosition.x;
					netSyncData.SelfPosY = Controller.LogicPosition.y;
					netSyncData.SelfPosZ = Controller.LogicPosition.z;
					netSyncData.TargetPosX = _traceTargetPos.x;
					netSyncData.TargetPosY = _traceTargetPos.y;
					netSyncData.TargetPosZ = _traceTargetPos.z;
					netSyncData.nParam0 = direction;
					StageUpdate.RegisterPetSendAndRun(sNetSerialID, 2, JsonConvert.SerializeObject(netSyncData), true);
				}
				else if (_beam != null && _oldDirection != direction)
				{
					NetSyncData netSyncData2 = new NetSyncData();
					netSyncData2.SelfPosX = Controller.LogicPosition.x;
					netSyncData2.SelfPosY = Controller.LogicPosition.y;
					netSyncData2.SelfPosZ = Controller.LogicPosition.z;
					netSyncData2.TargetPosX = _traceTargetPos.x;
					netSyncData2.TargetPosY = _traceTargetPos.y;
					netSyncData2.TargetPosZ = _traceTargetPos.z;
					netSyncData2.nParam0 = direction;
					StageUpdate.RegisterPetSendAndRun(sNetSerialID, 1, JsonConvert.SerializeObject(netSyncData2), true);
				}
				_oldDirection = direction;
			}
		}
		else
		{
			if (_follow_Player != null && _follow_Player.IsLocalPlayer && _beam == null && PetWeapons[0].MagazineRemain > 0f)
			{
				Vector3 position = base.transform.position;
				if (Mathf.Abs(position.x * 1000f - (float)_traceTargetPos.x) < 400f && Mathf.Abs(position.y * 1000f - (float)_traceTargetPos.y) < 1000f)
				{
					UpdateSkillCD(0);
					CreateSkillBullet();
					NetSyncData netSyncData3 = new NetSyncData();
					Controller.LogicPosition = new VInt3(_transform.localPosition);
					netSyncData3.SelfPosX = Controller.LogicPosition.x;
					netSyncData3.SelfPosY = Controller.LogicPosition.y;
					netSyncData3.SelfPosZ = Controller.LogicPosition.z;
					netSyncData3.TargetPosX = _traceTargetPos.x;
					netSyncData3.TargetPosY = _traceTargetPos.y;
					netSyncData3.TargetPosZ = _traceTargetPos.z;
					netSyncData3.nParam0 = direction;
					StageUpdate.RegisterPetSendAndRun(sNetSerialID, 2, JsonConvert.SerializeObject(netSyncData3), true);
				}
				else
				{
					SyncTracePosition();
				}
			}
			UpdateTracePos();
		}
		if (_lifeTimer.GetMillisecond() > _nLifeTime)
		{
			SetActive(false);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_summer-penguin_000", base.transform.position, Quaternion.identity, Array.Empty<object>());
		}
	}

	protected void SyncTracePosition()
	{
		if (!(_follow_Player == null) && _follow_Player.IsLocalPlayer)
		{
			VInt3 vInt = ((_autoAim.AutoAimTarget == null || !(_autoAim.AutoAimTarget.AimTransform != null)) ? (_follow_Player.Controller.LogicPosition + new VInt3(3000 * _follow_Player.direction, -1000, 0)) : new VInt3(_autoAim.AutoAimTarget.AimTransform.transform.position + Vector3.down * 3f));
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			netSyncData.TargetPosX = vInt.x;
			netSyncData.TargetPosY = vInt.y;
			netSyncData.TargetPosZ = vInt.z;
			netSyncData.nParam0 = direction;
			StageUpdate.RegisterPetSendAndRun(sNetSerialID, 1, JsonConvert.SerializeObject(netSyncData), true);
		}
	}

	protected void UpdateTracePos()
	{
		if (bSetTracePos && _beam == null && PetWeapons[0].MagazineRemain > 0f)
		{
			VInt3 vInt = _traceTargetPos - Controller.LogicPosition;
			vInt.z = 0;
			_velocity.x = (int)(vInt.vec3.normalized.x * _moveSpeed * 1000f);
			_velocity.y = (int)(vInt.vec3.normalized.y * _moveSpeed * 1000f);
			if ((float)Mathf.Abs(vInt.x) < Mathf.Abs((float)_velocity.x * GameLogicUpdateManager.m_fFrameLen))
			{
				_velocity.x = vInt.x * 5;
			}
			if ((float)Mathf.Abs(vInt.y) < Mathf.Abs((float)_velocity.y * GameLogicUpdateManager.m_fFrameLen))
			{
				_velocity.y = vInt.y * 5;
			}
		}
		else
		{
			_velocity = VInt3.zero;
		}
	}

	public override void UpdateFunc()
	{
		if (Activate)
		{
			_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			_currentFrame = 0f;
		}
	}

	public override void UpdateDirecion()
	{
		if (_bTraceType || _follow_Player.IsLocalPlayer)
		{
			base.UpdateDirecion();
		}
	}

	public override void UpdateSkillCD(int skillIndex)
	{
		PetWeapons[skillIndex].MagazineRemain -= PetWeapons[skillIndex].BulletData.n_USE_COST;
		PetWeapons[skillIndex].LastUseTimer.TimerStart();
	}

	public void SetParam(long lifeTime, bool isTraceType, float moveSpeed, IAimTarget target = null)
	{
		_nLifeTime = lifeTime;
		_bTraceType = isTraceType;
		_moveSpeed = moveSpeed;
		_assignTarget = target;
		if (_bTraceType)
		{
			_autoAim.UpdateAimRange(12f);
			_autoAim.SetEnable(true);
			_autoAim.SetTarget(_assignTarget);
		}
	}

	protected void CreateSkillBullet()
	{
		Vector3 pDirection = ((direction == 1) ? Vector3.right : Vector3.left);
		BulletBase bulletBase = null;
		bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BeamBullet>("p_waterbeam_000");
		bulletBase.UpdateBulletData(PetWeapons[0].BulletData, _follow_Player.sPlayerName, _follow_Player.GetNowRecordNO(), _follow_Player.nBulletRecordID++);
		bulletBase.SetBulletAtk(_follow_Player.PlayerSkills[follow_skill_id].weaponStatus, selfBuffManager.sBuffStatus);
		bulletBase.BulletLevel = _follow_Player.PlayerSkills[follow_skill_id].SkillLV;
		bulletBase.SetPetBullet();
		bulletBase.SoundSource.ForcePlaySE("SkillSE_CH042_000", "irs_crystal02_lp");
		if (!_bTraceType)
		{
			bulletBase.Active(_shootPoint, pDirection, base.TargetMask);
			bulletBase.transform.parent = _shootPoint;
			bulletBase.BackCallback = OnBeamBackToPoolCallback;
		}
		else
		{
			bulletBase.Active(_shootPointUp, Vector3.up, base.TargetMask);
			bulletBase.transform.parent = _shootPointUp;
			bulletBase.BackCallback = OnBeamBackToPoolCallback;
		}
		bulletBase.transform.localPosition = Vector3.zero;
		_beam = bulletBase;
	}

	protected void OnBeamBackToPoolCallback(object obj)
	{
		if ((bool)_beam)
		{
			_beam.transform.parent = null;
			_beam = null;
		}
		if (Activate && _bTraceType && _lifeTimer.GetMillisecond() + 500 < _nLifeTime)
		{
			_nLifeTime = _lifeTimer.GetMillisecond() + 500;
		}
	}
}
