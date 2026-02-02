using System;
using CallbackDefs;
using UnityEngine;

public class EM110_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Rotate = 1,
		Thunder = 2
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		MAX_SUBSTATUS = 4
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private float _currentFrame;

	private int[] _animationHash;

	private BS045_Controller _parentController;

	private Vector3 EulerRotate = new Vector3(30f, 0f, 0f);

	private Transform HitPoint;

	private int NextFrame;

	private int rotatedirect = 1;

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
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		HitPoint = OrangeBattleUtility.FindChildRecursive(ref target, "FX_Point", true);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(20f, 8f);
		base.AllowAutoAim = false;
	}

	protected override void Start()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_wolf-sigma_hitground_000", 2);
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity.x = 0;
			break;
		case MainStatus.Thunder:
			NextFrame = GameLogicUpdateManager.GameFrame + 20;
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
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Rotate:
			if (Controller.Collisions.below)
			{
				_velocity = VInt3.zero;
				ModelTransform.localEulerAngles = new Vector3(45 * -base.direction, -90f, 0f);
				_collideBullet.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
				string s_MODEL = EnemyData.s_MODEL;
				if (s_MODEL == "enemy_em220")
				{
					base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig16");
				}
				else
				{
					base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig08");
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_wolf-sigma_hitground_000", HitPoint, Quaternion.Euler(0f, 0f, 0f), new Vector3(1f, 1f, 1f), Array.Empty<object>());
				}
				SetStatus(MainStatus.Thunder);
			}
			break;
		case MainStatus.Thunder:
			if (GameLogicUpdateManager.GameFrame > NextFrame)
			{
				string s_MODEL = EnemyData.s_MODEL;
				if (s_MODEL == "enemy_em220")
				{
					base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig17");
				}
				else
				{
					PlaySE("BossSE03", "bs021_ptmsig09");
				}
				Vector3 vector = Vector3.down * 0.36f;
				s_MODEL = EnemyData.s_MODEL;
				if (s_MODEL == "enemy_em220")
				{
					vector = Vector3.up * 0.5f;
				}
				BS045_ThunderBullet bS045_ThunderBullet = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, vector + HitPoint.position, Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS045_ThunderBullet;
				BS045_ThunderBullet bS045_ThunderBullet2 = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, vector + HitPoint.position, Vector3.left, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS045_ThunderBullet;
				if ((bool)bS045_ThunderBullet2)
				{
					bS045_ThunderBullet2.SetEndPos(_parentController.GetRoomEdge(-1));
				}
				if ((bool)bS045_ThunderBullet)
				{
					bS045_ThunderBullet.SetEndPos(_parentController.GetRoomEdge());
				}
				NextFrame += 20;
			}
			if ((int)_parentController.Hp <= 0)
			{
				_characterMaterial.ChangeDissolveColor(new Color(1f, 1f, 1f));
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Idle:
			break;
		}
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)(-base.direction));
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			if (_mainStatus == MainStatus.Rotate)
			{
				ModelTransform.Rotate(EulerRotate * -base.direction * rotatedirect);
			}
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		_characterMaterial.ChangeDissolveColor(new Color(0.1f, 0f, 0.3f));
		_maxGravity = OrangeBattleUtility.FP_MaxGravity * 4;
		IgnoreGravity = true;
		base.SetActive(isActive);
		bDeadShock = false;
		if (isActive)
		{
			ModelTransform.localEulerAngles = new Vector3(0f, -90f, 0f);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_collideBullet.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		}
		else
		{
			_collideBullet.BackToPool();
		}
		_characterMaterial.Appear(null, 0.1f);
	}

	public virtual void SetPositionAndRotation(Vector3 pos, int direct)
	{
		base.direction = direct;
		base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * -1f);
		base.transform.position = pos;
	}

	public void SetParameter(VInt3 speed, int direct)
	{
		_velocity = speed;
		rotatedirect = direct;
		if (_velocity.x * base.direction * rotatedirect < 0)
		{
			base.direction *= -1;
		}
		UpdateDirection();
		_transform.position = new Vector3(_transform.position.x, _transform.position.y, 0f);
		SetStatus(MainStatus.Rotate);
	}

	public void SetParent(BS045_Controller parent)
	{
		_parentController = parent;
	}

	protected override void UpdateGravity()
	{
		if (!IgnoreGravity)
		{
			if ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above))
			{
				_velocity.y = 0;
			}
			_velocity.y += OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(_maxGravity.i * 3));
		}
	}
}
