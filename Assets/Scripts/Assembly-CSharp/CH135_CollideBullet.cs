using UnityEngine;

public sealed class CH135_CollideBullet : CollideBullet
{
	[SerializeField]
	public string[] DelaySE = new string[2] { "", "" };

	[SerializeField]
	public float delayTime;

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		if ((bool)base.SoundSource)
		{
			base.SoundSource.PlaySE(DelaySE[0], DelaySE[1], delayTime);
		}
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		if ((bool)base.SoundSource)
		{
			base.SoundSource.PlaySE(DelaySE[0], DelaySE[1], delayTime);
		}
	}
}
