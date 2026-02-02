using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class ModuleDrag : MonoBehaviour
{
	public float m_Drag = 1f;

	public float m_Mass = 1f;

	public bool m_AngularDrag;

	private ParticleSystem m_System;

	private ParticleSystem.Particle[] m_Particles;

	private void LateUpdate()
	{
		InitializeIfNeeded();
		int particles = m_System.GetParticles(m_Particles);
		for (int i = 0; i < particles; i++)
		{
			float num = 1f - m_Drag * Time.deltaTime / m_Mass;
			m_Particles[i].velocity *= num;
			if (m_AngularDrag)
			{
				m_Particles[i].rotation3D *= num;
			}
		}
		m_System.SetParticles(m_Particles, particles);
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
}
