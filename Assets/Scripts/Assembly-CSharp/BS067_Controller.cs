#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS067_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	protected internal struct TeamMember
	{
		public int KobunID;

		public GameObject Kobun;

		public TeamMember(int ID, GameObject Member)
		{
			KobunID = ID;
			Kobun = Member;
		}
	}

	protected internal struct KobunData
	{
		public enum KobunTypeList
		{
			Hummer = 0,
			Rocket = 1,
			Kamikaze = 2,
			IronGun = 3,
			Catch = 4,
			None = 5
		}

		public int KobunHp;

		public KobunTypeList KobunType;

		public bool debutover;

		public void SetHp(int NewHp)
		{
			KobunHp = NewHp;
		}

		public void SetType(int NewType)
		{
			KobunType = (KobunTypeList)NewType;
		}

		public KobunData(int NewType, int NewHp, bool hasdebut = false)
		{
			KobunHp = NewHp;
			KobunType = (KobunTypeList)NewType;
			debutover = hasdebut;
		}
	}

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

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private float _currentFrame;

	[SerializeField]
	private int[] DefaultSkillCard = new int[5] { 0, 1, 2, 3, 4 };

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	private float RoomXCenter;

	private float GroundY;

	private Vector3 LeftBorn;

	private Vector3 RightBorn;

	private int KobunNum = 5;

	private List<TeamMember> KobunTeam = new List<TeamMember>();

	protected internal Dictionary<int, KobunData> KobunDic = new Dictionary<int, KobunData>();

	private int SpawnCDTime = 2;

	private List<int> NextSpawnFrame = new List<int>();

	private EnemyControllerBase _lastKobun;

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
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		FallDownSE = new string[2] { "BossSE03", "bs022_kobun19" };
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
			{
				Vector2 vector = _transform.position;
				SpawnKobuns(0, vector, ref KobunDic);
				SpawnKobuns(1, Vector2.left * 1.5f + vector, ref KobunDic);
				SpawnKobuns(2, Vector2.right * 0.5f + vector, ref KobunDic);
				SpawnKobuns(3, Vector2.right * 1.5f + vector, ref KobunDic);
				SpawnKobuns(4, Vector2.left * 0.5f + vector, ref KobunDic);
				break;
			}
			case SubStatus.Phase2:
				if (CheckDebutOver() && IntroCallBack != null)
				{
					IntroCallBack();
				}
				break;
			}
			break;
		case MainStatus.Skill0:
			if (_subStatus == SubStatus.Phase0)
			{
				Vector3 bornPos4 = RightBorn;
				if ((bool)Target && Target._transform.position.x > RoomXCenter)
				{
					bornPos4 = LeftBorn;
				}
				SpawnKobuns(0, bornPos4, ref KobunDic);
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Skill1:
			if (_subStatus == SubStatus.Phase0)
			{
				Vector3 bornPos5 = RightBorn + Vector3.right * 0.5f;
				if ((bool)Target && Target._transform.position.x > RoomXCenter)
				{
					bornPos5 = LeftBorn - Vector3.right * 0.5f;
				}
				SpawnKobuns(1, bornPos5, ref KobunDic);
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Skill2:
			if (_subStatus == SubStatus.Phase0)
			{
				Vector3 bornPos3 = RightBorn;
				if ((bool)Target && Target._transform.position.x > RoomXCenter)
				{
					bornPos3 = LeftBorn;
				}
				SpawnKobuns(2, bornPos3, ref KobunDic);
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Skill3:
			if (_subStatus == SubStatus.Phase0)
			{
				Vector3 bornPos = RightBorn + Vector3.right;
				if ((bool)Target && Target._transform.position.x > RoomXCenter)
				{
					bornPos = LeftBorn - Vector3.right;
				}
				SpawnKobuns(3, bornPos, ref KobunDic);
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Skill4:
			if (_subStatus == SubStatus.Phase0)
			{
				Vector3 bornPos2 = RightBorn;
				if ((bool)Target && Target._transform.position.x > RoomXCenter)
				{
					bornPos2 = LeftBorn;
				}
				SpawnKobuns(4, bornPos2, ref KobunDic);
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Die:
			if (_subStatus != 0)
			{
				int num = 1;
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
				SetStatus(MainStatus.Debut, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				SetStatus(MainStatus.Debut, SubStatus.Phase2);
				break;
			case SubStatus.Phase2:
				if (_introReady)
				{
					IntroCallBack();
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Idle:
		{
			if (bWaitNetStatus || NextSpawnFrame.Count <= 0 || KobunTeam.Count >= 2)
			{
				break;
			}
			for (int i = 0; i < NextSpawnFrame.Count; i++)
			{
				if (GameLogicUpdateManager.GameFrame > NextSpawnFrame[i])
				{
					NextSpawnFrame.Remove(NextSpawnFrame[i]);
					UpdateRandomState();
				}
			}
			break;
		}
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
		if (isActive)
		{
			IgnoreGravity = true;
			CheckRoomSize();
			KobunDic.Clear();
			SetStatus(MainStatus.Debut);
		}
		else if (_lastKobun != null)
		{
			_lastKobun.BackToPool();
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
		List<int> skillCard = SkillCard;
		for (int i = 0; i < KobunTeam.Count; i++)
		{
			skillCard.Remove(KobunTeam[i].KobunID);
		}
		int result = -1;
		if (skillCard.Count > 0)
		{
			result = skillCard[OrangeBattleUtility.Random(0, skillCard.ToArray().Length)];
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

	protected internal void SetKobunHP(int KobunID, int ReturnHp)
	{
		int kobunHp = KobunDic[KobunID].KobunHp;
		HurtPassParam hurtPassParam = new HurtPassParam();
		if (ReturnHp <= 0)
		{
			RemoveKobun(KobunID);
			KobunNum--;
			SkillCard.Remove(KobunID);
			List<int> list = new List<int>(DefaultSkillCard);
			list.Remove(KobunID);
			DefaultSkillCard = list.ToArray();
			if (KobunNum <= 2)
			{
				StopKobunBack();
			}
			hurtPassParam.dmg = kobunHp;
		}
		else
		{
			hurtPassParam.dmg = kobunHp - ReturnHp;
		}
		KobunData value = KobunDic[KobunID];
		value.SetHp(ReturnHp);
		KobunDic[KobunID] = value;
		Hurt(hurtPassParam);
		if ((int)Hp <= 0)
		{
			CallEventEnemyPoint(999);
		}
	}

	private void SpawnKobuns(int WeaponID, Vector3 BornPos, ref Dictionary<int, KobunData> KobunDictionary)
	{
		bool flag = true;
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[WeaponID].BulletData.f_EFFECT_X], sNetSerialID + WeaponID, 16);
		if (!enemyControllerBase)
		{
			return;
		}
		if (KobunDictionary.ContainsKey(WeaponID))
		{
			KobunData value = new KobunData(WeaponID, KobunDictionary[WeaponID].KobunHp);
			KobunDictionary[WeaponID] = value;
		}
		else
		{
			KobunData value2 = new KobunData(WeaponID, enemyControllerBase.Hp);
			KobunDictionary.Add(WeaponID, value2);
			flag = false;
		}
		bool need = true;
		if (KobunNum <= 2)
		{
			need = false;
		}
		switch (WeaponID)
		{
		case 0:
		{
			EM125_Controller eM125_Controller = enemyControllerBase as EM125_Controller;
			eM125_Controller.SetParam(this, WeaponID, KobunDic[WeaponID].KobunHp, need);
			eM125_Controller.SetPositionAndRotation(BornPos, base.direction == -1);
			eM125_Controller.SetRoomPos(LeftBorn, RightBorn);
			eM125_Controller.SetActive(true);
			if (!flag)
			{
				eM125_Controller.SetDebut();
			}
			break;
		}
		case 1:
		{
			EM126_Controller eM126_Controller = enemyControllerBase as EM126_Controller;
			eM126_Controller.SetParam(this, WeaponID, KobunDic[WeaponID].KobunHp, need);
			eM126_Controller.SetPositionAndRotation(BornPos, base.direction == -1);
			eM126_Controller.SetRoomPos(LeftBorn, RightBorn);
			eM126_Controller.SetActive(true);
			if (!flag)
			{
				eM126_Controller.SetDebut();
			}
			break;
		}
		case 2:
		{
			EM127_Controller eM127_Controller = enemyControllerBase as EM127_Controller;
			eM127_Controller.SetParam(this, WeaponID, KobunDic[WeaponID].KobunHp, need);
			eM127_Controller.SetPositionAndRotation(BornPos, base.direction == -1);
			eM127_Controller.SetRoomPos(LeftBorn, RightBorn);
			eM127_Controller.SetActive(true);
			if (!flag)
			{
				eM127_Controller.SetDebut();
			}
			break;
		}
		case 3:
		{
			EM128_Controller eM128_Controller = enemyControllerBase as EM128_Controller;
			eM128_Controller.SetParam(this, WeaponID, KobunDic[WeaponID].KobunHp, need);
			eM128_Controller.SetPositionAndRotation(BornPos, base.direction == -1);
			eM128_Controller.SetRoomPos(LeftBorn, RightBorn);
			eM128_Controller.SetActive(true);
			if (!flag)
			{
				eM128_Controller.SetDebut();
			}
			break;
		}
		case 4:
		{
			EM129_Controller eM129_Controller = enemyControllerBase as EM129_Controller;
			eM129_Controller.SetParam(this, WeaponID, KobunDic[WeaponID].KobunHp, need);
			eM129_Controller.SetPositionAndRotation(BornPos, base.direction == -1);
			eM129_Controller.SetRoomPos(LeftBorn, RightBorn);
			eM129_Controller.SetActive(true);
			if (!flag)
			{
				eM129_Controller.SetDebut();
			}
			break;
		}
		}
		TeamMember item = new TeamMember(WeaponID, enemyControllerBase.gameObject);
		KobunTeam.Add(item);
	}

	public void RemoveKobun(int KobunID)
	{
		for (int i = 0; i < KobunTeam.Count; i++)
		{
			if (KobunTeam[i].KobunID != KobunID)
			{
				continue;
			}
			KobunTeam.Remove(KobunTeam[i]);
			int num = ((KobunNum <= 1) ? 1 : 2);
			int num2 = GameLogicUpdateManager.GameFrame;
			if (NextSpawnFrame.Count + KobunTeam.Count < num)
			{
				if (NextSpawnFrame.Count > 0)
				{
					int num3 = NextSpawnFrame[NextSpawnFrame.Count - 1];
					int gameFrame = GameLogicUpdateManager.GameFrame;
					num2 = ((num3 + 40 > gameFrame) ? num3 : gameFrame);
				}
				NextSpawnFrame.Add(num2 + SpawnCDTime * 20);
			}
			break;
		}
	}

	public void SetKobunDebutOver(int ID, bool hasdebut)
	{
		KobunData value = KobunDic[ID];
		value.debutover = hasdebut;
		KobunDic[ID] = value;
	}

	private bool CheckDebutOver()
	{
		foreach (KeyValuePair<int, KobunData> item in KobunDic)
		{
			if (!item.Value.debutover)
			{
				return false;
			}
		}
		return true;
	}

	private void StopKobunBack()
	{
		for (int i = 0; i < KobunTeam.Count; i++)
		{
			switch (KobunTeam[i].KobunID)
			{
			case 0:
				KobunTeam[i].Kobun.GetComponent<EM125_Controller>().SetNeedBack(false);
				break;
			case 1:
				KobunTeam[i].Kobun.GetComponent<EM126_Controller>().SetNeedBack(false);
				break;
			case 2:
				KobunTeam[i].Kobun.GetComponent<EM127_Controller>().SetNeedBack(false);
				break;
			case 3:
				KobunTeam[i].Kobun.GetComponent<EM128_Controller>().SetNeedBack(false);
				break;
			case 4:
				KobunTeam[i].Kobun.GetComponent<EM129_Controller>().SetNeedBack(false);
				break;
			}
		}
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

	public void SetDeadCompelete(EnemyControllerBase lastkobun)
	{
		_lastKobun = lastkobun;
		StartCoroutine(BossDieFlow(_lastKobun.GetTargetPoint()));
	}

	private void CallEventEnemyPoint(int nID)
	{
		EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
		stageEventCall.nID = nID;
		stageEventCall.tTransform = OrangeBattleUtility.CurrentCharacter.transform;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
	}
}
