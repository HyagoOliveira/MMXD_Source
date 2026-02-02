using UnityEngine;

public class BS011_FireBallController : BS011_PartsController
{
	private ParticleSystem _fireballParticle;

	[SerializeField]
	public Transform root;

	public override void Awake()
	{
		base.Awake();
		_fireballParticle = root.GetComponent<ParticleSystem>();
		_fireballParticle.Pause(true);
	}

	public void Start()
	{
	}

	public override int SetVisible(bool visible = true)
	{
		base.SetVisible(visible);
		if (visible)
		{
			_fireballParticle.Play(true);
			OrangeBattleUtility.ChangeLayersRecursively<Transform>(root, ManagedSingleton<OrangeLayerManager>.Instance.RenderSPEnemy);
		}
		else
		{
			_fireballParticle.Pause(true);
			OrangeBattleUtility.ChangeLayersRecursively<Transform>(root, ManagedSingleton<OrangeLayerManager>.Instance.DefaultLayer);
		}
		return -1;
	}

	public override int SetDestroy()
	{
		base.SetDestroy();
		MasterController.VanishStatus = true;
		_fireballParticle.Stop(true);
		return -1;
	}
}
