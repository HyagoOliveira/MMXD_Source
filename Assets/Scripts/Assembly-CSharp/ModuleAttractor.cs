using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class ModuleAttractor : MonoBehaviour
{
	public AnimationCurve m_Falloff = AnimationCurve.EaseInOut(0f, 10f, 5f, 0f);

	public AnimationCurve m_ForceOverEmitterDuration = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public Transform m_Attractor;

	[Space(20f)]
	public Vector2 m_ForceRange = new Vector2(0.5f, 1f);

	public int m_RandomSeed = 10;

	private int m_CurrentSeed = 10;

	private Vector2 m_CurrentForceRange = new Vector2(0f, 0f);

	private float[] m_ForceArray = new float[0];

	private ParticleSystem m_System;

	private ParticleSystem.Particle[] m_Particles;

	private void Start()
	{
		m_ForceArray = new float[0];
		m_CurrentForceRange = m_ForceRange;
	}

	private void FillForceArray()
	{
		Random.State state = Random.state;
		Random.InitState(m_RandomSeed);
		for (int i = 0; i < m_ForceArray.Length; i++)
		{
			m_ForceArray[i] = Random.Range(m_ForceRange.x, m_ForceRange.y);
		}
		Random.state = state;
		m_CurrentSeed = m_RandomSeed;
	}

	private void LateUpdate()
	{
		if ((bool)m_Attractor)
		{
			InitializeIfNeeded();
			if (m_ForceArray.Length < m_System.main.maxParticles)
			{
				m_ForceArray = new float[m_System.main.maxParticles];
				FillForceArray();
			}
			if (m_CurrentSeed != m_RandomSeed)
			{
				FillForceArray();
			}
			if (m_CurrentForceRange != m_ForceRange)
			{
				m_CurrentForceRange = m_ForceRange;
				FillForceArray();
			}
			int particles = m_System.GetParticles(m_Particles);
			for (int i = 0; i < particles; i++)
			{
				float value = Vector3.Distance(m_Attractor.transform.position, m_Particles[i].position);
				value = Mathf.Clamp(value, 0f, m_Falloff[m_Falloff.length - 1].value);
				Vector3 vector = (m_Attractor.transform.position - m_Particles[i].position) * m_Falloff.Evaluate(value) * Time.deltaTime;
				vector *= m_ForceOverEmitterDuration.Evaluate(m_System.time) * m_ForceArray[i];
				m_Particles[i].velocity += vector;
			}
			m_System.SetParticles(m_Particles, particles);
		}
	}

	private void InitializeIfNeeded()
	{
		if (m_System == null)
		{
			m_System = GetComponent<ParticleSystem>();
		}
		if (m_Particles == null || m_Particles.Length < m_System.main.maxParticles)
		{
			m_Particles = new ParticleSystem.Particle[m_System.main.maxParticles];
		}
	}

	private void OnDrawGizmosSelected()
	{
		if ((bool)m_Attractor)
		{
			Gizmos.DrawWireSphere(m_Attractor.transform.position, m_Falloff[m_Falloff.length - 1].time);
		}
	}
}
