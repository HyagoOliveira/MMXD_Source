using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	[Serializable]
	public abstract class ShareDataObject : ScriptableObject, IDataVerify, IDataHash
	{
		[SerializeField]
		protected int dataHash;

		[SerializeField]
		protected int dataVersion;

		public int SaveDataHash
		{
			get
			{
				return dataHash;
			}
			set
			{
				dataHash = value;
			}
		}

		public int SaveDataVersion
		{
			get
			{
				return dataVersion;
			}
			set
			{
				dataVersion = value;
			}
		}

		public abstract int GetDataHash();

		public abstract int GetVersion();

		public abstract Define.Error VerifyData();

		public virtual void CreateVerifyData()
		{
			dataHash = GetDataHash();
			dataVersion = GetVersion();
		}

		public virtual string GetInformation()
		{
			return "No information.";
		}

		public static T CreateShareData<T>(string dataName) where T : ShareDataObject
		{
			T val = ScriptableObject.CreateInstance<T>();
			val.name = dataName;
			return val;
		}

		public static bool RemoveNullAndDuplication<T>(List<T> data)
		{
			bool result = false;
			int num = 0;
			while (num < data.Count)
			{
				T val = data[num];
				if (val == null)
				{
					data.RemoveAt(num);
					result = true;
				}
				else if (data.IndexOf(val) < num)
				{
					data.RemoveAt(num);
					result = true;
				}
				else
				{
					num++;
				}
			}
			return result;
		}

		public static T Clone<T>(T source) where T : ShareDataObject
		{
			if (source == null)
			{
				return null;
			}
			T val = UnityEngine.Object.Instantiate(source);
			val.name = source.name;
			return val;
		}
	}
}
