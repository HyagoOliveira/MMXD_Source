using System;
using UnityEngine;

public class BS011_PartsController : MonoBehaviour
{
	public EnemyControllerBase MasterController;

	public Transform _transform;

	public Transform ShootPoint;

	protected CollideBullet _collideBullet;

	protected EnemyCollider _enemyCollider;

	protected LayerMask _targetMask;

	protected int _busy = -1;

	public OrangeCriSource SoundSource;

	public virtual void Awake()
	{
		_transform = base.transform;
		_enemyCollider = GetComponent<EnemyCollider>();
		_targetMask = ManagedSingleton<OrangeLayerManager>.Instance.EnemyUseMask;
		MasterController = GetComponentInParent<EnemyControllerBase>();
		ShootPoint = OrangeBattleUtility.FindChildRecursive(_transform, "ShootPoint", true);
		if (ShootPoint == null)
		{
			ShootPoint = _transform;
		}
		_collideBullet = ShootPoint.gameObject.AddOrGetComponent<CollideBullet>();
		_collideBullet.SoundSource.Initial(OrangeSSType.BOSS);
		SoundSource = _collideBullet.SoundSource;
	}

	public void UpdateBulletData(SKILL_TABLE bulletData, string owner = "")
	{
		_collideBullet.UpdateBulletData(bulletData, owner);
	}

	public bool IsBusy()
	{
		return _busy != -1;
	}

	public virtual bool IsVisible()
	{
		if (!_enemyCollider || !_enemyCollider.IsColliderEnable())
		{
			return false;
		}
		return true;
	}

	public virtual int SetVisible(bool visible = true)
	{
		if ((bool)_enemyCollider)
		{
			_enemyCollider.SetColliderEnable(visible);
		}
		if (visible)
		{
			_collideBullet.SetBulletAtk(null, MasterController.selfBuffManager.sBuffStatus, MasterController.EnemyData);
			_collideBullet.Active(_targetMask);
			MasterController.AllowAutoAim = true;
			MasterController.VanishStatus = false;
			MasterController.AimTransform = _transform;
		}
		else
		{
			MasterController.AllowAutoAim = false;
			MasterController.VanishStatus = true;
			MasterController.AimTransform = MasterController._transform;
			_collideBullet.BackToPool();
		}
		return -1;
	}

	public virtual int SetDestroy()
	{
		if ((bool)_enemyCollider)
		{
			_enemyCollider.SetColliderEnable(false);
		}
		MasterController.AllowAutoAim = false;
		MasterController.AimTransform = MasterController._transform;
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_explode_001", new Vector3(_transform.position.x, _transform.position.y, _transform.position.z), Quaternion.identity, Array.Empty<object>());
		SoundSource.PlaySE("HitSE", 103);
		_collideBullet.BackToPool();
		return -1;
	}

	public virtual void Hurt()
	{
	}
}
