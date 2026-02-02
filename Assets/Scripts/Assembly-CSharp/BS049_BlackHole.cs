using StageLib;
using UnityEngine;

public class BS049_BlackHole : CollideBullet, ILogicUpdate
{
	[SerializeField]
	private float gravitational = 100f;

	[SerializeField]
	private Vector2 GravityForce = new Vector2(3f, 0.8f);

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		BS049_Controller bS049_Controller = null;
		foreach (StageUpdate.EnemyCtrlID runEnemy in StageUpdate.runEnemys)
		{
			if ((bool)runEnemy.mEnemy && (bool)(runEnemy.mEnemy as BS049_Controller))
			{
				bS049_Controller = runEnemy.mEnemy as BS049_Controller;
				if ((int)bS049_Controller.Hp <= 0)
				{
					return;
				}
			}
		}
		if (bS049_Controller == null)
		{
			BackToPool();
			return;
		}
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		foreach (StageUpdate.EnemyCtrlID runEnemy2 in StageUpdate.runEnemys)
		{
			if ((bool)runEnemy2.mEnemy && runEnemy2.mEnemy is EM093_Controller)
			{
				(runEnemy2.mEnemy as EM093_Controller)._needExp = true;
			}
			else if ((bool)runEnemy2.mEnemy && (bool)(runEnemy2.mEnemy as BS049_Controller))
			{
				bS049_Controller = runEnemy2.mEnemy as BS049_Controller;
				bS049_Controller.BlackHoleExist = true;
			}
		}
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.CLOSE_FX, BackToPool);
	}

	public override void BackToPool()
	{
		base.BackToPool();
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		foreach (StageUpdate.EnemyCtrlID runEnemy in StageUpdate.runEnemys)
		{
			if ((bool)runEnemy.mEnemy && (bool)(runEnemy.mEnemy as BS049_Controller))
			{
				(runEnemy.mEnemy as BS049_Controller).BlackHoleExist = false;
			}
		}
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.CLOSE_FX, BackToPool);
	}

	public virtual void LogicUpdate()
	{
		if (!IsActivate)
		{
			return;
		}
		foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
		{
			if ((int)runPlayer.Hp > 0)
			{
				VInt3 vInt = new VInt3((_transform.position - runPlayer._transform.position).normalized * GravityForce * gravitational) / 1000f;
				runPlayer.AddForce(new VInt3(vInt.x, 0, 0));
				runPlayer.AddForceFieldProxy(new VInt3(0, vInt.y, 0));
			}
		}
	}
}
