using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM215_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Move = 1,
		Skill = 2,
		Hurt = 3
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

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus _mainStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SubStatus _subStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private float _currentFrame;

	private Vector3 StartPos;

	private Vector3 EndPos;

	private float MoveDis;

	private int CanStopCount;

	[SerializeField]
	private int MoveSpd = 6000;

	[SerializeField]
	private int SkillSpeed = 6000;

	[SerializeField]
	private float SkillActTime1 = 20f;

	[SerializeField]
	private float SkillActTime2 = 5f;

	private int ActionTimes;

	private int ActionFrame;

	private bool GoReturn;

	private Vector3 BornPos;

	[SerializeField]
	private ParticleSystem FireModel;

	public BossCorpsTool CorpsTool;

	private Vector3 _OldVector3;

	private Vector3 _NowVecrot3;

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
			if (CorpsTool != null)
			{
				CorpsTool.fightState = BossCorpsTool.FightState.Rest;
			}
			_velocity = VInt3.zero;
			_NowVecrot3 = Vector3.zero;
			_OldVector3 = Vector3.zero;
			break;
		case MainStatus.Move:
			if (_subStatus == SubStatus.Phase0)
			{
				StartPos = NowPos;
				if (GoReturn)
				{
					EndPos = BornPos;
				}
				else
				{
					EndPos = StartPos;
				}
				PlayBossSE("BossSE", "bs011_panda02");
				MoveDis = Vector2.Distance(EndPos, StartPos);
				_velocity = new VInt3((EndPos - StartPos).normalized) * MoveSpd * 0.001f;
				_NowVecrot3 = (_OldVector3 = _velocity.vec3);
			}
			break;
		case MainStatus.Skill:
		{
			if (_subStatus != 0)
			{
				break;
			}
			for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
			{
				if ((bool)StageUpdate.runPlayers[i])
				{
					Target = StageUpdate.runPlayers[i];
					break;
				}
			}
			if (!Target)
			{
				break;
			}
			StartPos = NowPos;
			EndPos = Target._transform.position;
			if (Mathf.Abs(Target._transform.position.x - NowPos.x) > Mathf.Abs(Target._transform.position.y - NowPos.y))
			{
				MoveDis = Mathf.Abs(Target._transform.position.x - NowPos.x);
				if (Target._transform.position.x > NowPos.x)
				{
					_velocity = VInt3.right * SkillSpeed * 0.001f;
				}
				else
				{
					_velocity = -VInt3.right * SkillSpeed * 0.001f;
				}
			}
			else
			{
				MoveDis = Mathf.Abs(Target._transform.position.y - NowPos.y);
				if (Target._transform.position.y > NowPos.y)
				{
					_velocity = VInt3.up * SkillSpeed * 0.001f;
				}
				else
				{
					_velocity = -VInt3.up * SkillSpeed * 0.001f;
				}
			}
			_NowVecrot3 = _velocity.vec3;
			if (_OldVector3.normalized != _NowVecrot3.normalized)
			{
				if (MoveDis > 0.5f)
				{
					PlayBossSE("BossSE", "bs011_panda02");
				}
				_OldVector3 = _NowVecrot3;
			}
			break;
		}
		}
		AiTimer.TimerStart();
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			if (CanStopCount < 10)
			{
				CanStopCount++;
			}
			else if (FireModel.isPlaying)
			{
				SwitchFireModel(false);
			}
			return;
		}
		BaseLogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (FireModel.isPlaying)
			{
				SwitchFireModel(false);
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!bWaitNetStatus)
			{
				bool flag = (bool)Target;
			}
			break;
		case MainStatus.Move:
			if (_subStatus == SubStatus.Phase0 && Vector2.Distance(NowPos, StartPos) >= MoveDis)
			{
				_transform.position = EndPos;
				Controller.LogicPosition = new VInt3(EndPos);
				if (GoReturn)
				{
					PlaySE("BossSE", "bs011_panda03");
					SetStatus(MainStatus.Idle);
				}
				else
				{
					SetStatus(MainStatus.Skill);
				}
			}
			break;
		case MainStatus.Skill:
			if (_subStatus == SubStatus.Phase0 && Vector2.Distance(NowPos, StartPos) >= MoveDis)
			{
				SetStatus(MainStatus.Skill);
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
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		IgnoreGravity = true;
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			GoReturn = false;
			SetStatus(MainStatus.Idle);
			if (CorpsTool == null && CorpsTool.Master == null)
			{
				CorpsTool = new BossCorpsTool(this, Hp, true);
				for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
				{
					BS121_Controller component = StageUpdate.runEnemys[i].mEnemy.GetComponent<BS121_Controller>();
					if ((bool)component && component.Activate)
					{
						CorpsTool.Master = component;
					}
				}
			}
			if (CorpsTool != null)
			{
				CorpsTool.fightState = BossCorpsTool.FightState.Rest;
			}
		}
		else
		{
			_collideBullet.BackToPool();
			FireModel.Clear();
			FireModel.Stop();
			if (CorpsTool != null)
			{
				CorpsTool.fightState = BossCorpsTool.FightState.Dead;
			}
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, ModelTransform.localScale.z * (float)base.direction);
		_transform.position = pos;
		BornPos = NowPos;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		return base.Hurt(tHurtPassParam);
	}

	public void GoAttack()
	{
		GoReturn = false;
		CorpsTool.fightState = BossCorpsTool.FightState.Fighting;
		SwitchFireModel(true);
		if (_mainStatus != MainStatus.Skill)
		{
			SetStatus(MainStatus.Skill);
		}
	}

	public void GoBack()
	{
		GoReturn = true;
		if (_mainStatus != MainStatus.Move)
		{
			SetStatus(MainStatus.Move);
		}
	}

	private void SwitchFireModel(bool onoff)
	{
		if (onoff)
		{
			FireModel.Play();
			if ((bool)_collideBullet)
			{
				_collideBullet.Active(targetMask);
			}
		}
		else
		{
			FireModel.Pause();
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
		}
	}
}
