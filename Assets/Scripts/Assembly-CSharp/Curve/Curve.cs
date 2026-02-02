using System;
using System.Collections.Generic;
using UnityEngine;

namespace Curve
{
	public abstract class Curve
	{
		protected List<Vector3> points;

		protected bool closed;

		protected float[] cacheArcLengths;

		private bool needsUpdate;

		public Curve(List<Vector3> points, bool closed = false)
		{
			this.points = points;
			this.closed = closed;
		}

		protected abstract Vector3 GetPoint(float t);

		protected virtual Vector3 GetTangent(float t)
		{
			float num = 0.001f;
			float num2 = t - num;
			float num3 = t + num;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			if (num3 > 1f)
			{
				num3 = 1f;
			}
			Vector3 point = GetPoint(num2);
			return (GetPoint(num3) - point).normalized;
		}

		public Vector3 GetPointAt(float u)
		{
			float utoTmapping = GetUtoTmapping(u);
			return GetPoint(utoTmapping);
		}

		public Vector3 GetTangentAt(float u)
		{
			float utoTmapping = GetUtoTmapping(u);
			return GetTangent(utoTmapping);
		}

		private float[] GetLengths(int divisions = -1)
		{
			if (divisions < 0)
			{
				divisions = 200;
			}
			if (cacheArcLengths != null && cacheArcLengths.Length == divisions + 1 && !needsUpdate)
			{
				return cacheArcLengths;
			}
			needsUpdate = false;
			float[] array = new float[divisions + 1];
			Vector3 b = GetPoint(0f);
			array[0] = 0f;
			float num = 0f;
			for (int i = 1; i <= divisions; i++)
			{
				Vector3 point = GetPoint(1f * (float)i / (float)divisions);
				num = (array[i] = num + Vector3.Distance(point, b));
				b = point;
			}
			cacheArcLengths = array;
			return array;
		}

		protected float GetUtoTmapping(float u)
		{
			float[] lengths = GetLengths();
			int num = 0;
			int num2 = lengths.Length;
			float num3 = u * lengths[num2 - 1];
			int num4 = 0;
			int num5 = num2 - 1;
			while (num4 <= num5)
			{
				num = Mathf.FloorToInt((float)num4 + (float)(num5 - num4) / 2f);
				float num6 = lengths[num] - num3;
				if (num6 < 0f)
				{
					num4 = num + 1;
					continue;
				}
				if (num6 > 0f)
				{
					num5 = num - 1;
					continue;
				}
				num5 = num;
				break;
			}
			num = num5;
			if (Mathf.Approximately(lengths[num], num3))
			{
				return 1f * (float)num / (float)(num2 - 1);
			}
			float num7 = lengths[num];
			float num8 = lengths[num + 1] - num7;
			float num9 = (num3 - num7) / num8;
			return 1f * ((float)num + num9) / (float)(num2 - 1);
		}

		public List<FrenetFrame> ComputeFrenetFrames(int segments, bool closed)
		{
			Vector3 rhs = default(Vector3);
			Vector3[] array = new Vector3[segments + 1];
			Vector3[] array2 = new Vector3[segments + 1];
			Vector3[] array3 = new Vector3[segments + 1];
			for (int i = 0; i <= segments; i++)
			{
				float u = 1f * (float)i / (float)segments;
				array[i] = GetTangentAt(u).normalized;
			}
			array2[0] = default(Vector3);
			array3[0] = default(Vector3);
			float num = float.MaxValue;
			float num2 = Mathf.Abs(array[0].x);
			float num3 = Mathf.Abs(array[0].y);
			float num4 = Mathf.Abs(array[0].z);
			if (num2 <= num)
			{
				num = num2;
				rhs.Set(1f, 0f, 0f);
			}
			if (num3 <= num)
			{
				num = num3;
				rhs.Set(0f, 1f, 0f);
			}
			if (num4 <= num)
			{
				rhs.Set(0f, 0f, 1f);
			}
			Vector3 normalized = Vector3.Cross(array[0], rhs).normalized;
			array2[0] = Vector3.Cross(array[0], normalized);
			array3[0] = Vector3.Cross(array[0], array2[0]);
			for (int j = 1; j <= segments; j++)
			{
				array2[j] = array2[j - 1];
				array3[j] = array3[j - 1];
				Vector3 axis = Vector3.Cross(array[j - 1], array[j]);
				if (axis.magnitude > float.Epsilon)
				{
					axis.Normalize();
					float num5 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(array[j - 1], array[j]), -1f, 1f));
					array2[j] = Quaternion.AngleAxis(num5 * 57.29578f, axis) * array2[j];
				}
				array3[j] = Vector3.Cross(array[j], array2[j]).normalized;
			}
			if (closed)
			{
				float num5 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(array2[0], array2[segments]), -1f, 1f));
				num5 /= (float)segments;
				if (Vector3.Dot(array[0], Vector3.Cross(array2[0], array2[segments])) > 0f)
				{
					num5 = 0f - num5;
				}
				for (int k = 1; k <= segments; k++)
				{
					array2[k] = Quaternion.AngleAxis((float)Math.PI / 180f * num5 * (float)k, array[k]) * array2[k];
					array3[k] = Vector3.Cross(array[k], array2[k]);
				}
			}
			List<FrenetFrame> list = new List<FrenetFrame>();
			int num6 = array.Length;
			for (int l = 0; l < num6; l++)
			{
				FrenetFrame item = new FrenetFrame(array[l], array2[l], array3[l]);
				list.Add(item);
			}
			return list;
		}
	}
}
