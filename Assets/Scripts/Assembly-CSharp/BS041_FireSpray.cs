using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BS041_FireSpray : SprayBullet
{
	protected override IEnumerator OnStartMove()
	{
		m_LastUpdateShowTime = Time.realtimeSinceStartup;
		Phase = BulletPhase.Normal;
		base.SoundSource.Initial(OrangeSSType.BOSS);
		base.SoundSource.UpdateDistanceCall();
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
				float num2 = BulletData.f_DISTANCE / 2f;
				float num3 = num2;
				float num4 = Mathf.Sin(num * ((float)Math.PI / 180f));
				float num5 = num3 * num4;
				float num6 = num5;
				float num7 = num2;
				List<Transform> list = new List<Transform>();
				for (int k = 0; k < bulletcirclenum; k++)
				{
					int num8 = Physics2D.CircleCastNonAlloc(-gameObject.transform.right * num7 + gameObject.transform.position, num6, Vector2.zero, hits, float.PositiveInfinity, UseMask);
					for (int l = 0; l < num8; l++)
					{
						if (!list.Contains(hits[l].collider.transform))
						{
							list.Add(hits[l].collider.transform);
						}
					}
					num3 -= num6;
					num3 /= num4 + 1f;
					num6 = num5 * (num3 / num2);
					num7 -= num6 * 2f;
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
				if ((Time.realtimeSinceStartup - m_LastUpdateShowTime) * 1000f >= (float)BulletData.n_FIRE_SPEED)
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

	public void OnDrawGizmos()
	{
		if (BulletData == null)
		{
			return;
		}
		Gizmos.color = Color.yellow;
		if (Phase == BulletPhase.Normal)
		{
			float num = BulletData.f_ANGLE;
			if (num == 0f)
			{
				num = 20f;
			}
			GameObject gameObject = base.transform.GetChild(0).gameObject;
			float num2 = BulletData.f_DISTANCE / 2f;
			float num3 = num2;
			float num4 = Mathf.Sin(num * ((float)Math.PI / 180f));
			float num5 = num3 * num4;
			float num6 = num5;
			float num7 = num2;
			for (int i = 0; i < bulletcirclenum; i++)
			{
				Gizmos.DrawSphere((Vector2)(-gameObject.transform.right * num7 + gameObject.transform.position), num6);
				num3 -= num6;
				num3 /= num4 + 1f;
				num6 = num5 * (num3 / num2);
				num7 -= num6 * 2f;
			}
		}
	}
}
