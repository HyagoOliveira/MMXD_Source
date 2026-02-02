using System.Collections.Generic;
using StageLib;
using UnityEngine;

public class FallingFloor : MonoBehaviour, IManagedUpdateBehavior, ILogicUpdate
{
	[SerializeField]
	public bool IsForceSE;

	[SerializeField]
	public BattleSE rakkaSE = BattleSE.CRI_BATTLESE_BT_BLOCK_RAKKA;

	[SerializeField]
	public BattleSE fallingSE;

	private VInt3 _velocity;

	protected float DistanceDelta;

	public Controller2D Controller;

	private Transform _transform;

	private bool _startShake;

	private bool _fall;

	private int nFallFrameCount;

	private List<Transform> listSelf = new List<Transform>();

	public int nSkillID;

	public bool bNeedAttack;

	private const float fFallBias = 0.01f;

	public bool bRebornEvent;

	public bool bShakeFallDisable;

	private int nShakeFrameCount;

	public int nShakeFrame = 30;

	public float fShakeDis = 1f;

	public OrangeCriSource SoundSource;

	[SerializeField]
	private bool visible;

	private Renderer meshRenderer;

	private bool bEnable;

	[SerializeField]
	private string[] ShakeSE;

	private void Awake()
	{
		Controller = GetComponentInChildren<Controller2D>();
		_transform = base.transform;
	}

	private void Start()
	{
		meshRenderer = OrangeGameUtility.AddOrGetRenderer<Renderer>(base.gameObject);
	}

	private void OnEnable()
	{
		bEnable = true;
		if (_fall)
		{
			if (!MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.CheckUpdateContain(this))
			{
				MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
			}
			if (!MonoBehaviourSingleton<UpdateManager>.Instance.CheckUpdateContain(this))
			{
				MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
			}
		}
	}

	private void OnDisable()
	{
		bEnable = false;
		while (MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.CheckUpdateContain(this))
		{
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		}
		while (MonoBehaviourSingleton<UpdateManager>.Instance.CheckUpdateContain(this))
		{
			MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
		}
	}

	public void UpdateFunc()
	{
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, DistanceDelta);
	}

	public void LogicUpdate()
	{
		if (bShakeFallDisable)
		{
			if (nShakeFrameCount < nShakeFrame)
			{
				if (nShakeFrameCount == 0 && ShakeSE.Length == 2)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play(ShakeSE[0], ShakeSE[1]);
				}
				nShakeFrameCount++;
				Controller.LogicPosition = new VInt3(Controller.LogicPosition.vec3 + new Vector3(fShakeDis, 0f, 0f));
				DistanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
				fShakeDis = 0f - fShakeDis;
			}
			else
			{
				Controller.LogicPosition = new VInt3(Controller.LogicPosition.vec3 + new Vector3(fShakeDis, 0f, 0f));
				DistanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
				bShakeFallDisable = false;
				Controller.Collider2D.enabled = false;
			}
			return;
		}
		if (_fall)
		{
			UpdateGravity();
		}
		Vector3 vector = _velocity.vec3 * GameLogicUpdateManager.m_fFrameLen;
		Vector2 vector2 = vector.xy();
		float num = vector2.magnitude;
		RaycastHit2D raycastHit2D = StageResManager.ObjMoveCollisionWithBoxCheck(Controller, vector2 / num, num, Controller.collisionMask, listSelf);
		if ((bool)raycastHit2D)
		{
			_velocity.y = 0;
			if (raycastHit2D.distance <= 0.01f)
			{
				nFallFrameCount = 0;
				DistanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3);
				if (DistanceDelta > 0.01f)
				{
					DistanceDelta *= Time.deltaTime / GameLogicUpdateManager.m_fFrameLen;
				}
				else
				{
					DistanceDelta = 0f;
				}
				return;
			}
			vector2 = vector2 / num * raycastHit2D.distance;
			num = raycastHit2D.distance;
			vector.x = vector2.x;
			vector.y = vector2.y;
			if (nFallFrameCount > 0)
			{
				if (visible || IsForceSE)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(rakkaSE);
				}
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
			}
		}
		else
		{
			nFallFrameCount++;
		}
		if (bNeedAttack)
		{
			for (int num2 = StageUpdate.runPlayers.Count - 1; num2 >= 0; num2--)
			{
				if (StageUpdate.runPlayers[num2].UsingVehicle)
				{
					RideArmorController component = StageUpdate.runPlayers[num2].transform.root.GetComponent<RideArmorController>();
					RaycastHit2D raycastHit2D2 = StageResManager.ObjMoveCollisionWithBoxCheck(component.Controller, Vector2.up, num, LayerMask.GetMask("Block", "SemiBlock"), null, listSelf);
					if ((bool)raycastHit2D2)
					{
						Vector3 zero = Vector3.zero;
						if (component.ModelTransform.localScale.z == 1f)
						{
							BoxCollider2D component2 = raycastHit2D2.transform.GetComponent<BoxCollider2D>();
							if (component2 != null)
							{
								zero.x = component2.bounds.min.x - (component.ModelTransform.position.x + component.Controller.Collider2D.bounds.size.x);
								if (zero.x > 0f)
								{
									zero.x = 0f;
								}
							}
						}
						else
						{
							BoxCollider2D component2 = raycastHit2D2.transform.GetComponent<BoxCollider2D>();
							if (component2 != null)
							{
								zero.x = component2.bounds.max.x - (component.ModelTransform.position.x - component.Controller.Collider2D.bounds.size.x);
								if (zero.x < 0f)
								{
									zero.x = 0f;
								}
							}
						}
						component.transform.localPosition = component.transform.localPosition + zero;
						component.Controller.LogicPosition = new VInt3(component.Controller.LogicPosition.vec3 + zero);
						EventManager.StageSkillAtkTargetParam stageSkillAtkTargetParam = new EventManager.StageSkillAtkTargetParam();
						stageSkillAtkTargetParam.tTrans = component.transform;
						stageSkillAtkTargetParam.nSkillID = nSkillID;
						stageSkillAtkTargetParam.bAtkNoCast = true;
						stageSkillAtkTargetParam.tPos = component.transform.position;
						stageSkillAtkTargetParam.tDir = component.transform.position;
						stageSkillAtkTargetParam.bBuff = false;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam);
					}
				}
				else
				{
					RaycastHit2D raycastHit2D2 = StageResManager.ObjMoveCollisionWithBoxCheck(StageUpdate.runPlayers[num2].Controller, Vector2.up, num, LayerMask.GetMask("Block", "SemiBlock"), null, listSelf);
					if ((bool)raycastHit2D2)
					{
						Vector3 zero2 = Vector3.zero;
						if (StageUpdate.runPlayers[num2].ModelTransform.localScale.z == 1f)
						{
							BoxCollider2D component2 = raycastHit2D2.transform.GetComponent<BoxCollider2D>();
							if (component2 != null)
							{
								zero2.x = component2.bounds.min.x - (StageUpdate.runPlayers[num2].ModelTransform.position.x + StageUpdate.runPlayers[num2].Controller.Collider2D.bounds.size.x);
								if (zero2.x > 0f)
								{
									zero2.x = 0f;
								}
							}
						}
						else
						{
							BoxCollider2D component2 = raycastHit2D2.transform.GetComponent<BoxCollider2D>();
							if (component2 != null)
							{
								zero2.x = component2.bounds.max.x - (StageUpdate.runPlayers[num2].ModelTransform.position.x - StageUpdate.runPlayers[num2].Controller.Collider2D.bounds.size.x);
								if (zero2.x < 0f)
								{
									zero2.x = 0f;
								}
							}
						}
						StageUpdate.runPlayers[num2].transform.localPosition = StageUpdate.runPlayers[num2].transform.localPosition + zero2;
						StageUpdate.runPlayers[num2].Controller.LogicPosition = new VInt3(StageUpdate.runPlayers[num2].Controller.LogicPosition.vec3 + zero2);
						EventManager.StageSkillAtkTargetParam stageSkillAtkTargetParam2 = new EventManager.StageSkillAtkTargetParam();
						stageSkillAtkTargetParam2.tTrans = StageUpdate.runPlayers[num2].transform;
						stageSkillAtkTargetParam2.nSkillID = nSkillID;
						stageSkillAtkTargetParam2.bAtkNoCast = true;
						stageSkillAtkTargetParam2.tPos = StageUpdate.runPlayers[num2].transform.position;
						stageSkillAtkTargetParam2.tDir = StageUpdate.runPlayers[num2].transform.position;
						stageSkillAtkTargetParam2.bBuff = false;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam2);
						if (!StageUpdate.runPlayers[num2].Controller.CheckCanMovePositionX(Controller.Collider2D) && bRebornEvent)
						{
							OrangeCharacter orangeCharacter = StageUpdate.runPlayers[num2];
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_REBORNEVENT, orangeCharacter, orangeCharacter.transform.position);
						}
					}
				}
			}
			for (int num3 = StageUpdate.runEnemys.Count - 1; num3 >= 0; num3--)
			{
				RaycastHit2D raycastHit2D2 = StageResManager.ObjMoveCollisionWithBoxCheck(StageUpdate.runEnemys[num3].mEnemy.Controller, Vector2.up, num, LayerMask.GetMask("Block", "SemiBlock"), null, listSelf);
				if ((bool)raycastHit2D2)
				{
					StageUpdate.runEnemys[num3].mEnemy.Hp = 0;
					StageUpdate.runEnemys[num3].mEnemy.Hurt(new HurtPassParam());
				}
			}
		}
		Controller.LogicPosition = new VInt3(Controller.LogicPosition.vec3 + vector);
		DistanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
	}

	protected void UpdateGravity()
	{
		_velocity.y += OrangeBattleUtility.FP_Gravity * 2 * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
	}

	public void TriggerFall()
	{
		if (_fall)
		{
			return;
		}
		if (bEnable)
		{
			if (!MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.CheckUpdateContain(this))
			{
				MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
			}
			if (!MonoBehaviourSingleton<UpdateManager>.Instance.CheckUpdateContain(this))
			{
				MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
			}
		}
		Controller.LogicPosition = new VInt3(_transform.localPosition);
		_fall = true;
		nFallFrameCount = 0;
		if ((visible || IsForceSE) && fallingSE != 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(fallingSE);
		}
		listSelf.Add(base.transform);
		Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
		foreach (Collider2D collider2D in componentsInChildren)
		{
			listSelf.Add(collider2D.transform);
		}
	}

	private void OnBecameVisible()
	{
		visible = true;
	}

	private void OnBecameInvisible()
	{
		visible = false;
	}
}
