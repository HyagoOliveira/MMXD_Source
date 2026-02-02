using UnityEngine;

public class CH109_RibbonBullet : LineLinkBullet
{
	protected float defLinePos0X = -2f;

	protected Vector3 oldEndPos;

	protected bool bBreakLink;

	[SerializeField]
	private Transform fxStartPoint;

	[SerializeField]
	private Transform fxEndpoint;

	[SerializeField]
	private LineRenderer fxLine;

	[SerializeField]
	private string[] BreakSE;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		bBreakLink = false;
		Update_Effect(true);
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		bBreakLink = false;
		Update_Effect(true);
	}

	public override void TargetAimPositionOnActive(Vector3 targetAimPosition)
	{
		oldEndPos = targetAimPosition;
	}

	protected void Update_Effect(bool bInit = false)
	{
		if (_pShoterSOB == null)
		{
			return;
		}
		_transform.position = _pShoterSOB.AimPosition;
		_transform.rotation = Quaternion.identity;
		if (_pLinkTargetSOB != null)
		{
			if (bInit || (int)_pLinkTargetSOB.Hp > 0)
			{
				fxEndpoint.position = _pLinkTargetSOB.AimPosition;
				oldEndPos = _pLinkTargetSOB.AimPosition;
			}
			else
			{
				fxEndpoint.position = oldEndPos;
			}
		}
		else
		{
			fxEndpoint.position = oldEndPos;
		}
		Vector3 lineRender = fxEndpoint.position - _pShoterSOB.AimPosition;
		SetLineRender(lineRender);
	}

	protected void SetLineRender(Vector3 distance)
	{
		Vector3 position = distance;
		position.x = (position.x + defLinePos0X) * -1f;
		position.z *= -1f;
		fxLine.SetPosition(0, position);
	}

	protected void Update()
	{
		if (!bIsEnd && (_pLinkTargetSOB == null || (int)_pLinkTargetSOB.Hp <= 0 || _pShoterSOB == null || (int)_pShoterSOB.Hp <= 0))
		{
			BreakLine();
		}
	}

	public override void LateUpdateFunc()
	{
		base.LateUpdateFunc();
		if (!bIsEnd && !bBreakLink)
		{
			Update_Effect();
		}
	}

	protected override void BreakLine()
	{
		bBreakLink = true;
		if (ActivateTimer.GetMillisecond() >= 500)
		{
			if (_bShoterIsLocalPlayer && refPBMShoter != null && refPBMShoter.CheckHasEffectByCONDITIONID(LineLinkBullet.nFlagBuffLineLink))
			{
				refPBMShoter.RemoveBuffByCONDITIONID(LineLinkBullet.nFlagBuffLineLink);
			}
			if (BreakSE.Length >= 2)
			{
				PlaySE(BreakSE[0], BreakSE[1]);
			}
			Stop();
			BackToPool();
		}
	}
}
