using System.Collections.Generic;
using UnityEngine;

public class SplitBullet : BasicBullet
{
	private Vector3 lastPos;

	private PerBuffManager.BuffStatus buffStatus;

	private MOB_TABLE enemyData;

	private LayerMask targetMask;

	private List<Collider2D> ignoreColliders = new List<Collider2D>();

	public void AddIgnoreCollider(Collider2D _collider)
	{
		if (_collider != null && !ignoreColliders.Contains(_collider))
		{
			ignoreColliders.Add(_collider);
		}
	}

	public void RemoveIgnoreCollider(Collider2D _collider)
	{
		if (ignoreColliders.Contains(_collider))
		{
			ignoreColliders.Remove(_collider);
		}
	}

	public void SetupData(PerBuffManager.BuffStatus _buffStatus, MOB_TABLE _enemyData, LayerMask _targetMask)
	{
		buffStatus = _buffStatus;
		enemyData = _enemyData;
		targetMask = _targetMask;
		Phase = BulletPhase.Normal;
	}

	public override void LateUpdateFunc()
	{
		lastPos = _transform.position;
		base.LateUpdateFunc();
	}

	protected override void MoveBullet()
	{
		base.MoveBullet();
		RaycastHit2D raycastHit2D = Physics2D.Raycast(lastPos, _transform.position - lastPos, Vector3.Distance(_transform.position, lastPos) + 1f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		if (raycastHit2D.collider != null && !ignoreColliders.Contains(raycastHit2D.collider) && BulletData.n_LINK_SKILL != 0)
		{
			SKILL_TABLE tSkillTable = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[BulletData.n_LINK_SKILL];
			BulletBase.TryShotBullet(tSkillTable, raycastHit2D.point + raycastHit2D.normal.normalized * 0.25f, Quaternion.Euler(0f, 0f, 90f) * raycastHit2D.normal, null, buffStatus, enemyData, targetMask);
			BulletBase.TryShotBullet(tSkillTable, raycastHit2D.point + raycastHit2D.normal.normalized * 0.25f, Quaternion.Euler(0f, 0f, -90f) * raycastHit2D.normal, null, buffStatus, enemyData, targetMask);
			ignoreColliders.Clear();
			BackToPool();
		}
	}
}
