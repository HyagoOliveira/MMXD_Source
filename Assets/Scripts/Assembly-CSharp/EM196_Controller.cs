using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM196_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Skill0 = 1,
		Skill1 = 2,
		Skill2 = 3,
		Hurt = 4
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		MAX_SUBSTATUS = 3
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private bool isSticky;

	[SerializeField]
	private float RotateAngle;

	private float NowAngle;

	private Vector3 StartPos;

	private Vector3 EndPos;

	private float MoveDis;

	private bool CanAtk = true;

	public bool hasHit;

	private Transform targetTransform;

	private Vector3 stickOffset = Vector3.up;

	[SerializeField]
	private float Skill1SuicideTime = 4f;

	[SerializeField]
	private float Skill2SuicideTime = 1f;

	private int SuicideFrame;

	private int Skill0AtkTimes;

	[SerializeField]
	private int MoveSpeed = 10000;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
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

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		base.AimPoint = Vector3.zero;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		IgnoreGravity = true;
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
			CanAtk = true;
			break;
		case MainStatus.Skill0:
			CanAtk = true;
			StartPos = NowPos;
			MoveDis = Vector2.Distance(StartPos, EndPos);
			_velocity = new VInt3((EndPos - StartPos).normalized) * MoveSpeed * 0.001f;
			break;
		case MainStatus.Skill1:
			hasHit = true;
			CanAtk = false;
			stickOffset = NowPos - targetTransform.position;
			SuicideFrame = GameLogicUpdateManager.GameFrame + (int)(Skill1SuicideTime * 20f);
			break;
		case MainStatus.Skill2:
			CanAtk = false;
			SuicideFrame = GameLogicUpdateManager.GameFrame + (int)(Skill2SuicideTime * 20f);
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
		BaseLogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			RotateSelf();
			break;
		case MainStatus.Skill0:
			RotateSelf();
			if (Vector2.Distance(NowPos, StartPos) > MoveDis)
			{
				_transform.position = EndPos;
				Controller.LogicPosition = new VInt3(NowPos);
				Skill0AtkTimes++;
				if (Skill0AtkTimes > 3)
				{
					SetStatus(MainStatus.Skill2);
				}
				else
				{
					SetStatus(MainStatus.Idle);
				}
			}
			break;
		case MainStatus.Skill1:
			if (targetTransform != null)
			{
				_transform.position = targetTransform.position + stickOffset;
			}
			if (GameLogicUpdateManager.GameFrame > SuicideFrame)
			{
				Suicide();
			}
			break;
		case MainStatus.Skill2:
			if (GameLogicUpdateManager.GameFrame > SuicideFrame)
			{
				Suicide();
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
		switch (_mainStatus)
		{
		case MainStatus.Skill0:
			_transform.position = Vector3.MoveTowards(_transform.position, Controller.LogicPosition.vec3, distanceDelta);
			break;
		case MainStatus.Skill1:
			if (targetTransform != null)
			{
				_transform.position = targetTransform.position + stickOffset;
			}
			Controller.LogicPosition = new VInt3(NowPos);
			break;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			IgnoreGravity = true;
			hasHit = false;
			Skill0AtkTimes = 0;
			ModelTransform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.HitCallback = TailHit;
			_collideBullet.Active(targetMask);
			SetStatus(MainStatus.Idle);
		}
		else
		{
			_collideBullet.BackToPool();
			_collideBullet.HitCallback = null;
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

	private Vector3 GetTargetPos(bool realcenter = false)
	{
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if ((bool)Target)
		{
			if (realcenter)
			{
				TargetPos = new VInt3(Target.Controller.GetRealCenterPos());
			}
			else
			{
				TargetPos = new VInt3(Target.GetTargetPoint() + Vector3.up * 0.15f);
			}
			return TargetPos.vec3;
		}
		return NowPos + Vector3.right * 3f * base.direction;
	}

	public bool SetAtkPos(Vector3 atkpos, bool bPlaySE = true)
	{
		if (!CanAtk)
		{
			return false;
		}
		EndPos = atkpos;
		if (bPlaySE)
		{
			PlayBossSE("BossSE05", "bs044_magne07");
		}
		SetStatus(MainStatus.Skill0);
		return true;
	}

	public void SetSuicide()
	{
		Suicide();
	}

	private void Suicide()
	{
		(BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, NowPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
		Hp = 0;
		Hurt(new HurtPassParam());
	}

	private void TailHit(object obj)
	{
		if (obj == null)
		{
			return;
		}
		Collider2D collider2D = obj as Collider2D;
		if (collider2D != null)
		{
			Target = OrangeBattleUtility.GetHitTargetOrangeCharacter(collider2D);
			if (Target != null)
			{
				targetTransform = Target._transform;
			}
		}
		if (targetTransform != null)
		{
			_collideBullet.Sleep();
			SetStatus(MainStatus.Skill1);
		}
	}

	private void RotateSelf()
	{
		NowAngle += RotateAngle;
		_transform.rotation = Quaternion.Euler(0f, NowAngle, 180f);
	}

	protected override void UpdateGravity()
	{
	}
}
