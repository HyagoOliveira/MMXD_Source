using UnityEngine;

namespace DigitalRuby.RainMaker
{
	public class RainScript2D : BaseRainScript
	{
		private static readonly Color32 explosionColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		private float cameraMultiplier = 1f;

		private Bounds visibleBounds;

		public Vector3 min;

		public Vector3 max;

		private float yOffset;

		private float visibleWorldWidth;

		private float initialEmissionRain;

		private Vector2 initialStartSpeedRain;

		private Vector2 initialStartSizeRain;

		private Vector2 initialStartSpeedMist;

		private Vector2 initialStartSizeMist;

		private Vector2 initialStartSpeedExplosion;

		private Vector2 initialStartSizeExplosion;

		private readonly ParticleSystem.Particle[] particles = new ParticleSystem.Particle[2048];

		[Tooltip("The starting y offset for rain and mist. This will be offset as a percentage of visible height from the top of the visible world.")]
		public float RainHeightMultiplier;

		[Tooltip("The total width of the rain and mist as a percentage of visible width")]
		public float RainWidthMultiplier = 1.5f;

		[Tooltip("Collision mask for the rain particles")]
		public LayerMask CollisionMask = -1;

		[Tooltip("Lifetime to assign to rain particles that have collided. 0 for instant death. This can allow the rain to penetrate a little bit beyond the collision point.")]
		[Range(0f, 0.5f)]
		public float CollisionLifeTimeRain = 0.02f;

		[Tooltip("Multiply the velocity of any mist colliding by this amount")]
		[Range(0f, 0.99f)]
		public float RainMistCollisionMultiplier = 0.75f;

		protected override bool UseRainMistSoftParticles
		{
			get
			{
				return false;
			}
		}

		private void EmitExplosion(ref Vector3 pos)
		{
			for (int num = Random.Range(2, 5); num != 0; num--)
			{
				float x = Random.Range(-2f, 2f) * cameraMultiplier;
				float y = Random.Range(1f, 3f) * cameraMultiplier;
				float startLifetime = Random.Range(0.1f, 0.2f);
				float startSize = Random.Range(0.05f, 0.1f) * cameraMultiplier;
				ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
				emitParams.position = pos;
				emitParams.velocity = new Vector3(x, y, 0f);
				emitParams.startLifetime = startLifetime;
				emitParams.startSize = startSize;
				emitParams.startColor = explosionColor;
				RainExplosionParticleSystem.Emit(emitParams, 1);
			}
		}

		private void TransformParticleSystem(ParticleSystem p, Vector2 initialStartSpeed, Vector2 initialStartSize)
		{
			if (!(p == null))
			{
				if (FollowCamera)
				{
					p.transform.position = new Vector3(Camera.transform.position.x, visibleBounds.max.y + yOffset, p.transform.position.z);
				}
				else
				{
					p.transform.position = new Vector3(p.transform.position.x, visibleBounds.max.y + yOffset, p.transform.position.z);
				}
				p.transform.localScale = new Vector3(visibleWorldWidth * RainWidthMultiplier, 1f, 1f);
				ParticleSystem.MainModule main = p.main;
				ParticleSystem.MinMaxCurve startSpeed = main.startSpeed;
				ParticleSystem.MinMaxCurve startSize = main.startSize;
				startSpeed.constantMin = initialStartSpeed.x * cameraMultiplier;
				startSpeed.constantMax = initialStartSpeed.y * cameraMultiplier;
				startSize.constantMin = initialStartSize.x * cameraMultiplier;
				startSize.constantMax = initialStartSize.y * cameraMultiplier;
				main.startSpeed = startSpeed;
				main.startSize = startSize;
			}
		}

		protected override void Start()
		{
			base.Start();
			initialEmissionRain = RainFallParticleSystem.emission.rateOverTime.constant;
			initialStartSpeedRain = new Vector2(RainFallParticleSystem.main.startSpeed.constantMin, RainFallParticleSystem.main.startSpeed.constantMax);
			initialStartSizeRain = new Vector2(RainFallParticleSystem.main.startSize.constantMin, RainFallParticleSystem.main.startSize.constantMax);
			if (RainMistParticleSystem != null)
			{
				initialStartSpeedMist = new Vector2(RainMistParticleSystem.main.startSpeed.constantMin, RainMistParticleSystem.main.startSpeed.constantMax);
				initialStartSizeMist = new Vector2(RainMistParticleSystem.main.startSize.constantMin, RainMistParticleSystem.main.startSize.constantMax);
			}
			if (RainExplosionParticleSystem != null)
			{
				initialStartSpeedExplosion = new Vector2(RainExplosionParticleSystem.main.startSpeed.constantMin, RainExplosionParticleSystem.main.startSpeed.constantMax);
				initialStartSizeExplosion = new Vector2(RainExplosionParticleSystem.main.startSize.constantMin, RainExplosionParticleSystem.main.startSize.constantMax);
			}
		}

		protected override void Update()
		{
			base.Update();
			cameraMultiplier = Camera.orthographicSize * 0.25f;
			visibleBounds.min = min;
			visibleBounds.max = new Vector3(max.x, MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position.y + 4.5f, max.z);
			visibleWorldWidth = visibleBounds.size.x;
			yOffset = (visibleBounds.max.y - visibleBounds.min.y) * RainHeightMultiplier;
			TransformParticleSystem(RainFallParticleSystem, initialStartSpeedRain, initialStartSizeRain);
			TransformParticleSystem(RainMistParticleSystem, initialStartSpeedMist, initialStartSizeMist);
			TransformParticleSystem(RainExplosionParticleSystem, initialStartSpeedExplosion, initialStartSizeExplosion);
		}

		protected override float RainFallEmissionRate()
		{
			return initialEmissionRain * RainIntensity;
		}
	}
}
