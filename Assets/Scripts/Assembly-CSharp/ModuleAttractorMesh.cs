using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class ModuleAttractorMesh : MonoBehaviour
{
	public bool m_AttractionPointsGizmos;

	[Space(20f)]
	public SkinnedMeshRenderer m_MeshAttractor;

	private Vector3[] m_AttractionPoints;

	private Vector3 m_AttractorPreviousPosition;

	[Range(0f, 5f)]
	public float m_PointSpread = 0.2f;

	[Range(0f, 10f)]
	public int m_MaximumScans = 5;

	private float m_PointSpreadPrevious;

	private int m_MaximumScansPrevious;

	[Space(20f)]
	public AnimationCurve m_Falloff = AnimationCurve.EaseInOut(0f, 10f, 5f, 0f);

	public AnimationCurve m_ForceOverEmitterDuration = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public Vector2 m_ForceRange = new Vector2(0.5f, 1f);

	[Space(20f)]
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
		m_AttractionPoints = new Vector3[0];
		m_PointSpreadPrevious = m_PointSpread;
		m_MaximumScansPrevious = m_MaximumScans;
		if ((bool)m_MeshAttractor)
		{
			m_AttractorPreviousPosition = m_MeshAttractor.transform.position;
		}
	}

	private void FillAttractionPoints()
	{
		Random.State state = Random.state;
		Random.InitState(m_RandomSeed);
		if ((bool)m_MeshAttractor)
		{
			List<Vector3> list = new List<Vector3>();
			m_MeshAttractor.sharedMesh.GetVertices(list);
			for (int i = 0; i < m_AttractionPoints.Length; i++)
			{
				int num = Random.Range(0, list.Count);
				m_AttractionPoints[i] = m_MeshAttractor.transform.TransformPoint(list[num]);
				if (i <= 0 || m_MaximumScans <= 0)
				{
					continue;
				}
				int num2 = Mathf.Clamp(i - m_MaximumScans, 0, m_MaximumScans);
				int num3 = num;
				int num4 = num;
				if (num2 > 0)
				{
					for (int j = 0; j < num2; j++)
					{
						num = Random.Range(0, list.Count);
						float num5 = Vector3.Distance(m_MeshAttractor.transform.TransformPoint(list[num]), m_AttractionPoints[i - 1]);
						float num6 = 0.1f;
						if (num5 > m_PointSpread && num5 < m_PointSpread + num6)
						{
							num3 = num;
						}
					}
				}
				if (num3 != num4)
				{
					m_AttractionPoints[i] = m_MeshAttractor.transform.TransformPoint(list[num3]);
				}
			}
		}
		Random.state = state;
		m_CurrentSeed = m_RandomSeed;
		m_AttractorPreviousPosition = m_MeshAttractor.transform.position;
		m_PointSpreadPrevious = m_PointSpread;
		m_MaximumScansPrevious = m_MaximumScans;
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
		if ((bool)m_MeshAttractor)
		{
			InitializeIfNeeded();
			if (m_ForceArray.Length < m_System.main.maxParticles)
			{
				m_ForceArray = new float[m_System.main.maxParticles];
				FillForceArray();
			}
			if (m_AttractionPoints.Length < m_System.main.maxParticles)
			{
				m_AttractionPoints = new Vector3[m_System.main.maxParticles];
				FillAttractionPoints();
			}
			if (m_MeshAttractor.transform.position != m_AttractorPreviousPosition)
			{
				FillAttractionPoints();
			}
			if (m_PointSpreadPrevious != m_PointSpread)
			{
				FillAttractionPoints();
			}
			if (m_MaximumScansPrevious != m_MaximumScans)
			{
				FillAttractionPoints();
			}
			if (m_CurrentSeed != m_RandomSeed)
			{
				FillForceArray();
				FillAttractionPoints();
			}
			if (m_CurrentForceRange != m_ForceRange)
			{
				m_CurrentForceRange = m_ForceRange;
				FillForceArray();
			}
			int particles = m_System.GetParticles(m_Particles);
			for (int i = 0; i < particles; i++)
			{
				float value = Vector3.Distance(m_AttractionPoints[i], m_Particles[i].position);
				value = Mathf.Clamp(value, 0f, m_Falloff[m_Falloff.length - 1].value);
				Vector3 vector = (m_AttractionPoints[i] - m_Particles[i].position) * m_Falloff.Evaluate(value) * Time.deltaTime;
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
		if (!m_MeshAttractor)
		{
			return;
		}
		Gizmos.DrawWireSphere(m_MeshAttractor.transform.position, m_Falloff[m_Falloff.length - 1].time);
		if (m_AttractionPointsGizmos)
		{
			float radius = 0.025f;
			Gizmos.color = Color.red;
			for (int i = 0; i < m_AttractionPoints.Length; i++)
			{
				Gizmos.DrawSphere(m_AttractionPoints[i], radius);
			}
		}
	}
}
