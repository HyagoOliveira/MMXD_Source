using UnityEngine;

public class CH064_VmKickSubCollider : MonoBehaviour
{
	[SerializeField]
	private CH064_VmKickBullet _mainBullet;

	protected virtual void OnTriggerEnter2D(Collider2D col)
	{
		OnTriggerHit(col);
	}

	protected virtual void OnTriggerStay2D(Collider2D col)
	{
		OnTriggerHit(col);
	}

	protected void OnTriggerHit(Collider2D col)
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && !(_mainBullet == null) && !col.isTrigger)
		{
			StageObjParam component = col.GetComponent<StageObjParam>();
			if (component != null && component.tLinkSOB != null)
			{
				_mainBullet.OnTriggerHit(col);
			}
			else if ((bool)col.GetComponent<StageHurtObj>())
			{
				_mainBullet.OnTriggerHit(col);
			}
		}
	}
}
