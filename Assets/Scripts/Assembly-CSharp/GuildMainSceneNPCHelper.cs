using System;
using PathCreation;
using UnityEngine;

public class GuildMainSceneNPCHelper : OrangePathFollower<GuildMainSceneNPCHelper>
{
	private readonly string WALKING_PARAM = "IsWalking";

	public bool IsPaused;

	[SerializeField]
	private float _modelShowDelay;

	[SerializeField]
	private float _modelHideDelay;

	private readonly Quaternion _modelRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));

	private readonly Quaternion _modelRotationReverse = Quaternion.Euler(new Vector3(0f, 180f, 0f));

	private Animator _animator;

	private GameObject _model;

	private bool _effectTeleportIn;

	private bool _effectTeleportOut;

	private float _currentTime;

	private float _modelTime;

	public void Awake()
	{
		_animator = GetComponentInChildren<Animator>();
		_model = _animator.gameObject;
	}

	public override void SetPathCreator(PathCreator pathCreator, float startDelayTime, float endDelayTime, bool isReverse = false)
	{
		base.SetPathCreator(pathCreator, startDelayTime, endDelayTime, isReverse);
		_model.transform.localRotation = (isReverse ? _modelRotationReverse : _modelRotation);
		_model.SetActive(false);
		_effectTeleportIn = false;
		_effectTeleportOut = false;
	}

	protected override bool CheckStartDelay()
	{
		bool flag = base.CheckStartDelay();
		if (flag)
		{
			if (!_effectTeleportIn)
			{
				_effectTeleportIn = true;
				_currentTime = Time.time;
				_modelTime = _currentTime + _modelShowDelay;
				FxBase fxBase = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(GuildMainSceneNPCController.FxName.fx_guild_teleport_in.ToString(), base.transform.position, Quaternion.identity, Array.Empty<object>());
				if (fxBase != null)
				{
					fxBase.transform.localScale = _model.transform.localScale;
				}
			}
			else
			{
				_currentTime += Time.deltaTime;
				if (_currentTime >= _modelTime)
				{
					_model.SetActive(true);
				}
			}
		}
		_animator.SetBool(WALKING_PARAM, !flag);
		return flag;
	}

	protected override bool CheckEndDelay()
	{
		bool flag = base.CheckEndDelay();
		if (flag)
		{
			if (!_effectTeleportOut)
			{
				_effectTeleportOut = true;
				_currentTime = Time.time;
				_modelTime = _currentTime + _endDelayTime - _modelHideDelay;
				FxBase fxBase = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(GuildMainSceneNPCController.FxName.fx_guild_teleport_out.ToString(), base.transform.position, Quaternion.identity, Array.Empty<object>());
				if (fxBase != null)
				{
					fxBase.transform.localScale = _model.transform.localScale;
				}
			}
			else
			{
				_currentTime += Time.deltaTime;
				if (_currentTime >= _modelTime)
				{
					_model.SetActive(false);
				}
			}
		}
		_animator.SetBool(WALKING_PARAM, !flag);
		return flag;
	}

	public override void Update()
	{
		if (!IsPaused)
		{
			base.Update();
		}
	}
}
