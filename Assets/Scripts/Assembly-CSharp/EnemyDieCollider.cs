using System;
using UnityEngine;

public class EnemyDieCollider : PoolBaseObject
{
	private static float SimulateTimer;

	[SerializeField]
	private float force = 1f;

	[SerializeField]
	private float upwardsModifier = 1f;

	[SerializeField]
	private ForceMode forceMode = ForceMode.VelocityChange;

	[SerializeField]
	private float ExplosionTime = 0.8f;

	public string Name;

	public Texture MainTex;

	public Renderer[] Renderers;

	public Rigidbody[] Rigids;

	private MaterialPropertyBlock mpb;

	protected bool isSimulatorMode;

	private int tweenUid = -1;

	private void Start()
	{
		if (null != MainTex)
		{
			mpb = new MaterialPropertyBlock();
			OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
			Renderers[0].GetPropertyBlock(mpb);
			mpb.SetTexture(instance.i_MainTex, MainTex);
			for (int i = 0; i < Renderers.Length; i++)
			{
				Renderers[i].SetPropertyBlock(mpb);
			}
		}
	}

	public virtual void ActiveExplosion(bool p_isSimulatorMode = false)
	{
		isSimulatorMode = p_isSimulatorMode;
		Rigidbody[] rigids = Rigids;
		for (int i = 0; i < rigids.Length; i++)
		{
			rigids[i].isKinematic = false;
		}
		rigids = Rigids;
		for (int i = 0; i < rigids.Length; i++)
		{
			rigids[i].AddExplosionForce(1f * force, 0.5f * UnityEngine.Random.insideUnitSphere + base.transform.position, 0f, upwardsModifier, forceMode);
		}
		SimulateTimer = Time.timeSinceLevelLoad + Time.fixedDeltaTime;
		tweenUid = LeanTween.value(1f, 0f, ExplosionTime).setOnUpdate((Action<float>)delegate
		{
			float timeSinceLevelLoad = Time.timeSinceLevelLoad;
			if (SimulateTimer < timeSinceLevelLoad)
			{
				float num = Time.deltaTime / Time.fixedDeltaTime;
				while (num > 0f)
				{
					num -= 1f;
					Physics.Simulate(Time.fixedDeltaTime);
				}
				SimulateTimer = Time.timeSinceLevelLoad + Time.fixedDeltaTime;
			}
		}).setEaseOutCubic()
			.setOnComplete((Action)delegate
			{
				tweenUid = -1;
				BackToPool();
			})
			.uniqueId;
	}

	private void OnDisable()
	{
		LeanTween.cancel(ref tweenUid);
	}

	public override void BackToPool()
	{
		for (int i = 0; i < Rigids.Length; i++)
		{
			Rigids[i].isKinematic = true;
			Rigids[i].transform.localPosition = Vector3.zero;
			Rigids[i].transform.localRotation = Quaternion.identity;
		}
		if (!isSimulatorMode)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, Name);
		}
	}
}
