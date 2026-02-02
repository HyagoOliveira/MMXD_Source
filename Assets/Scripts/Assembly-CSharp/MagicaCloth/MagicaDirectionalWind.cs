using UnityEngine;

namespace MagicaCloth
{
	[HelpURL("https://magicasoft.jp/directional-wind/")]
	[AddComponentMenu("MagicaCloth/MagicaDirectionalWind")]
	public class MagicaDirectionalWind : WindComponent
	{
		[SerializeField]
		[Range(0f, 50f)]
		private float main = 5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float turbulence = 1f;

		private float oldMain;

		private float oldTurbulence;

		public float Main
		{
			get
			{
				return main;
			}
			set
			{
				main = Mathf.Clamp(value, 0f, 50f);
			}
		}

		public float Turbulence
		{
			get
			{
				return turbulence;
			}
			set
			{
				turbulence = Mathf.Clamp01(value);
			}
		}

		public Vector3 MainDirection
		{
			get
			{
				return base.transform.forward;
			}
		}

		public Vector3 CurrentDirection
		{
			get
			{
				if (windId >= 0)
				{
					return CreateSingleton<MagicaPhysicsManager>.Instance.Wind.windDataList[windId].direction;
				}
				return MainDirection;
			}
		}

		protected override void CreateWind()
		{
			windId = CreateSingleton<MagicaPhysicsManager>.Instance.Wind.CreateWind(PhysicsManagerWindData.WindType.Direction, main, turbulence);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (windId >= 0)
			{
				bool flag = false;
				if (main != oldMain)
				{
					flag = true;
				}
				if (turbulence != oldTurbulence)
				{
					flag = true;
				}
				if (flag)
				{
					oldMain = main;
					oldTurbulence = turbulence;
					CreateSingleton<MagicaPhysicsManager>.Instance.Wind.SetParameter(windId, main, turbulence);
				}
			}
		}
	}
}
