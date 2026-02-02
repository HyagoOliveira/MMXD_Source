using System;
using UnityEngine;

public class VampireBatController : batController, IManagedUpdateBehavior
{
	private string[] healSE = new string[2] { "BattleSE", "bt_hp02" };

	protected override void Awake()
	{
		base.Awake();
		_collideBullet.HitCallback = RefillHp;
		dirLeft = new Vector3(0f, 180f, 0f);
		dirRight = new Vector3(0f, 90f, 0f);
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
	}

	public void RefillHp(object col)
	{
		Hp = Math.Min((int)Hp + (int)MaxHp / 10, MaxHp);
		UpdateHurtAction();
		PlaySE(healSE[0], healSE[1]);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(EnemyWeapons[1].BulletData.s_HIT_FX, base.AimTransform, Quaternion.identity, Array.Empty<object>());
	}
}
