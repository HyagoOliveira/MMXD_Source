using System;
using UnityEngine;

namespace MagicaCloth
{
	[Serializable]
	public class SpringData : ShareDataObject
	{
		[Serializable]
		public class DeformerData : IDataHash
		{
			public int deformerDataHash;

			public int vertexCount;

			public int[] useVertexIndexList;

			public float[] weightList;

			public int UseVertexCount
			{
				get
				{
					if (useVertexIndexList != null)
					{
						return useVertexIndexList.Length;
					}
					return 0;
				}
			}

			public int GetDataHash()
			{
				return 0 + deformerDataHash + vertexCount.GetDataHash() + UseVertexCount.GetDataHash();
			}
		}

		private const int DATA_VERSION = 2;

		public DeformerData deformerData;

		public Vector3 initScale;

		public int UseVertexCount
		{
			get
			{
				int num = 0;
				if (deformerData != null)
				{
					num += deformerData.UseVertexCount;
				}
				return num;
			}
		}

		public override int GetDataHash()
		{
			int num = 0;
			if (deformerData != null)
			{
				num += deformerData.GetDataHash();
			}
			return num;
		}

		public override int GetVersion()
		{
			return 2;
		}

		public override Define.Error VerifyData()
		{
			if (dataHash == 0)
			{
				return Define.Error.InvalidDataHash;
			}
			if (deformerData == null)
			{
				return Define.Error.DeformerNull;
			}
			return Define.Error.None;
		}
	}
}
