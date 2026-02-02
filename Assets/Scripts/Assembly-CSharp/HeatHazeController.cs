using System.Collections;
using UnityEngine;

public class HeatHazeController : MonoBehaviour
{
	[SerializeField]
	private bool m_runtimeVisibilityCheck;

	private CameraControl m_cameraControl;

	private ParticleSystemRenderer m_particleSystemRenderer;

	private void Start()
	{
		m_particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
		Material material = m_particleSystemRenderer.material;
		if (!material)
		{
			return;
		}
		m_cameraControl = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.GetComponent<CameraControl>();
		if ((bool)m_cameraControl)
		{
			material.SetTexture("_MainTex", m_cameraControl.GetScreenRenderTarget());
			StartCoroutine(HideEffectBehindPlayerCoroutine());
			if (m_runtimeVisibilityCheck)
			{
				InvokeRepeating("HideEffectBehindPlayer", 0f, 1f);
			}
		}
	}

	private IEnumerator HideEffectBehindPlayerCoroutine()
	{
		while (m_cameraControl.Target == null)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		HideEffectBehindPlayer();
	}

	private void HideEffectBehindPlayer()
	{
		if ((bool)m_cameraControl.Target)
		{
			if (base.transform.position.z > m_cameraControl.Target.transform.position.z)
			{
				m_particleSystemRenderer.enabled = false;
			}
			else
			{
				m_particleSystemRenderer.enabled = true;
			}
		}
	}
}
