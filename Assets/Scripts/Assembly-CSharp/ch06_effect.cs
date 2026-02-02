using UnityEngine;

public class ch06_effect : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (base.transform.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.FxLayer)
		{
			base.transform.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.FxLayer;
		}
	}
}
