using System;
using UnityEngine;

public class Boss01BallBullet : CollideBullet
{
	private ContactPoint2D[] hitAry;

	private int _swingType;

	private bool _effectGenerated;

	protected void OnCollisionEnter2D(Collision2D col)
	{
		if (IsActivate && _swingType == 0 && ((1 << col.gameObject.layer) & (int)BlockMask) != 0 && !_effectGenerated)
		{
			_effectGenerated = true;
			Vector2 point = col.contacts[0].point;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_maoh_000", point, Quaternion.identity, Array.Empty<object>());
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_maoh_rock", point, Quaternion.identity, Array.Empty<object>());
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0f, false);
			base.SoundSource.PlaySE("BossSE", 1);
		}
	}

	protected virtual void OnCollisionStay2D(Collision2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			return;
		}
		Transform root = _transform.root;
		if (!IsActivate || (root != MonoBehaviourSingleton<PoolManager>.Instance.transform && col.transform.IsChildOf(root)) || ((1 << col.gameObject.layer) & (int)TargetMask) == 0)
		{
			return;
		}
		StageObjParam component = col.collider.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null)
		{
			if (component.tLinkSOB.GetSOBType() == 4)
			{
				PetControllerBase component2 = col.collider.GetComponent<PetControllerBase>();
				if (component2 != null && component2.ignoreColliderBullet())
				{
					return;
				}
			}
			if ((int)component.tLinkSOB.Hp > 0)
			{
				Hit(col.collider);
			}
		}
		else
		{
			PlayerCollider component3 = col.collider.GetComponent<PlayerCollider>();
			if (component3 != null && component3.IsDmgReduceShield())
			{
				Hit(col.collider);
			}
			else if (col.gameObject.GetComponentInParent<StageHurtObj>() != null)
			{
				Hit(col.collider);
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_maoh_000", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_maoh_rock", 5);
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nInNetID, nDirection);
		_hitCollider.isTrigger = false;
	}

	public void Active(LayerMask pTargetMask, int swingType)
	{
		_swingType = swingType;
		_effectGenerated = false;
		Active(pTargetMask);
	}
}
