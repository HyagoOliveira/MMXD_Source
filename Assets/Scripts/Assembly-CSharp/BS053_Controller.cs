#define RELEASE
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS053_Controller : EnemyControllerBase, IManagedUpdateBehavior, IF_Master
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Skill2 = 4,
		Skill3 = 5,
		Skill4 = 6,
		Die = 7
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		MAX_SUBSTATUS = 5
	}

	private enum Members
	{
		ChopRegister = 1,
		Shuriken = 0
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

	private int[] _animationHash;

	[SerializeField]
	private int[] DefaultSkillCard = new int[3] { 0, 1, 2 };

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	private float RoomXCenter;

	private float GroundY;

	private Vector3 LeftBorn;

	private Vector3 RightBorn;

	private float DebutWaitTime = 1f;

	private float DebutWaitFrame;

	[SerializeField]
	private int MemberNum = 2;

	private List<ValueTuple<int, BossCorpsTool.FightState>> FLFighting = new List<ValueTuple<int, BossCorpsTool.FightState>>();

	protected internal Dictionary<int, BossCorpsTool> MemberTeam = new Dictionary<int, BossCorpsTool>();

	[SerializeField]
	private float DebutDistance = 1.5f;

	private float DebutIntervalTime = 1f;

	private int DebutIntervalFrame;

	[SerializeField]
	private int[] ChopAI;

	[SerializeField]
	private int[] ShurikenAI;

	private List<ValueTuple<int, int>> AISet = new List<ValueTuple<int, int>>();

	[SerializeField]
	private Tuple<ValueTuple<int, int>>[] newt = new Tuple<ValueTuple<int, int>>[3];

	private EnemyControllerBase _lastMember;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private int ChopID
	{
		get
		{
			return 1;
		}
	}

	private int ShuriID
	{
		get
		{
			return 0;
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
			case "Skill4":
				NextSkill = MainStatus.Skill4;
				break;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		SetAIList();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
	}

	private void SetAIList()
	{
		int num = 0;
		num = ((ChopAI.Length <= ShurikenAI.Length) ? ChopAI.Length : ShurikenAI.Length);
		for (int i = 0; i < num; i++)
		{
			AISet.Add(new ValueTuple<int, int>(ChopAI[i], ShurikenAI[i]));
		}
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0 || MemberTeam.Count < 2)
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
			bool flag = netSyncData.sParam0 != string.Empty;
		}
		SetStatus((MainStatus)nSet);
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
			case SubStatus.Phase0:
				DebutWaitFrame = GameLogicUpdateManager.GameFrame + (int)(DebutWaitTime * 20f);
				break;
			case SubStatus.Phase1:
				DebutIntervalFrame = GameLogicUpdateManager.GameFrame + (int)(DebutIntervalTime * 20f);
				SpawnMembers(ChopID, _transform.position + Vector3.left * (DebutDistance / 2f), ref MemberTeam);
				break;
			case SubStatus.Phase2:
				SpawnMembers(ShuriID, _transform.position + Vector3.right * (DebutDistance / 2f), ref MemberTeam);
				break;
			}
			break;
		case MainStatus.Skill0:
			if (_subStatus == SubStatus.Phase0)
			{
				MemberTeam[ChopID].SendMission(AISet[0].Item1);
				MemberTeam[ShuriID].SendMission(AISet[0].Item2);
			}
			break;
		case MainStatus.Skill1:
			if (_subStatus == SubStatus.Phase0)
			{
				MemberTeam[ChopID].SendMission(AISet[1].Item1);
				MemberTeam[ShuriID].SendMission(AISet[1].Item2);
			}
			break;
		case MainStatus.Skill2:
			if (_subStatus == SubStatus.Phase0)
			{
				MemberTeam[ChopID].SendMission(AISet[2].Item1);
				MemberTeam[ShuriID].SendMission(AISet[2].Item2);
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				break;
			}
			break;
		}
		AiTimer.TimerStart();
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.Idle:
		{
			int num = RandomCard(2);
			if (num > -1)
			{
				mainStatus = (MainStatus)num;
			}
			break;
		}
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
		}
	}

	public override void LogicUpdate()
	{
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
				if ((float)GameLogicUpdateManager.GameFrame > DebutWaitFrame)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > DebutIntervalFrame)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				SetStatus(MainStatus.Debut, SubStatus.Phase3);
				break;
			case SubStatus.Phase3:
				if (CheckDebutOver())
				{
					if (IntroCallBack != null)
					{
						IntroCallBack();
					}
					if (_introReady)
					{
						IntroCallBack();
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus)
			{
				UpdateRandomState();
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (CheckMissionOver(ChopID))
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (CheckMissionOverAll())
				{
					SetStatus(MainStatus.Idle);
				}
				else
				{
					MemberTeam[ShuriID].StopMission();
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (CheckMissionOver(ChopID))
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (CheckMissionOverAll())
				{
					SetStatus(MainStatus.Idle);
				}
				else
				{
					MemberTeam[ShuriID].StopMission();
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (CheckMissionOver(ChopID))
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (CheckMissionOverAll())
				{
					SetStatus(MainStatus.Idle);
				}
				else
				{
					MemberTeam[ShuriID].StopMission();
				}
				break;
			}
			break;
		case MainStatus.Die:
		{
			SubStatus subStatus = _subStatus;
			break;
		}
		case MainStatus.Skill3:
		case MainStatus.Skill4:
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
			IgnoreGravity = true;
			CheckRoomSize();
			MemberTeam.Clear();
			SetStatus(MainStatus.Debut);
			return;
		}
		if (_lastMember != null)
		{
			_lastMember.BackToPool();
		}
		foreach (KeyValuePair<int, BossCorpsTool> item in MemberTeam)
		{
			if (item.Value.Member.InGame)
			{
				item.Value.Member.BackToPool();
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
		base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * -1f);
		base.transform.position = pos;
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

	private int RandomCard(int StartPos)
	{
		if (SkillCard.ToArray().Length < 1)
		{
			SkillCard = new List<int>(DefaultSkillCard);
		}
		int result = -1;
		if (SkillCard.Count > 0)
		{
			result = SkillCard[OrangeBattleUtility.Random(0, SkillCard.ToArray().Length)];
			SkillCard.Remove(result);
			return result + StartPos;
		}
		return result;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	protected internal void SetMemberHP(object[] objs)
	{
		for (int i = 0; i < objs.Length; i++)
		{
			if (objs[i] == null)
			{
				Debug.LogError("成員回傳Hp 的資料有誤，搜尋 SetMemberHp");
				return;
			}
		}
		int num = 0;
		int num2 = 0;
		num = int.Parse(objs[0].ToString());
		num2 = int.Parse(objs[1].ToString());
		int mobHp = MemberTeam[num].MobHp;
		HurtPassParam hurtPassParam = new HurtPassParam();
		if (num2 <= 0)
		{
			num2 = 0;
			int num3 = 0;
			for (int j = 0; j < FLFighting.Count; j++)
			{
				if (FLFighting[j].Item1 == num)
				{
					FLFighting[j] = new ValueTuple<int, BossCorpsTool.FightState>(num, BossCorpsTool.FightState.Dead);
					continue;
				}
				num3 += MemberTeam[FLFighting[j].Item1].MobHp;
				MemberTeam[FLFighting[j].Item1].StopMission();
				MemberTeam[FLFighting[j].Item1].Automatic();
			}
			hurtPassParam.dmg = (int)Hp - num3;
		}
		else
		{
			hurtPassParam.dmg = mobHp - num2;
		}
		MemberTeam[num].MobHp = num2;
		Hurt(hurtPassParam);
		if ((int)Hp > 0)
		{
			return;
		}
		foreach (KeyValuePair<int, BossCorpsTool> item in MemberTeam)
		{
			item.Value.isBossDead = true;
		}
	}

	private void SpawnMembers(int WeaponID, Vector3 BornPos, ref Dictionary<int, BossCorpsTool> MemberDictionary)
	{
		if (MemberDictionary.ContainsKey(WeaponID) && MemberDictionary[WeaponID].Member != null)
		{
			return;
		}
		bool flag = true;
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[WeaponID].BulletData.f_EFFECT_X], sNetSerialID + WeaponID, 16);
		if (!enemyControllerBase)
		{
			return;
		}
		if (!MemberDictionary.ContainsKey(WeaponID))
		{
			BossCorpsTool value = new BossCorpsTool(enemyControllerBase, enemyControllerBase.Hp);
			MemberDictionary.Add(WeaponID, value);
			flag = false;
		}
		BossCorpsTool bossCorpsTool = MemberDictionary[WeaponID];
		bossCorpsTool.Master = this;
		EnemyControllerBase enemyControllerBase2 = enemyControllerBase;
		if ((object)enemyControllerBase2 != null)
		{
			BS087_Controller bS087_Controller;
			if ((object)(bS087_Controller = enemyControllerBase2 as BS087_Controller) == null)
			{
				BS088_Controller bS088_Controller;
				if ((object)(bS088_Controller = enemyControllerBase2 as BS088_Controller) != null)
				{
					BS088_Controller bS088_Controller2 = bS088_Controller;
					bossCorpsTool.SetIDAndCB(WeaponID, SetMemberHP, RemoveMember);
					bossCorpsTool.fightState = BossCorpsTool.FightState.Fighting;
					bS088_Controller2.SetParam(bossCorpsTool, MemberDictionary[WeaponID].MobHp);
					bS088_Controller2.SetPositionAndRotation(BornPos, base.direction == -1);
					bS088_Controller2.SetActive(true);
					if (!flag)
					{
						bS088_Controller2.SetDebut();
					}
				}
			}
			else
			{
				BS087_Controller bS087_Controller2 = bS087_Controller;
				bossCorpsTool.SetIDAndCB(WeaponID, SetMemberHP, RemoveMember);
				bossCorpsTool.fightState = BossCorpsTool.FightState.Fighting;
				bS087_Controller2.SetParam(bossCorpsTool, MemberDictionary[WeaponID].MobHp);
				bS087_Controller2.SetPositionAndRotation(BornPos, base.direction == -1);
				bS087_Controller2.SetActive(true);
				if (!flag)
				{
					bS087_Controller2.SetDebut();
				}
			}
		}
		FLFighting.Add(new ValueTuple<int, BossCorpsTool.FightState>(WeaponID, bossCorpsTool.fightState));
	}

	public bool RegistMemberBornBySync<T>(T member, int nowhp) where T : EnemyControllerBase
	{
		if ((object)member != null)
		{
			BS087_Controller bS087_Controller;
			if ((object)(bS087_Controller = member as BS087_Controller) == null)
			{
				BS088_Controller bS088_Controller;
				if ((object)(bS088_Controller = member as BS088_Controller) != null)
				{
					BS088_Controller bS088_Controller2 = bS088_Controller;
					if (!MemberTeam.ContainsKey(ShuriID))
					{
						BossCorpsTool bossCorpsTool = new BossCorpsTool(bS088_Controller2, nowhp);
						bossCorpsTool.Master = this;
						MemberTeam.Add(ShuriID, bossCorpsTool);
						MemberTeam[ShuriID].SetIDAndCB(ShuriID, SetMemberHP, RemoveMember);
						MemberTeam[ShuriID].fightState = BossCorpsTool.FightState.Fighting;
						bS088_Controller2.SetParam(MemberTeam[ShuriID], nowhp);
						FLFighting.Add(new ValueTuple<int, BossCorpsTool.FightState>(ShuriID, MemberTeam[ShuriID].fightState));
						return true;
					}
					MemberTeam[ShuriID].SetIDAndCB(ShuriID, SetMemberHP, RemoveMember);
					MemberTeam[ShuriID].fightState = BossCorpsTool.FightState.Fighting;
					bS088_Controller2.SetParam(MemberTeam[ShuriID], nowhp);
					Debug.LogWarning("已經註冊過手裏劍，不再重新註冊");
				}
			}
			else
			{
				BS087_Controller bS087_Controller2 = bS087_Controller;
				if (!MemberTeam.ContainsKey(ChopID))
				{
					BossCorpsTool bossCorpsTool2 = new BossCorpsTool(bS087_Controller2, nowhp);
					bossCorpsTool2.Master = this;
					MemberTeam.Add(ChopID, bossCorpsTool2);
					MemberTeam[ChopID].SetIDAndCB(ChopID, SetMemberHP, RemoveMember);
					MemberTeam[ChopID].fightState = BossCorpsTool.FightState.Fighting;
					bS087_Controller2.SetParam(MemberTeam[ChopID], nowhp);
					FLFighting.Add(new ValueTuple<int, BossCorpsTool.FightState>(ChopID, MemberTeam[ChopID].fightState));
					return true;
				}
				MemberTeam[ChopID].SetIDAndCB(ChopID, SetMemberHP, RemoveMember);
				MemberTeam[ChopID].fightState = BossCorpsTool.FightState.Fighting;
				bS087_Controller2.SetParam(MemberTeam[ChopID], nowhp);
				Debug.LogWarning("已經註冊過斬擊者，不再重新註冊");
			}
		}
		return false;
	}

	public void RemoveMember(object obj)
	{
		if (obj == null)
		{
			Debug.LogError("成員回傳ID 的資料有誤，搜尋 RemoveMember");
			return;
		}
		int num = 0;
		num = int.Parse(obj.ToString());
		for (int i = 0; i < FLFighting.Count; i++)
		{
			if (FLFighting[i].Item1 != num)
			{
				continue;
			}
			FLFighting[i] = new ValueTuple<int, BossCorpsTool.FightState>(FLFighting[i].Item1, MemberTeam[FLFighting[i].Item1].fightState);
			if (FLFighting[i].Item2 == BossCorpsTool.FightState.Dead)
			{
				if (!CheckAllDie())
				{
					MemberTeam[FLFighting[i].Item1].ComeBack(true);
				}
				else
				{
					SetDeadCompelete(MemberTeam[FLFighting[i].Item1].Member);
				}
			}
		}
	}

	private bool CheckDebutOver()
	{
		foreach (KeyValuePair<int, BossCorpsTool> item in MemberTeam)
		{
			if (!item.Value.hasDebut)
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckMissionOverAll()
	{
		foreach (KeyValuePair<int, BossCorpsTool> item in MemberTeam)
		{
			if (!item.Value.CheckMissionProgress() && item.Value.isObedient && (int)item.Value.Member.Hp > 0)
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckMissionOver(int MobID)
	{
		if ((int)MemberTeam[MobID].Member.Hp <= 0 && MemberTeam[MobID].isObedient)
		{
			return true;
		}
		return MemberTeam[MobID].WaitMission;
	}

	private bool CheckAllDie()
	{
		foreach (var item in FLFighting)
		{
			if (item.Item2 != BossCorpsTool.FightState.Dead)
			{
				return false;
			}
		}
		return true;
	}

	protected override void UploadEnemyStatus(int status = -1, bool SetHp = false, object[] Param = null, object[] SParam = null)
	{
		if (status != -1)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			if (SParam != null)
			{
				netSyncData.sParam0 = SParam[0].ToString();
			}
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, status, JsonConvert.SerializeObject(netSyncData));
		}
		else
		{
			Debug.LogError("MainStatus值為空");
		}
	}

	private void CheckRoomSize()
	{
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.down, float.PositiveInfinity, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		if ((bool)raycastHit2D)
		{
			GroundY = raycastHit2D.point.y + 0.1f;
		}
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.left, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		if ((bool)raycastHit2D2 && (bool)raycastHit2D3)
		{
			float num = (raycastHit2D3.distance + raycastHit2D2.distance) / 2f;
			Vector3 vector = (raycastHit2D3.point + raycastHit2D2.point) / 2f;
			RoomXCenter = vector.x;
			vector.y = GroundY;
			LeftBorn = new Vector3(0f - (num - 2f), 0f, 0f) + vector;
			RightBorn = new Vector3(num - 2f, 0f, 0f) + vector;
		}
	}

	private void SetDeadCompelete(EnemyControllerBase lastMember)
	{
		_lastMember = lastMember;
		switch (AiState)
		{
		case AI_STATE.mob_002:
			base.DeadPlayCompleted = false;
			StartCoroutine(BossDieFlow(_lastMember.GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
			break;
		case AI_STATE.mob_003:
			base.DeadPlayCompleted = true;
			StartCoroutine(BossDieFlow(_lastMember.GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
			break;
		default:
			base.DeadPlayCompleted = false;
			StartCoroutine(BossDieFlow(_lastMember.GetTargetPoint()));
			break;
		}
	}

	private void CallEventEnemyPoint(int nID)
	{
		EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
		stageEventCall.nID = nID;
		stageEventCall.tTransform = OrangeBattleUtility.CurrentCharacter.transform;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
	}

	public void ReportObjects(object[] value)
	{
	}

	public object[] GetValues(object[] param = null)
	{
		return new object[1] { _transform.position };
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
	}
}
