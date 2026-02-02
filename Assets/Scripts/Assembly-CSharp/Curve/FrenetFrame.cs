using UnityEngine;

namespace Curve
{
	public class FrenetFrame
	{
		private Vector3 tangent;

		private Vector3 normal;

		private Vector3 binormal;

		public Vector3 Tangent
		{
			get
			{
				return tangent;
			}
		}

		public Vector3 Normal
		{
			get
			{
				return normal;
			}
		}

		public Vector3 Binormal
		{
			get
			{
				return binormal;
			}
		}

		public FrenetFrame(Vector3 t, Vector3 n, Vector3 bn)
		{
			tangent = t;
			normal = n;
			binormal = bn;
		}
	}
}
