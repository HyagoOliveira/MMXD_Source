using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(EventTrigger))]
public class ButtonHoldingHelper : MonoBehaviour, IManagedUpdateBehavior
{
	[Serializable]
	private class OffsetValueEvent : UnityEvent<float>
	{
	}

	[Serializable]
	private class StepSetting
	{
		public float Value;

		public int Interval;

		public int Times;
	}

	[SerializeField]
	[Range(100f, 5000f)]
	private int _holdingThreshold = 1000;

	[SerializeField]
	private List<StepSetting> _stepSettingList = new List<StepSetting>
	{
		new StepSetting
		{
			Value = 1f,
			Interval = 100,
			Times = 10
		},
		new StepSetting
		{
			Value = 1f,
			Interval = 50,
			Times = 10
		},
		new StepSetting
		{
			Value = 1f,
			Interval = 25,
			Times = 10
		},
		new StepSetting
		{
			Value = 1f,
			Interval = 10,
			Times = 10
		},
		new StepSetting
		{
			Value = 1f,
			Interval = 5,
			Times = 10
		},
		new StepSetting
		{
			Value = 1f,
			Interval = 2,
			Times = 10
		},
		new StepSetting
		{
			Value = 1f,
			Interval = 1,
			Times = 0
		}
	};

	[SerializeField]
	private OffsetValueEvent _onOffsetValueEvent;

	private bool _isHolding;

	private DateTime _tickStamp;

	private DateTime _tickStampThreshold;

	private int _step;

	private int _stepCount;

	private void Start()
	{
		EventTrigger component = GetComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerDown
		};
		entry.callback.AddListener(OnPointerDown);
		EventTrigger.Entry entry2 = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerUp
		};
		entry2.callback.AddListener(OnPointerUp);
		component.triggers.Add(entry);
		component.triggers.Add(entry2);
		if (_stepSettingList.Count == 0)
		{
			_stepSettingList.Add(new StepSetting
			{
				Interval = 100,
				Value = 1f
			});
		}
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
		_onOffsetValueEvent = null;
	}

	public void UpdateFunc()
	{
		if (!_isHolding || (DateTime.Now - _tickStampThreshold).TotalMilliseconds < (double)_holdingThreshold)
		{
			return;
		}
		StepSetting stepSetting = _stepSettingList[_step];
		if ((DateTime.Now - _tickStamp).TotalMilliseconds < (double)stepSetting.Interval)
		{
			return;
		}
		OffsetValueEvent onOffsetValueEvent = _onOffsetValueEvent;
		if (onOffsetValueEvent != null)
		{
			onOffsetValueEvent.Invoke(stepSetting.Value);
		}
		_tickStamp = DateTime.Now;
		if (_step + 1 < _stepSettingList.Count)
		{
			_stepCount++;
			if (_stepCount >= stepSetting.Times)
			{
				_step++;
				_stepCount = 0;
			}
		}
	}

	public void OnPointerDown(BaseEventData eventData)
	{
		_step = 0;
		_stepCount = 0;
		_tickStamp = DateTime.Now;
		_tickStampThreshold = _tickStamp;
		_isHolding = true;
		StepSetting stepSetting = _stepSettingList[_step];
		OffsetValueEvent onOffsetValueEvent = _onOffsetValueEvent;
		if (onOffsetValueEvent != null)
		{
			onOffsetValueEvent.Invoke(stepSetting.Value);
		}
	}

	public void OnPointerUp(BaseEventData eventData)
	{
		_isHolding = false;
		_step = 0;
		_stepCount = 0;
	}
}
