using System;
using UnityEngine;

public class FxCulling : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem[] particleSystems;

	[SerializeField]
	private float cullingRadius = 10f;

	private CullingGroup cullingGroup;

	private void Start()
	{
		BattleFxCamera battleFxCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.GetBattleFxCamera();
		if (null != battleFxCamera)
		{
			cullingGroup = new CullingGroup();
			cullingGroup.targetCamera = battleFxCamera._camera;
			cullingGroup.SetBoundingSpheres(new BoundingSphere[1]
			{
				new BoundingSphere(base.transform.position, cullingRadius)
			});
			cullingGroup.SetBoundingSphereCount(1);
			CullingGroup obj = cullingGroup;
			obj.onStateChanged = (CullingGroup.StateChanged)Delegate.Combine(obj.onStateChanged, new CullingGroup.StateChanged(OnStateChanged));
		}
	}

	private void OnStateChanged(CullingGroupEvent sphere)
	{
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(particleSystems, sphere.isVisible);
	}

	private void OnDestroy()
	{
		if (cullingGroup != null)
		{
			cullingGroup.Dispose();
		}
	}
}
