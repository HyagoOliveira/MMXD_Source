using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM166_Controller : EnemyLoopBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Link = 1,
		Shoot = 2,
		CD = 3,
		Hurt = 4
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		MAX_SUBSTATUS = 3
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_SHOOT = 1,
		ANI_HURT = 2,
		MAX_ANIMATION_ID = 3
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	[Header("翔蟲設定")]
	[SerializeField]
	[Tooltip("開啟Link距離")]
	private float CanLinkDis = 4f;

	[SerializeField]
	[Tooltip("推出玩家力道")]
	private int PullForce = 12000;

	[SerializeField]
	[Tooltip("推出過程時間")]
	private float PullTime = 0.5f;

	[SerializeField]
	[Tooltip("額外推出力道X")]
	private int PullForceX = 5000;

	[SerializeField]
	[Tooltip("額外推出力道Y")]
	private int PullForceY;

	private int PullFrame;

	private bool HasDead;

	[SerializeField]
	[Tooltip("消失時間")]
	private float DisappearTime = 0.5f;

	private int DisappearFrame;

	[Header("連結特效設定")]
	[SerializeField]
	[Tooltip("開始特效")]
	private ParticleSystem LinkFxS;

	[SerializeField]
	[Tooltip("持續特效")]
	private ParticleSystem LinkFxD;

	[SerializeField]
	[Tooltip("結束特效")]
	private ParticleSystem LinkFxF;

	[SerializeField]
	private float FxLength = 3.5f;

	private float FxLengthFix;

	[SerializeField]
	[Tooltip("特效時間")]
	private float LinkFxTime = 0.5f;

	private int LinkFxFrame;

	[Header("鱗粉特效")]
	[SerializeField]
	[Tooltip("鱗粉持續特效")]
	private ParticleSystem ScalePowderFxD;

	[SerializeField]
	[Tooltip("鱗粉消失特效")]
	private ParticleSystem ScalePowderFxE;

	[Header("提示特效")]
	[SerializeField]
	[Tooltip("綠圈特效")]
	private ParticleSystem CircleFx;

	private bool HasOpenCircle;

	[Header("操龍專用")]
	[SerializeField]
	[Tooltip("是否為操龍用翔蟲")]
	private bool IsWyvernRiding;

	private Vector3 PullVelocity = Vector3.zero;

	protected bool _virtualButtonInitialized;

	protected readonly VirtualButton[] _virtualButton = new VirtualButton[5];

	private bool HasShowLinkIcon;

	private bool HasLockButton;

	private OrangeCharacter LinkTarget;

	private float distance;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
		if (IsWyvernRiding)
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.ENTER_OR_LEAVE_RIDE_ARMOR, SetLinkable);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
		if (IsWyvernRiding)
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.ENTER_OR_LEAVE_RIDE_ARMOR, SetLinkable);
		}
	}

	private void SetLinkable(bool playerEnterRideArmor)
	{
		if (Activate && playerEnterRideArmor)
		{
			Vector3 vector = new Vector3(10000f, 10000f);
			Controller.LogicPosition = new VInt3(vector);
			_transform.localPosition = vector;
			if ((bool)LinkTarget)
			{
				SwitchKakeriMushiIcon();
				SwitchButtonEnable(true);
			}
			LinkTarget = null;
		}
	}

	private void HashAnimator()
	{
		_animationHash = new int[3];
		_animationHash[0] = Animator.StringToHash("EM096@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM096@move_loop");
		_animationHash[2] = Animator.StringToHash("EM096@hurt_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		if (LinkFxS == null)
		{
			LinkFxS = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_MHR_flybug_001_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (LinkFxD == null)
		{
			LinkFxD = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_MHR_flybug_004_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (LinkFxF == null)
		{
			LinkFxF = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_MHR_flybug_003_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (ScalePowderFxD == null)
		{
			ScalePowderFxD = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_Big_Illumina_002_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (ScalePowderFxE == null)
		{
			ScalePowderFxE = OrangeBattleUtility.FindChildRecursive(ref childs, "fxduring_MHR_flybug_004_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (CircleFx == null)
		{
			ScalePowderFxE = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_MHR_flybug_000_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		HashAnimator();
		LoadParts(ref childs);
		base.AimPoint = Vector3.zero;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		AiTimer.TimerStart();
		base.SoundSource.Initial(OrangeSSType.ENEMY);
		base.SoundSource.MaxDistance = 12f;
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
			if ((bool)LinkTarget && LinkTarget.IgnoreGravity)
			{
				LinkTarget.IgnoreGravity = false;
			}
			if (HasShowLinkIcon)
			{
				SwitchKakeriMushiIcon();
			}
			if (HasLockButton)
			{
				CloseLinkFx(LinkFxD);
				PlayLinkFX(LinkFxF);
				SwitchButtonEnable(true);
				ScalePowderFxD.Stop();
			}
			break;
		case MainStatus.Link:
			if ((bool)LinkTarget)
			{
				LinkTarget.SetSpeed(0, 0);
				LinkTarget.SetStatus(OrangeCharacter.MainStatus.JUMP, OrangeCharacter.SubStatus.TELEPORT_POSE);
				LinkTarget.IgnoreGravity = true;
				PlayLinkFX(LinkFxS);
				ScalePowderFxD.Play();
				if (HasOpenCircle)
				{
					PlaySE("BattleSE02", "bt_kakerimushi02");
					HasOpenCircle = false;
					LeanTween.cancel(CircleFx.gameObject);
					LeanTween.scale(CircleFx.gameObject, Vector3.zero, 0.2f);
				}
			}
			if (!HasLockButton)
			{
				SwitchButtonEnable();
			}
			LinkFxFrame = GameLogicUpdateManager.GameFrame + (int)(LinkFxTime * 20f);
			break;
		case MainStatus.Shoot:
			if ((bool)LinkTarget)
			{
				LinkTarget.IgnoreGravity = false;
			}
			PullVelocity = (Controller.GetRealCenterPos() - LinkTarget.Controller.GetRealCenterPos()).normalized;
			PullFrame = GameLogicUpdateManager.GameFrame + (int)(PullTime * 20f);
			if (!HasLockButton)
			{
				SwitchButtonEnable();
			}
			break;
		case MainStatus.CD:
			_velocity = VInt3.zero;
			DisappearFrame = GameLogicUpdateManager.GameFrame + (int)(DisappearTime * 20f);
			if ((bool)LinkTarget && LinkTarget.IgnoreGravity)
			{
				LinkTarget.IgnoreGravity = false;
			}
			if (HasShowLinkIcon)
			{
				SwitchKakeriMushiIcon();
			}
			if (HasLockButton)
			{
				CloseLinkFx(LinkFxD);
				PlayLinkFX(LinkFxF);
				SwitchButtonEnable(true);
				ScalePowderFxD.Stop();
				ScalePowderFxE.Play();
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Link:
		case MainStatus.Shoot:
		case MainStatus.CD:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		BaseLogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!LinkTarget)
			{
				LinkTarget = Target;
			}
			if (!HasOpenCircle && (bool)Target)
			{
				PlaySE("BattleSE02", "bt_kakerimushi01");
				HasOpenCircle = true;
				if (!CircleFx.isPlaying)
				{
					CircleFx.Play();
				}
				CircleFx.transform.localScale = Vector3.zero;
				StartCoroutine(StageResManager.TweenObjScale(CircleFx.gameObject, Vector3.one * (CanLinkDis / 2f), 0.5f));
			}
			else if (HasOpenCircle && !Target)
			{
				HasOpenCircle = false;
				LeanTween.cancel(CircleFx.gameObject);
				StartCoroutine(StageResManager.TweenObjScale(CircleFx.gameObject, Vector3.zero, 0.5f));
			}
			if (!LinkTarget)
			{
				break;
			}
			if (Vector2.SqrMagnitude(Controller.GetRealCenterPos() - LinkTarget.Controller.GetRealCenterPos()) < CanLinkDis * CanLinkDis)
			{
				switch (LinkTarget.CurMainStatus)
				{
				case OrangeCharacter.MainStatus.IDLE:
				case OrangeCharacter.MainStatus.WALK:
				case OrangeCharacter.MainStatus.DASH:
				case OrangeCharacter.MainStatus.JUMP:
				case OrangeCharacter.MainStatus.FALL:
					if (!HasDead && !HasShowLinkIcon)
					{
						SwitchKakeriMushiIcon(true);
					}
					break;
				default:
					if (HasShowLinkIcon)
					{
						SwitchKakeriMushiIcon();
					}
					break;
				}
			}
			else if (HasShowLinkIcon)
			{
				SwitchKakeriMushiIcon();
			}
			break;
		case MainStatus.Link:
			if (!LinkTarget)
			{
				LinkTarget = _enemyAutoAimSystem.GetClosetPlayer();
			}
			if (!LinkTarget || (int)LinkTarget.Hp <= 0)
			{
				SetStatus(MainStatus.Idle);
			}
			else if (GameLogicUpdateManager.GameFrame > LinkFxFrame)
			{
				SetStatus(MainStatus.Shoot);
			}
			break;
		case MainStatus.Shoot:
			if (!LinkTarget)
			{
				LinkTarget = _enemyAutoAimSystem.GetClosetPlayer();
			}
			if (!LinkTarget || (int)LinkTarget.Hp <= 0)
			{
				SetStatus(MainStatus.CD);
				break;
			}
			if ((bool)LinkTarget)
			{
				VInt vInt = (VInt)(OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000);
				int num = PullForceX * ((PullVelocity.x >= 0f) ? 1 : (-1));
				int y = PullForceY * ((PullVelocity.y >= 0f) ? 1 : (-1));
				LinkTarget.AddForce(new VInt3((int)(PullVelocity.x * (float)PullForce) + num, y, 0));
				LinkTarget.AddForceFieldProxy(new VInt3(0, (int)(PullVelocity.y * (float)PullForce), 0));
			}
			if (GameLogicUpdateManager.GameFrame > PullFrame - 2)
			{
				if (HasShowLinkIcon)
				{
					SwitchKakeriMushiIcon();
				}
				if (HasLockButton)
				{
					CloseLinkFx(LinkFxD);
					PlayLinkFX(LinkFxF);
					SwitchButtonEnable(true);
					ScalePowderFxD.Stop();
					ScalePowderFxE.Play();
				}
			}
			if (GameLogicUpdateManager.GameFrame > PullFrame)
			{
				SetStatus(MainStatus.CD);
			}
			break;
		case MainStatus.CD:
			if (GameLogicUpdateManager.GameFrame > DisappearFrame)
			{
				HurtPassParam hurtPassParam = new HurtPassParam();
				DeadBehavior(ref hurtPassParam);
			}
			break;
		}
	}

	public override void UpdateFunc()
	{
		if (!Activate)
		{
			return;
		}
		base.UpdateFunc();
		Vector3 localPosition = _transform.localPosition;
		_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		switch (_mainStatus)
		{
		case MainStatus.Shoot:
			if (HasLockButton)
			{
				PlayLinkFX(LinkFxD);
			}
			break;
		case MainStatus.Idle:
		case MainStatus.Link:
			break;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			HasDead = false;
			ModelTransform.localEulerAngles = new Vector3(0f, 90 * base.direction, 0f);
			IgnoreGravity = true;
			ModelTransform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			HasShowLinkIcon = false;
			HasLockButton = false;
			HasOpenCircle = false;
			_virtualButtonInitialized = false;
			InitializeVirtualButton();
			if ((bool)ScalePowderFxD)
			{
				ScalePowderFxD.Stop();
			}
			if ((bool)CircleFx)
			{
				CircleFx.Stop();
			}
			if ((bool)LinkFxD)
			{
				LinkFxD.Stop();
			}
			SetStatus(MainStatus.Idle);
		}
		else
		{
			LinkTarget = null;
		}
	}

	public void SetCD()
	{
		SetStatus(MainStatus.CD);
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
		ModelTransform.localEulerAngles = new Vector3(0f, 90 * base.direction, 0f);
		_transform.position = pos;
	}

	private void UpdateDirection(int forceDirection = 0)
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
		ModelTransform.localEulerAngles = new Vector3(0f, 90 * base.direction, 0f);
	}

	private Vector3 GetTargetPos(bool realcenter = true)
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
				TargetPos = new VInt3(Target.GetTargetPoint());
			}
			return TargetPos.vec3;
		}
		return _transform.position + Vector3.right * 30f * base.direction;
	}

	protected bool InitializeVirtualButton()
	{
		if (_virtualButtonInitialized)
		{
			_virtualButton[0].ClearStick();
			_virtualButton[1].ClearStick();
			_virtualButton[2].ClearStick();
			return true;
		}
		for (int i = 0; i < _virtualButton.Length; i++)
		{
			switch (i)
			{
			default:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.JUMP);
				break;
			case 1:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.DASH);
				break;
			case 2:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.SKILL0);
				break;
			case 3:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.SKILL1);
				break;
			case 4:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.FS_SKILL);
				break;
			}
			if (_virtualButton[i] == null)
			{
				return false;
			}
		}
		_virtualButtonInitialized = true;
		return true;
	}

	private void SwitchKakeriMushiIcon(bool onoff = false)
	{
		if (!_virtualButtonInitialized && !InitializeVirtualButton())
		{
			return;
		}
		if (onoff)
		{
			if (HasDead || !Activate)
			{
				return;
			}
			_virtualButton[0].UpdateIconByBundlePath(AssetBundleScriptableObject.Instance.m_uiPath + "ui_battleinfo", "UI_battle_button_event_link");
			if (!LinkTarget)
			{
				LinkTarget = _enemyAutoAimSystem.GetClosetPlayer();
			}
			if ((bool)LinkTarget)
			{
				LinkTarget.PlayerPressJumpCB = StartLink;
			}
		}
		else
		{
			_virtualButton[0].UpdateIconByBundlePath(AssetBundleScriptableObject.Instance.m_uiPath + "ui_battleinfo", "UI_battle_button_jump");
			if (!LinkTarget)
			{
				LinkTarget = _enemyAutoAimSystem.GetClosetPlayer();
			}
			if ((bool)LinkTarget)
			{
				LinkTarget.PlayerResetPressJumpCB();
			}
		}
		HasShowLinkIcon = onoff;
	}

	private void SwitchButtonEnable(bool onoff = false)
	{
		if (!_virtualButtonInitialized && !InitializeVirtualButton())
		{
			return;
		}
		if ((bool)LinkTarget)
		{
			if (onoff)
			{
				LinkTarget.ConnectStandardCtrlCB();
				LinkTarget.SetBanSkill(false);
			}
			else
			{
				if (HasDead || !Activate)
				{
					return;
				}
				LinkTarget.RemoveSelfLRCB();
				LinkTarget.RemoveSelfDashJumpCB();
				LinkTarget.RemoveSelfGigaAtackCB();
				LinkTarget.SetBanSkill(true);
			}
		}
		HasLockButton = !onoff;
		_virtualButton[0].SetBanMask(!onoff);
		_virtualButton[1].SetBanMask(!onoff);
		_virtualButton[2].SetBanMask(!onoff);
		_virtualButton[3].SetBanMask(!onoff);
		_virtualButton[4].SetBanMask(!onoff);
	}

	private void StartLink()
	{
		if (HasDead || !Activate)
		{
			SwitchKakeriMushiIcon();
			SwitchButtonEnable(true);
			return;
		}
		if (!LinkTarget)
		{
			LinkTarget = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if (!HasLockButton && (bool)LinkTarget)
		{
			SetStatus(MainStatus.Link);
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		HasDead = true;
		if (HasShowLinkIcon)
		{
			SwitchKakeriMushiIcon();
		}
		if (HasLockButton)
		{
			SwitchButtonEnable(true);
		}
		if (bNeedDead && (bool)LinkTarget && LinkTarget.IgnoreGravity)
		{
			LinkTarget.IgnoreGravity = false;
			LinkTarget = null;
		}
		BackToPool();
	}

	public override void BackToPool()
	{
		base.BackToPool();
	}

	private void PlayLinkFX(ParticleSystem linkfx)
	{
		if ((bool)LinkTarget)
		{
			if (!linkfx.gameObject.activeSelf)
			{
				linkfx.gameObject.SetActive(true);
			}
			if (!linkfx.isPlaying)
			{
				linkfx.Play();
			}
			linkfx.transform.position = LinkTarget.Controller.GetRealCenterPos();
			linkfx.transform.LookAt(Controller.GetRealCenterPos());
			FxLengthFix = Vector2.Distance(LinkTarget.Controller.GetRealCenterPos(), Controller.GetRealCenterPos()) / FxLength;
			linkfx.transform.localScale = new Vector3(1f, 1f, FxLengthFix);
		}
	}

	private void CloseLinkFx(ParticleSystem linkfx)
	{
		if (linkfx.isPlaying)
		{
			linkfx.Stop();
		}
		linkfx.gameObject.SetActive(false);
	}
}
