#define RELEASE
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM202_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		CountDown = 1,
		Hurt = 2
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

	[SerializeField]
	private BoxCollider2D BoxCollider;

	[SerializeField]
	private BoxCollider2D ButtonCollider;

	[SerializeField]
	private Transform PlatformButton;

	[SerializeField]
	private ParticleSystem IdleNum3;

	[SerializeField]
	private ParticleSystem[] CountDownNum = new ParticleSystem[4];

	private bool isCounting;

	private int CountFrame;

	[SerializeField]
	private bool NeedStop;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
	}

	private Vector2 OriginOffset
	{
		get
		{
			return new Vector2(0f, -0.6f);
		}
	}

	private Vector2 OriginSize
	{
		get
		{
			return new Vector2(1.2f, 1.2f);
		}
	}

	private Vector2 PushedOffset
	{
		get
		{
			return new Vector2(0f, -0.65f);
		}
	}

	private Vector2 PushedSize
	{
		get
		{
			return new Vector2(1.2f, 1.1f);
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
		_animator = ModelTransform.GetComponent<Animator>();
		BoxCollider = OrangeBattleUtility.FindChildRecursive(ref childs, "BoxCollider", true).GetComponent<BoxCollider2D>();
		ButtonCollider = OrangeBattleUtility.FindChildRecursive(ref childs, "ButtonCollider", true).GetComponent<BoxCollider2D>();
		PlatformButton = OrangeBattleUtility.FindChildRecursive(ref childs, "O041_2", true);
		IdleNum3 = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_BOMB-FACTORY_004", true).GetComponent<ParticleSystem>();
		CountDownNum[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_BOMB-FACTORY_003", true).GetComponent<ParticleSystem>();
		CountDownNum[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_BOMB-FACTORY_002", true).GetComponent<ParticleSystem>();
		CountDownNum[2] = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_BOMB-FACTORY_001", true).GetComponent<ParticleSystem>();
		CountDownNum[3] = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_BOMB-FACTORY_000", true).GetComponent<ParticleSystem>();
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		base.AimPoint = Vector3.zero;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(8f);
		StageObjParam[] componentsInChildren = GetComponentsInChildren<StageObjParam>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].tLinkSOB = null;
		}
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
			SwitchFx(IdleNum3, true);
			break;
		case MainStatus.CountDown:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("EnemySE02", "em039_ctbomb02");
				if ((bool)BoxCollider)
				{
					BoxCollider.offset = PushedOffset;
					BoxCollider.size = PushedSize;
				}
				if ((bool)PlatformButton)
				{
					PlatformButton.localPosition = Vector3.back * 0.1f;
				}
				SwitchFx(IdleNum3, false);
				SwitchFx(CountDownNum[0], true);
				CountFrame = GameLogicUpdateManager.GameFrame + 20;
				break;
			case SubStatus.Phase1:
				PlaySE("EnemySE02", "em039_ctbomb02");
				SwitchFx(CountDownNum[0], false);
				SwitchFx(CountDownNum[1], true);
				CountFrame = GameLogicUpdateManager.GameFrame + 20;
				break;
			case SubStatus.Phase2:
				PlaySE("EnemySE02", "em039_ctbomb02");
				SwitchFx(CountDownNum[1], false);
				SwitchFx(CountDownNum[2], true);
				CountFrame = GameLogicUpdateManager.GameFrame + 20;
				break;
			case SubStatus.Phase3:
				PlaySE("EnemySE02", "em039_ctbomb03");
				SwitchFx(CountDownNum[2], false);
				SwitchFx(CountDownNum[3], true);
				CountFrame = GameLogicUpdateManager.GameFrame + 20;
				break;
			case SubStatus.Phase4:
				SwitchFx(CountDownNum[3], false);
				Suicide();
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
		{
			foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
			{
				if (ButtonCollider.bounds.Contains(runPlayer.Controller.LogicPosition.vec3) && runPlayer.Controller.Collisions.below)
				{
					PlaySE("EnemySE02", "em039_ctbomb01");
					isCounting = true;
					UploadEnemyStatus(1);
				}
			}
			break;
		}
		case MainStatus.CountDown:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame >= CountFrame)
				{
					SetStatus(MainStatus.CountDown, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame >= CountFrame)
				{
					SetStatus(MainStatus.CountDown, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (GameLogicUpdateManager.GameFrame >= CountFrame)
				{
					SetStatus(MainStatus.CountDown, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame >= CountFrame)
				{
					SetStatus(MainStatus.CountDown, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				break;
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
			isCounting = false;
			IgnoreGravity = true;
			SetColliderEnable(false);
			if ((bool)BoxCollider)
			{
				BoxCollider.offset = OriginOffset;
				BoxCollider.size = OriginSize;
			}
			if ((bool)PlatformButton)
			{
				PlatformButton.localPosition = Vector3.zero;
			}
			ModelTransform.gameObject.SetActive(true);
			SetStatus(MainStatus.Idle);
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

	public void SetSuicide()
	{
		Suicide();
	}

	private void Suicide()
	{
		(BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, NowPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
		Hp = 0;
		Hurt(new HurtPassParam());
	}

	public override void ObjCtrl(GameObject tObj, StageCtrlInsTruction tSCE)
	{
		if (EnemyID != 0)
		{
			switch (tSCE.tStageCtrl)
			{
			case 18:
				enemylock();
				break;
			case 19:
				Unlock();
				break;
			}
		}
	}
}
