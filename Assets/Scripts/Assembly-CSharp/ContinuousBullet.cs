using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousBullet : BulletBase
{
	public enum BulletPhase
	{
		Normal = 0,
		Result = 1,
		BackToPool = 2
	}

	[SerializeField]
	private bool AlwaysChkHit = true;

	[HideInInspector]
	public BulletPhase Phase;

	protected HashSet<Transform> _hitList;

	private float m_LastUpdateShowTime;

	private bool fastone_fx;

	private bool IsHit;

	private bool _playFirstSeInNormalPhase;

	private bool _isMuteSE;

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.AddOrGetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
		_hitList = new HashSet<Transform>();
		HitBlockCallback = HitBlockPlaySE;
	}

	public void HitBlockPlaySE(object objs)
	{
		if (_HitGuardSE != null)
		{
			OrangeCriPoint aPoint = MonoBehaviourSingleton<AudioManager>.Instance.GetAPoint();
			if (aPoint != null)
			{
				Transform trans = objs as Transform;
				aPoint.Play(14f, trans, _HitGuardSE[0], _HitGuardSE[1]);
			}
		}
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		IsHit = AlwaysChkHit;
		_playFirstSeInNormalPhase = !AlwaysChkHit;
		_isMuteSE = isMuteSE;
		if (_playFirstSeInNormalPhase)
		{
			isMuteSE = true;
		}
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		if (_playFirstSeInNormalPhase)
		{
			isMuteSE = _isMuteSE;
		}
		isHitBlock = false;
	}

	protected override IEnumerator OnStartMove()
	{
		m_LastUpdateShowTime = Time.realtimeSinceStartup;
		nThrough = BulletData.n_THROUGH / 100;
		fastone_fx = false;
		_transform.eulerAngles = new Vector3(0f, 0f, 0f);
		GameObject gameObject = base.transform.GetChild(0).transform.gameObject;
		Vector3 beginPosition = _transform.position;
		lightning lightning2 = gameObject.GetComponent<lightning>();
		if (IsHit)
		{
			lightning2.SetActive(true);
			lightning2.MaxLine = BulletData.f_DISTANCE;
		}
		if ((bool)MasterTransform && !string.IsNullOrEmpty(FxMuzzleFlare))
		{
			if (BulletData.n_USE_FX_FOLLOW == 0)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxMuzzleFlare, MasterTransform.position, _transform.rotation * BulletQuaternion, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxMuzzleFlare, MasterTransform, BulletQuaternion, Array.Empty<object>());
			}
		}
		do
		{
			switch (Phase)
			{
			case BulletPhase.Normal:
			{
				RaycastHit2D raycastHit2D = Physics2D.Raycast(beginPosition, Direction, BulletData.f_DISTANCE, UseMask);
				if ((bool)raycastHit2D)
				{
					IsHit = true;
					if (_playFirstSeInNormalPhase)
					{
						_playFirstSeInNormalPhase = false;
						isMuteSE = _isMuteSE;
						PlayUseSE();
						isMuteSE = true;
					}
					_transform.position = MasterTransform.position;
					lightning2.SetActive(true);
					lightning2.MaxLine = BulletData.f_DISTANCE;
					List<Vector3> hitList = lightning2._hitList;
					StageObjParam component = raycastHit2D.transform.GetComponent<StageObjParam>();
					if (component != null)
					{
						if (component.tLinkSOB != null && (int)component.tLinkSOB.Hp > 0)
						{
							hitList[0] = component.tLinkSOB.GetTargetPoint();
						}
					}
					else if (raycastHit2D.transform.GetComponent<StageHurtObj>() != null)
					{
						hitList[0] = raycastHit2D.collider.bounds.center;
					}
					else
					{
						hitList[0] = raycastHit2D.point;
					}
					if (!CheckHitList(ref _hitList, raycastHit2D.collider.transform))
					{
						_hitList.Add(raycastHit2D.collider.transform);
						Collider2D[] array = Physics2D.OverlapCircleAll(hitList[0], BulletData.f_RANGE, UseMask);
						for (int i = 0; i < array.Length; i++)
						{
							if (CheckHitList(ref _hitList, array[i].transform) || _hitList.Count >= nThrough)
							{
								continue;
							}
							component = array[i].transform.GetComponent<StageObjParam>();
							if (component != null)
							{
								if (component.tLinkSOB != null && (int)component.tLinkSOB.Hp > 0)
								{
									hitList.Add(component.tLinkSOB.GetTargetPoint());
									_hitList.Add(array[i].transform);
								}
							}
							else if (array[i].transform.GetComponent<StageHurtObj>() != null)
							{
								hitList.Add(array[i].bounds.center);
								_hitList.Add(array[i].transform);
							}
						}
						if (!fastone_fx)
						{
							int num = 0;
							foreach (Transform hit in _hitList)
							{
								MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, hitList[num], BulletQuaternion, Array.Empty<object>());
								CaluDmg(BulletData, hit);
								if (nThrough > 0)
								{
									nThrough--;
								}
								num++;
							}
							fastone_fx = true;
						}
					}
					lightning2.Update_Lightning();
					if ((Time.realtimeSinceStartup - m_LastUpdateShowTime) * 1000f >= (float)BulletData.n_FIRE_SPEED)
					{
						Phase = BulletPhase.BackToPool;
					}
					break;
				}
				if (!IsHit)
				{
					Phase = BulletPhase.BackToPool;
					break;
				}
				if (_playFirstSeInNormalPhase)
				{
					_playFirstSeInNormalPhase = false;
					isMuteSE = _isMuteSE;
					PlayUseSE();
					isMuteSE = true;
				}
				_transform.position = MasterTransform.position;
				OrangeCharacter component2 = MasterTransform.root.GetComponent<OrangeCharacter>();
				if (component2 != null)
				{
					Vector3 shootDirection = component2.ShootDirection;
					lightning2._hitList[0] = _transform.position + shootDirection * BulletData.f_DISTANCE;
					if (!fastone_fx)
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, lightning2.nullhit, BulletQuaternion, Array.Empty<object>());
						fastone_fx = true;
					}
					lightning2.Update_Lightning();
					if ((Time.realtimeSinceStartup - m_LastUpdateShowTime) * 1000f >= (float)BulletData.n_FIRE_SPEED)
					{
						Phase = BulletPhase.BackToPool;
					}
				}
				else
				{
					Phase = BulletPhase.BackToPool;
				}
				break;
			}
			case BulletPhase.Result:
				foreach (Transform hit2 in _hitList)
				{
					CaluDmg(BulletData, hit2);
					if (nThrough > 0)
					{
						nThrough--;
					}
				}
				Phase = BulletPhase.BackToPool;
				break;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (Phase != BulletPhase.BackToPool);
		Stop();
		BackToPool();
	}

	public override void Hit(Collider2D col)
	{
		if (!_hitList.Contains(col.transform))
		{
			int layer = col.gameObject.layer;
			BulletPhase phase = Phase;
			int num = 1;
		}
	}

	public override void BackToPool()
	{
		Phase = BulletPhase.Normal;
		_hitList.Clear();
		lightning component = base.transform.GetChild(0).transform.gameObject.GetComponent<lightning>();
		component.ClearBuffLine();
		component.SetActive(false);
		base.BackToPool();
	}

	protected override void PlayHitSE(OrangeCharacter oc = null, Transform hitPoint = null)
	{
		if (isHitBlock)
		{
			return;
		}
		OrangeCriSource orangeCriSource = hitPoint.gameObject.AddOrGetComponent<OrangeCriSource>();
		if (_HitSE[0] == "HitSE" || _HitSE[0] == "BossSE")
		{
			if (oc != null)
			{
				if (oc.IsLocalPlayer)
				{
					if (oc.UseHitSE)
					{
						oc.SoundSource.PlaySE("HitSE", "ht_player01");
					}
				}
				else if (oc.UseHitSE)
				{
					if (_HitSE[0] == "HitSE")
					{
						if (_HitSE[1].EndsWith("01") || _HitSE[1].EndsWith("03"))
						{
							string[] array = _HitSE[1].Split('0');
							if (array[0] == "ht_player")
							{
								array[0] = "ht_trw";
							}
							oc.SoundSource.PlaySE(_HitSE[0], array[0] + "02");
						}
						else
						{
							oc.SoundSource.PlaySE(_HitSE[0], _HitSE[1]);
						}
					}
					else
					{
						oc.SoundSource.PlaySE(_HitSE[0], _HitSE[1]);
					}
				}
			}
			else if (orangeCriSource != null)
			{
				needPlayEndSE = false;
				if (_HitSE[1] == "ht_player01")
				{
					orangeCriSource.PlaySE("HitSE", "ht_trw02");
				}
				else
				{
					orangeCriSource.PlaySE(_HitSE[0], _HitSE[1]);
				}
			}
		}
		else if (orangeCriSource != null)
		{
			orangeCriSource.PlaySE(_HitSE[0], _HitSE[1]);
		}
		else
		{
			base.SoundSource.PlaySE(_HitSE[0], _HitSE[1]);
		}
		needWeaponImpactSE = false;
	}
}
