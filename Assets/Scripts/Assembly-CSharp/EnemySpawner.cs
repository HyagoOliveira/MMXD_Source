using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	public int EnemyLoaderIdx;

	private EnemyControllerBase enemySpawn;

	[HideInInspector]
	public float posX;

	private void Awake()
	{
		posX = base.transform.position.x;
	}

	public void CallEnemySpawn()
	{
		if (!enemySpawn || !enemySpawn.InGame)
		{
			BornEnemy();
		}
	}

	private void BornEnemy()
	{
		enemySpawn = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<EnemyControllerBase>(ManagedSingleton<OrangeTableHelper>.Instance.GetMob(EnemyLoaderIdx).s_MODEL);
		enemySpawn.SetPositionAndRotation(base.transform.position, false);
		enemySpawn.SetActive(true);
	}
}
