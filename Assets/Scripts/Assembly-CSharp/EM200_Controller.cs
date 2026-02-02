using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using UnityEngine;

public class EM200_Controller : EnemyControllerBase, IManagedUpdateBehavior
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

	[SerializeField]
	[ReadOnly]
	private MainStatus _mainStatus;

	[SerializeField]
	[ReadOnly]
	private SubStatus _subStatus;

	private bool isSticky;

	[SerializeField]
	private float RotateAngle;

	private float NowAngle;

	private Vector3 StartPos;

	private Vector3 EndPos;

	private Vector3 ShieldBackEndPos;

	private float MoveDis;

	public bool hasBack;

	[SerializeField]
	private float Skill1AtkInterrval = 3f;

	[SerializeField]
	private SkinnedMeshRenderer ShieldMesh;

	[SerializeField]
	private float VibrationDis = 0.5f;

	private int VDirection = 1;

	[SerializeField]
	private float VibrationTime = 0.5f;

	private int Skill1AtkFrame;

	private bool NeedUseSkill2;

	[SerializeField]
	private int MoveSpeed = 12000;

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
			Skill1AtkFrame = GameLogicUpdateManager.GameFrame + (int)(Skill1AtkInterrval * 20f);
			break;
		case MainStatus.Skill0:
			StartPos = NowPos;
			MoveDis = Vector2.Distance(StartPos, EndPos);
			_velocity = new VInt3((EndPos - StartPos).normalized) * MoveSpeed * 0.001f;
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE06", "bs050_dopsig15");
				Skill1AtkFrame = GameLogicUpdateManager.GameFrame + (int)(VibrationTime * 20f);
				break;
			case SubStatus.Phase1:
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				StartPos = NowPos;
				EndPos = GetTargetPos(true);
				MoveDis = Vector2.Distance(StartPos, EndPos);
				_velocity = new VInt3((EndPos - StartPos).normalized) * MoveSpeed * 0.001f;
				break;
			}
			break;
		case MainStatus.Skill2:
			PlaySE("BossSE06", "bs050_dopsig15");
			StartPos = NowPos;
			MoveDis = Vector2.Distance(StartPos, ShieldBackEndPos);
			_velocity = new VInt3((ShieldBackEndPos - StartPos).normalized) * MoveSpeed * 0.001f;
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
			if (NeedUseSkill2)
			{
				SetStatus(MainStatus.Skill2);
			}
			else if (!bWaitNetStatus && Skill1AtkFrame < GameLogicUpdateManager.GameFrame)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					UploadEnemyStatus(2);
				}
			}
			break;
		case MainStatus.Skill0:
		case MainStatus.Skill2:
			RotateSelf();
			break;
		case MainStatus.Skill1:
			RotateSelf();
			if (_subStatus != 0)
			{
				int num = 1;
			}
			else if (GameLogicUpdateManager.GameFrame > Skill1AtkFrame)
			{
				SetStatus(MainStatus.Skill1, SubStatus.Phase1);
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
		_transform.position = Vector3.MoveTowards(_transform.position, Controller.LogicPosition.vec3, distanceDelta);
		switch (_mainStatus)
		{
		case MainStatus.Skill0:
			if (Vector2.Distance(NowPos, StartPos) >= MoveDis)
			{
				_transform.position = EndPos;
				Controller.LogicPosition = new VInt3(NowPos);
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ModelTransform.position = new Vector3((float)VDirection * VibrationDis, 0f, 0f) + ModelTransform.position;
				VDirection *= -1;
				break;
			case SubStatus.Phase1:
				if (Vector2.Distance(NowPos, StartPos) >= MoveDis)
				{
					_transform.position = EndPos;
					Controller.LogicPosition = new VInt3(NowPos);
					_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
					_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
					_collideBullet.Active(targetMask);
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			if (Vector2.Distance(NowPos, StartPos) >= MoveDis)
			{
				hasBack = true;
				ShieldMesh.enabled = false;
				_transform.position = EndPos;
				Controller.LogicPosition = new VInt3(NowPos);
				SetStatus(MainStatus.Idle);
			}
			break;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			IgnoreGravity = true;
			hasBack = false;
			ShieldMesh.enabled = true;
			NeedUseSkill2 = false;
			ModelTransform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
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

	public void SetSuicide()
	{
		Suicide();
	}

	private void Suicide()
	{
		Hp = 0;
		Hurt(new HurtPassParam());
	}

	private void RotateSelf()
	{
		NowAngle += RotateAngle;
		ModelTransform.localRotation = Quaternion.Euler(0f, NowAngle, 0f);
	}

	protected override void UpdateGravity()
	{
	}

	public void SetSkill0(Vector3 endpos)
	{
		EndPos = endpos;
		SetStatus(MainStatus.Skill0);
	}

	public void SetSkill2(Vector3 endpos)
	{
		ShieldBackEndPos = endpos;
		NeedUseSkill2 = true;
	}
}
