using UnityEngine;

public class MeleeCollider : MonoBehaviour
{
	private BulletBase _masterScript;

	private LayerMask _targetMask;

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && (bool)_masterScript && ((1 << col.gameObject.layer) & (int)_targetMask) != 0 && !col.isTrigger)
		{
			_masterScript.Hit(col);
		}
	}

	private void OnTriggerStay2D(Collider2D col)
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && (bool)_masterScript && ((1 << col.gameObject.layer) & (int)_targetMask) != 0 && !col.isTrigger)
		{
			_masterScript.Hit(col);
		}
	}

	public void SetMaster(BulletBase target)
	{
		_masterScript = target;
	}

	public void UpdateMask(LayerMask mask)
	{
		_targetMask = mask;
	}
}
