using System;
using UnityEngine;

namespace MagicaCloth
{
	[Serializable]
	public class UpdateTimeManager
	{
		public enum UpdateCount
		{
			_60 = 60,
			_90_Default = 90,
			_120 = 120,
			_150 = 150,
			_180 = 180
		}

		public enum UpdateMode
		{
			UnscaledTime = 0,
			OncePerFrame = 1,
			DelayUnscaledTime = 10
		}

		[SerializeField]
		private UpdateCount updatePerSeccond = UpdateCount._60;

		[SerializeField]
		private UpdateMode updateMode;

		private float timeScale = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float futurePredictionRate = 1f;

		[SerializeField]
		private bool updateBoneScale = true;

		public int UpdatePerSecond
		{
			get
			{
				return (int)updatePerSeccond;
			}
		}

		public float UpdateIntervalTime
		{
			get
			{
				return 1f / (float)UpdatePerSecond;
			}
		}

		public float UpdatePower
		{
			get
			{
				return 90f / (float)UpdatePerSecond;
			}
		}

		public float TimeScale
		{
			get
			{
				return timeScale;
			}
			set
			{
				timeScale = Mathf.Clamp01(value);
			}
		}

		public float DeltaTime
		{
			get
			{
				return Time.deltaTime;
			}
		}

		public float AverageDeltaTime
		{
			get
			{
				return Time.smoothDeltaTime;
			}
		}

		public bool IsUnscaledUpdate
		{
			get
			{
				if (updateMode != 0)
				{
					return updateMode == UpdateMode.DelayUnscaledTime;
				}
				return true;
			}
		}

		public bool IsDelay
		{
			get
			{
				return updateMode == UpdateMode.DelayUnscaledTime;
			}
		}

		public float FuturePredictionRate
		{
			get
			{
				return futurePredictionRate;
			}
			set
			{
				futurePredictionRate = Mathf.Clamp01(value);
			}
		}

		public bool UpdateBoneScale
		{
			get
			{
				return updateBoneScale;
			}
			set
			{
				updateBoneScale = value;
			}
		}

		public UpdateMode GetUpdateMode()
		{
			return updateMode;
		}

		public void SetUpdateMode(UpdateMode mode)
		{
			updateMode = mode;
		}

		public void SetUpdatePerSecond(UpdateCount ucount)
		{
			updatePerSeccond = ucount;
		}
	}
}
