#define RELEASE
using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM199_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
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

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private float IdleFloatFrame = 5f;

	private int IdleFloatSpd = 10;

	private int IdleFloatUpDown = 1;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private Vector3[] AtkPos = new Vector3[9];

	private Vector3 DebutPos;

	private float Degree;

	private float EndDegree = 160f;

	private float RotateSpd = 90f;

	private int RotateDirection = 1;

	private float Length;

	[SerializeField]
	private float LengthCrease = 2f;

	[SerializeField]
	private Vector3 Skill0Offset = Vector3.up;

	[SerializeField]
	private float SkillOffsetMulti = 3f;

	[SerializeField]
	private Transform Skill0ShootPos;

	[SerializeField]
	private int Skill0MoveSpeed = 7500;

	[SerializeField]
	private float Skill0WaitTime = 5f;

	private int NextAtkPos = 8;

	private int Skill0AtkPos = 1;

	private int Skill0WaitFrame;

	private bool CanAtk = true;

	private Vector3 ShootAngle = Vector3.down;

	private Vector3 StartPos;

	private Vector3 EndPos;

	private float moveDis;

	[Header("Mob002 攻擊次數")]
	[SerializeField]
	private int Mob002ActTimes = 5;

	[SerializeField]
	private float Mob002Skill0WaitTime = 6f;

	private int ActTimes;

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
		if (Skill0ShootPos == null)
		{
			Skill0ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPos", true);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		base.AimPoint = Vector3.zero;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(30f);
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
		case MainStatus.Debut:
			if (_subStatus == SubStatus.Phase0)
			{
				Degree = (Length = 0f);
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				StartPos = _transform.position;
				ValueTuple<int, float>[] array = new ValueTuple<int, float>[9];
				float[] array2 = new float[9];
				EndPos = GetTargetPos();
				for (int i = 0; i < 9; i++)
				{
					array[i] = new ValueTuple<int, float>(i, Vector2.Distance(AtkPos[i], EndPos));
					array2[i] = array[i].Item2;
				}
				Array.Sort(array2);
				for (int j = 0; j < 9; j++)
				{
					if (array[j].Item2 == array2[Skill0AtkPos])
					{
						NextAtkPos = array[j].Item1;
					}
				}
				EndPos = AtkPos[NextAtkPos];
				moveDis = Vector2.Distance(StartPos, EndPos);
				if (moveDis > 0.2f)
				{
					_velocity = new VInt3((EndPos - StartPos).normalized) * Skill0MoveSpeed * 0.001f;
					break;
				}
				_transform.position = EndPos;
				Controller.LogicPosition = new VInt3(_transform.position);
				SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				return;
			}
			case SubStatus.Phase1:
			{
				EndPos = GetTargetPos();
				Vector3 normalized = (EndPos - Skill0ShootPos.position).normalized;
				ModelTransform.localRotation = Quaternion.Euler(0f - Vector2.SignedAngle(Vector2.right, normalized), 90f, 0f);
				_velocity = VInt3.zero;
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, Skill0ShootPos, normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_002)
				{
					Skill0WaitFrame = GameLogicUpdateManager.GameFrame + (int)(Mob002Skill0WaitTime * 20f);
				}
				else
				{
					Skill0WaitFrame = GameLogicUpdateManager.GameFrame + (int)(Skill0WaitTime * 20f);
				}
				break;
			}
			}
			break;
		}
		AiTimer.TimerStart();
	}

	private void UpdateDirection(int forceDirection = 0, bool back = false)
	{
		int num = base.direction;
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (TargetPos.x > Controller.LogicPosition.x)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		if (back)
		{
			base.direction = -base.direction;
		}
		if (num != base.direction)
		{
			ShootAngle.x = 0f - ShootAngle.x;
			Skill0Offset.x = 0f - Skill0Offset.x;
			Vector3 eulerAngles = ModelTransform.localRotation.eulerAngles;
			ModelTransform.localRotation = Quaternion.Euler(0f - eulerAngles.x, eulerAngles.y, eulerAngles.z);
		}
		int direction2 = base.direction;
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
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
			if (!Target)
			{
				foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
				{
					if (runPlayer.IsAlive())
					{
						Target = runPlayer;
					}
				}
			}
			if (!bWaitNetStatus && CanAtk && (bool)Target)
			{
				SetStatus(MainStatus.Skill0);
			}
			else if (!Target && (float)GameLogicUpdateManager.GameFrame > IdleFloatFrame)
			{
				_velocity.y = IdleFloatSpd * IdleFloatUpDown;
				IdleFloatUpDown *= -1;
			}
			break;
		case MainStatus.Debut:
			if (_subStatus == SubStatus.Phase0)
			{
				Vector3 vector = Quaternion.Euler(0f, 0f, Degree) * (Vector3.right * Length * RotateDirection);
				ShootAngle = Quaternion.Euler(0f, 0f, 90f) * vector;
				Skill0Offset = -ShootAngle.normalized;
				ModelTransform.localRotation = Quaternion.Euler(-90f + (0f - Vector2.SignedAngle(Vector2.right, vector)), 90f, 0f);
				Controller.LogicPosition = new VInt3(DebutPos + vector);
				Degree += RotateSpd * GameLogicUpdateManager.m_fFrameLen * (float)RotateDirection;
				Length += LengthCrease * GameLogicUpdateManager.m_fFrameLen;
				if (Degree * (float)RotateDirection >= EndDegree)
				{
					UpdateDirection(1);
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Vector2.Distance(_transform.position, StartPos) >= moveDis)
				{
					_transform.position = EndPos;
					Controller.LogicPosition = new VInt3(_transform.position);
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			{
				if (GameLogicUpdateManager.GameFrame <= Skill0WaitFrame)
				{
					break;
				}
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_002)
				{
					if (--ActTimes > 0)
					{
						SetStatus(MainStatus.Idle);
						break;
					}
					SetStatus(MainStatus.Idle);
					SetDie();
				}
				else
				{
					SetStatus(MainStatus.Idle);
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
		if (isActive)
		{
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_002)
			{
				ActTimes = Mob002ActTimes;
			}
			IgnoreGravity = true;
			CanAtk = true;
			DebutPos = _transform.position;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			CheckRoomSize();
			SetStatus(MainStatus.Debut);
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
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
		return _transform.position + Vector3.right * 1f * base.direction;
	}

	public void SetEndDegree(float degree)
	{
		EndDegree = degree;
	}

	public void SetRotateSpd(int spd = 90)
	{
		RotateSpd = spd;
	}

	public void SetRotateDirection(int direc = 1)
	{
		RotateDirection = direc;
		Degree *= RotateDirection;
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer);
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
			MaxPos = new Vector3(_transform.position.x + 1f, _transform.position.y + 6f, 0f);
			MinPos = new Vector3(_transform.position.x - 8f, _transform.position.y, 0f);
			return;
		}
		MaxPos = new Vector3(raycastHit2D2.point.x, raycastHit2D3.point.y, 0f);
		MinPos = new Vector3(raycastHit2D.point.x, raycastHit2D4.point.y, 0f);
		Vector3 vector2 = (MaxPos + MinPos) / 2f;
		AtkPos[0] = new Vector3(MinPos.x + 2f, MaxPos.y - 1.5f, 0f);
		AtkPos[1] = new Vector3(vector2.x, MaxPos.y - 1.5f, 0f);
		AtkPos[2] = new Vector3(MaxPos.x - 2f, MaxPos.y - 1.5f, 0f);
		AtkPos[3] = new Vector3(MinPos.x + 2f, vector2.y, 0f);
		AtkPos[4] = new Vector3(vector2.x, vector2.y, 0f);
		AtkPos[5] = new Vector3(MaxPos.x - 2f, vector2.y, 0f);
		AtkPos[6] = new Vector3(MinPos.x + 2f, MinPos.y + 1.5f, 0f);
		AtkPos[7] = new Vector3(vector2.x, MinPos.y + 1.5f, 0f);
		AtkPos[8] = new Vector3(MaxPos.x - 2f, MinPos.y + 1.5f, 0f);
	}

	public void SetSkill0AtkPos(int pos)
	{
		Skill0AtkPos = pos;
	}

	public void StopAtk(bool stop)
	{
		CanAtk = !stop;
	}

	public void SetDie()
	{
		Hp = 0;
		Hurt(new HurtPassParam());
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
	}
}
