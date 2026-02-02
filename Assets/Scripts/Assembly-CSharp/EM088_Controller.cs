#define RELEASE
using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using UnityEngine;

public class EM088_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Grow = 1,
		Flower = 2,
		Shoot = 3,
		NoFlower = 4,
		Hurt = 5,
		IdleWaitNet = 6
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		MAX_SUBSTATUS = 6
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_FLOWER = 1,
		ANI_HURT = 2,
		MAX_ANIMATION_ID = 3
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	[SerializeField]
	private int ShootFrame;

	private float distance;

	private Vector3[] _patrolPaths = new Vector3[0];

	private int _patrolIndex;

	private Transform ShootPoint;

	private GameObject FlowerMesh;

	private GameObject ObjInfoBar;

	private bool isClosedSOB;

	[SerializeField]
	private int GrowSpeed = 4500;

	private readonly int _HashAngle = Animator.StringToHash("angle");

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	private void HashAnimator()
	{
		_animationHash = new int[3];
		_animationHash[0] = Animator.StringToHash("EM088@idle2_loop");
		_animationHash[1] = Animator.StringToHash("EM088@flowering");
		_animationHash[2] = Animator.StringToHash("EM088@hurt_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = OrangeBattleUtility.FindChildRecursive(ref childs, "EM088_G", true).GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		ShootPoint = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPoint", true);
		FlowerMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "EM088_BodyMesh_sub", true).gameObject;
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		HashAnimator();
		LoadParts(ref childs);
		base.AimPoint = Vector3.zero;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(8f);
		AiTimer.TimerStart();
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
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
		}
		SetStatus((MainStatus)nSet);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Grow:
			if (_subStatus == SubStatus.Phase0)
			{
				_velocity = new VInt3((_patrolPaths[1] - _patrolPaths[0]).normalized) * GrowSpeed * 0.001f;
				distance = Vector3.Distance(_patrolPaths[0], _patrolPaths[1]);
			}
			break;
		case MainStatus.Flower:
			if (_subStatus == SubStatus.Phase0)
			{
				_velocity = VInt3.zero;
			}
			break;
		case MainStatus.Shoot:
			if (_subStatus == SubStatus.Phase0)
			{
				_velocity = VInt3.zero;
				ShootFrame = GameLogicUpdateManager.GameFrame + EnemyWeapons[1].BulletData.n_FIRE_SPEED * 20 / 1000;
			}
			break;
		case MainStatus.NoFlower:
		{
			SetColliderEnable(false);
			_velocity = VInt3.zero;
			ObjInfoBar componentInChildren = _transform.GetComponentInChildren<ObjInfoBar>();
			if ((bool)componentInChildren)
			{
				componentInChildren.gameObject.SetActive(false);
			}
			base.AllowAutoAim = false;
			isClosedSOB = false;
			FlowerMesh.SetActive(false);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_MOB_EXPLODE0", ShootPoint.position, Quaternion.identity, Array.Empty<object>());
			break;
		}
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Grow:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Flower:
			_currentAnimationId = AnimationID.ANI_FLOWER;
			break;
		case MainStatus.Shoot:
		case MainStatus.NoFlower:
			return;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	public override void LogicUpdate()
	{
		if (!Activate || !_enemyAutoAimSystem)
		{
			return;
		}
		BaseLogicUpdate();
		if (IsStun)
		{
			return;
		}
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!bWaitNetStatus && (bool)Target)
			{
				UploadEnemyStatus(1);
			}
			break;
		case MainStatus.Grow:
			if (_subStatus == SubStatus.Phase0 && distance <= 0f)
			{
				SetStatus(MainStatus.Flower);
			}
			break;
		case MainStatus.Flower:
			if (_subStatus == SubStatus.Phase0 && _currentFrame > 1f)
			{
				SetStatus(MainStatus.Shoot);
			}
			break;
		case MainStatus.Shoot:
			if (_subStatus == SubStatus.Phase0 && ShootFrame < GameLogicUpdateManager.GameFrame)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					Vector3 pDirection = Target._transform.position - _transform.position;
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPoint, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
					SetStatus(MainStatus.Shoot);
				}
			}
			break;
		case MainStatus.NoFlower:
			if (!isClosedSOB)
			{
				StageObjParam[] componentsInChildren = GetComponentsInChildren<StageObjParam>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].tLinkSOB = null;
				}
				isClosedSOB = true;
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			Vector3 localPosition = _transform.localPosition;
			_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			MainStatus mainStatus = _mainStatus;
			if (mainStatus == MainStatus.Grow && _subStatus == SubStatus.Phase0)
			{
				distance -= Vector3.Distance(localPosition, _transform.localPosition);
			}
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			IgnoreGravity = true;
			_patrolIndex = 0;
			FlowerMesh.SetActive(true);
			base.AllowAutoAim = true;
			Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.AddOrGetComponent<StageObjParam>().tLinkSOB = this;
			}
			EnemyCollider[] componentsInChildren2 = GetComponentsInChildren<EnemyCollider>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].gameObject.AddOrGetComponent<StageObjParam>().tLinkSOB = this;
			}
			SetStatus(MainStatus.Idle);
		}
		else
		{
			_patrolPaths = new Vector3[0];
			_collideBullet.BackToPool();
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
		ModelTransform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * (float)base.direction);
		_transform.position = pos;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
		base.SetPatrolPath(isLoop, nMoveSpeed, paths);
		if (nMoveSpeed > 0)
		{
			GrowSpeed = nMoveSpeed;
		}
		_patrolPaths = new Vector3[paths.Length];
		for (int i = 0; i < paths.Length; i++)
		{
			_patrolPaths[i] = paths[i];
		}
		if (_patrolPaths.Length < 2)
		{
			Debug.LogError("EM088 棘蔓花 小怪需要至少長度為2的巡邏路線決定生長方向跟長度");
		}
		_transform.position = _patrolPaths[0];
		Controller.LogicPosition = new VInt3(_transform.position);
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (_mainStatus == MainStatus.Idle || _mainStatus == MainStatus.Grow)
		{
			return Hp;
		}
		if ((int)Hp - (int)tHurtPassParam.dmg <= 0 && tHurtPassParam.IsPlayer)
		{
			tHurtPassParam.dmg = (int)Hp - 1;
			if (_mainStatus != MainStatus.NoFlower)
			{
				SetColliderEnable(false);
				ObjInfoBar componentInChildren = _transform.GetComponentInChildren<ObjInfoBar>();
				if ((bool)componentInChildren)
				{
					componentInChildren.gameObject.SetActive(false);
				}
				base.AllowAutoAim = false;
				PlaySE("HitSE", "ht_dead01");
				UploadEnemyStatus(4);
			}
		}
		return base.Hurt(tHurtPassParam);
	}

	public override void Explosion()
	{
		if ((bool)ExplosionPart)
		{
			EnemyDieCollider poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<EnemyDieCollider>(ExplosionPart.Name);
			poolObj.transform.position = _transform.position;
			poolObj.transform.rotation = Quaternion.Euler(Vector3.up * (90 - 90 * base.direction));
			poolObj.ActiveExplosion();
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_MOB_EXPLODE0", AimPosition, Quaternion.identity, Array.Empty<object>());
	}
}
