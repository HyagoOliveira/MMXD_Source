using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	public static class DataHashExtensions
	{
		public const int NullHash = 397610387;

		public const int NumberHash = 932781045;

		public static int GetDataHash(this object data)
		{
			Object @object = data as Object;
			if ((object)@object != null)
			{
				if (@object != null)
				{
					if (@object is Transform)
					{
						return (@object as Transform).name.GetHashCode();
					}
					if (@object is GameObject)
					{
						return (@object as GameObject).name.GetHashCode();
					}
					if (@object is Mesh)
					{
						Mesh mesh = @object as Mesh;
						return 0 + mesh.vertexCount.GetDataHash() + mesh.triangles.Length.GetDataHash() + mesh.subMeshCount.GetDataHash() + mesh.isReadable.GetDataHash();
					}
					return 932781045 + data.GetHashCode();
				}
				return 397610387;
			}
			if (data != null)
			{
				return 932781045 + data.GetHashCode();
			}
			return 397610387;
		}

		public static int GetDataHash(this IDataHash data)
		{
			return data.GetDataHash();
		}

		public static int GetDataHash<T>(this T[] data)
		{
			int num = 0;
			if (data != null)
			{
				foreach (T val in data)
				{
					num *= 31;
					IDataHash dataHash = val as IDataHash;
					num = ((dataHash == null) ? (num + val.GetDataHash()) : (num + dataHash.GetDataHash()));
				}
			}
			return num;
		}

		public static int GetDataHash<T>(this List<T> data)
		{
			int num = 0;
			if (data != null)
			{
				foreach (T datum in data)
				{
					num *= 31;
					IDataHash dataHash = datum as IDataHash;
					num = ((dataHash == null) ? (num + datum.GetDataHash()) : (num + dataHash.GetDataHash()));
				}
			}
			return num;
		}

		public static int GetDataCountHash<T>(this T[] data)
		{
			if (data == null)
			{
				return 397610387;
			}
			return data.Length.GetDataHash();
		}

		public static int GetDataCountHash<T>(this List<T> data)
		{
			if (data == null)
			{
				return 397610387;
			}
			return data.Count.GetDataHash();
		}

		public static ulong GetVectorDataHash(Vector3 v)
		{
			uint3 @uint = math.asuint(v);
			ulong num = @uint.x;
			ulong num2 = @uint.y;
			ulong num3 = @uint.z;
			num += 1759446883;
			num2 += 1513705375;
			num3 += 3767841571u;
			num ^= num << 13;
			num2 ^= num2 >> 17;
			num3 ^= num3 << 15;
			num *= 2601761069u;
			num2 *= 1254033427;
			num3 *= 2248573027u;
			return num + num2 + num3 + 3612677113u;
		}
	}
}
