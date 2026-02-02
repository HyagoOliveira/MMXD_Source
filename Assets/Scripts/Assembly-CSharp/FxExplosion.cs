using UnityEngine;

public class FxExplosion : FxBase
{
	[SerializeField]
	private new ParticleSystem ps;

	public override void Active(params object[] p_params)
	{
		ParticleSystem.MainModule main = ps.main;
		main.startSize = (float)p_params[0];
		base.Active(p_params);
	}
}
