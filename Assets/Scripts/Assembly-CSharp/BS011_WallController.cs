using CallbackDefs;
using StageLib;
using UnityEngine;

public class BS011_WallController : MonoBehaviour
{
	private class Spike
	{
		public Transform _transform;

		public bool _open;
	}

	private CharacterMaterial _material;

	private PlatformController _platformController;

	private Transform _transform;

	private Spike _upperSpike;

	private Spike _lowerSpike;

	public EnemyControllerBase MasterController;

	protected CollideBullet _collideBullet;

	private bool _rollBullet;

	public OrangeCriSource SoundSource;

	private int _busy = -1;

	private bool _wallClose;

	public float WallBeginPos = 14.5f;

	public float WallEndPos = 7.3f;

	public float SpikeBeginPos = 4.9f;

	public float SpikeEndPos = 3.9f;

	private Spike _targetSpike;

	private float _beginPos;

	private float _endPos;

	public float Direction = 1f;

	[SerializeField]
	public Transform ShootPoint;

	public virtual void Awake()
	{
		if (_collideBullet == null)
		{
			SetCollideBullet();
		}
	}

	private void Start()
	{
		_transform = base.transform;
		_material = GetComponent<CharacterMaterial>();
		_platformController = base.gameObject.AddOrGetComponent<PlatformController>();
		Transform[] target = base.transform.GetComponentsInChildren<Transform>(true);
		_upperSpike = new Spike
		{
			_transform = OrangeBattleUtility.FindChildRecursive(ref target, "UpperSpike", true),
			_open = false
		};
		_lowerSpike = new Spike
		{
			_transform = OrangeBattleUtility.FindChildRecursive(ref target, "LowerSpike", true),
			_open = false
		};
	}

	public int ToggleWall(float duration)
	{
		if (_busy != -1)
		{
			return _busy;
		}
		_beginPos = WallBeginPos;
		_endPos = WallEndPos;
		if (_wallClose)
		{
			_beginPos = WallEndPos;
			_endPos = WallBeginPos;
		}
		SoundSource.PlaySE("BossSE", 119);
		SoundSource.PlaySE("BossSE", 122);
		_busy = 1;
		StartCoroutine(StageResManager.TweenFloatCoroutine(_beginPos, _endPos, duration, delegate(float pos)
		{
			Vector3 moveDis2 = Vector3.right * pos * Direction - _transform.localPosition + Vector3.back;
			_platformController.PushMove(moveDis2);
			_platformController.UpdateFunc();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.25f, false);
		}, delegate
		{
			Vector3 moveDis = Vector3.right * _endPos * Direction - _transform.localPosition + Vector3.back;
			_platformController.PushMove(moveDis);
			_platformController.UpdateFunc();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
			_wallClose = !_wallClose;
			_busy = -1;
			SoundSource.PlaySE("BossSE", 120);
			SoundSource.PlaySE("BossSE", 121);
			_collideBullet.BackToPool();
		}));
		return _busy;
	}

	public int ToggleSpike(float duration, bool lower)
	{
		if (_busy != -1)
		{
			return _busy;
		}
		Spike targetSpike = _upperSpike;
		_beginPos = SpikeEndPos;
		_endPos = SpikeBeginPos;
		if (lower)
		{
			targetSpike = _lowerSpike;
		}
		if (targetSpike._open)
		{
			_beginPos = SpikeBeginPos;
			_endPos = SpikeEndPos;
		}
		_busy = 1;
		StartCoroutine(StageResManager.TweenFloatCoroutine(_beginPos, _endPos, duration, delegate(float pos)
		{
			Vector3 localPosition = targetSpike._transform.localPosition;
			localPosition.x = pos * (0f - Direction);
			targetSpike._transform.localPosition = localPosition;
		}, delegate
		{
			targetSpike._open = !targetSpike._open;
			_busy = -1;
		}));
		return _busy;
	}

	public bool isBusy()
	{
		return _busy != -1;
	}

	public void Appear(Callback callback = null)
	{
		_material.Appear(callback);
	}

	public void Disappear(Callback callback = null)
	{
		_material.Disappear(callback);
	}

	public void UpdateBulletData(SKILL_TABLE bulletData)
	{
		if (_collideBullet == null)
		{
			SetCollideBullet();
		}
		_collideBullet.UpdateBulletData(bulletData);
		_collideBullet.SetBulletAtk(null, MasterController.selfBuffManager.sBuffStatus, MasterController.EnemyData);
	}

	public void ActiveBullect(bool active, LayerMask mask)
	{
		if (active)
		{
			_collideBullet.Active(mask);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	private void SetCollideBullet()
	{
		_transform = base.transform;
		MasterController = GetComponentInParent<EnemyControllerBase>();
		if (ShootPoint == null)
		{
			ShootPoint = OrangeBattleUtility.FindChildRecursive(_transform, "ShootPoint", true);
		}
		if (ShootPoint == null)
		{
			ShootPoint = _transform;
		}
		_collideBullet = ShootPoint.gameObject.GetComponent<CollideBullet>();
		if (!_collideBullet)
		{
			_collideBullet = ShootPoint.gameObject.AddComponent<CollideBullet>();
		}
		_collideBullet.Init();
		_collideBullet.SoundSource.Initial(OrangeSSType.BOSS);
		SoundSource = _collideBullet.SoundSource;
	}
}
