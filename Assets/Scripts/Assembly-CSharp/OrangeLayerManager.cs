using UnityEngine;

public class OrangeLayerManager : ManagedSingleton<OrangeLayerManager>
{
	public int PlayerLayer { get; private set; }

	public int EnemyLayer { get; private set; }

	public int PvpPlayerLayer { get; private set; }

	public int BulletLayer { get; private set; }

	public int ObstacleLayer { get; private set; }

	public int DefaultLayer { get; private set; }

	public int FxLayer { get; private set; }

	public int BlockLayer { get; private set; }

	public int BlockEnemyLayer { get; private set; }

	public int BlockPlayerLayer { get; private set; }

	public int VehicleLayer { get; private set; }

	public int SemiBlockLayer { get; private set; }

	public int RenderTextureLayer { get; private set; }

	public int RenderPlayer { get; private set; }

	public int RenderEnemy { get; private set; }

	public int RenderSPEnemy { get; private set; }

	public int AISLayer { get; private set; }

	public int WallKickMask { get; private set; }

	public int BulletIgnoreBlockMask { get; private set; }

	public LayerMask PlayerUseMask { get; private set; }

	public LayerMask EnemyUseMask { get; private set; }

	public LayerMask PvpPlayerUseMask { get; private set; }

	public override void Initialize()
	{
		BulletScriptableObject instance = BulletScriptableObject.Instance;
		PlayerUseMask = (int)instance.BulletLayerMaskEnemy | (int)instance.BulletLayerMaskPvpPlayer;
		EnemyUseMask = (int)instance.BulletLayerMaskPlayer | (int)instance.BulletLayerMaskPvpPlayer;
		PvpPlayerUseMask = (int)instance.BulletLayerMaskPlayer | (int)instance.BulletLayerMaskEnemy;
		PlayerLayer = LayermaskToLayer(instance.BulletLayerMaskPlayer);
		EnemyLayer = LayermaskToLayer(instance.BulletLayerMaskEnemy);
		PvpPlayerLayer = LayermaskToLayer(instance.BulletLayerMaskPvpPlayer);
		BulletLayer = LayermaskToLayer(instance.BulletLayerMaskBullet);
		ObstacleLayer = LayermaskToLayer(instance.BulletLayerMaskObstacle);
		DefaultLayer = LayerMask.NameToLayer("Default");
		FxLayer = LayerMask.NameToLayer("TransparentFX");
		BlockLayer = LayerMask.NameToLayer("Block");
		BlockEnemyLayer = LayerMask.NameToLayer("BlockEnemy");
		BlockPlayerLayer = LayerMask.NameToLayer("BlockPlayer");
		SemiBlockLayer = LayerMask.NameToLayer("SemiBlock");
		RenderTextureLayer = LayerMask.NameToLayer("RenderTexture");
		VehicleLayer = LayerMask.NameToLayer("Vehicle");
		BulletIgnoreBlockMask = LayerMask.NameToLayer("BulletIgnoreBlock");
		RenderPlayer = LayerMask.NameToLayer("RenderPlayer");
		RenderEnemy = LayerMask.NameToLayer("RenderEnemy");
		RenderSPEnemy = LayerMask.NameToLayer("RenderSPEnemy");
		AISLayer = LayerMask.NameToLayer("AutoAimSystem");
		WallKickMask = LayerMask.GetMask("BlockPlayer", "NoWallKick");
	}

	public override void Dispose()
	{
	}

	private int LayermaskToLayer(LayerMask layerMask)
	{
		int num = 0;
		int num2 = layerMask.value;
		while (num2 > 0)
		{
			num2 >>= 1;
			num++;
		}
		return num - 1;
	}
}
