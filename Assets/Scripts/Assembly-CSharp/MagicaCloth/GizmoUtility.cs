using UnityEngine;

namespace MagicaCloth
{
	public static class GizmoUtility
	{
		public static readonly Color ColorDynamic = new Color(1f, 1f, 1f);

		public static readonly Color ColorKinematic = new Color(1f, 1f, 0f);

		public static readonly Color ColorInvalid = new Color(0.5f, 0.5f, 0.5f);

		public static readonly Color ColorCollider = new Color(0f, 1f, 0f);

		public static readonly Color ColorNonSelectedCollider = new Color(0.5f, 0.3f, 0f);

		public static readonly Color ColorTriangle = new Color(1f, 0f, 1f);

		public static readonly Color ColorStructLine = new Color(0f, 1f, 1f);

		public static readonly Color ColorBendLine = new Color(0f, 0.5f, 1f);

		public static readonly Color ColorNearLine = new Color(0.55f, 0.5f, 0.7f);

		public static readonly Color ColorRotationLine = new Color(1f, 0.65f, 0f);

		public static readonly Color ColorAdjustLine = new Color(1f, 1f, 0f);

		public static readonly Color ColorAirLine = new Color(0.55f, 0.5f, 0.7f);

		public static readonly Color ColorBasePosition = new Color(1f, 0f, 0f);

		public static readonly Color ColorDirectionMoveLimit = new Color(0f, 1f, 1f);

		public static readonly Color ColorPenetration = new Color(1f, 0.3f, 0f);

		public static readonly Color ColorDeformerPoint = new Color(1f, 1f, 1f);

		public static readonly Color ColorDeformerPointRange = new Color(0.5f, 0.2f, 0f);

		public static readonly Color ColorWind = new Color(0.55f, 0.592f, 0.796f);

		public static void DrawWireCapsule(Vector3 pos, Quaternion rot, Vector3 scl, Vector3 ldir, Vector3 lup, float length, float startRadius, float endRadius, bool resetMatrix = true)
		{
			Gizmos.matrix = Matrix4x4.TRS(pos, rot, scl);
			Vector3 vector = ldir * length;
			Gizmos.DrawWireSphere(-vector, startRadius);
			Gizmos.DrawWireSphere(vector, endRadius);
			for (int i = 0; i < 360; i += 45)
			{
				Quaternion quaternion = Quaternion.AngleAxis(i, ldir);
				Vector3 vector2 = quaternion * (lup * startRadius);
				Vector3 vector3 = quaternion * (lup * endRadius);
				Gizmos.DrawLine(-vector + vector2, vector + vector3);
			}
			Gizmos.matrix = Matrix4x4.TRS(pos, rot * Quaternion.AngleAxis(45f, ldir), scl);
			Gizmos.DrawWireSphere(-vector, startRadius);
			Gizmos.DrawWireSphere(vector, endRadius);
			if (resetMatrix)
			{
				Gizmos.matrix = Matrix4x4.identity;
			}
		}

		public static void DrawWireSphere(Vector3 pos, Quaternion rot, Vector3 scl, float radius, bool drawSphere, bool drawAxis, bool resetMatrix = true)
		{
			Gizmos.matrix = Matrix4x4.TRS(pos, rot, scl);
			if (drawSphere)
			{
				Gizmos.DrawWireSphere(Vector3.zero, radius);
			}
			if (drawAxis)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(Vector3.zero, Vector3.right * 0.03f);
				Gizmos.color = Color.green;
				Gizmos.DrawLine(Vector3.zero, Vector3.up * 0.03f);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(Vector3.zero, Vector3.forward * 0.03f);
			}
			if (resetMatrix)
			{
				Gizmos.matrix = Matrix4x4.identity;
			}
		}

		public static void DrawWireCube(Vector3 pos, Quaternion rot, Vector3 size, bool resetMatrix = true)
		{
			Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
			Gizmos.DrawWireCube(Vector3.zero, size);
			if (resetMatrix)
			{
				Gizmos.matrix = Matrix4x4.identity;
			}
		}

		public static void DrawWireCone(Vector3 pos, Quaternion rot, float length, float radius, int div = 8)
		{
			Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
			Vector3 vector = Vector3.forward * length;
			Vector3 from = vector;
			for (int i = 0; i < div; i++)
			{
				Vector3 vector2 = Quaternion.AngleAxis((float)i / (float)div * 360f, Vector3.forward) * Vector3.right * radius;
				Gizmos.DrawLine(Vector3.zero, vector + vector2);
				Gizmos.DrawLine(vector, vector + vector2);
				if (i > 0)
				{
					Gizmos.DrawLine(from, vector + vector2);
				}
				from = vector + vector2;
			}
			Gizmos.DrawLine(from, vector + Vector3.right * radius);
			Gizmos.matrix = Matrix4x4.identity;
		}

		public static void DrawWireArrow(Vector3 pos, Quaternion rot, Vector3 size, bool cross = false)
		{
			Gizmos.matrix = Matrix4x4.TRS(pos, rot, size);
			Vector3[] array = new Vector3[5]
			{
				new Vector3(0f, 0f, -1f),
				new Vector3(0f, 0.5f, -1f),
				new Vector3(0f, 0.5f, 0f),
				new Vector3(0f, 1f, 0f),
				new Vector3(0f, 0f, 1f)
			};
			float angle = (cross ? 90f : 180f);
			int num = (cross ? 4 : 2);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < array.Length - 1; j++)
				{
					Gizmos.DrawLine(array[j], array[j + 1]);
				}
				rot *= Quaternion.AngleAxis(angle, Vector3.forward);
				Gizmos.matrix = Matrix4x4.TRS(pos, rot, size);
			}
			Gizmos.matrix = Matrix4x4.identity;
		}

		public static void DrawAxis(Vector3 pos, Quaternion rot, float size, bool resetMatrix = true)
		{
			Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
			Gizmos.color = Color.red;
			Gizmos.DrawRay(Vector3.zero, Vector3.right * size);
			Gizmos.color = Color.green;
			Gizmos.DrawRay(Vector3.zero, Vector3.up * size);
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(Vector3.zero, Vector3.forward * size);
			if (resetMatrix)
			{
				Gizmos.matrix = Matrix4x4.identity;
			}
		}
	}
}
