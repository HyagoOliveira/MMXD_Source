using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class EM043_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private List<int> moveDirLists = new List<int>();

	private List<int> moveVecLists = new List<int>();

	private bool bNeedNext;

	public bool bCanBreak;

	private VInt3 tRealVelocity = VInt3.zero;

	private Vector3 tmpRealVelocity;

	private bool IgnoreFristSE = true;

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
		base.AimPoint = Vector3.up * 0.9f;
		base.AllowAutoAim = false;
		StageObjParam[] componentsInChildren = GetComponentsInChildren<StageObjParam>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].tLinkSOB = null;
		}
		IgnoreGravity = true;
		IgnoreGlobalVelocity = true;
		Controller.UseIgnoreSelf = true;
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		string[] array = smsg.Split(',');
		if (array.Length != 0)
		{
			int num = array.Length / 2;
			for (int i = 0; i < num; i++)
			{
				moveDirLists.Add(int.Parse(array[i * 2]));
				moveVecLists.Add(int.Parse(array[i * 2 + 1]));
			}
			bNeedNext = true;
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		BaseUpdate();
		if (IgnoreGravity)
		{
			tmpRealVelocity = (tRealVelocity.vec3 + _velocityExtra.vec3) * GameLogicUpdateManager.m_fFrameLen + _velocityShift.vec3;
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
		if (tmpRealVelocity.x != 0f || (tmpRealVelocity.y != 0f && IgnoreGravity))
		{
			Controller.UpdateRaycastOrigins();
			RaycastHit2D raycastHit2D = Controller.ObjectMeeting(tmpRealVelocity.x, tmpRealVelocity.y, Controller.collisionMask);
			if ((bool)raycastHit2D)
			{
				if (tmpRealVelocity.x != 0f && tmpRealVelocity.y == 0f)
				{
					if (raycastHit2D.distance > 0.005f)
					{
						tmpRealVelocity.x = ((tmpRealVelocity.x > 0f) ? (raycastHit2D.distance - 0.005f) : (0f - raycastHit2D.distance + 0.005f));
					}
					else
					{
						tmpRealVelocity.x = 0f;
					}
				}
				else if (raycastHit2D.distance > 0.005f)
				{
					tmpRealVelocity.y = ((tmpRealVelocity.y > 0f) ? (raycastHit2D.distance - 0.005f) : (0f - raycastHit2D.distance + 0.005f));
				}
				else
				{
					tmpRealVelocity.y = 0f;
				}
				bNeedNext = true;
			}
			_velocity = new VInt3(tmpRealVelocity / GameLogicUpdateManager.m_fFrameLen);
			base.LogicUpdate();
			Vector3 zero = Vector3.zero;
			if (tmpRealVelocity.x != 0f)
			{
				raycastHit2D = Controller.ObjectMeeting(tmpRealVelocity.x, 0f, BulletScriptableObject.Instance.BulletLayerMaskPlayer);
				if ((bool)raycastHit2D)
				{
					OrangeCharacter component = raycastHit2D.transform.GetComponent<OrangeCharacter>();
					if (component == null)
					{
						PetControllerBase component2 = raycastHit2D.transform.GetComponent<PetControllerBase>();
						if ((bool)component2 && (int)component2.Hp > 0)
						{
							component2.Hp = 0;
							component2.Hurt(new HurtPassParam());
						}
					}
					if ((bool)component && (int)component.Hp > 0)
					{
						float num = ((tmpRealVelocity.x > 0f) ? (tmpRealVelocity.x - raycastHit2D.distance) : (tmpRealVelocity.x + raycastHit2D.distance));
						bool flag = false;
						component.Controller.UpdateRaycastOrigins();
						RaycastHit2D raycastHit2D2 = component.Controller.ObjectMeeting(num, 0f, component.Controller.collisionMask);
						if ((bool)raycastHit2D2)
						{
							zero.x = ((num > 0f) ? (raycastHit2D2.distance - 0.005f) : (0f - raycastHit2D2.distance + 0.005f));
							flag = true;
						}
						Vector3 position = component.transform.position;
						Vector3 vec = component.Controller.LogicPosition.vec3;
						if (flag)
						{
							position += zero;
							vec += zero;
							component.transform.position = position;
							component.Controller.LogicPosition = new VInt3(vec);
							if (component.Controller.CheckCanMovePositionY(Controller.Collider2D))
							{
								SKILL_TABLE value;
								if (!component.IsUnBreak() && EnemyData.n_SKILL_1 != -1 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(EnemyData.n_SKILL_1, out value))
								{
									RefPassiveskill.TriggerSkill(value, 1, 4095, component.selfBuffManager, component.selfBuffManager, 0);
								}
							}
							else
							{
								SKILL_TABLE value2;
								if (!component.IsUnBreak() && EnemyData.n_SKILL_1 != -1 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(EnemyData.n_SKILL_1, out value2))
								{
									RefPassiveskill.TriggerSkill(value2, 1, 4095, component.selfBuffManager, component.selfBuffManager, 0);
								}
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_REBORNEVENT, component, component.transform.position);
							}
						}
						else
						{
							zero.x = num;
							zero.y = 0f;
							position += zero;
							vec += zero;
							component.transform.position = position;
							component.Controller.LogicPosition = new VInt3(vec);
						}
					}
					zero = Vector3.zero;
				}
				raycastHit2D = Controller.ObjectMeeting(0f, 0.03f, BulletScriptableObject.Instance.BulletLayerMaskPlayer);
				if ((bool)raycastHit2D)
				{
					OrangeCharacter component3 = raycastHit2D.transform.GetComponent<OrangeCharacter>();
					if (component3 == null)
					{
						PetControllerBase component4 = raycastHit2D.transform.GetComponent<PetControllerBase>();
						if ((bool)component4 && (int)component4.Hp > 0)
						{
							component4.Hp = 0;
							component4.Hurt(new HurtPassParam());
						}
					}
					if ((bool)component3 && (int)component3.Hp > 0)
					{
						component3.Controller.UpdateRaycastOrigins();
						RaycastHit2D raycastHit2D2 = component3.Controller.ObjectMeeting(tmpRealVelocity.x, 0f, component3.Controller.collisionMask);
						zero.x = tmpRealVelocity.x;
						zero.y = 0f;
						if ((bool)raycastHit2D2)
						{
							if (tmpRealVelocity.x > 0f)
							{
								if (raycastHit2D2.distance > 0.01f)
								{
									zero.x = raycastHit2D2.distance - 0.005f;
								}
								else
								{
									zero.x = 0f;
								}
							}
							else if (raycastHit2D2.distance > 0.01f)
							{
								zero.x = 0f - raycastHit2D2.distance + 0.005f;
							}
							else
							{
								zero.x = 0f;
							}
						}
						Vector3 vec2 = component3.Controller.LogicPosition.vec3;
						vec2 += zero;
						component3.Controller.LogicPosition = new VInt3(vec2);
						component3.DistanceDelta = Vector3.Distance(component3.transform.localPosition, component3.Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
					}
				}
				raycastHit2D = Controller.ObjectMeeting(tmpRealVelocity.x, 0f, BulletScriptableObject.Instance.BulletLayerMaskEnemy);
				if ((bool)raycastHit2D)
				{
					EnemyControllerBase enemyControllBase = StageObjParam.GetEnemyControllBase(raycastHit2D.transform);
					if (enemyControllBase != null && (int)enemyControllBase.Hp > 0)
					{
						float num2 = ((tmpRealVelocity.x > 0f) ? (tmpRealVelocity.x - raycastHit2D.distance) : (tmpRealVelocity.x + raycastHit2D.distance));
						bool flag2 = false;
						enemyControllBase.Controller.UpdateRaycastOrigins();
						RaycastHit2D raycastHit2D2 = enemyControllBase.Controller.ObjectMeeting(num2, 0f, enemyControllBase.Controller.collisionMask);
						if ((bool)raycastHit2D2)
						{
							zero.x = ((num2 > 0f) ? (raycastHit2D2.distance - 0.005f) : (0f - raycastHit2D2.distance + 0.005f));
							flag2 = true;
						}
						Vector3 position2 = enemyControllBase.transform.position;
						Vector3 vec3 = enemyControllBase.Controller.LogicPosition.vec3;
						if (flag2)
						{
							if ((int)enemyControllBase.Hp > 0)
							{
								enemyControllBase.Hp = 0;
								enemyControllBase.Hurt(new HurtPassParam());
							}
						}
						else
						{
							zero.x = num2;
							zero.y = 0f;
							position2 += zero;
							vec3 += zero;
							enemyControllBase.transform.position = position2;
							enemyControllBase.Controller.LogicPosition = new VInt3(vec3);
						}
					}
					zero = Vector3.zero;
				}
				raycastHit2D = Controller.ObjectMeeting(0f, 0.03f, BulletScriptableObject.Instance.BulletLayerMaskEnemy);
				if ((bool)raycastHit2D)
				{
					EnemyControllerBase enemyControllBase2 = StageObjParam.GetEnemyControllBase(raycastHit2D.transform);
					if (enemyControllBase2 != null && (int)enemyControllBase2.Hp > 0)
					{
						enemyControllBase2.Controller.UpdateRaycastOrigins();
						RaycastHit2D raycastHit2D2 = enemyControllBase2.Controller.ObjectMeeting(tmpRealVelocity.x, 0f, enemyControllBase2.Controller.collisionMask);
						zero.x = tmpRealVelocity.x;
						zero.y = 0f;
						if ((bool)raycastHit2D2)
						{
							if (tmpRealVelocity.x > 0f)
							{
								if (raycastHit2D2.distance > 0.01f)
								{
									zero.x = raycastHit2D2.distance - 0.005f;
								}
								else
								{
									zero.x = 0f;
								}
							}
							else if (raycastHit2D2.distance > 0.01f)
							{
								zero.x = 0f - raycastHit2D2.distance + 0.005f;
							}
							else
							{
								zero.x = 0f;
							}
						}
						Vector3 vec4 = enemyControllBase2.Controller.LogicPosition.vec3;
						vec4 += zero;
						enemyControllBase2.Controller.LogicPosition = new VInt3(vec4);
						enemyControllBase2.distanceDelta = Vector3.Distance(enemyControllBase2.transform.localPosition, enemyControllBase2.Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
					}
				}
			}
			if (tmpRealVelocity.y != 0f)
			{
				raycastHit2D = Controller.ObjectMeeting(0f, tmpRealVelocity.y, BulletScriptableObject.Instance.BulletLayerMaskPlayer);
				if ((bool)raycastHit2D)
				{
					OrangeCharacter component5 = raycastHit2D.transform.GetComponent<OrangeCharacter>();
					if (component5 == null)
					{
						PetControllerBase component6 = raycastHit2D.transform.GetComponent<PetControllerBase>();
						if ((bool)component6 && (int)component6.Hp > 0)
						{
							component6.Hp = 0;
							component6.Hurt(new HurtPassParam());
						}
					}
					if ((bool)component5 && (int)component5.Hp > 0)
					{
						float num3 = ((tmpRealVelocity.y > 0f) ? (tmpRealVelocity.y - raycastHit2D.distance) : (tmpRealVelocity.y + raycastHit2D.distance));
						bool flag3 = false;
						RaycastHit2D raycastHit2D2 = component5.Controller.ObjectMeeting(0f, num3, component5.Controller.collisionMask);
						if ((bool)raycastHit2D2)
						{
							zero.y = ((num3 > 0f) ? (raycastHit2D2.distance - 0.005f) : (0f - raycastHit2D2.distance + 0.005f));
							flag3 = true;
						}
						Vector3 position3 = component5.transform.position;
						Vector3 vec5 = component5.Controller.LogicPosition.vec3;
						if (flag3)
						{
							position3 += zero;
							vec5 += zero;
							component5.transform.position = position3;
							component5.Controller.LogicPosition = new VInt3(vec5);
							if (component5.Controller.CheckCanMovePositionX(Controller.Collider2D))
							{
								SKILL_TABLE value3;
								if (!component5.IsUnBreak() && EnemyData.n_SKILL_1 != -1 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(EnemyData.n_SKILL_1, out value3))
								{
									RefPassiveskill.TriggerSkill(value3, 1, 4095, component5.selfBuffManager, component5.selfBuffManager, 0);
								}
							}
							else
							{
								SKILL_TABLE value4;
								if (!component5.IsUnBreak() && EnemyData.n_SKILL_1 != -1 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(EnemyData.n_SKILL_1, out value4))
								{
									RefPassiveskill.TriggerSkill(value4, 1, 4095, component5.selfBuffManager, component5.selfBuffManager, 0);
								}
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_REBORNEVENT, component5, component5.transform.position);
							}
						}
						else
						{
							zero.x = 0f;
							zero.y = num3;
							position3 += zero;
							vec5 += zero;
							component5.transform.position = position3;
							component5.Controller.LogicPosition = new VInt3(vec5);
						}
					}
					zero = Vector3.zero;
				}
				raycastHit2D = Controller.ObjectMeeting(0f, tmpRealVelocity.y, BulletScriptableObject.Instance.BulletLayerMaskEnemy);
				if ((bool)raycastHit2D)
				{
					EnemyControllerBase enemyControllBase3 = StageObjParam.GetEnemyControllBase(raycastHit2D.transform);
					if (enemyControllBase3 != null && (int)enemyControllBase3.Hp > 0)
					{
						enemyControllBase3.Hp = 0;
						enemyControllBase3.Hurt(new HurtPassParam());
					}
				}
			}
		}
		else if (!IgnoreGravity)
		{
			_velocity = new VInt3(tmpRealVelocity / GameLogicUpdateManager.m_fFrameLen);
			base.LogicUpdate();
			tmpRealVelocity = _velocity.vec3 * GameLogicUpdateManager.m_fFrameLen;
		}
		if (!bNeedNext)
		{
			return;
		}
		if (moveDirLists.Count > 0)
		{
			switch (moveDirLists[0])
			{
			case 8:
				tRealVelocity = new VInt3(moveVecLists[0], 0, 0);
				break;
			case 4:
				tRealVelocity = new VInt3(-moveVecLists[0], 0, 0);
				break;
			case 2:
				tRealVelocity = new VInt3(0, moveVecLists[0], 0);
				break;
			case 1:
				tRealVelocity = new VInt3(0, -moveVecLists[0], 0);
				break;
			case 16:
				IgnoreGravity = false;
				IgnoreGlobalVelocity = false;
				_velocity = VInt3.zero;
				tRealVelocity = VInt3.zero;
				break;
			}
			moveDirLists.RemoveAt(0);
		}
		else
		{
			tRealVelocity = VInt3.zero;
		}
		bNeedNext = false;
		if (!IgnoreFristSE)
		{
			base.SoundSource.PlaySE("BattleSE", 79);
			IgnoreFristSE = true;
		}
		else
		{
			IgnoreFristSE = false;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (bCanBreak || (int)Hp <= 0)
		{
			base.Hurt(tHurtPassParam);
		}
		return Hp;
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
}
