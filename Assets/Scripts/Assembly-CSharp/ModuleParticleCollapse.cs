using UnityEngine;

[ExecuteInEditMode]
public class ModuleParticleCollapse : MonoBehaviour
{
	private int _CollapseStrength = Shader.PropertyToID("_CollapseStrength");

	public ParticleSystemRenderer m_ParticlesRenderer;

	public AnimationCurve m_CollapseStrength = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	private ParticleSystem m_System;

	private void LateUpdate()
	{
		if (m_System == null)
		{
			m_System = GetComponent<ParticleSystem>();
		}
		if ((bool)m_ParticlesRenderer && (bool)m_System)
		{
			m_ParticlesRenderer.sharedMaterial.SetFloat(_CollapseStrength, m_CollapseStrength.Evaluate(m_System.time / m_System.main.duration));
		}
	}
}
