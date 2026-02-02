using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using UnityEngine;

public class EM059_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Drop = 1,
		Walk = 2,
		Recover = 3,
		IdleWaitNet = 4
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		MAX_SUBSTATUS = 4
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_WALK = 1,
		ANI_RECOVER = 2,
		ANI_HURT = 3,
		MAX_ANIMATION_ID = 4
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private float _currentFrame;

	private int[] _animationHash;

	private float RotateY = 53.84f;

	[SerializeField]
	private int WalkSpeed = 2500;

	[SerializeField]
	private int JumpForce = 9000;

	[SerializeField]
	private int GravityReduce = 1200;

	private bool _isJumping;

	private bool _isRecovering;

	public float BulletPos = 9f;

	protected BulletBase[] RockBullet = new BulletBase[4];

	private float Vibration = 0.08f;

	private int VDirection = 1;

	private bool _isVOver = true;

	private int MaxShellHP = 600;

	private int ShellHP;

	public Transform[] ShellDebris;

	private OrangeTimer timer = OrangeTimerManager.GetTimer();

	private ParticleSystem ShellRevert;

	[SerializeField]
	private float distanceX = 2f;

	private bool bHit;

	private int lastHitPosX = int.MaxValue;

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
		ShellDebris = OrangeBattleUtility.FindAllChildRecursive(ref target, "EM059_SubMesh");
		ShellRevert = OrangeBattleUtility.FindChildRecursive(ref target, "fx_ShellRevert", true).gameObject.GetComponent<ParticleSystem>();
		_mainStatus = MainStatus.Idle;
		_subStatus = SubStatus.Phase0;
		base.AimPoint = new Vector3(0f, 0f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(10f, 5f);
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		bool flag = false;
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
			if (netSyncData.nParam0 > 0)
			{
				ShellHP -= netSyncData.nParam0;
				float num = (float)ShellHP / (float)MaxShellHP;
				if (num < 0f && ShellDebris[2].gameObject.activeSelf)
				{
					ShellDebris[2].gameObject.SetActive(false);
					flag = true;
				}
				if (num < 0.25f && ShellDebris[1].gameObject.activeSelf)
				{
					ShellDebris[1].gameObject.SetActive(false);
					flag = true;
				}
				if (num < 0.5f && ShellDebris[3].gameObject.activeSelf)
				{
					ShellDebris[3].gameObject.SetActive(false);
					ShellDebris[0].gameObject.SetActive(false);
					flag = true;
				}
				if (num < 0.75f && ShellDebris[4].gameObject.activeSelf)
				{
					ShellDebris[4].gameObject.SetActive(false);
					flag = true;
				}
				if (flag)
				{
					PlayFX();
					PlaySE(EnemySE02.CRI_ENEMYSE02_EM034_BALL03);
				}
			}
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
		}
		if (nSet != 0)
		{
			SetStatus((MainStatus)nSet);
		}
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
		ModelTransform.localEulerAngles = new Vector3(ModelTransform.localEulerAngles.x, (float)base.direction * RotateY, ModelTransform.localEulerAngles.z);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			ModelTransform.localEulerAngles = new Vector3(ModelTransform.localEulerAngles.x, 180f, ModelTransform.localEulerAngles.z);
			_velocity.x = 0;
			break;
		case MainStatus.Drop:
			_isJumping = false;
			ModelTransform.localEulerAngles = new Vector3(ModelTransform.localEulerAngles.x, 53.84f, ModelTransform.localEulerAngles.z);
			UpdateDirection();
			break;
		case MainStatus.Walk:
			_isJumping = true;
			_velocity.x = base.direction * WalkSpeed;
			bHit = false;
			break;
		case MainStatus.Recover:
			_isJumping = false;
			break;
		}
		AiTimer.TimerStart();
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
			IgnoreGravity = true;
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				timer.TimerStart();
				_isVOver = false;
				SetStatus(MainStatus.Drop);
			}
			break;
		case MainStatus.Walk:
			if (Controller.Collisions.left || Controller.Collisions.right)
			{
				bHit = true;
				PlayFX();
				UpdateDirection(-base.direction);
				_velocity.x = base.direction * WalkSpeed;
			}
			if ((bool)Target)
			{
				TargetPos = new VInt3(Target._transform.position);
				if ((Target._transform.position.x + (float)base.direction * distanceX - _transform.position.x) * (float)base.direction < 0f)
				{
					UpdateDirection();
					_velocity.x = base.direction * WalkSpeed;
					bHit = false;
				}
			}
			if (bHit)
			{
				PlaySE(EnemySE02.CRI_ENEMYSE02_EM034_BALL02);
				bHit = false;
			}
			if (Controller.Collisions.below)
			{
				_velocity.y = JumpForce;
			}
			if (CheckMoveFall(_velocity))
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Drop:
		case MainStatus.Recover:
			break;
		}
	}

	public override void FalldownUpdate()
	{
		if (!isFall)
		{
			isFall = !Controller.Collisions.below;
			return;
		}
		if (!IgnoreGravity)
		{
			_velocity.y += GravityReduce;
		}
		if (Controller.Collisions.below)
		{
			PlayFX();
			isFall = false;
			PlaySE(EnemySE02.CRI_ENEMYSE02_EM034_BALL01);
		}
	}

	public void UpdateFunc()
	{
		if (!Activate)
		{
			return;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		MainStatus mainStatus = MainStatus.Idle;
		switch (_mainStatus)
		{
		case MainStatus.Drop:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (timer.GetMillisecond() < 600 && !_isVOver)
			{
				ModelTransform.position = new Vector3(0f, (float)VDirection * Vibration, 0f) + ModelTransform.position;
				VDirection *= -1;
			}
			else if (!_isVOver)
			{
				ModelTransform.position = new Vector3(0f, 0f, 0f) + ModelTransform.position;
				PlaySE(EnemySE02.CRI_ENEMYSE02_EM034_BALL03);
				PlayFX();
				IgnoreGravity = false;
				timer.TimerStop();
				_isVOver = true;
			}
			if (Controller.Collisions.below)
			{
				UpdateDirection();
				SetStatus(MainStatus.Walk);
			}
			if (ShellHP < 0)
			{
				mainStatus = MainStatus.Recover;
				_velocity.x = 0;
				IgnoreGravity = true;
				_isRecovering = false;
				timer.TimerStart();
			}
			break;
		case MainStatus.Walk:
			if (ShellHP < 0)
			{
				mainStatus = MainStatus.Recover;
				_velocity.x = 0;
				IgnoreGravity = true;
				_isRecovering = false;
				timer.TimerStart();
			}
			break;
		case MainStatus.Recover:
			if (_isRecovering)
			{
				ModelTransform.position = new Vector3((float)VDirection * Vibration, 0f, 0f) + ModelTransform.position;
				VDirection *= -1;
			}
			if (!_isRecovering && timer.GetMillisecond() > 1000)
			{
				PlaySE(EnemySE02.CRI_ENEMYSE02_EM034_BALL04_LG);
				ShellRevert.Play();
				_isRecovering = true;
				GenRecoverBullet(BulletPos);
			}
			else if (timer.GetMillisecond() > 2000)
			{
				Transform[] shellDebris = ShellDebris;
				for (int i = 0; i < shellDebris.Length; i++)
				{
					shellDebris[i].gameObject.SetActive(true);
				}
				ShellHP = MaxShellHP;
				timer.TimerStop();
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					UpdateDirection();
				}
				PlaySE(EnemySE02.CRI_ENEMYSE02_EM034_BALL04_STOP);
				ShellRevert.Stop();
				IgnoreGravity = false;
				SetStatus(MainStatus.Walk);
			}
			break;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus, false, new object[1] { 0 });
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		Transform[] shellDebris = ShellDebris;
		for (int i = 0; i < shellDebris.Length; i++)
		{
			shellDebris[i].gameObject.SetActive(true);
		}
		MaxShellHP = Hp;
		Hp = (int)Hp + MaxShellHP;
		ShellHP = MaxShellHP;
		SetStatus(MainStatus.Idle);
		ShellRevert.Stop();
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
		base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * -1f);
		base.transform.position = pos;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (_mainStatus.Equals(MainStatus.Idle))
		{
			timer.TimerStart();
			_isVOver = false;
			SetStatus(MainStatus.Drop);
		}
		if (ShellHP < 0)
		{
			return OverrideBaseHurt(tHurtPassParam);
		}
		int num = tHurtPassParam.dmg;
		UploadEnemyStatus((int)_mainStatus, true, false, new object[1] { num });
		tHurtPassParam.dmg = 0;
		return OverrideBaseHurt(tHurtPassParam);
	}

	private ObscuredInt OverrideBaseHurt(HurtPassParam tHurtPassParam)
	{
		OrangeBattleUtility.UpdateEnemyHp(ref Hp, ref tHurtPassParam.dmg);
		if ((int)Hp > 0)
		{
			base.IsHidden = false;
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Hurt();
			}
		}
		else
		{
			base.IsHidden = true;
			DeadBehavior(ref tHurtPassParam);
		}
		return Hp;
	}

	protected virtual void GenRecoverBullet(float BulletPos)
	{
		RockBullet[0] = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, new Vector3(0f - BulletPos, 0f - BulletPos, 0f) + ModelTransform.position, new Vector3(1f, 1f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		RockBullet[1] = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, new Vector3(0f - BulletPos, BulletPos, 0f) + ModelTransform.position, new Vector3(1f, -1f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		RockBullet[2] = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, new Vector3(BulletPos, 0f - BulletPos, 0f) + ModelTransform.position, new Vector3(-1f, 1f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		RockBullet[3] = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, new Vector3(BulletPos, BulletPos, 0f) + ModelTransform.position, new Vector3(-1f, -1f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
	}

	protected virtual void PlayFX()
	{
		if (lastHitPosX != Controller.LogicPosition.x)
		{
			lastHitPosX = Controller.LogicPosition.x;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_em059_icebroken_000", ModelTransform.transform, Quaternion.identity, Array.Empty<object>());
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_isRecovering)
		{
			BulletBase[] rockBullet = RockBullet;
			for (int i = 0; i < rockBullet.Length; i++)
			{
				(rockBullet[i] as BasicBullet).ChangeDirection(new Vector3(0f, 0f, -90f));
			}
			_isRecovering = false;
		}
		base.DeadBehavior(ref tHurtPassParam);
	}
}
