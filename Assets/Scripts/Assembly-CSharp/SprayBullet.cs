#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayBullet : BulletBase
{
	public enum BulletPhase
	{
		Normal = 0,
		Result = 1,
		BackToPool = 2
	}

	[HideInInspector]
	public BulletPhase Phase;

	protected HashSet<Transform> _hitList;

	protected float m_LastUpdateShowTime;

	private bool fastone_fx;

	public int efx_DISTANCE_type;

	protected int bulletcirclenum = 4;

	protected RaycastHit2D[] hits = new RaycastHit2D[10];

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.AddOrGetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
		_hitList = new HashSet<Transform>();
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nInNetID, nDirection);
		if (Phase == BulletPhase.Normal)
		{
			bulletcirclenum = 1;
			float num = BulletData.f_ANGLE;
			if (num == 0f)
			{
				num = 20f;
			}
			num *= 0.5f;
			float f_DISTANCE = BulletData.f_DISTANCE;
			float num2 = f_DISTANCE;
			float num3 = Mathf.Tan(num * ((float)Math.PI / 180f));
			float num4 = num2 * num3;
			float num5 = num4;
			float num6 = 0f;
			while (num6 < f_DISTANCE && bulletcirclenum < 10)
			{
				bulletcirclenum++;
				num6 += num5 * 1.5f;
				num2 -= num5 * 1.5f;
				num5 = num4 * (num2 / f_DISTANCE);
			}
		}
	}

	private void Debug_Show()
	{
		GameObject gameObject = base.transform.GetChild(0).gameObject;
		Vector3 position = gameObject.transform.position;
		Vector3 end = position + Direction * BulletData.f_DISTANCE;
		Vector3 vector = Quaternion.Euler(0f, 0f, 30f) * gameObject.transform.forward.normalized;
		Vector3 end2 = position + vector * BulletData.f_DISTANCE;
		Vector3 vector2 = Quaternion.Euler(0f, 0f, -30f) * gameObject.transform.forward.normalized;
		Vector3 end3 = position + vector2 * BulletData.f_DISTANCE;
		Debug.DrawLine(position, end, Color.red);
		Debug.DrawLine(position, end2, Color.green);
		Debug.DrawLine(position, end3, Color.green);
	}

	protected override IEnumerator OnStartMove()
	{
		m_LastUpdateShowTime = 0f;
		Phase = BulletPhase.Normal;
		if (efx_DISTANCE_type == 0)
		{
			for (int i = 0; i < bulletFxArray.Length; i++)
			{
				ParticleSystem.MainModule main = bulletFxArray[i].main;
				main.startLifetime = BulletData.f_DISTANCE / 5f;
			}
		}
		else if (efx_DISTANCE_type == 1)
		{
			bulletFxArray[0].transform.localScale = new Vector3(BulletData.f_DISTANCE / 8f, 1f, 1f);
		}
		else if (efx_DISTANCE_type == 2)
		{
			for (int j = 0; j < bulletFxArray.Length; j++)
			{
				ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = bulletFxArray[j].velocityOverLifetime;
				if (velocityOverLifetime.enabled)
				{
					velocityOverLifetime.z = BulletData.f_DISTANCE / 5f * 2f;
				}
			}
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
			if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				continue;
			}
			switch (Phase)
			{
			case BulletPhase.Normal:
			{
				float num = BulletData.f_ANGLE;
				if (num == 0f)
				{
					num = 20f;
				}
				if ((bool)MasterTransform)
				{
					_transform.position = MasterTransform.position;
				}
				GameObject gameObject = base.transform.GetChild(0).gameObject;
				num *= 0.5f;
				float num2 = BulletData.f_DISTANCE / 2f;
				float num3 = num2;
				float num4 = Mathf.Tan(num * ((float)Math.PI / 180f));
				float num5 = num3 * num4;
				float num6 = num5;
				float num7 = num2;
				List<Transform> list = new List<Transform>();
				for (int k = 0; k < bulletcirclenum; k++)
				{
					int num8 = Physics2D.CircleCastNonAlloc(gameObject.transform.forward * num7 + gameObject.transform.position, num6, Vector2.zero, hits, float.PositiveInfinity, UseMask);
					for (int l = 0; l < num8; l++)
					{
						if (!list.Contains(hits[l].collider.transform))
						{
							list.Add(hits[l].collider.transform);
						}
					}
					num7 -= num6 * 1.5f;
					num3 -= num6 * 1.5f;
					num6 = num5 * (num3 / num2);
				}
				list.Sort(delegate(Transform t1, Transform t2)
				{
					float num9 = Vector3.Distance(t1.position, _transform.position);
					float value = Vector3.Distance(t2.position, _transform.position);
					return num9.CompareTo(value);
				});
				for (int m = 0; m < list.Count; m++)
				{
					if (!CheckHitList(ref _hitList, list[m]))
					{
						_hitList.Add(list[m]);
						CaluDmg(BulletData, list[m]);
					}
				}
				m_LastUpdateShowTime += Time.deltaTime;
				if (m_LastUpdateShowTime * 1000f >= (float)BulletData.n_FIRE_SPEED)
				{
					Phase = BulletPhase.Result;
				}
				break;
			}
			case BulletPhase.Result:
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
	}

	public override void BackToPool()
	{
		Phase = BulletPhase.Normal;
		_hitList.Clear();
		base.BackToPool();
	}
}
