#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using StageLib;
using UnityEngine;

public class MeleeBullet : BulletBase
{
	public enum HitStopSkill
	{
		ryuenjin = 10101,
		ryuenjinVer2 = 10102
	}

	public int DESTROY_FRAME = 3;

	public const int MaxHit = 5;

	public bool IsActivate;

	private bool IsDestroy;

	private int DestoryFrame = int.MaxValue;

	public bool isUseHitStop2Self;

	private BoxCollider2D _boxCollider;

	private Rigidbody2D _rigidbody2D;

	private Transform _rootTransform;

	private Transform _endTransform;

	private HashSet<Transform> _ignoreList;

	private OrangeTimer _clearTimer;

	private SlashType _slashType;

	private Dictionary<Transform, int> HitCount = new Dictionary<Transform, int>();

	private string[] oriHitSE;

	private bool isFristHit = true;

	private SlashColliderInfo[] SlashColliderAry;

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || GameLogicUpdateManager.GameFrame >= DestoryFrame || col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0)
		{
			return;
		}
		StageObjParam component = col.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null)
		{
			if ((int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
			return;
		}
		PlayerCollider component2 = col.GetComponent<PlayerCollider>();
		if (component2 != null && component2.IsDmgReduceShield())
		{
			Hit(col);
		}
		else if (col.gameObject.GetComponentInParent<StageHurtObj>() != null)
		{
			Hit(col);
		}
	}

	private void OnTriggerStay2D(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || GameLogicUpdateManager.GameFrame >= DestoryFrame || col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0)
		{
			return;
		}
		StageObjParam component = col.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null)
		{
			if ((int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
			return;
		}
		PlayerCollider component2 = col.gameObject.GetComponent<PlayerCollider>();
		if (component2 != null && component2.IsDmgReduceShield())
		{
			Hit(col);
		}
		else if (col.gameObject.GetComponentInParent<StageHurtObj>() != null)
		{
			Hit(col);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_clearTimer = OrangeTimerManager.GetTimer();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_slash_000", 5);
		_rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody2D.useFullKinematicContacts = true;
		_boxCollider = base.gameObject.AddOrGetComponent<BoxCollider2D>();
		_boxCollider.isTrigger = true;
		_boxCollider.enabled = false;
		_boxCollider.size = new Vector2(2f, 1.4f);
		_boxCollider.offset = new Vector2(0f, 0.7f);
		IsDestroy = false;
		DestoryFrame = int.MaxValue;
		_ignoreList = new HashSet<Transform>();
	}

	public void Active(SlashType pSlashType, LayerMask pTargetMask, WeaponStruct weaponStruct)
	{
		_slashType = pSlashType;
		IsDestroy = false;
		DestoryFrame = int.MaxValue;
		if (pSlashType == SlashType.Skill)
		{
			UpdateBulletData(weaponStruct.FastBulletDatas[0], Owner);
		}
		else
		{
			UpdateBulletData(weaponStruct.FastBulletDatas[(int)pSlashType], Owner);
		}
		TargetMask = pTargetMask;
		UseMask = (int)BulletScriptableObjectInstance.BulletLayerMaskObstacle | (int)TargetMask;
		if (CoroutineMove != null)
		{
			StopCoroutine(CoroutineMove);
		}
		CoroutineMove = StartCoroutine(OnStartMove());
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nInNetID, nDirection);
		if (ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(FxImpact))
		{
			FxImpact = "fxhit_slash_000";
		}
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Debug.Log("NOPE");
	}

	protected override IEnumerator OnStartMove()
	{
		IsActivate = true;
		_boxCollider.enabled = true;
		_clearTimer.TimerStart();
		_rigidbody2D.WakeUp();
		do
		{
			if (_clearTimer.GetMillisecond() >= BulletData.n_FIRE_SPEED)
			{
				_clearTimer.TimerStart();
				_ignoreList.Clear();
				foreach (KeyValuePair<Transform, int> item in HitCount)
				{
					if (BulletData.n_DAMAGE_COUNT == 0)
					{
						if (item.Value >= 5)
						{
							_ignoreList.Add(item.Key);
						}
					}
					else if (item.Value >= BulletData.n_DAMAGE_COUNT)
					{
						_ignoreList.Add(item.Key);
					}
				}
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (!IsDestroy || GameLogicUpdateManager.GameFrame <= DestoryFrame);
		BackToPool();
		yield return null;
	}

	public override void BackToPool()
	{
		IsDestroy = false;
		DestoryFrame = int.MaxValue;
		IsActivate = false;
		_boxCollider.enabled = false;
		_rigidbody2D.Sleep();
		_HitSE = oriHitSE;
		isFristHit = true;
		isBuffTrigger = false;
		ClearList();
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_BULLET_UNREGISTER, this);
	}

	public void ClearList()
	{
		_ignoreList.Clear();
		HitCount.Clear();
	}

	public override void Hit(Collider2D col)
	{
		if (CheckHitList(ref _ignoreList, col.transform))
		{
			return;
		}
		_ignoreList.Add(col.transform);
		OrangeCharacter mainPlayerOC = StageUpdate.GetMainPlayerOC();
		if (mainPlayerOC != null && mainPlayerOC.IsLocalPlayer)
		{
			if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
			{
				foreach (int value2 in Enum.GetValues(typeof(HitStopSkill)))
				{
					if (BulletData.n_ID != value2)
					{
						continue;
					}
					if (mainPlayerOC.IsHitStop)
					{
						return;
					}
					mainPlayerOC.IsHitStop = true;
					EnemyControllerBase componentInParent = col.gameObject.GetComponentInParent<EnemyControllerBase>();
					if (componentInParent != null)
					{
						if (componentInParent.EnemyData.n_TYPE != 2)
						{
							componentInParent.ActiveShake(0.03f);
						}
						StartCoroutine(componentInParent.HitStop());
						if (!isFristHit)
						{
							_HitSE = _HitGuardSE;
						}
						else
						{
							isFristHit = false;
						}
					}
				}
			}
			if (isUseHitStop2Self)
			{
				if (mainPlayerOC.IsHitStop)
				{
					return;
				}
				mainPlayerOC.IsHitStop = true;
				if ((float)_clearTimer.GetMillisecond() < 0.1f)
				{
					mainPlayerOC.IsHitStop = false;
					return;
				}
				if (mainPlayerOC.IsHitStop)
				{
					StartCoroutine(mainPlayerOC.HitStop());
					if (!isFristHit)
					{
						_HitSE = _HitGuardSE;
					}
					else
					{
						isFristHit = false;
					}
				}
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, col.transform.position, Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0, 90)), Array.Empty<object>());
		int value = -1;
		HitCount.TryGetValue(col.transform, out value);
		if (value == -1)
		{
			HitCount.Add(col.transform, 1);
		}
		else
		{
			HitCount[col.transform] = value + 1;
		}
		CaluDmg(BulletData, col.transform, 0f, 0.5f);
	}

	public void UpdateCollider(int index, bool isLeft)
	{
		base.transform.localRotation = Quaternion.Euler(0f, (!isLeft) ? 0f : 180f, 0f);
		_boxCollider.offset = SlashColliderAry[index].offset;
		_boxCollider.size = SlashColliderAry[index].size;
	}

	public SlashColliderInfo GetSlashCollider(int index, int gender = 0)
	{
		return SlashColliderAry[index];
	}

	public void UpdateWeaponData(SKILL_TABLE pData, string owner)
	{
		BulletID = pData.n_ID;
		Owner = owner;
		BulletData = pData;
		if ((BulletData.n_FLAG & 1) == 0)
		{
			BlockMask = BulletScriptableObjectInstance.BulletLayerMaskObstacle;
		}
		if (((uint)BulletData.n_FLAG & 2u) != 0)
		{
			BulletMask = BulletScriptableObjectInstance.BulletLayerMaskBullet;
		}
		SlashColliderAry = new SlashColliderInfo[20];
		SlashColliderAry[0] = new SlashColliderInfo
		{
			offset = new Vector2(0.8f, 1.4f),
			size = new Vector2(2.75f, 2.8f),
			timing = 0.46f
		};
		SlashColliderAry[1] = new SlashColliderInfo
		{
			offset = new Vector2(1.1f, 1f),
			size = new Vector2(4f, 2f),
			timing = 0.54f
		};
		SlashColliderAry[2] = new SlashColliderInfo
		{
			offset = new Vector2(1.1f, 1f),
			size = new Vector2(4f, 2f),
			timing = 0.48f
		};
		SlashColliderAry[3] = new SlashColliderInfo
		{
			offset = new Vector2(1.1f, 1f),
			size = new Vector2(3.5f, 2f),
			timing = 0.48f
		};
		SlashColliderAry[4] = new SlashColliderInfo
		{
			offset = new Vector2(1.2f, 2.2f),
			size = new Vector2(4f, 4.4f),
			timing = 0.57f
		};
		SlashColliderAry[9] = new SlashColliderInfo
		{
			offset = new Vector2(0f, 1f),
			size = new Vector2(4f, 2f),
			timing = 0.47f
		};
		SlashColliderAry[6] = new SlashColliderInfo
		{
			offset = new Vector2(1.1f, 1f),
			size = new Vector2(3.2f, 2f),
			timing = 0.46f
		};
		SlashColliderAry[7] = new SlashColliderInfo
		{
			offset = new Vector2(1.1f, 1f),
			size = new Vector2(3.2f, 2f),
			timing = 0.46f
		};
		SlashColliderAry[8] = new SlashColliderInfo
		{
			offset = new Vector2(0f, 0.8f),
			size = new Vector2(3.8f, 3.8f),
			timing = 0.1f
		};
		SlashColliderAry[10] = new SlashColliderInfo
		{
			offset = new Vector2(0f, 0.8f),
			size = new Vector2(3.8f, 3.8f),
			timing = 0.1f
		};
		SlashColliderAry[19] = new SlashColliderInfo
		{
			offset = new Vector2(1.5f, 1.5f),
			size = new Vector2(2.14f, 3f),
			timing = 0.64f
		};
		_UseSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_USE_SE);
		_HitSE = (oriHitSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_HIT_SE));
		GetHistGuardSE();
		if (_UseSE[0] != "" && (_UseSE[1].EndsWith("_lp") || _UseSE[1].EndsWith("_lg")))
		{
			checkLoopSE = true;
		}
	}

	public void SetDestroy(OrangeCharacter.MainStatus _mainStatus, OrangeCharacter.SubStatus _subStatus)
	{
		DestoryFrame = GameLogicUpdateManager.GameFrame;
		if (_mainStatus == OrangeCharacter.MainStatus.IDLE)
		{
			if (_subStatus == OrangeCharacter.SubStatus.SLASH5_END)
			{
				DestoryFrame += DESTROY_FRAME;
				IsDestroy = true;
			}
		}
		else if (CoroutineMove != null)
		{
			StopCoroutine(CoroutineMove);
			BackToPool();
		}
	}
}
