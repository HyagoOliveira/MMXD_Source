using UnityEngine;

public class CH066_CatchKobunBullet : CH066_BombKobunBullet
{
	protected override void HashAnimation()
	{
		_animationHash = new int[1];
		_animationHash[0] = Animator.StringToHash("BS067@skill_4_step1_loop");
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Hp = MaxHp;
		MasterTransform = pTransform;
		_transform.position = pTransform.position;
		_controller.LogicPosition = new VInt3(_transform.localPosition);
		Direction = pDirection;
		Velocity = BulletData.n_SPEED * pDirection;
		UpdateParticleRotationDirection(Velocity.x > 0f);
		_moveDistance = 0f;
		TargetMask = pTargetMask;
		UseMask = (int)BlockMask | (int)pTargetMask;
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		SetAnimation();
		SetEmotions(4);
		SetWeapon(99);
		if (CoroutineMove != null)
		{
			StopCoroutine(CoroutineMove);
		}
		CoroutineMove = StartCoroutine(OnStartMove());
		base.SoundSource.UpdateDistanceCall();
		PlayUseSE();
		needWeaponImpactSE = true;
	}
}
