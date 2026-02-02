using System;
using UnityEngine;

public class LineLinkBullet : BasicBullet
{
	public static int nFlagBuffLineLink = -10;

	protected IAimTarget _pLinkTarget;

	protected StageObjBase _pLinkTargetSOB;

	protected StageObjBase _pShoterSOB;

	protected bool _bShoterIsLocalPlayer;

	public override Vector3 GetCreateBulletPosition
	{
		get
		{
			if (_pLinkTargetSOB != null)
			{
				return _pLinkTargetSOB.AimPosition;
			}
			return _transform.position;
		}
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		bCanUseInEventBullet = false;
		LinkTarget(pTarget);
		PlayHitFx();
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		bCanUseInEventBullet = false;
		LinkTarget(pTarget);
		PlayHitFx();
	}

	protected void LinkTarget(IAimTarget pTarget)
	{
		if (pTarget != null)
		{
			_pLinkTarget = pTarget;
			_pLinkTargetSOB = _pLinkTarget as StageObjBase;
			if ((bool)_pLinkTargetSOB)
			{
				Vector3 vector = _pLinkTargetSOB.AimPosition - _pLinkTargetSOB.transform.position;
				CaluDmg(BulletData, _pLinkTargetSOB.transform, vector.x, vector.y);
			}
		}
		else
		{
			_pLinkTarget = null;
			_pLinkTargetSOB = null;
		}
		_bShoterIsLocalPlayer = false;
		if (refPBMShoter != null && refPBMShoter.SOB != null)
		{
			_pShoterSOB = refPBMShoter.SOB;
			OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
			if (orangeCharacter != null && orangeCharacter.IsLocalPlayer)
			{
				_bShoterIsLocalPlayer = true;
				refPBMShoter.AddBuff(nFlagBuffLineLink, 0, 0, 0, false, orangeCharacter.sPlayerID);
			}
		}
	}

	public override void LateUpdateFunc()
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || refPSShoter == null)
		{
			return;
		}
		bool flag = false;
		if (_pLinkTargetSOB != null && (int)_pLinkTargetSOB.Hp > 0 && _pShoterSOB != null && (int)_pShoterSOB.Hp > 0)
		{
			float distance = -1f;
			if (refPBMShoter.SOB != null && _pLinkTargetSOB != null)
			{
				distance = Vector2.Distance(refPBMShoter.SOB.AimTransform.position.xy(), _pLinkTargetSOB.AimTransform.position.xy());
			}
			flag = refPSShoter.LineSkillTrigger(distance, nWeaponCheck, refPBMShoter, _pLinkTargetSOB.selfBuffManager, base.CreateBulletDetail);
			if ((float)ActivateTimer.GetMillisecond() >= BulletData.f_EFFECT_Z * 1000f)
			{
				flag = true;
			}
			if (_pLinkTargetSOB.VanishStatus)
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (!_bShoterIsLocalPlayer && refPBMShoter != null && ActivateTimer.GetMillisecond() >= 200 && !refPBMShoter.CheckHasEffectByCONDITIONID(nFlagBuffLineLink))
		{
			flag = true;
		}
		if (flag)
		{
			BreakLine();
		}
	}

	protected virtual void BreakLine()
	{
		if (_bShoterIsLocalPlayer && refPBMShoter != null && refPBMShoter.CheckHasEffectByCONDITIONID(nFlagBuffLineLink))
		{
			refPBMShoter.RemoveBuffByCONDITIONID(nFlagBuffLineLink);
		}
		Stop();
		BackToPool();
	}

	public override void BackToPool()
	{
		base.BackToPool();
		_pLinkTarget = null;
		_pLinkTargetSOB = null;
		_pShoterSOB = null;
		_bShoterIsLocalPlayer = false;
	}

	private void PlayHitFx()
	{
		if (_pLinkTargetSOB != null && !ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(BulletData.s_HIT_FX))
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(BulletData.s_HIT_FX, _pLinkTargetSOB.AimPosition, Quaternion.identity, Array.Empty<object>());
		}
	}
}
