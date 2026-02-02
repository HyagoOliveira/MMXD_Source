using UnityEngine;

public class EM109_Controller : BS044_SubHeadController
{
	[SerializeField]
	private float moveSpeed = 1f;

	[SerializeField]
	private ParticleSystem iceFallParticle;

	[SerializeField]
	private ParticleSystem iceHitParticle;

	[SerializeField]
	private CollideBullet iceHitCollider;

	[SerializeField]
	private float skillDuration = 4f;

	[SerializeField]
	private Direction skillDirection;

	protected override void Awake()
	{
		base.Awake();
		iceHitParticle.Stop();
		iceFallParticle.Stop();
	}

	public override void SetActive(bool _isActive)
	{
		base.SetActive(_isActive);
		if (_isActive)
		{
			iceHitCollider.UpdateBulletData(EnemyWeapons[1].BulletData);
			iceHitCollider.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			iceHitParticle.Stop();
			iceFallParticle.Stop();
			ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				ParticleSystem.MainModule main = componentsInChildren[i].main;
				main.duration = skillDuration;
			}
		}
	}

	protected override void SetStatusOfSkill()
	{
		switch (subStatus)
		{
		case SubStatus.Phase0:
			_velocity = VInt3.zero;
			SetFacingDirection(skillDirection);
			break;
		case SubStatus.Phase1:
			iceFallParticle.Play();
			break;
		case SubStatus.Phase2:
			CalibrateIceHitPosition();
			iceHitParticle.Play();
			iceHitCollider.Active(targetMask);
			_velocity = new VInt3(modelTransform.forward * moveSpeed);
			break;
		case SubStatus.Phase3:
			iceHitCollider.BackToPool();
			_velocity = VInt3.zero;
			break;
		}
	}

	protected override void UpdateStatusOfSkill()
	{
		switch (subStatus)
		{
		case SubStatus.Phase0:
			if (modelTransform.eulerAngles.y == currentDirection)
			{
				SetStatus(mainStatus, SubStatus.Phase1);
			}
			break;
		case SubStatus.Phase1:
			if (AiTimer.GetMillisecond() > 500)
			{
				SetStatus(MainStatus.Skill, SubStatus.Phase2);
			}
			break;
		case SubStatus.Phase2:
			if ((float)AiTimer.GetMillisecond() > skillDuration * 1000f)
			{
				SetStatus(MainStatus.Skill, SubStatus.Phase3);
			}
			break;
		case SubStatus.Phase3:
			if (AiTimer.GetMillisecond() > 1000)
			{
				iceFallParticle.Stop();
				iceHitParticle.Stop();
				RegisterStatus(MainStatus.Idle);
			}
			break;
		}
	}

	protected void CalibrateIceHitPosition()
	{
		blockCollider.enabled = false;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(shootPointTransform.position, Vector3.down, 100f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		if (raycastHit2D.transform != null)
		{
			Vector3 position = new Vector3(shootPointTransform.position.x + shootPointTransform.right.x * Vector2.Distance(shootPointTransform.position, raycastHit2D.point) * 0.7f, raycastHit2D.point.y + 0.5f, shootPointTransform.position.z);
			iceHitParticle.transform.position = position;
			iceHitCollider._transform.position = position;
		}
		blockCollider.enabled = true;
	}
}
