#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS121_Controller : EnemyControllerBase, IManagedUpdateBehavior, IF_Master
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Skill2 = 4,
		Skill3 = 5,
		Die = 6
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

	private enum SkillPattern
	{
		State1 = 1,
		State2 = 2,
		State3 = 3,
		State4 = 4,
		State5 = 5,
		State6 = 6,
		State7 = 7,
		State8 = 8,
		State9 = 9,
		State10 = 10,
		State11 = 11,
		State12 = 12,
		MaxState = 13
	}

	private enum Parts
	{
		Orange = 0,
		Yellow = 1,
		Purple = 2,
		Black = 3,
		Mouse = 4
	}

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus _mainStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SubStatus _subStatus;

	private int nDeadCount;

	private int[] _animationHash;

	private int[] DefaultSkillCard = new int[6] { 0, 1, 2, 3, 4, 5 };

	private static int[] DefaultRangedSkillCard = new int[3] { 3, 4, 5 };

	private List<int> RangedSKC = new List<int>();

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[Header("通用")]
	private int ActionFrame;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private Vector3 CenterPos;

	private RaycastHit2D hitup;

	private RaycastHit2D hitdown;

	private RaycastHit2D hitright;

	private RaycastHit2D hitleft;

	private int blocklayer;

	[Header("目前屬下")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private BossCorpsTool[] Corps = new BossCorpsTool[2];

	private int SpawnCount;

	[Header("登場")]
	[SerializeField]
	private float DebutTime = 1f;

	private int DebutWaitFrame;

	[Header("生眼睛")]
	[SerializeField]
	private Transform LEyePos;

	[SerializeField]
	private Transform REyePos;

	private bool CallEyeBack;

	[Header("生嘴")]
	[SerializeField]
	private Transform MousePos;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private BossCorpsTool MouseCorp;

	[Header("關門")]
	[SerializeField]
	private Transform LDoorObj;

	[SerializeField]
	private Transform RDoorObj;

	[SerializeField]
	private float DoorMoveDis = 5f;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private Vector3 LDoorClosePos;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private Vector3 RDoorClosePos;

	[SerializeField]
	private float DoorMoveTime = 2.5f;

	[Header("開門")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private Vector3 LDoorOpenPos;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private Vector3 RDoorOpenPos;

	private bool CanNextState;

	[Header("AI控制")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SkillPattern skillPattern = SkillPattern.State1;

	private SkillPattern nextState = SkillPattern.State1;

	[Header("待機等待Frame")]
	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private bool bPlayLoop;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
	}

	private bool isCorpsBacked
	{
		get
		{
			bool flag = true;
			BossCorpsTool[] corps = Corps;
			foreach (BossCorpsTool bossCorpsTool in corps)
			{
				if (bossCorpsTool != null && bossCorpsTool.fightState == BossCorpsTool.FightState.Fighting)
				{
					flag = false;
					break;
				}
			}
			if (!flag || MouseCorp == null)
			{
				return flag;
			}
			if (MouseCorp.fightState == BossCorpsTool.FightState.Fighting)
			{
				flag = false;
				EM215_Controller eM215_Controller = MouseCorp.Member as EM215_Controller;
				if ((bool)eM215_Controller)
				{
					eM215_Controller.GoBack();
				}
				eM215_Controller = null;
			}
			if (flag)
			{
				Corps[0] = (Corps[1] = null);
			}
			return flag;
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

	protected void ChangeDebugMode()
	{
		DebugMode = !DebugMode;
	}

	protected void ChangeSetSkill(object[] param)
	{
		string text = param[0] as string;
		if (!(text == string.Empty))
		{
			switch (text)
			{
			case "Idle":
				NextSkill = MainStatus.Idle;
				break;
			case "Skill0":
				NextSkill = MainStatus.Skill0;
				break;
			case "Skill1":
				NextSkill = MainStatus.Skill1;
				break;
			case "Skill2":
				NextSkill = MainStatus.Skill2;
				break;
			case "Skill3":
				NextSkill = MainStatus.Skill3;
				break;
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		_transform.position = pos;
	}

	public override void BossIntro(Action cb)
	{
		IntroCallBack = cb;
		_introReady = true;
	}

	public override void Unlock()
	{
		_unlockReady = true;
		base.AllowAutoAim = true;
		if ((int)Hp > 0)
		{
			SetColliderEnable(true);
		}
		if (InGame)
		{
			Activate = true;
		}
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
	}

	protected override void Awake()
	{
		base.Awake();
		blocklayer = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer);
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		base.AimPoint = new Vector3(0f, 0.6f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
		base.Start();
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
			if (netSyncData.nParam0 > 0 && netSyncData.nParam0 < 13)
			{
				skillPattern = (SkillPattern)netSyncData.nParam0;
			}
			if (netSyncData.sParam0 != "" && netSyncData.sParam0 != null)
			{
				try
				{
					SpawnCount = int.Parse(netSyncData.sParam0);
				}
				catch
				{
					SpawnCount = GameLogicUpdateManager.GameFrame;
				}
			}
		}
		SetStatus((MainStatus)nSet);
	}

	private void DoorSE(bool StartStop)
	{
		if (StartStop)
		{
			if (!bPlayLoop)
			{
				PlaySE("BossSE", "bs011_panda04_lp");
				bPlayLoop = true;
			}
		}
		else if (bPlayLoop)
		{
			PlaySE("BossSE", "bs011_panda04_stop");
			PlaySE("BossSE", "bs011_panda05");
			bPlayLoop = false;
		}
	}

	private void UpdateDirection(int forceDirection = 0, bool back = false)
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
		if (back)
		{
			base.direction = -base.direction;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			}
			break;
		case MainStatus.Idle:
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				switch (skillPattern)
				{
				case SkillPattern.State1:
					Corps[0] = SpawnEnemy(Parts.Orange, REyePos.position);
					SwitchDoor(LDoorObj, false);
					SwitchDoor(RDoorObj, false);
					break;
				case SkillPattern.State2:
					Corps[0] = SpawnEnemy(Parts.Yellow, REyePos.position);
					break;
				case SkillPattern.State3:
					SwitchDoor(LDoorObj, false);
					SwitchDoor(RDoorObj, false);
					Corps[0] = SpawnEnemy(Parts.Purple, REyePos.position);
					Corps[1] = SpawnEnemy(Parts.Black, LEyePos.position);
					break;
				case SkillPattern.State4:
					SwitchDoor(LDoorObj, false);
					SwitchDoor(RDoorObj, false);
					Corps[0] = SpawnEnemy(Parts.Black, REyePos.position);
					Corps[1] = SpawnEnemy(Parts.Orange, LEyePos.position);
					break;
				case SkillPattern.State5:
					SwitchDoor(LDoorObj, false);
					SwitchDoor(RDoorObj, false);
					Corps[0] = SpawnEnemy(Parts.Yellow, REyePos.position);
					break;
				case SkillPattern.State6:
					Corps[0] = SpawnEnemy(Parts.Purple, REyePos.position);
					break;
				case SkillPattern.State7:
					SwitchDoor(LDoorObj, false);
					SwitchDoor(RDoorObj, false);
					Corps[0] = SpawnEnemy(Parts.Orange, REyePos.position);
					Corps[1] = SpawnEnemy(Parts.Yellow, LEyePos.position);
					break;
				case SkillPattern.State8:
					SwitchDoor(LDoorObj, false);
					SwitchDoor(RDoorObj, false);
					Corps[0] = SpawnEnemy(Parts.Purple, REyePos.position);
					break;
				case SkillPattern.State9:
					Corps[1] = SpawnEnemy(Parts.Black, LEyePos.position);
					break;
				case SkillPattern.State10:
					Corps[0] = SpawnEnemy(Parts.Orange, REyePos.position);
					break;
				case SkillPattern.State11:
					SwitchDoor(LDoorObj, false);
					SwitchDoor(RDoorObj, false);
					Corps[0] = SpawnEnemy(Parts.Purple, REyePos.position);
					Corps[1] = SpawnEnemy(Parts.Yellow, LEyePos.position);
					break;
				case SkillPattern.State12:
					SwitchDoor(LDoorObj, false);
					SwitchDoor(RDoorObj, false);
					Corps[0] = SpawnEnemy(Parts.Black, REyePos.position);
					break;
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (MouseCorp == null)
				{
					MouseCorp = SpawnEnemy(Parts.Mouse, MousePos.position);
				}
				if (MouseCorp != null)
				{
					MouseCorp.Member._transform.position = MousePos.position;
					MouseCorp.Member.Controller.LogicPosition = new VInt3(MousePos.position);
					EM215_Controller eM215_Controller = MouseCorp.Member as EM215_Controller;
					if ((bool)eM215_Controller)
					{
						eM215_Controller.GoAttack();
					}
					eM215_Controller = null;
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				CanNextState = false;
				bool isLeft = true;
				Transform Door = LDoorObj;
				float x2 = LDoorClosePos.x;
				switch (skillPattern)
				{
				case SkillPattern.State2:
				case SkillPattern.State9:
					isLeft = false;
					break;
				case SkillPattern.State6:
				case SkillPattern.State10:
					isLeft = true;
					break;
				default:
					Debug.LogError("目前AI階段不該進來這裡 AI階段是： " + skillPattern);
					break;
				}
				if (!isLeft)
				{
					Door = RDoorObj;
					x2 = RDoorClosePos.x;
				}
				SwitchDoor(Door, true);
				DoorSE(true);
				LeanTween.value(base.gameObject, Door.position.x, x2, DoorMoveTime).setOnUpdate(delegate(float f)
				{
					Vector3 position = Door.position;
					Door.position = new Vector3(f, Door.position.y, Door.position.z);
					DoorMoveCheck(Door, position, isLeft);
				}).setOnComplete((Action)delegate
				{
					DoorSE(false);
					CanNextState = true;
				});
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)((DoorMoveTime + 0.1f) * 20f);
				SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				break;
			}
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				CanNextState = false;
				bool flag = true;
				Transform Door2 = LDoorObj;
				float x = LDoorOpenPos.x;
				switch (skillPattern)
				{
				case SkillPattern.State2:
				case SkillPattern.State9:
					flag = false;
					break;
				case SkillPattern.State6:
				case SkillPattern.State10:
					flag = true;
					break;
				default:
					Debug.LogError("目前AI階段不該進來這裡 AI階段是： " + skillPattern);
					break;
				}
				if (!flag)
				{
					Door2 = RDoorObj;
					x = RDoorOpenPos.x;
				}
				DoorSE(true);
				LeanTween.value(base.gameObject, Door2.position.x, x, DoorMoveTime).setOnUpdate(delegate(float f)
				{
					Door2.position = new Vector3(f, Door2.position.y, Door2.position.z);
				}).setOnComplete((Action)delegate
				{
					DoorSE(false);
					CanNextState = true;
					SwitchDoor(Door2, false);
				});
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)((DoorMoveTime + 0.1f) * 20f);
				SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				break;
			}
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				if (bPlayLoop)
				{
					PlaySE("BossSE", "bs011_panda04_stop");
					bPlayLoop = false;
				}
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				base.DeadPlayCompleted = true;
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_002)
				{
					StartCoroutine(BossDieFlow(MousePos.position + Vector3.up, "FX_BOSS_EXPLODE2", false, false));
				}
				else
				{
					StartCoroutine(BossDieFlow(MousePos.position + Vector3.up, "FX_BOSS_EXPLODE2", false, false));
				}
				break;
			}
			}
			break;
		}
		AiTimer.TimerStart();
	}

	private void UpdateNextState(MainStatus status = MainStatus.Idle)
	{
		MainStatus mainStatus = status;
		if (status == MainStatus.Idle)
		{
			switch (_mainStatus)
			{
			case MainStatus.Debut:
				SetStatus(MainStatus.Idle);
				break;
			case MainStatus.Idle:
			{
				if (DebugMode)
				{
					mainStatus = NextSkill;
					break;
				}
				this.skillPattern = nextState;
				SkillPattern skillPattern = this.skillPattern;
				if (skillPattern == SkillPattern.State2 || skillPattern == SkillPattern.State6 || (uint)(skillPattern - 9) <= 1u)
				{
					mainStatus = MainStatus.Skill2;
					nextState = this.skillPattern + 1;
				}
				else
				{
					mainStatus = MainStatus.Skill0;
					nextState = this.skillPattern + 1;
				}
				if (nextState == SkillPattern.MaxState)
				{
					nextState = SkillPattern.State1;
				}
				break;
			}
			}
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus, false, new object[1] { (int)this.skillPattern }, new object[1] { SpawnCount.ToString() });
		}
	}

	public override void LogicUpdate()
	{
		if (_mainStatus == MainStatus.Debut)
		{
			BaseUpdate();
			UpdateGravity();
			Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
			distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > DebutWaitFrame)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (IntroCallBack != null)
				{
					IntroCallBack();
					if (!bWaitNetStatus)
					{
						UpdateNextState();
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (bWaitNetStatus || IdleWaitFrame >= GameLogicUpdateManager.GameFrame)
			{
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!Target)
			{
				for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
				{
					if ((int)StageUpdate.runPlayers[i].Hp > 0)
					{
						Target = StageUpdate.runPlayers[i];
						break;
					}
				}
			}
			if ((bool)Target)
			{
				UpdateNextState();
			}
			else
			{
				UpdateNextState();
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
			{
				SkillPattern skillPattern = this.skillPattern;
				if (skillPattern == SkillPattern.State2 || skillPattern == SkillPattern.State6 || (uint)(skillPattern - 9) <= 1u)
				{
					if (!bWaitNetStatus && CheckHost() && isCorpsBacked)
					{
						UploadEnemyStatus(5, false, new object[1] { (int)this.skillPattern }, new object[1] { SpawnCount.ToString() });
					}
				}
				else if (isCorpsBacked)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (!bWaitNetStatus && CheckHost())
				{
					UploadEnemyStatus(2, false, new object[1] { (int)this.skillPattern }, new object[1] { SpawnCount.ToString() });
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					CanNextState = true;
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (!bWaitNetStatus && CheckHost() && CanNextState)
				{
					LeanTween.cancel(base.gameObject);
					CanNextState = false;
					UploadEnemyStatus(3, false, new object[1] { (int)this.skillPattern }, new object[1] { SpawnCount.ToString() });
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					CanNextState = true;
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (isCorpsBacked && CanNextState)
				{
					switch (this.skillPattern)
					{
					case SkillPattern.State2:
					case SkillPattern.State9:
						SwitchDoor(RDoorObj, false);
						break;
					case SkillPattern.State6:
					case SkillPattern.State10:
						SwitchDoor(LDoorObj, false);
						break;
					default:
						Debug.LogError("目前AI階段不該進來這裡 AI階段是： " + this.skillPattern);
						break;
					}
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Die:
		{
			SubStatus subStatus = _subStatus;
			break;
		}
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		IgnoreGravity = true;
		if (isActive)
		{
			LDoorClosePos = LDoorObj.position + Vector3.right * DoorMoveDis;
			RDoorClosePos = RDoorObj.position + Vector3.left * DoorMoveDis;
			LDoorOpenPos = LDoorObj.position;
			RDoorOpenPos = RDoorObj.position;
			LDoorObj.GetComponent<StageObjParam>().tLinkSOB = null;
			RDoorObj.GetComponent<StageObjParam>().tLinkSOB = null;
			SwitchDoor(LDoorObj, false);
			SwitchDoor(RDoorObj, false);
			SetStatus(MainStatus.Debut);
			MouseCorp = SpawnEnemy(Parts.Mouse, MousePos.position);
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus == MainStatus.Die)
		{
			return;
		}
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		if (Corps[0] != null)
		{
			Corps[0].Member.BackToPool();
			Corps[0] = null;
		}
		if (Corps[1] != null)
		{
			Corps[1].Member.BackToPool();
			Corps[1] = null;
		}
		if (MouseCorp != null)
		{
			EM215_Controller eM215_Controller = MouseCorp.Member as EM215_Controller;
			eM215_Controller._transform.position = MousePos.position;
			eM215_Controller.Controller.LogicPosition = new VInt3(MousePos.position);
			if ((bool)eM215_Controller)
			{
				eM215_Controller.BackToPool();
			}
			eM215_Controller = null;
		}
		StageUpdate.SlowStage();
		SetColliderEnable(false);
		SetStatus(MainStatus.Die);
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

	private void SwitchCollideBullet(CollideBullet collide, bool onoff, int weaponid = -1, int targetlayer = 0)
	{
		if (!collide)
		{
			return;
		}
		if (weaponid != -1)
		{
			collide.UpdateBulletData(EnemyWeapons[weaponid].BulletData);
			collide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		}
		if (onoff)
		{
			if (!collide.IsActivate)
			{
				switch (targetlayer)
				{
				case 1:
					collide.Active(friendMask);
					break;
				case 2:
					collide.Active(neutralMask);
					break;
				default:
					collide.Active(targetMask);
					break;
				}
			}
		}
		else if (collide.IsActivate)
		{
			collide.BackToPool();
		}
	}

	private void SwitchFx(ParticleSystem Fx, bool onoff)
	{
		if ((bool)Fx)
		{
			if (onoff)
			{
				Fx.Play();
				return;
			}
			Fx.Stop();
			Fx.Clear();
		}
		else
		{
			Debug.Log(string.Concat("特效載入有誤，目前狀態是 ", _mainStatus, "的階段 ", _subStatus));
		}
	}

	private BossCorpsTool SpawnEnemy(Parts eye, Vector3 SpawnPos)
	{
		int num = (int)eye;
		MOB_TABLE enemy = GetEnemy((int)EnemyWeapons[num].BulletData.f_EFFECT_X);
		if (enemy == null)
		{
			Debug.LogError("要生成的怪物資料有誤，生怪技能ID " + num + " 怪物GroupID " + EnemyWeapons[num].BulletData.f_EFFECT_X);
			return null;
		}
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(enemy, sNetSerialID + SpawnCount);
		SpawnCount++;
		if ((bool)enemyControllerBase)
		{
			BossCorpsTool bossCorpsTool = null;
			EnemyControllerBase enemyControllerBase2 = enemyControllerBase;
			if ((object)enemyControllerBase2 != null)
			{
				EM211_Controller eM211_Controller;
				if ((object)(eM211_Controller = enemyControllerBase2 as EM211_Controller) == null)
				{
					EM212_Controller eM212_Controller;
					if ((object)(eM212_Controller = enemyControllerBase2 as EM212_Controller) == null)
					{
						EM213_Controller eM213_Controller;
						if ((object)(eM213_Controller = enemyControllerBase2 as EM213_Controller) == null)
						{
							EM214_Controller eM214_Controller;
							if ((object)(eM214_Controller = enemyControllerBase2 as EM214_Controller) == null)
							{
								EM215_Controller eM215_Controller;
								if ((object)(eM215_Controller = enemyControllerBase2 as EM215_Controller) != null)
								{
									EM215_Controller eM215_Controller2 = eM215_Controller;
									eM215_Controller2.CorpsTool = new BossCorpsTool(eM215_Controller2, eM215_Controller2.Hp, true);
									bossCorpsTool = eM215_Controller2.CorpsTool;
								}
							}
							else
							{
								EM214_Controller eM214_Controller2 = eM214_Controller;
								eM214_Controller2.CorpsTool = new BossCorpsTool(eM214_Controller2, eM214_Controller2.Hp, true);
								bossCorpsTool = eM214_Controller2.CorpsTool;
							}
						}
						else
						{
							EM213_Controller eM213_Controller2 = eM213_Controller;
							eM213_Controller2.CorpsTool = new BossCorpsTool(eM213_Controller2, eM213_Controller2.Hp, true);
							bossCorpsTool = eM213_Controller2.CorpsTool;
						}
					}
					else
					{
						EM212_Controller eM212_Controller2 = eM212_Controller;
						eM212_Controller2.CorpsTool = new BossCorpsTool(eM212_Controller2, eM212_Controller2.Hp, true);
						eM212_Controller2.SetMovePos(CheckRoomSize(SpawnPos));
						bossCorpsTool = eM212_Controller2.CorpsTool;
					}
				}
				else
				{
					EM211_Controller eM211_Controller2 = eM211_Controller;
					eM211_Controller2.CorpsTool = new BossCorpsTool(eM211_Controller2, eM211_Controller2.Hp, true);
					eM211_Controller2.SetMovePos(CheckRoomSize(SpawnPos));
					bossCorpsTool = eM211_Controller2.CorpsTool;
				}
			}
			if (bossCorpsTool != null)
			{
				enemyControllerBase.SetPositionAndRotation(SpawnPos, false);
				enemyControllerBase.SetActive(true);
				bossCorpsTool.Master = this;
				return bossCorpsTool;
			}
			Debug.LogError("Boss 生怪異常，請回報。 異常的怪物是： " + enemy.s_NAME);
		}
		return null;
	}

	private MOB_TABLE GetEnemy(int nGroupID)
	{
		MOB_TABLE[] mobArrayFromGroup = ManagedSingleton<OrangeTableHelper>.Instance.GetMobArrayFromGroup(nGroupID);
		for (int i = 0; i < mobArrayFromGroup.Length; i++)
		{
			if (mobArrayFromGroup[i].n_DIFFICULTY == StageUpdate.gDifficulty)
			{
				return mobArrayFromGroup[i];
			}
		}
		return null;
	}

	public void ReportObjects(object[] values)
	{
		int num = (int)values[0];
		if (num > 0)
		{
			HurtPassParam hurtPassParam = new HurtPassParam();
			hurtPassParam.dmg = num;
			Hurt(hurtPassParam);
		}
	}

	public object[] GetValues(object[] param = null)
	{
		return null;
	}

	private void CheckPlayerInsideDoor(Transform door, bool isLeft)
	{
		BoxCollider2D component = door.GetComponent<BoxCollider2D>();
		Vector2 point = door.position.xy() + new Vector2(0f - component.offset.x, component.offset.y);
		Collider2D collider2D = Physics2D.OverlapBox(point, component.size, 0f, LayerMask.GetMask("Player"));
		if (!collider2D)
		{
			return;
		}
		OrangeCharacter component2 = collider2D.gameObject.GetComponent<OrangeCharacter>();
		if ((bool)component2)
		{
			if (isLeft)
			{
				component2._transform.position = new Vector3(point.x + component.size.x / 2f + component2.Controller.Collider2D.size.x / 2f, component2._transform.position.y, component2._transform.position.z);
				component2.Controller.LogicPosition = new VInt3(component2._transform.position);
			}
			else
			{
				component2._transform.position = new Vector3(point.x - component.size.x / 2f - component2.Controller.Collider2D.size.x / 2f, component2._transform.position.y, component2._transform.position.z);
				component2.Controller.LogicPosition = new VInt3(component2._transform.position);
			}
		}
	}

	private void DoorMoveCheck(Transform door, Vector3 lastpos, bool isLeft)
	{
		BoxCollider2D component = door.GetComponent<BoxCollider2D>();
		if (component == null)
		{
			Debug.LogError("門沒有碰撞，請回報");
			return;
		}
		CheckPlayerInsideDoor(door, isLeft);
		MovePlayer(door, component, lastpos);
	}

	protected void MovePlayer(Transform door, BoxCollider2D BoxCol2D, Vector3 doorlastpos)
	{
		for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
		{
			OrangeCharacter orangeCharacter = StageUpdate.runPlayers[i];
			Vector2 zero = Vector2.zero;
			Controller2D controller = orangeCharacter.Controller;
			zero = controller.Collider2D.size;
			zero /= 2f;
			Vector3 realCenterPos = controller.GetRealCenterPos();
			Vector2 vector = new Vector2(door.position.x + BoxCol2D.offset.x - (BoxCol2D.size.x / 2f - 0.05f), door.position.y + BoxCol2D.offset.y - BoxCol2D.size.y / 2f - 0.05f);
			Vector2 vector2 = new Vector2(door.position.x + BoxCol2D.offset.x + BoxCol2D.size.x / 2f + 0.05f, door.position.y + BoxCol2D.offset.y + BoxCol2D.size.y / 2f + 0.05f);
			if (!(realCenterPos.x >= vector.x - zero.x) || !(realCenterPos.x <= vector2.x + zero.x) || !(realCenterPos.y >= vector.y - zero.y) || !(realCenterPos.y <= vector2.y + zero.y))
			{
				continue;
			}
			float num = zero.y * 2f + BoxCol2D.size.y / 2f + 0.05f;
			float num2 = BoxCol2D.size.x / 2f;
			float distance = zero.y - 0.015f;
			Vector3 vector3 = door.position - doorlastpos;
			Vector3 position = orangeCharacter._transform.position;
			if (Mathf.Abs(vector3.y) > 0f)
			{
				realCenterPos += vector3;
				hitup = OrangeBattleUtility.RaycastIgnoreSelf(realCenterPos, Vector2.up, distance, blocklayer, door);
				hitdown = OrangeBattleUtility.RaycastIgnoreSelf(realCenterPos, Vector2.down, distance, blocklayer, door);
				if ((bool)hitup && orangeCharacter._transform.position.y + vector3.y + 0.05f >= door.position.y + BoxCol2D.size.y / 2f + BoxCol2D.offset.y && realCenterPos.x - zero.x < vector2.x && realCenterPos.x + zero.x > vector.x)
				{
					vector3 += Vector3.down * (num + BoxCol2D.size.y / 2f);
				}
				if ((bool)hitdown && orangeCharacter._transform.position.y + vector3.y < door.position.y - BoxCol2D.size.y / 2f + BoxCol2D.offset.y && realCenterPos.x - zero.x < vector2.x && realCenterPos.x + zero.x > vector.x)
				{
					vector3 += Vector3.up * (num + BoxCol2D.size.y / 2f);
				}
				if ((bool)hitdown && vector3.y < 0f)
				{
					vector3.y = 0f;
				}
				else if ((bool)hitup && vector3.y > 0f)
				{
					vector3.y = 0f;
				}
			}
			realCenterPos = controller.GetRealCenterPos();
			orangeCharacter._transform.position += vector3;
			controller.LogicPosition = new VInt3(controller.LogicPosition.vec3 + vector3);
		}
	}

	private Vector3 CheckRoomSize(Vector3 RayOrigin)
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(RayOrigin, Vector3.left, 30f, layerMask);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(RayOrigin, Vector3.right, 30f, layerMask);
		RaycastHit2D raycastHit2D3 = Physics2D.Raycast(RayOrigin, Vector3.up, 30f, layerMask);
		RaycastHit2D raycastHit2D4 = Physics2D.Raycast(RayOrigin, Vector3.down, 30f, layerMask);
		bool flag = false;
		if (!raycastHit2D)
		{
			flag = true;
			Debug.LogError("沒有偵測到左邊界，之後一些技能無法準確判斷位置");
		}
		if (!raycastHit2D2)
		{
			flag = true;
			Debug.LogError("沒有偵測到右邊界，之後一些技能無法準確判斷位置");
		}
		if (!raycastHit2D4)
		{
			flag = true;
			Debug.LogError("沒有偵測到地板，之後一些技能無法準確判斷位置");
		}
		if (!raycastHit2D3)
		{
			flag = true;
			Debug.LogError("沒有偵測到天花板，之後一些技能無法準確判斷位置");
		}
		if (flag)
		{
			MaxPos = new Vector3(NowPos.x + 3f, NowPos.y + 6f, 0f);
			MinPos = new Vector3(NowPos.x - 6f, NowPos.y, 0f);
			CenterPos = (MaxPos + MinPos) / 2f;
		}
		MaxPos = new Vector3(raycastHit2D2.point.x, raycastHit2D3.point.y, 0f);
		MinPos = new Vector3(raycastHit2D.point.x, raycastHit2D4.point.y, 0f);
		CenterPos = (MaxPos + MinPos) / 2f;
		return CenterPos;
	}

	private void SwitchDoor(Transform door, bool onoff)
	{
		door.gameObject.SetActive(onoff);
	}
}
