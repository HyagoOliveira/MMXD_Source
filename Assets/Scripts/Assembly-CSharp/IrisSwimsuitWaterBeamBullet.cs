using UnityEngine;

public class IrisSwimsuitWaterBeamBullet : BeamBullet
{
	[SerializeField]
	private LineRenderer fxLineA;

	[SerializeField]
	private LineRenderer fxLineB;

	[SerializeField]
	private LineRenderer fxLineRun;

	[SerializeField]
	private LineRenderer fxLineBit;

	[SerializeField]
	private ParticleSystem fxMiddleWater;

	[SerializeField]
	private ParticleSystem fxWRim;

	[SerializeField]
	private ParticleSystem fxW1_1;

	[SerializeField]
	private ParticleSystem fxW1_2;

	[SerializeField]
	private ParticleSystem fxW1_3;

	[SerializeField]
	private ParticleSystem fxW2_1;

	[SerializeField]
	private ParticleSystem fxW2_2;

	protected override void Update_Effect()
	{
		float x = ((BoxCollider2D)_hitCollider).size.x;
		fxEndpoint.localPosition = new Vector3(0f, 0f, x);
		float num = 0f - x + 0.63f;
		float num2 = num * 0.5f;
		fxLineA.SetPosition(1, new Vector3(num, 0f, 0f));
		fxLineB.SetPosition(1, new Vector3(num, 0f, 0f));
		fxLineRun.transform.localPosition = new Vector3(0f, 0f, 0f - num2);
		fxLineRun.SetPosition(0, new Vector3(0f - num2, 0f, 0f));
		fxLineRun.SetPosition(1, new Vector3(num2, 0f, 0f));
		fxLineBit.transform.localPosition = new Vector3(0f, 0f, 0f - num2);
		fxLineBit.SetPosition(0, new Vector3(num2, 0f, 0f));
		fxLineBit.SetPosition(1, new Vector3(0f - num2, 0f, 0f));
		fxMiddleWater.transform.localPosition = new Vector3(0f, 0f, -0.05f);
		ParticleSystem.MainModule main = fxMiddleWater.main;
		main.startSizeXMultiplier = 1f;
		main.startSizeYMultiplier = x * 0.1f;
		main.startSizeZMultiplier = 1f;
		fxWRim.transform.localPosition = new Vector3(0f, 0f, (x + 0.42f) * 0.5f);
		ParticleSystem.ShapeModule shape = fxWRim.shape;
		shape.radius = Mathf.Abs(num2 * 0.05f);
		fxW1_3.transform.localPosition = new Vector3(0f, 0f, -0.12f);
		SetVelocityOverLifetimeModule(ref fxW1_3, new Vector3(num2, 0f, 0f));
		fxW1_2.transform.localPosition = new Vector3(0f, 0f, -0.12f - num2 * 0.5f);
		SetVelocityOverLifetimeModule(ref fxW1_2, new Vector3(num2, 0f, 0f));
		fxW1_1.transform.localPosition = new Vector3(0f, 0f, -0.12f - num2 * 1.5f);
		SetVelocityOverLifetimeModule(ref fxW1_3, new Vector3(num2, 0f, 0f));
		fxW2_2.transform.localPosition = new Vector3(0f, 0f, 0.84f);
		SetVelocityOverLifetimeModule(ref fxW2_2, new Vector3(0f, num2 * 3f / 4f, 0f));
		fxW2_1.transform.localPosition = new Vector3(0f, 0f, 0.84f - num2);
		SetVelocityOverLifetimeModule(ref fxW2_1, new Vector3(0f, num2 * 3f / 4f, 0f));
	}

	private void SetVelocityOverLifetimeModule(ref ParticleSystem ps, Vector3 value)
	{
		ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = ps.velocityOverLifetime;
		ParticleSystem.MinMaxCurve x = new ParticleSystem.MinMaxCurve(value.x);
		ParticleSystem.MinMaxCurve y = new ParticleSystem.MinMaxCurve(value.y);
		ParticleSystem.MinMaxCurve z = new ParticleSystem.MinMaxCurve(value.z);
		velocityOverLifetime.x = x;
		velocityOverLifetime.y = y;
		velocityOverLifetime.z = z;
	}
}
