using UnityEngine;

[ExecuteInEditMode]
public class WFX_Turbulence : MonoBehaviour
{
	public enum MoveMethodEnum
	{
		Position = 0,
		Velocity = 1,
		RelativePositionHalf = 2,
		RelativePosition = 3
	}

	public enum PerfomanceEnum
	{
		UltraRealTime = 0,
		High = 1,
		Low = 2
	}

	public float TurbulenceStrenght = 1f;

	public bool TurbulenceByTime;

	public AnimationCurve TurbulenceStrengthByTime = AnimationCurve.EaseInOut(1f, 1f, 1f, 1f);

	public Vector3 Frequency = new Vector3(1f, 1f, 1f);

	public Vector3 OffsetSpeed = new Vector3(0.5f, 0.5f, 0.5f);

	public Vector3 Amplitude = new Vector3(5f, 5f, 5f);

	public Vector3 GlobalForce;

	public MoveMethodEnum MoveMethod;

	public PerfomanceEnum Perfomance;

	private float lastStopTime;

	private Vector3 currentOffset;

	private float deltaTime;

	private float deltaTimeLastUpdateOffset;

	private ParticleSystem.Particle[] particleArray;

	private ParticleSystem particleSys;

	private float time;

	private int currentSplit;

	private float fpsTime;

	private int FPS;

	private int splitUpdate;

	private PerfomanceEnum perfomanceOldSettings;

	private bool skipFrame;

	private void Start()
	{
		particleSys = GetComponent<ParticleSystem>();
		if (particleArray == null || particleArray.Length < particleSys.main.maxParticles)
		{
			particleArray = new ParticleSystem.Particle[particleSys.main.maxParticles];
		}
		perfomanceOldSettings = Perfomance;
		UpdatePerfomanceSettings();
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			deltaTime = Time.realtimeSinceStartup - lastStopTime;
			lastStopTime = Time.realtimeSinceStartup;
			UpdateTurbulence();
			return;
		}
		deltaTime = Time.deltaTime;
		currentOffset += OffsetSpeed * deltaTime;
		if (Perfomance != perfomanceOldSettings)
		{
			perfomanceOldSettings = Perfomance;
			UpdatePerfomanceSettings();
		}
		time += deltaTime;
		if (FPS == 0)
		{
			UpdateTurbulence();
		}
		else if (QualitySettings.vSyncCount == 2)
		{
			UpdateTurbulence();
		}
		else if (QualitySettings.vSyncCount == 1)
		{
			if (Perfomance == PerfomanceEnum.Low)
			{
				if (skipFrame)
				{
					UpdateTurbulence();
				}
				skipFrame = !skipFrame;
			}
			if (Perfomance == PerfomanceEnum.High)
			{
				UpdateTurbulence();
			}
		}
		else if (QualitySettings.vSyncCount == 0)
		{
			if (time >= fpsTime)
			{
				time = 0f;
				UpdateTurbulence();
				deltaTimeLastUpdateOffset = 0f;
			}
			else
			{
				deltaTimeLastUpdateOffset += deltaTime;
			}
		}
	}

	private void UpdatePerfomanceSettings()
	{
		if (Perfomance == PerfomanceEnum.UltraRealTime)
		{
			FPS = 0;
			splitUpdate = 2;
		}
		if (Perfomance == PerfomanceEnum.High)
		{
			FPS = 60;
			splitUpdate = 2;
		}
		if (Perfomance == PerfomanceEnum.Low)
		{
			FPS = 30;
			splitUpdate = 2;
		}
		fpsTime = 1f / (float)FPS;
	}

	private void UpdateTurbulence()
	{
		int particles = particleSys.GetParticles(particleArray);
		int num = 1;
		int num2;
		int num3;
		if (splitUpdate > 1)
		{
			num2 = particles / splitUpdate * currentSplit;
			num3 = particles / splitUpdate * (currentSplit + 1);
			num = splitUpdate;
		}
		else
		{
			num2 = 0;
			num3 = particles;
		}
		for (int i = num2; i < num3; i++)
		{
			ParticleSystem.Particle particle = particleArray[i];
			float num4 = 1f;
			if (TurbulenceByTime)
			{
				num4 = TurbulenceStrengthByTime.Evaluate(1f - particle.remainingLifetime / particle.startLifetime);
			}
			Vector3 position = particle.position;
			position.x /= Frequency.x;
			position.y /= Frequency.y;
			position.z /= Frequency.z;
			Vector3 vector = default(Vector3);
			float num5 = deltaTime + deltaTimeLastUpdateOffset;
			vector.x = ((Mathf.PerlinNoise(position.z - currentOffset.z, position.y - currentOffset.y) * 2f - 1f) * Amplitude.x + GlobalForce.x) * num5;
			vector.y = ((Mathf.PerlinNoise(position.x - currentOffset.x, position.z - currentOffset.z) * 2f - 1f) * Amplitude.y + GlobalForce.y) * num5;
			vector.z = ((Mathf.PerlinNoise(position.y - currentOffset.y, position.x - currentOffset.x) * 2f - 1f) * Amplitude.z + GlobalForce.z) * num5;
			vector *= TurbulenceStrenght * num4 * (float)num;
			if (MoveMethod == MoveMethodEnum.Position)
			{
				particleArray[i].position += vector;
			}
			if (MoveMethod == MoveMethodEnum.Velocity)
			{
				particleArray[i].velocity += vector;
			}
			if (MoveMethod == MoveMethodEnum.RelativePositionHalf)
			{
				particleArray[i].position += vector;
				particleArray[i].velocity = particleArray[i].velocity * 0.5f + vector.normalized / 10f;
			}
			if (MoveMethod == MoveMethodEnum.RelativePosition)
			{
				particleArray[i].position += vector;
				particleArray[i].velocity = particleArray[i].velocity * 0.9f + vector.normalized / 10f;
			}
		}
		particleSys.SetParticles(particleArray, particles);
		currentSplit++;
		if (currentSplit >= splitUpdate)
		{
			currentSplit = 0;
		}
	}
}
