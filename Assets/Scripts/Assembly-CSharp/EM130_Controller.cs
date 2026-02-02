using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using enums;

public class EM130_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private int DestoryFrame;

	private float DestoryTime = 0.5f;

	private bool isDead;

	[SerializeField]
	private int MoveSpd = 6000;

	[SerializeField]
	private ParticleSystem ShieldFx;

	[SerializeField]
	private ParticleSystem FragmentFx;

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
		Transform[] componentsInChildren = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(componentsInChildren);
		base.AimPoint = new Vector3(0f, 0f, 0f);
		base.AllowAutoAim = false;
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		if (Controller.Collisions.left || (Controller.Collisions.right && (int)Hp > 0))
		{
			Hp = 0;
			Hurt(new HurtPassParam());
		}
		if (GameLogicUpdateManager.GameFrame > DestoryFrame && isDead)
		{
			BackToPool();
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		IgnoreGravity = true;
		_transform.position = new Vector3(_transform.position.x, _transform.position.y, 0f);
		bDeadShock = false;
		if (isActive)
		{
			FragmentFx.Stop();
			ShieldFx.Play();
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.HitCallback = HitTarget;
			_collideBullet.Active(targetMask);
			isDead = false;
			_velocity.x = MoveSpd * base.direction;
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	public void SetPositionAndRotation(Vector3 pos, int direct = 1)
	{
		base.direction = direct;
		ModelTransform.localEulerAngles = new Vector3(0f, -15 * base.direction, 0f);
		ModelTransform.localScale = new Vector3(Mathf.Abs(ModelTransform.localScale.x) * (float)base.direction, ModelTransform.localScale.y, ModelTransform.localScale.z);
		_transform.position = pos;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (tHurtPassParam.wpnType != WeaponType.Melee && (int)Hp > 0)
		{
			return Hp;
		}
		return base.Hurt(tHurtPassParam);
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (DeadCallback != null)
		{
			DeadCallback();
		}
		StageObjParam component = GetComponent<StageObjParam>();
		if (component != null && component.nEventID != 0)
		{
			EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
			stageEventCall.nID = component.nEventID;
			component.nEventID = 0;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
		}
		PlaySE("BossSE03", "bs110_raider04");
		isDead = true;
		ShieldFx.Stop();
		ShieldFx.Clear();
		FragmentFx.Play();
		DestoryFrame = GameLogicUpdateManager.GameFrame + (int)(DestoryTime * 20f);
		_velocity = VInt3.zero;
		_collideBullet.BackToPool();
	}

	private void LoadParts(Transform[] childs)
	{
		if (!ModelTransform)
		{
			ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		}
		if (!_collideBullet)
		{
			_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "BodyCollider").gameObject.AddOrGetComponent<CollideBullet>();
		}
		if (!FragmentFx)
		{
			FragmentFx = OrangeBattleUtility.FindChildRecursive(ref childs, "FragmentFx", true).GetComponent<ParticleSystem>();
		}
		if (!ShieldFx)
		{
			ShieldFx = OrangeBattleUtility.FindChildRecursive(ref childs, "ShieldFx", true).GetComponent<ParticleSystem>();
		}
	}

	private void HitTarget(object obj)
	{
		if (obj != null)
		{
			Collider2D collider2D = obj as Collider2D;
			if (collider2D != null && (bool)collider2D.gameObject.GetComponent<OrangeCharacter>())
			{
				Hp = 0;
				Hurt(new HurtPassParam());
			}
		}
	}
}
