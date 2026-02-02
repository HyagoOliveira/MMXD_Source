using UnityEngine;

namespace MagicaCloth
{
	public class StopWatch
	{
		private float startTime;

		private float endTime;

		public float ElapsedSeconds
		{
			get
			{
				return endTime - startTime;
			}
		}

		public float ElapsedMilliseconds
		{
			get
			{
				return (endTime - startTime) * 1000f;
			}
		}

		public StopWatch Start()
		{
			startTime = Time.realtimeSinceStartup;
			return this;
		}

		public StopWatch Stop()
		{
			endTime = Time.realtimeSinceStartup;
			return this;
		}
	}
}
