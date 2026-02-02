using System;
using StageLib;
using UnityEngine;

public class BS054_Controller : BS019_Controller
{
	private bool bPlaySE_bs019_round;

	private RainCameraController mCamera_efx;

	protected override void Awake()
	{
		FX_BOSS_EXPLODE1 = "FX_BOSS_EXPLODE1";
		FX_BOSS_EXPLODE2 = "FX_BOSS_EXPLODE2";
		FX_SKILL1 = "fxuse_bs19_skill1";
		FX_FIRE_EXPLOSION = "Fire_Explosion_000";
		FX_FIRE_FLASH = "fxuse_bsFire_Flash_000";
		FX_USE_TARGET = "fxuseTarget";
		FX_DURING_CF0 = "fxduring_CF_0_000";
		Fire_Start_X = 590f;
		IsBossVer = true;
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "scenfx");
		if ((bool)transform)
		{
			mCamera_efx = transform.GetComponent<RainCameraController>();
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		_velocityShift.z = 0;
		BaseLogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (AILevel == 1)
			{
				if (Controller.Collisions.below)
				{
					UpdateRandomState();
				}
			}
			else if (_currentFrame > 1f && Controller.Collisions.below)
			{
				UpdateRandomState();
			}
			break;
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_debutEnd)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Run:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 3.0 || (base.direction == -1 && Controller.Collisions.left) || (base.direction == 1 && Controller.Collisions.right))
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			{
				if (!Controller.Collisions.below)
				{
					break;
				}
				SetStatus(_mainStatus, SubStatus.Phase2);
				if (!mCamera_efx.IsPlaying)
				{
					mCamera_efx.Play();
				}
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_FIRE_FLASH, new Vector3(_LFoot_Point.position.x, _LFoot_Point.position.y - 1f, _LFoot_Point.position.z - 2f), Quaternion.identity, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_FIRE_FLASH, new Vector3(_RFoot_Point.position.x, _RFoot_Point.position.y - 1f, _RFoot_Point.position.z - 2f), Quaternion.identity, Array.Empty<object>());
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1.25f, false);
				int num = OrangeBattleUtility.Random(5, 11);
				float num2 = 30 / num;
				for (int i = 0; i < num; i++)
				{
					BasicBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BasicBullet>(EnemyWeapons[3].BulletData.s_MODEL);
					if ((bool)poolObj)
					{
						poolObj.isForceSE = true;
						poolObj.needPlayEndSE = true;
						Vector3 vector = new Vector3(Fire_Start_X + (float)i * num2, OrangeBattleUtility.Random(2, 4), 0f);
						RaycastHit2D[] array = Physics2D.RaycastAll(vector, Vector3.down, 200f, LayerMask.GetMask("SemiBlock"));
						EnemyWeapons[3].BulletData.f_DISTANCE = 500f;
						poolObj.UpdateBulletData(EnemyWeapons[3].BulletData);
						poolObj.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
						if (array.Length != 0)
						{
							int num3 = OrangeBattleUtility.Random(0, array.Length);
							poolObj.FreeDISTANCE = Vector3.Distance(vector, array[num3].transform.position) - 1f;
							Vector3 vector2 = new Vector3(vector.x, vector.y, vector.z);
							Vector3 vector3 = new Vector3(array[num3].transform.position.x, array[num3].transform.position.y, array[num3].transform.position.z);
							float distance = Mathf.Abs(Vector3.Distance(vector2, vector3));
							Vector3 vector4 = (vector2.xy() - vector3.xy()).normalized;
							MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>(FX_USE_TARGET, vector2, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, -vector4)), Array.Empty<object>()).SetEffect(distance, new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), 2f);
						}
						poolObj.needPlayEndSE = true;
						poolObj.Active(vector, Vector3.down, targetMask);
						poolObj.UpdateFx();
					}
				}
				bPlaySE_bs019_round = true;
				PlayBossSE("BossSE", 38);
				break;
			}
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
					PlayBossSE("BossSE", 39);
					bPlaySE_bs019_round = false;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Spin:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_SKILL1, base.transform, Quaternion.identity, Array.Empty<object>());
					_LHandCollideBullet.Active(targetMask);
					_RHandCollideBullet.Active(targetMask);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 3.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					if (_RHandCollideBullet.IsActivate)
					{
						_RHandCollideBullet.IsDestroy = true;
					}
					if (_LHandCollideBullet.IsActivate)
					{
						_LHandCollideBullet.IsDestroy = true;
					}
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Punch:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
					_LHandCollideBullet.Active(targetMask);
					_RHandCollideBullet.Active(targetMask);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					if (_RHandCollideBullet.IsActivate)
					{
						_RHandCollideBullet.IsDestroy = true;
					}
					if (_LHandCollideBullet.IsActivate)
					{
						_LHandCollideBullet.IsDestroy = true;
					}
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 0.4)
				{
					if (nDeadCount > 10)
					{
						SetStatus(_mainStatus, SubStatus.Phase1);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			case SubStatus.Phase1:
				switch (_DieStatus)
				{
				case DieStatus.Left:
					if (_currentFrame > 0.05f)
					{
						OrangeBattleUtility.LockPlayer();
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_BOSS_EXPLODE1, L_Explode1, Quaternion.identity, Array.Empty<object>());
						_DieStatus = DieStatus.Right;
						PlayBossSE("HitSE", 102);
					}
					break;
				case DieStatus.Right:
					if (_currentFrame > 0.35f)
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_BOSS_EXPLODE1, R_Explode1, Quaternion.identity, Array.Empty<object>());
						_DieStatus = DieStatus.Mid;
						PlayBossSE("HitSE", 103);
					}
					break;
				case DieStatus.Mid:
					if (_currentFrame > 0.8f)
					{
						StartCoroutine(BossDieFlow(base.AimTransform));
						_DieStatus = DieStatus.End;
					}
					break;
				case DieStatus.End:
					break;
				}
				break;
			}
			break;
		case MainStatus.Laser:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
					Left_Laser_bullet.gameObject.SetActive(true);
					Left_LinePoint_Object.lineStart.transform.position = Left_eye_Light.transform.position;
				}
				break;
			case SubStatus.Phase1:
				Left_LinePoint_Object.lineStart.transform.position = new Vector3(Left_eye_Light.transform.position.x, Left_eye_Light.transform.position.y, 0f);
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				Left_LinePoint_Object.lineStart.transform.position = new Vector3(Left_eye_Light.transform.position.x, Left_eye_Light.transform.position.y, 0f);
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
					eye_Light.gameObject.SetActive(false);
					Left_Laser_bullet.gameObject.SetActive(false);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.Hurt:
		case MainStatus.IdleWaitNet:
			break;
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus == MainStatus.Dead)
		{
			return;
		}
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		if (IsBossVer)
		{
			StageUpdate.SlowStage();
			if (bPlaySE_bs019_round)
			{
				PlayBossSE("BossSE", 39);
				bPlaySE_bs019_round = false;
			}
		}
		SetStatus(MainStatus.Dead);
	}

	protected override void UpdateDirection(int forceDirection = 0)
	{
		base.UpdateDirection(forceDirection);
		_characterMaterial.UpdateTex((base.direction != -1) ? (-1) : 0);
	}

	public override void Unlock()
	{
		_unlockReady = true;
		base.AllowAutoAim = true;
		_debutEnd = true;
		if ((int)Hp > 0)
		{
			SetColliderEnable(true);
		}
		if (InGame)
		{
			Activate = true;
		}
	}
}
