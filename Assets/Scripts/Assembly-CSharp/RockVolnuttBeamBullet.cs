using UnityEngine;

public class RockVolnuttBeamBullet : BeamBullet
{
	[SerializeField]
	private LineRenderer[] fxLineRenders;

	[SerializeField]
	private ParticleSystem fxParticleRoot;

	[SerializeField]
	private ParticleSystem fxParticleEnd;

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		fxParticleRoot.Play();
	}

	protected override void Update_Effect()
	{
		BoxCollider2D boxCollider2D = (BoxCollider2D)_hitCollider;
		float x = boxCollider2D.size.x;
		for (int i = 0; i < fxLineRenders.Length; i++)
		{
			fxLineRenders[i].SetPosition(1, new Vector3(0f - x, 0f, 0f));
		}
		fxParticleEnd.transform.localPosition = new Vector3(0f, 0f, x);
		boxCollider2D.offset = new Vector2(x * 0.5f, 0f);
		fxParticleRoot.Play();
	}
}
