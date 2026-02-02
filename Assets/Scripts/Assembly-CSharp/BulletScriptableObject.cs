using UnityEngine;

[CreateAssetMenu]
public class BulletScriptableObject : ScriptableObject
{
	private static BulletScriptableObject _mInstance;

	public LayerMask BulletLayerMaskEnemy;

	public LayerMask BulletLayerMaskPlayer;

	public LayerMask BulletLayerMaskBullet;

	public LayerMask BulletLayerMaskObstacle;

	public LayerMask BulletLayerMaskPvpPlayer;

	public static BulletScriptableObject Instance
	{
		get
		{
			if (_mInstance == null)
			{
				_mInstance = Resources.Load<BulletScriptableObject>("BulletScriptableObject");
			}
			return _mInstance;
		}
	}
}
