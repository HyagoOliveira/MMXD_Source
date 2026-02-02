using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class MeltingObject : MonoBehaviour
{
	public MeshRenderer m_MeltingObjectRenderer;

	public ParticleSystemRenderer m_ParticlesRenderer;

	[Space(20f)]
	public AnimationCurve m_CutoffValue = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	public AnimationCurve m_CollapseStrength = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	public AnimationCurve m_EmissionStrength = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1f);

	private Vector4[] m_ParticlePositions = new Vector4[8];

	private float[] m_ParticleSizes = new float[8];

	private float[] m_ParticleAlpha = new float[8];

	private float[] m_ParticleRed = new float[8];

	private ParticleSystem m_System;

	private ParticleSystem.Particle[] m_Particles;

	private void LateUpdate()
	{
		InitializeIfNeeded();
		int particles = m_System.GetParticles(m_Particles);
		for (int i = 0; i < 8; i++)
		{
			m_ParticlePositions[i].x = m_Particles[i].position.x;
			m_ParticlePositions[i].y = m_Particles[i].position.y;
			m_ParticlePositions[i].z = m_Particles[i].position.z;
			m_ParticleSizes[i] = m_Particles[i].GetCurrentSize(m_System);
			m_ParticleAlpha[i] = (float)(int)m_Particles[i].GetCurrentColor(m_System).a / 255f;
			m_ParticleRed[i] = (float)(int)m_Particles[i].GetCurrentColor(m_System).r / 255f;
		}
		if ((bool)m_MeltingObjectRenderer)
		{
			m_MeltingObjectRenderer.sharedMaterial.SetVectorArray("_ParticlePositions", m_ParticlePositions);
			m_MeltingObjectRenderer.sharedMaterial.SetFloatArray("_ParticleSizes", m_ParticleSizes);
			m_MeltingObjectRenderer.sharedMaterial.SetFloatArray("_ParticleAlpha", m_ParticleAlpha);
			m_MeltingObjectRenderer.sharedMaterial.SetFloatArray("_ParticleRed", m_ParticleRed);
		}
		m_System.SetParticles(m_Particles, particles);
		m_MeltingObjectRenderer.sharedMaterial.SetFloat("_CutoffThreshold", m_CutoffValue.Evaluate(m_System.time / m_System.main.duration));
		m_MeltingObjectRenderer.sharedMaterial.SetFloat("_CollapseStrength", m_CollapseStrength.Evaluate(m_System.time / m_System.main.duration));
		m_MeltingObjectRenderer.sharedMaterial.SetFloat("_EmissionStrength", m_EmissionStrength.Evaluate(m_System.time / m_System.main.duration));
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
