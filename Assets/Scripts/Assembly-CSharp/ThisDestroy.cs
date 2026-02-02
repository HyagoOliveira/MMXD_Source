using UnityEngine;

public class ThisDestroy : MonoBehaviour
{
	private ParticleSystem _particleSystem;

	private void Start()
	{
		_particleSystem = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		if (!_particleSystem.IsAlive())
		{
			Object.Destroy(base.gameObject);
		}
	}
}
