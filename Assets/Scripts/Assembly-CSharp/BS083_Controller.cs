#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS083_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Die = 4
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		MAX_SUBSTATUS = 4
	}

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus _mainStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SubStatus _subStatus;

	private int nDeadCount;

	private int[] DefaultSkillCard = new int[6] { 0, 1, 2, 3, 4, 5 };

	private static int[] DefaultRangedSkillCard = new int[3] { 3, 4, 5 };

	private List<int> RangedSKC = new List<int>();

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[Header("通用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	[SerializeField]
	private Transform ShootPos;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private float GroundYPos;

	private int ActionFrame;

	private bool HasActed;

	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("護盾相關")]
	[SerializeField]
	private float ShellHpPercent = 30f;

	[SerializeField]
	private float ShellRecoverInterval = 10f;

	[SerializeField]
	private SkinnedMeshRenderer[] ShellMesh = new SkinnedMeshRenderer[3];

	[SerializeField]
	private int ShellBulletNum = 3;

	[SerializeField]
	private ParticleSystem CoreFx;

	[SerializeField]
	private ParticleSystem Skill0UseFx;

	[SerializeField]
	private float Skill0Dis = 12f;

	[SerializeField]
	private float Skill0WaitTime = 1.5f;

	private int ShellHp = 10000;

	private bool HasShellBroken;

	[Header("召喚小怪")]
	[SerializeField]
	private EM199_Controller[] EM199s = new EM199_Controller[0];

	[SerializeField]
	private Vector3 EM199SpawnOffset = new Vector3(0.1f, 0.3f, 0f);

	[SerializeField]
	private float HpStep = 30f;

	private bool[] hasSpawn = new bool[0];

	private int SpawnCount;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	[SerializeField]
	private CharacterMaterial customMaterial;

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

	protected void ChangeDebugMode()
	{
		DebugMode = !DebugMode;
	}

	protected void ChangeSetSkill(object[] param)
	{
		string text = param[0] as string;
		if (text == string.Empty)
		{
			return;
		}
		switch (text)
		{
		case "Idle":
			NextSkill = MainStatus.Idle;
			break;
		case "Skill0":
			NextSkill = MainStatus.Skill0;
			break;
		case "Skill1":
			hasSpawn[0] = false;
			if (EM199s[0] != null)
			{
				EM199s[0].BackToPool();
				EM199s[0] = null;
			}
			break;
		case "Skill2":
			hasSpawn[1] = false;
			if (EM199s[1] != null)
			{
				EM199s[1].BackToPool();
				EM199s[1] = null;
			}
			break;
		case "Skill3":
			hasSpawn[2] = false;
			if (EM199s[2] != null)
			{
				EM199s[2].BackToPool();
				EM199s[2] = null;
			}
			break;
		case "Skill4":
			hasSpawn[3] = false;
			if (EM199s[3] != null)
			{
				EM199s[3].BackToPool();
				EM199s[3] = null;
			}
			break;
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
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (CoreFx == null)
		{
			CoreFx = OrangeBattleUtility.FindChildRecursive(ref childs, "CoreFx", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill0UseFx == null)
		{
			Skill0UseFx = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill0UseFx", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		base.AimPoint = new Vector3(0f, 0.2f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
		base.Start();
		if ((bool)customMaterial)
		{
			customMaterial.RimColor = new Color(1f, 1f, 1f, 0.7058824f);
			customMaterial.UpdateProperty();
		}
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
			if (netSyncData.nParam0 >= 0)
			{
				float num = ShellMesh.Length;
				ShellHp = netSyncData.nParam0;
				for (int i = 0; (float)i < num; i++)
				{
					if ((float)ShellHp <= (float)(int)MaxHp * (ShellHpPercent / 100f) * (num - (float)(i + 1)) / num)
					{
						ShellMesh[i].enabled = false;
					}
				}
				if (ShellHp > 0)
				{
					HasShellBroken = false;
				}
				else if (!HasShellBroken)
				{
					HasShellBroken = true;
					ActionFrame = GameLogicUpdateManager.GameFrame + (int)(ShellRecoverInterval * 20f);
				}
			}
		}
		if (nSet != 0)
		{
			SetStatus((MainStatus)nSet);
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
			if (_subStatus == SubStatus.Phase0)
			{
			}
			break;
		case MainStatus.Idle:
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			if (_subStatus == SubStatus.Phase0)
			{
				for (float num = 0f; num < (float)ShellBulletNum; num += 1f)
				{
					Vector3 vector = new Vector3(1f * (float)(-base.direction), 1f * ((0f - num) / (float)ShellBulletNum), 0f).normalized * Skill0Dis;
					(BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos.position - vector, vector, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BasicBullet).FreeDISTANCE = Skill0Dis;
				}
				PlayBossSE("BossSE05", "bs113_magna04");
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill0WaitTime * 20f);
			}
			break;
		case MainStatus.Skill1:
			if (_subStatus == SubStatus.Phase0)
			{
				PlayBossSE("BossSE05", "bs113_magna01");
				hasSpawn[0] = true;
				EM199s[0] = SpawnEnemy(NowPos + EM199SpawnOffset);
				EM199s[0].SetEndDegree(160f);
				EM199s[0].SetSkill0AtkPos(8);
				hasSpawn[3] = true;
				EM199s[3] = SpawnEnemy(NowPos + EM199SpawnOffset);
				EM199s[3].SetEndDegree(120f);
				EM199s[3].SetSkill0AtkPos(7);
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				base.DeadPlayCompleted = true;
				nDeadCount = 0;
				break;
			case SubStatus.Phase1:
				StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
				break;
			}
			break;
		}
		AiTimer.TimerStart();
	}

	private void UpdateRandomState(MainStatus status = MainStatus.Idle)
	{
		MainStatus mainStatus = status;
		if (status == MainStatus.Idle)
		{
			switch (_mainStatus)
			{
			case MainStatus.Debut:
				SetStatus(MainStatus.Idle);
				return;
			case MainStatus.Idle:
				mainStatus = (MainStatus)RandomCard(3);
				break;
			}
		}
		if (DebugMode)
		{
			mainStatus = NextSkill;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus, false, new object[1] { ShellHp });
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
				if (_introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (IntroCallBack != null)
				{
					IntroCallBack();
					if (!bWaitNetStatus)
					{
						UpdateRandomState();
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
			if (!bWaitNetStatus && IdleWaitFrame < GameLogicUpdateManager.GameFrame)
			{
				if (!hasSpawn[0] && CheckHost())
				{
					UploadEnemyStatus(3, false, new object[1] { ShellHp });
				}
				if (HasShellBroken && GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					UploadEnemyStatus(2, false, new object[1] { ShellHp });
				}
			}
			break;
		case MainStatus.Skill0:
			if (_subStatus == SubStatus.Phase0 && GameLogicUpdateManager.GameFrame > ActionFrame)
			{
				ShellHp = (int)((float)(int)MaxHp * (ShellHpPercent / 100f));
				HasShellBroken = false;
				SkinnedMeshRenderer[] shellMesh = ShellMesh;
				for (int i = 0; i < shellMesh.Length; i++)
				{
					shellMesh[i].enabled = true;
				}
				if ((bool)Skill0UseFx)
				{
					PlayBossSE("BossSE05", "bs113_magna05");
					Skill0UseFx.Play();
				}
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Skill1:
			if (_subStatus == SubStatus.Phase0)
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (nDeadCount > 5)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				else
				{
					nDeadCount++;
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			}
			break;
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
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			if ((bool)CoreFx)
			{
				CoreFx.Play();
			}
			EM199s = new EM199_Controller[4];
			hasSpawn = new bool[4];
			HasShellBroken = false;
			ShellHp = (int)((float)(int)MaxHp * (ShellHpPercent / 100f));
			ObjInfoBar componentInChildren = _transform.GetComponentInChildren<ObjInfoBar>();
			if ((bool)componentInChildren)
			{
				componentInChildren.ForceSetNewPosition(Vector3.up * 3f);
			}
			SetStatus(MainStatus.Debut);
			return;
		}
		EM199_Controller[] eM199s = EM199s;
		foreach (EM199_Controller eM199_Controller in eM199s)
		{
			if ((bool)eM199_Controller)
			{
				eM199_Controller.BackToPool();
			}
		}
		_collideBullet.BackToPool();
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
	}

	private int RandomCard(int StartPos)
	{
		if (SkillCard.ToArray().Length < 1)
		{
			SkillCard = new List<int>(DefaultSkillCard);
		}
		int num = SkillCard[OrangeBattleUtility.Random(0, SkillCard.ToArray().Length)];
		SkillCard.Remove(num);
		return num + StartPos;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus == MainStatus.Die)
		{
			return;
		}
		if ((bool)CoreFx)
		{
			CoreFx.Stop();
		}
		if ((bool)Skill0UseFx)
		{
			Skill0UseFx.Stop();
		}
		EM199_Controller[] eM199s = EM199s;
		foreach (EM199_Controller eM199_Controller in eM199s)
		{
			if ((bool)eM199_Controller)
			{
				eM199_Controller.StopAtk(true);
			}
		}
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		HasShellBroken = true;
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

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (!HasShellBroken)
		{
			if (ShellHp > (int)tHurtPassParam.dmg)
			{
				ShellHp -= tHurtPassParam.dmg;
				tHurtPassParam.dmg = 0;
				UploadEnemyStatus(0, false, new object[1] { ShellHp });
			}
			else if (ShellHp > 0)
			{
				HasShellBroken = true;
				ShellHp = 0;
				tHurtPassParam.dmg = 0;
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(ShellRecoverInterval * 20f);
				UploadEnemyStatus(0, false, new object[1] { ShellHp });
			}
		}
		ObscuredInt obscuredInt = base.Hurt(tHurtPassParam);
		if (!hasSpawn[1] && (int)obscuredInt <= (int)((float)(int)MaxHp * (100f - HpStep * 1f) / 100f))
		{
			hasSpawn[1] = true;
			EM199s[1] = SpawnEnemy(NowPos + EM199SpawnOffset);
			EM199s[1].SetEndDegree(140f);
			EM199s[1].SetSkill0AtkPos(6);
		}
		if (!hasSpawn[2] && (int)obscuredInt <= (int)((float)(int)MaxHp * (100f - HpStep * 2f) / 100f))
		{
			hasSpawn[2] = true;
			EM199s[2] = SpawnEnemy(NowPos + EM199SpawnOffset);
			EM199s[2].SetEndDegree(100f);
			EM199s[2].SetSkill0AtkPos(5);
		}
		return obscuredInt;
	}

	private EM199_Controller SpawnEnemy(Vector3 SpawnPos)
	{
		int num = 2;
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
			enemyControllerBase.SetPositionAndRotation(SpawnPos, base.direction == 1);
			enemyControllerBase.SetActive(true);
		}
		return enemyControllerBase as EM199_Controller;
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
}
