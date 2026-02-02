using System;
using Unity.Mathematics;

namespace MagicaCloth
{
	public static class MathUtility
	{
		public static float3 Project(float3 v, float3 n)
		{
			return math.dot(v, n) * n;
		}

		public static float Angle(float3 v1, float3 v2)
		{
			float num = math.length(v1);
			float num2 = math.length(v2);
			return math.acos(math.dot(v1, v2) / (num * num2));
		}

		public static float3 ClampVector(float3 v, float minlength, float maxlength)
		{
			float num = math.length(v);
			if (num > 1E-06f)
			{
				if (num > maxlength)
				{
					v *= maxlength / num;
				}
				else if (num < minlength)
				{
					v *= minlength / num;
				}
			}
			return v;
		}

		public static float3 ClampDistance(float3 from, float3 to, float maxlength)
		{
			float num = math.distance(from, to);
			if (num <= maxlength)
			{
				return to;
			}
			float s = maxlength / num;
			return math.lerp(from, to, s);
		}

		public static bool ClampAngle(float3 dir, float3 basedir, float maxAngle, out float3 outdir)
		{
			float3 x = math.normalize(dir);
			float3 y = math.normalize(basedir);
			float num = math.dot(x, y);
			float num2 = math.acos(num);
			if (num2 <= maxAngle)
			{
				outdir = dir;
				return false;
			}
			float num3 = (num2 - maxAngle) / num2;
			float3 x2 = math.cross(x, y);
			if (math.abs(1f + num) < 1E-06f)
			{
				num2 = (float)Math.PI;
				x2 = ((!(x.x > x.y) || !(x.x > x.z)) ? math.cross(x, new float3(1f, 0f, 0f)) : math.cross(x, new float3(0f, 1f, 0f)));
			}
			else if (math.abs(1f - num) < 1E-06f)
			{
				outdir = dir;
				return false;
			}
			quaternion q = quaternion.AxisAngle(math.normalize(x2), num2 * num3);
			outdir = math.mul(q, dir);
			return true;
		}

		public static quaternion FromToRotation(float3 from, float3 to, float t = 1f)
		{
			float3 x = math.normalize(from);
			float3 y = math.normalize(to);
			float num = math.dot(x, y);
			float num2 = math.acos(num);
			float3 x2 = math.cross(x, y);
			if (math.abs(1f + num) < 1E-06f)
			{
				num2 = (float)Math.PI;
				x2 = ((!(x.x > x.y) || !(x.x > x.z)) ? math.cross(x, new float3(1f, 0f, 0f)) : math.cross(x, new float3(0f, 1f, 0f)));
			}
			else if (math.abs(1f - num) < 1E-06f)
			{
				return quaternion.identity;
			}
			return quaternion.AxisAngle(math.normalize(x2), num2 * t);
		}

		public static quaternion FromToRotation(quaternion from, quaternion to, float t = 1f)
		{
			return FromToRotation(math.forward(from), math.forward(to), t);
		}

		public static float Angle(quaternion q)
		{
			return math.acos(math.forward(q).z);
		}

		public static float Angle(quaternion a, quaternion b)
		{
			return math.acos(math.dot(a, b));
		}

		public static quaternion ClampAngle(quaternion from, quaternion to, float maxAngle)
		{
			float num = Angle(from, to);
			if (num <= maxAngle)
			{
				return to;
			}
			float t = maxAngle / num;
			return math.slerp(from, to, t);
		}

		public static float ClosestPtPointSegmentRatio(float3 c, float3 a, float3 b)
		{
			float3 @float = b - a;
			return math.saturate(math.dot(c - a, @float) / math.dot(@float, @float));
		}

		public static float3 ClosestPtPointSegment(float3 c, float3 a, float3 b)
		{
			float3 @float = b - a;
			float x = math.dot(c - a, @float) / math.dot(@float, @float);
			x = math.saturate(x);
			return a + x * @float;
		}

		public static bool IntersectPointPlane(float3 planePos, float3 planeDir, float3 pos, out float3 outpos)
		{
			float3 @float = pos - planePos;
			if (math.dot(planeDir, @float) < 0f)
			{
				float3 float2 = Project(@float, planeDir);
				outpos = pos - float2;
				return true;
			}
			outpos = pos;
			return false;
		}

		public static float IntersectPointPlaneDist(float3 planePos, float3 planeDir, float3 pos, out float3 outPos)
		{
			float3 @float = pos - planePos;
			float3 float2 = Project(@float, planeDir);
			float num = math.length(float2);
			if (math.dot(planeDir, @float) < 0f)
			{
				outPos = pos - float2;
				return 0f - num;
			}
			outPos = pos;
			return num;
		}

		public static bool IntersectSegmentPlane(float3 a, float3 b, float3 p, float3 pn, out float3 opos)
		{
			float3 @float = b - a;
			float num = (math.dot(pn, p) - math.dot(pn, a)) / math.dot(pn, @float);
			if (num >= 0f && num <= 1f)
			{
				opos = a + num * @float;
				return true;
			}
			opos = 0;
			return false;
		}

		public static bool IntersectPointSphere(float3 sc, float sr, float3 pos, out float3 outPos)
		{
			float3 x = pos - sc;
			float num = math.length(x);
			if (num < sr && num > 1E-05f)
			{
				outPos = pos + math.normalize(x) * (sr - num);
				return true;
			}
			outPos = pos;
			return false;
		}

		public static bool IntersectPointSphere(float3 p, float3 sc, float sr)
		{
			return math.lengthsq(p - sc) <= sr * sr;
		}

		public static bool IntersectRaySphere(float3 p, float3 d, float3 sc, float sr, out float3 q, out float t)
		{
			q = 0;
			t = 0f;
			float3 @float = p - sc;
			float num = math.dot(@float, d);
			float num2 = math.dot(@float, @float) - sr * sr;
			if (num2 > 0f && num > 0f)
			{
				return false;
			}
			float num3 = num * num - num2;
			if (num3 < 0f)
			{
				return false;
			}
			t = 0f - num - math.sqrt(num3);
			if (t < 0f)
			{
				t = 0f;
			}
			q = p + t * d;
			return true;
		}

		public static bool IntersectLineSphare(float3 a, float3 b, float3 sc, float sr, out float3 q)
		{
			float3 x = b - a;
			float num = math.length(x);
			if (num == 0f)
			{
				q = a;
				return IntersectPointSphere(a, sc, sr);
			}
			float3 d = math.normalize(x);
			float t;
			if (IntersectRaySphere(a, d, sc, sr, out q, out t) && math.distance(a, q) < num)
			{
				return true;
			}
			return false;
		}

		public static bool IntersectRayCone(float3 o, float3 d, float3 c, float3 v, float cost, out float t, out float3 p)
		{
			p = 0;
			t = 0f;
			float num = cost * cost;
			float num2 = math.dot(d, v);
			float num3 = num2 * num2 - num;
			float3 @float = c - o;
			float num4 = math.dot(@float, v);
			float num5 = 2f * (num2 * num4 - math.dot(d, @float * num));
			float num6 = num4 * num4 - math.dot(@float, @float * num);
			float num7 = num5 * num5 - 4f * num3 * num6;
			if (num7 < 0f)
			{
				return false;
			}
			if (num7 == 0f)
			{
				t = (0f - num5) / (2f * num3);
				t = 0f - t;
				p = o + d * t;
				float num8 = math.dot(v, p - c);
				if (t < 0f || num8 < 0f)
				{
					return false;
				}
			}
			else
			{
				float num9 = math.sqrt(num7);
				float num10 = (0f - num5 - num9) / (2f * num3);
				float num11 = (0f - num5 + num9) / (2f * num3);
				num10 = 0f - num10;
				num11 = 0f - num11;
				float3 float2 = o + d * num10;
				float3 float3 = o + d * num11;
				float num12 = math.dot(v, float2 - c);
				float num13 = math.dot(v, float3 - c);
				bool flag = num10 >= 0f && num12 >= 0f;
				bool flag2 = num11 >= 0f && num13 >= 0f;
				if (flag && flag2)
				{
					if (num10 < num11)
					{
						t = num10;
						p = float2;
					}
					else
					{
						t = num11;
						p = float3;
					}
				}
				else if (flag)
				{
					t = num10;
					p = float2;
				}
				else
				{
					if (!flag2)
					{
						return false;
					}
					t = num11;
					p = float3;
				}
			}
			return true;
		}

		public static bool IntersectLineConeSurface(float3 a, float3 b, float3 d, float dlen, float3 c, float3 v, float cost, float3 c1, float3 c2, out float3 p)
		{
			p = 0;
			float t;
			if (!IntersectRayCone(a, d, c, v, cost, out t, out p))
			{
				return false;
			}
			if (t > dlen)
			{
				return false;
			}
			float3 @float = c2 - c1;
			float num = math.dot(p - c1, @float) / math.dot(@float, @float);
			if (num < 0f || num > 1f)
			{
				return false;
			}
			return true;
		}

		public static bool IntersectLineCylinderSurface(float3 sa, float3 sb, float3 p, float3 q, float r, out float t)
		{
			t = 0f;
			float3 @float = q - p;
			float3 float2 = sa - p;
			float3 float3 = sb - sa;
			float num = math.dot(float2, @float);
			float num2 = math.dot(float3, @float);
			float num3 = math.dot(@float, @float);
			if (num < 0f && num + num2 < 0f)
			{
				return false;
			}
			if (num > num3 && num + num2 > num3)
			{
				return false;
			}
			float num4 = math.dot(float3, float3);
			float num5 = math.dot(float2, float3);
			float num6 = num3 * num4 - num2 * num2;
			float num7 = math.dot(float2, float2) - r * r;
			float num8 = num3 * num7 - num * num;
			if (math.abs(num6) < 1E-06f)
			{
				return false;
			}
			float num9 = num3 * num5 - num2 * num;
			float num10 = num9 * num9 - num6 * num8;
			if (num10 < 0f)
			{
				return false;
			}
			t = (0f - num9 - math.sqrt(num10)) / num6;
			if (t < 0f || t > 1f)
			{
				return false;
			}
			if (num + t * num2 < 0f)
			{
				if (num2 <= 0f)
				{
					return false;
				}
				t = (0f - num) / num2;
				return num7 + 2f * t * (num5 + t * num4) <= 0f;
			}
			if (num + t * num2 > num3)
			{
				if (num2 >= 0f)
				{
					return false;
				}
				t = (num3 - num) / num2;
				return num7 + num3 - 2f * num + t * (2f * (num5 - num2) + t * num4) <= 0f;
			}
			return true;
		}

		public static bool IntersectLineCylinderSurface(float3 a, float3 b, float3 c1, float3 c2, float r1, float r2, out float3 p)
		{
			p = 0;
			if (math.abs(1f - r1 / r2) > 0.001f)
			{
				float3 @float = b - a;
				float num = math.length(@float);
				@float /= num;
				float num2 = math.distance(c1, c2);
				float num3 = 0f;
				float num4 = 0f;
				float num5 = 0f;
				float3 float2;
				float3 c3;
				float num6;
				float num7;
				if (r1 < r2)
				{
					float2 = c2 - c1;
					float2 /= num2;
					num3 = r2 - r1;
					num4 = r1 / (num3 / num2);
					c3 = c1 - float2 * num4;
					num6 = num2 + num4;
					num7 = r2;
				}
				else
				{
					float2 = c1 - c2;
					float2 /= num2;
					num3 = r1 - r2;
					num4 = r2 / (num3 / num2);
					c3 = c2 - float2 * num4;
					num6 = num2 + num4;
					num7 = r1;
				}
				float num8 = math.sqrt(num6 * num6 + num7 * num7);
				num5 = num6 / num8;
				return IntersectLineConeSurface(a, b, @float, num, c3, float2, num5, c1, c2, out p);
			}
			float t;
			if (IntersectLineCylinderSurface(a, b, c1, c2, r1, out t))
			{
				p = math.lerp(a, b, t);
				return true;
			}
			return false;
		}

		public static bool IntersectLineCapsule(float3 a, float3 b, float3 c1, float3 c2, float r1, float r2, out float3 p)
		{
			p = a;
			float s = ClosestPtPointSegmentRatio(a, c1, c2);
			float num = math.distance(a, math.lerp(c1, c2, s));
			float num2 = math.lerp(r1, r2, s);
			if (num <= num2)
			{
				return true;
			}
			float3 x = c2 - c1;
			if (IntersectLineSphare(a, b, c1, r1, out p) && math.dot(x, p - c1) <= 0f)
			{
				return true;
			}
			if (IntersectLineSphare(a, b, c2, r2, out p) && math.dot(x, p - c2) >= 0f)
			{
				return true;
			}
			return IntersectLineCylinderSurface(a, b, c1, c2, r1, r2, out p);
		}

		public static bool IntersectTrianglePointDistance(float3 p, float3 p0, float3 p1, float3 p2, float restDist, float compressionStiffness, float stretchStiffness, out float3 corr, out float3 corr0, out float3 corr1, out float3 corr2)
		{
			corr = 0;
			corr0 = 0;
			corr1 = 0;
			corr2 = 0;
			float num = 1f / 3f;
			float num2 = num;
			float num3 = num;
			float3 @float = p1 - p0;
			float3 float2 = p2 - p0;
			float3 x = p - p0;
			float num4 = math.dot(@float, @float);
			float num5 = math.dot(float2, @float);
			float num6 = math.dot(x, @float);
			float num7 = num5;
			float num8 = math.dot(float2, float2);
			float num9 = math.dot(x, float2);
			float num10 = num4 * num8 - num5 * num7;
			if (num10 != 0f)
			{
				float num11 = (num6 * num8 - num5 * num9) / num10;
				float num12 = (num4 * num9 - num6 * num7) / num10;
				num = 1f - num11 - num12;
				num2 = num11;
				num3 = num12;
				if (num < 0f)
				{
					float3 float3 = p2 - p1;
					float num13 = math.dot(float3, float3);
					num12 = ((num13 == 0f) ? 0.5f : (math.dot(float3, p - p1) / num13));
					if (num12 < 0f)
					{
						num12 = 0f;
					}
					if (num12 > 1f)
					{
						num12 = 1f;
					}
					num = 0f;
					num2 = 1f - num12;
					num3 = num12;
				}
				else if (num2 < 0f)
				{
					float3 float4 = p0 - p2;
					float num14 = math.dot(float4, float4);
					num12 = ((num14 == 0f) ? 0.5f : (math.dot(float4, p - p2) / num14));
					if (num12 < 0f)
					{
						num12 = 0f;
					}
					if (num12 > 1f)
					{
						num12 = 1f;
					}
					num2 = 0f;
					num3 = 1f - num12;
					num = num12;
				}
				else if (num3 < 0f)
				{
					float3 float5 = p1 - p0;
					float num15 = math.dot(float5, float5);
					num12 = ((num15 == 0f) ? 0.5f : (math.dot(float5, p - p0) / num15));
					if (num12 < 0f)
					{
						num12 = 0f;
					}
					if (num12 > 1f)
					{
						num12 = 1f;
					}
					num3 = 0f;
					num = 1f - num12;
					num2 = num12;
				}
			}
			float3 float6 = p0 * num + p1 * num2 + p2 * num3;
			float3 x2 = p - float6;
			float num16 = math.length(x2);
			if (num16 > restDist)
			{
				return false;
			}
			x2 = math.normalize(x2);
			float num17 = num16 - restDist;
			float3 float7 = x2;
			float3 float8 = -x2 * num;
			float3 float9 = -x2 * num2;
			float3 float10 = -x2 * num3;
			float num18 = 1f + num * num + num2 * num2 + num3 * num3;
			if (num18 == 0f)
			{
				return false;
			}
			num18 = num17 / num18;
			num18 = ((!(num17 < 0f)) ? (num18 * stretchStiffness) : (num18 * compressionStiffness));
			if (num18 == 0f)
			{
				return false;
			}
			corr = (0f - num18) * float7;
			corr0 = (0f - num18) * float8;
			corr1 = (0f - num18) * float9;
			corr2 = (0f - num18) * float10;
			return true;
		}

		public static bool IntersectTrianglePointDistanceSide(float3 p, float3 p0, float3 p1, float3 p2, float restDist, float compressionStiffness, float stretchStiffness, float side, out float3 corr, out float3 corr0, out float3 corr1, out float3 corr2)
		{
			corr = 0;
			corr0 = 0;
			corr1 = 0;
			corr2 = 0;
			float num = 1f / 3f;
			float num2 = num;
			float num3 = num;
			float3 @float = p1 - p0;
			float3 float2 = p2 - p0;
			float3 x = p - p0;
			float num4 = math.dot(@float, @float);
			float num5 = math.dot(float2, @float);
			float num6 = math.dot(x, @float);
			float num7 = num5;
			float num8 = math.dot(float2, float2);
			float num9 = math.dot(x, float2);
			float num10 = num4 * num8 - num5 * num7;
			if (num10 != 0f)
			{
				float num11 = (num6 * num8 - num5 * num9) / num10;
				float num12 = (num4 * num9 - num6 * num7) / num10;
				num = 1f - num11 - num12;
				num2 = num11;
				num3 = num12;
				if (num < 0f)
				{
					float3 float3 = p2 - p1;
					float num13 = math.dot(float3, float3);
					num12 = ((num13 == 0f) ? 0.5f : (math.dot(float3, p - p1) / num13));
					if (num12 < 0f)
					{
						num12 = 0f;
					}
					if (num12 > 1f)
					{
						num12 = 1f;
					}
					num = 0f;
					num2 = 1f - num12;
					num3 = num12;
				}
				else if (num2 < 0f)
				{
					float3 float4 = p0 - p2;
					float num14 = math.dot(float4, float4);
					num12 = ((num14 == 0f) ? 0.5f : (math.dot(float4, p - p2) / num14));
					if (num12 < 0f)
					{
						num12 = 0f;
					}
					if (num12 > 1f)
					{
						num12 = 1f;
					}
					num2 = 0f;
					num3 = 1f - num12;
					num = num12;
				}
				else if (num3 < 0f)
				{
					float3 float5 = p1 - p0;
					float num15 = math.dot(float5, float5);
					num12 = ((num15 == 0f) ? 0.5f : (math.dot(float5, p - p0) / num15));
					if (num12 < 0f)
					{
						num12 = 0f;
					}
					if (num12 > 1f)
					{
						num12 = 1f;
					}
					num3 = 0f;
					num = 1f - num12;
					num2 = num12;
				}
			}
			float3 float6 = p0 * num + p1 * num2 + p2 * num3;
			float3 float7 = p - float6;
			float num16 = math.length(float7);
			if (num16 > restDist)
			{
				return false;
			}
			float3 x2 = math.cross(@float, float2) * side;
			float num17 = num16 - restDist;
			if (math.dot(x2, float7) < 0f)
			{
				float7 = -float7;
			}
			float7 = math.normalize(float7);
			float3 float8 = float7;
			float3 float9 = -float7 * num;
			float3 float10 = -float7 * num2;
			float3 float11 = -float7 * num3;
			float num18 = 1f + num * num + num2 * num2 + num3 * num3;
			if (num18 == 0f)
			{
				return false;
			}
			num18 = num17 / num18;
			num18 = ((!(num17 < 0f)) ? (num18 * stretchStiffness) : (num18 * compressionStiffness));
			if (num18 == 0f)
			{
				return false;
			}
			corr = (0f - num18) * float8;
			corr0 = (0f - num18) * float9;
			corr1 = (0f - num18) * float10;
			corr2 = (0f - num18) * float11;
			return true;
		}

		public static bool IntersectTrianglePointDistanceSide2(float3 p, float3 p0, float3 p1, float3 p2, float radius, float restDist, float compressionStiffness, float stretchStiffness, float side, out float3 corr, out float3 corr0, out float3 corr1, out float3 corr2)
		{
			corr = 0;
			corr0 = 0;
			corr1 = 0;
			corr2 = 0;
			float num = 1f / 3f;
			float num2 = num;
			float num3 = num;
			float3 @float = p1 - p0;
			float3 float2 = p2 - p0;
			float3 x = p - p0;
			float num4 = math.dot(@float, @float);
			float num5 = math.dot(float2, @float);
			float num6 = math.dot(x, @float);
			float num7 = num5;
			float num8 = math.dot(float2, float2);
			float num9 = math.dot(x, float2);
			float num10 = num4 * num8 - num5 * num7;
			if (num10 != 0f)
			{
				float num11 = (num6 * num8 - num5 * num9) / num10;
				float num12 = (num4 * num9 - num6 * num7) / num10;
				num = 1f - num11 - num12;
				num2 = num11;
				num3 = num12;
				if (num < 0f)
				{
					float3 float3 = p2 - p1;
					float num13 = math.dot(float3, float3);
					num12 = ((num13 == 0f) ? 0.5f : (math.dot(float3, p - p1) / num13));
					if (num12 < 0f)
					{
						num12 = 0f;
					}
					if (num12 > 1f)
					{
						num12 = 1f;
					}
					num = 0f;
					num2 = 1f - num12;
					num3 = num12;
				}
				else if (num2 < 0f)
				{
					float3 float4 = p0 - p2;
					float num14 = math.dot(float4, float4);
					num12 = ((num14 == 0f) ? 0.5f : (math.dot(float4, p - p2) / num14));
					if (num12 < 0f)
					{
						num12 = 0f;
					}
					if (num12 > 1f)
					{
						num12 = 1f;
					}
					num2 = 0f;
					num3 = 1f - num12;
					num = num12;
				}
				else if (num3 < 0f)
				{
					float3 float5 = p1 - p0;
					float num15 = math.dot(float5, float5);
					num12 = ((num15 == 0f) ? 0.5f : (math.dot(float5, p - p0) / num15));
					if (num12 < 0f)
					{
						num12 = 0f;
					}
					if (num12 > 1f)
					{
						num12 = 1f;
					}
					num3 = 0f;
					num = 1f - num12;
					num2 = num12;
				}
			}
			float3 float6 = p0 * num + p1 * num2 + p2 * num3;
			float3 x2 = p - float6;
			float num16 = math.length(x2);
			if (num16 > restDist)
			{
				return false;
			}
			float3 float11 = math.cross(@float, float2) * side;
			float num17 = num16 - restDist;
			x2 = math.normalize(x2);
			float3 float7 = x2;
			float3 float8 = -x2 * num;
			float3 float9 = -x2 * num2;
			float3 float10 = -x2 * num3;
			float num18 = 1f + num * num + num2 * num2 + num3 * num3;
			if (num18 == 0f)
			{
				return false;
			}
			num18 = num17 / num18;
			num18 = ((!(num17 < 0f)) ? (num18 * stretchStiffness) : (num18 * compressionStiffness));
			if (num18 == 0f)
			{
				return false;
			}
			corr = (0f - num18) * float7;
			corr0 = (0f - num18) * float8;
			corr1 = (0f - num18) * float9;
			corr2 = (0f - num18) * float10;
			return true;
		}

		public static float DistanceTrianglePoint(float3 p, float3 p0, float3 p1, float3 p2)
		{
			float num = 1f / 3f;
			float num2 = num;
			float num3 = num;
			float3 @float = p1 - p0;
			float3 float2 = p2 - p0;
			float3 x = p - p0;
			float num4 = math.dot(@float, @float);
			float num5 = math.dot(float2, @float);
			float num6 = math.dot(x, @float);
			float num7 = num5;
			float num8 = math.dot(float2, float2);
			float num9 = math.dot(x, float2);
			float num10 = num4 * num8 - num5 * num7;
			if (num10 != 0f)
			{
				float num11 = (num6 * num8 - num5 * num9) / num10;
				float num12 = (num4 * num9 - num6 * num7) / num10;
				num = 1f - num11 - num12;
				num2 = num11;
				num3 = num12;
				if (num < 0f)
				{
					float3 float3 = p2 - p1;
					float num13 = math.dot(float3, float3);
					num12 = ((num13 == 0f) ? 0.5f : (math.dot(float3, p - p1) / num13));
					if (num12 < 0f)
					{
						num12 = 0f;
					}
					if (num12 > 1f)
					{
						num12 = 1f;
					}
					num = 0f;
					num2 = 1f - num12;
					num3 = num12;
				}
				else if (num2 < 0f)
				{
					float3 float4 = p0 - p2;
					float num14 = math.dot(float4, float4);
					num12 = ((num14 == 0f) ? 0.5f : (math.dot(float4, p - p2) / num14));
					if (num12 < 0f)
					{
						num12 = 0f;
					}
					if (num12 > 1f)
					{
						num12 = 1f;
					}
					num2 = 0f;
					num3 = 1f - num12;
					num = num12;
				}
				else if (num3 < 0f)
				{
					float3 float5 = p1 - p0;
					float num15 = math.dot(float5, float5);
					num12 = ((num15 == 0f) ? 0.5f : (math.dot(float5, p - p0) / num15));
					if (num12 < 0f)
					{
						num12 = 0f;
					}
					if (num12 > 1f)
					{
						num12 = 1f;
					}
					num3 = 0f;
					num = 1f - num12;
					num2 = num12;
				}
			}
			float3 float6 = p0 * num + p1 * num2 + p2 * num3;
			return math.length(p - float6);
		}

		public static float DistanceTriangleCenter(float3 p, float3 p0, float3 p1, float3 p2)
		{
			float3 y = (p0 + p1 + p2) / 3f;
			return math.distance(p, y);
		}

		public static float DirectionPointTriangle(float3 p, float3 a, float3 b, float3 c)
		{
			float3 x = b - a;
			float3 y = c - a;
			float3 x2 = p - a;
			float3 y2 = math.cross(x, y);
			return math.sign(math.dot(x2, y2));
		}

		public static bool IntersectLineTriangle(float3 p, float3 q, float3 a, float3 b, float3 c, out float3 hitpos, out float t, out float3 n)
		{
			hitpos = 0;
			t = 0f;
			float3 x = b - a;
			float3 @float = c - a;
			float3 x2 = p - q;
			n = math.cross(x, @float);
			float num = math.dot(x2, n);
			if (num <= 0f)
			{
				return false;
			}
			float3 float2 = p - a;
			t = math.dot(float2, n);
			if (t < 0f)
			{
				return false;
			}
			if (t > num)
			{
				return false;
			}
			float3 y = math.cross(x2, float2);
			float num2 = math.dot(@float, y);
			if (num2 < 0f || num2 > num)
			{
				return false;
			}
			float num3 = 0f - math.dot(x, y);
			if (num3 < 0f || num2 + num3 > num)
			{
				return false;
			}
			float num4 = 1f / num;
			t *= num4;
			num2 *= num4;
			num3 *= num4;
			float num5 = 1f - num2 - num3;
			hitpos = a * num5 + b * num2 + c * num3;
			return true;
		}

		public static float ClosestPtSegmentSegment(float3 p1, float3 q1, float3 p2, float3 q2, out float s, out float t, out float3 c1, out float3 c2)
		{
			s = 0f;
			t = 0f;
			float3 @float = q1 - p1;
			float3 float2 = q2 - p2;
			float3 y = p1 - p2;
			float num = math.dot(@float, @float);
			float num2 = math.dot(float2, float2);
			float num3 = math.dot(float2, y);
			if (num <= 1E-06f && num2 <= 1E-06f)
			{
				s = (t = 0f);
				c1 = p1;
				c2 = p2;
				return math.dot(c1 - c2, c1 - c2);
			}
			if (num <= 1E-06f)
			{
				s = 0f;
				t = math.saturate(num3 / num2);
			}
			else
			{
				float num4 = math.dot(@float, y);
				if (num2 <= 1E-06f)
				{
					t = 0f;
					s = math.saturate((0f - num4) / num);
				}
				else
				{
					float num5 = math.dot(@float, float2);
					float num6 = num * num2 - num5 * num5;
					if (num6 != 0f)
					{
						s = math.saturate((num5 * num3 - num4 * num2) / num6);
					}
					else
					{
						s = 0f;
					}
					t = (num5 * s + num3) / num2;
					if (t < 0f)
					{
						t = 0f;
						s = math.saturate((0f - num4) / num);
					}
					else if (t > 1f)
					{
						t = 1f;
						s = math.saturate((num5 - num4) / num);
					}
				}
			}
			c1 = p1 + @float * s;
			c2 = p2 + float2 * t;
			return math.dot(c1 - c2, c1 - c2);
		}

		public static float GetBezierValue(BezierParam bparam, float t)
		{
			return GetBezierValue(bparam.StartValue, bparam.EndValue, bparam.CurveValue, t);
		}

		public static float GetBezierValue(float sval, float eval, float curve, float t)
		{
			if (curve == 0f)
			{
				return math.lerp(sval, eval, t);
			}
			float num = math.lerp(eval, sval, curve * 0.5f + 0.5f);
			float num2 = 1f - t;
			return num2 * num2 * sval + 2f * num2 * t * num + t * t * eval;
		}
	}
}
