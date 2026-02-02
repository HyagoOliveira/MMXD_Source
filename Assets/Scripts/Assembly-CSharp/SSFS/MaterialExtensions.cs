using System.Collections;
using UnityEngine;

namespace SSFS
{
	public static class MaterialExtensions
	{
		public static IEnumerator SSFSPhaseTo(this Material m, float endPhase, float time = 1f)
		{
			float o = m.GetFloat("_Phase");
			while ((double)Mathf.Abs(o - endPhase) > 0.0001)
			{
				float maxDelta = Time.deltaTime / time;
				Mathf.MoveTowards(o, endPhase, maxDelta);
				yield return new WaitForEndOfFrame();
			}
		}

		public static void SyncKeyword(this Material m, string keyword, bool state)
		{
			if (!(m == null))
			{
				if (state)
				{
					m.EnableKeyword(keyword);
				}
				else
				{
					m.DisableKeyword(keyword);
				}
			}
		}

		public static void SetVector(this Material m, string name, Vector2 v1, Vector2 v2)
		{
			m.SetVector(name, new Vector4(v1.x, v1.y, v2.x, v2.y));
		}

		public static void GetVector(this Material m, string name, out Vector2 v1, out Vector2 v2)
		{
			Vector4 vector = m.GetVector(name);
			v1 = new Vector2(vector.x, vector.y);
			v2 = new Vector2(vector.z, vector.w);
		}

		public static Vector4 Append(this Vector2 v1, Vector2 v2)
		{
			return new Vector4(v1.x, v1.y, v2.x, v2.y);
		}

		public static void Split(this Vector4 v0, out Vector2 v1, out Vector2 v2)
		{
			v1 = new Vector2(v0.x, v0.y);
			v2 = new Vector2(v0.z, v0.w);
		}
	}
}
