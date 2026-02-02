using System;
using UnityEngine;

public class BS011_EyeController : BS011_PartsController
{
	private CharacterMaterial _material;

	private Transform _upperLid;

	private Transform _lowerLid;

	public float OpenValue = 35f;

	public float CloseValue;

	public float Duration = 0.5f;

	public void Start()
	{
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_upperLid = OrangeBattleUtility.FindChildRecursive(ref target, "up", true);
		_lowerLid = OrangeBattleUtility.FindChildRecursive(ref target, "dn", true);
		_material = GetComponent<CharacterMaterial>();
	}

	public int OpenEye(float duration)
	{
		if (_busy != -1)
		{
			return _busy;
		}
		SoundSource.PlaySE("BossSE", 123);
		return _busy = LeanTween.value(base.gameObject, CloseValue, OpenValue, duration).setOnUpdate(delegate(float val)
		{
			_upperLid.localEulerAngles = Vector3.right * (0f - val);
			_lowerLid.localEulerAngles = Vector3.right * val;
		}).setOnComplete((Action)delegate
		{
			_busy = -1;
		})
			.uniqueId;
	}

	public int CloseEye(float duration)
	{
		if (_busy != -1)
		{
			return _busy;
		}
		return _busy = LeanTween.value(base.gameObject, OpenValue, CloseValue, duration).setOnUpdate(delegate(float val)
		{
			_upperLid.localEulerAngles = Vector3.right * (0f - val);
			_lowerLid.localEulerAngles = Vector3.right * val;
		}).setOnComplete((Action)delegate
		{
			_busy = -1;
		})
			.uniqueId;
	}

	public override int SetVisible(bool visible = true)
	{
		base.SetVisible(visible);
		return _busy = (visible ? _material.Appear(delegate
		{
			_busy = -1;
		}) : _material.Disappear(delegate
		{
			_busy = -1;
		}));
	}

	public override int SetDestroy()
	{
		SetVisible(false);
		return base.SetDestroy();
	}

	public override void Hurt()
	{
		_material.Hurt();
	}
}
