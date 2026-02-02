#define RELEASE
using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM212_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Open = 1,
		Close = 2,
		Move = 3,
		Skill = 4,
		Hurt = 5
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

	private Vector3 MaxPos;

	private Vector3 MinPos;

	[SerializeField]
	private int MoveSpd = 6000;

	[SerializeField]
	private int SkillActTimes = 10;

	[SerializeField]
	private float SkillActFrame = 2f;

	private int ActionTimes;

	private int ActionFrame;

	private bool GoReturn;

	private Vector3 MovePos;

	private Vector3 BornPos;

	[SerializeField]
	private float OpenCloseTime = 0.5f;

	[SerializeField]
	private Transform _upperLid;

	[SerializeField]
	private Transform _lowerLid;

	[SerializeField]
	private float OpenValue = 35f;

	[SerializeField]
	private float UpperCloseValue = -60f;

	[SerializeField]
	private float LowerCloseValue = -120f;

	public BossCorpsTool CorpsTool;

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
			_velocity = VInt3.zero;
			break;
		case MainStatus.Open:
			_velocity = VInt3.zero;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(_characterMaterial.GetDissloveTime() * 20f);
				break;
			case SubStatus.Phase1:
				base.SoundSource.PlaySE("BossSE", 123);
				LeanTween.value(base.gameObject, UpperCloseValue, UpperCloseValue - OpenValue, OpenCloseTime).setOnUpdate(delegate(float val)
				{
					_upperLid.localEulerAngles = Vector3.right * val;
				});
				LeanTween.value(base.gameObject, LowerCloseValue, LowerCloseValue + OpenValue, OpenCloseTime).setOnUpdate(delegate(float val)
				{
					_lowerLid.localEulerAngles = Vector3.right * val;
				}).setOnComplete((Action)delegate
				{
					SetStatus(MainStatus.Move);
				});
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)((OpenCloseTime + 0.1f) * 20f);
				break;
			}
			break;
		case MainStatus.Close:
			_velocity = VInt3.zero;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				LeanTween.value(base.gameObject, UpperCloseValue - OpenValue, UpperCloseValue, OpenCloseTime).setOnUpdate(delegate(float val)
				{
					_upperLid.localEulerAngles = Vector3.right * val;
				});
				LeanTween.value(base.gameObject, LowerCloseValue + OpenValue, LowerCloseValue, OpenCloseTime).setOnUpdate(delegate(float val)
				{
					_lowerLid.localEulerAngles = Vector3.right * val;
				}).setOnComplete((Action)delegate
				{
					SetStatus(MainStatus.Close, SubStatus.Phase1);
				});
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)((OpenCloseTime + 0.1f) * 20f);
				break;
			case SubStatus.Phase1:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(_characterMaterial.GetDissloveTime() * 20f);
				break;
			}
			break;
		case MainStatus.Move:
			if (_subStatus == SubStatus.Phase0)
			{
				StartPos = NowPos;
				EndPos = (GoReturn ? BornPos : MovePos);
				MoveDis = Vector2.Distance(EndPos, StartPos);
				_velocity = new VInt3((EndPos - StartPos).normalized) * MoveSpd * 0.001f;
				_velocity.z = 0;
			}
			break;
		case MainStatus.Skill:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				ActionTimes = SkillActTimes;
				SetStatus(MainStatus.Skill, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(SkillActFrame * 20f);
				break;
			}
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
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!bWaitNetStatus)
			{
				bool flag = (bool)Target;
			}
			break;
		case MainStatus.Open:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Open, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Move);
				}
				break;
			}
			break;
		case MainStatus.Close:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Close, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					BackToPool();
				}
				break;
			}
			break;
		case MainStatus.Move:
			if (_subStatus == SubStatus.Phase0 && Vector2.Distance(NowPos, StartPos) >= MoveDis)
			{
				_transform.position = EndPos;
				Controller.LogicPosition = new VInt3(EndPos);
				SetStatus(GoReturn ? MainStatus.Close : MainStatus.Skill);
			}
			break;
		case MainStatus.Skill:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(MainStatus.Skill, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
			{
				if (GameLogicUpdateManager.GameFrame < ActionFrame)
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
				Vector3 pDirection = Vector3.down;
				if ((bool)Target)
				{
					pDirection = Target.GetTargetPoint() - NowPos;
				}
				BasicBullet basicBullet = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, NowPos, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BasicBullet;
				if ((bool)basicBullet)
				{
					basicBullet.BackCallback = SkillCallBack;
				}
				if (--ActionTimes < 0)
				{
					GoReturn = true;
					SetStatus(MainStatus.Move);
				}
				else
				{
					SetStatus(MainStatus.Skill, SubStatus.Phase1);
				}
				break;
			}
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
			_upperLid.localEulerAngles = Vector3.right * UpperCloseValue;
			_lowerLid.localEulerAngles = Vector3.right * LowerCloseValue;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			CheckRoomSize();
			GoReturn = false;
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
				CorpsTool.fightState = BossCorpsTool.FightState.Fighting;
			}
			SetStatus(MainStatus.Idle);
		}
		else
		{
			_velocity = VInt3.zero;
			_collideBullet.BackToPool();
			if (CorpsTool != null)
			{
				CorpsTool.fightState = BossCorpsTool.FightState.Dead;
			}
		}
	}

	public override void OnToggleCharacterMaterial(bool appear)
	{
		SetStatus(MainStatus.Open);
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

	public void SetMovePos(Vector3 pos)
	{
		MovePos = pos;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		int num = Hp;
		ObscuredInt obscuredInt = base.Hurt(tHurtPassParam);
		if (CorpsTool.Master != null)
		{
			CorpsTool.Master.ReportObjects(new object[1] { num - (int)obscuredInt });
		}
		return obscuredInt;
	}

	private void SkillCallBack(object obj)
	{
		BasicBullet basicBullet = null;
		if (obj != null)
		{
			basicBullet = obj as BasicBullet;
		}
		if (basicBullet == null)
		{
			Debug.LogError("子彈資料有誤。");
		}
		else if (basicBullet.isHitBlock)
		{
			Vector3 position = basicBullet._transform.position;
			float num = 5f;
			int num2 = 0;
			if (Mathf.Abs(MaxPos.y - position.y) < num)
			{
				num = Mathf.Abs(MaxPos.y - position.y);
				num2 = 1;
			}
			if (Mathf.Abs(MinPos.y - position.y) < num)
			{
				num = Mathf.Abs(MinPos.y - position.y);
				num2 = 2;
			}
			if (Mathf.Abs(MinPos.x - position.x) < num)
			{
				num = Mathf.Abs(MinPos.x - position.x);
				num2 = 3;
			}
			if (Mathf.Abs(MaxPos.x - position.x) < num)
			{
				num = Mathf.Abs(MaxPos.x - position.x);
				num2 = 4;
			}
			switch (num2)
			{
			case 1:
			case 2:
				BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, position, Vector3.left, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, position, Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			case 3:
			case 4:
				BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, position, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, position, Vector3.down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			}
			PlayBossSE("BossSE", "bs011_panda09");
		}
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y + 1f, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.up, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D4 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.down, 30f, layerMask, _transform);
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
		}
		else
		{
			MaxPos = new Vector3(raycastHit2D2.point.x, raycastHit2D3.point.y, 0f);
			MinPos = new Vector3(raycastHit2D.point.x, raycastHit2D4.point.y, 0f);
		}
	}
}
