using System;
using Newtonsoft.Json;
using UnityEngine;

namespace StageLib
{
	[Serializable]
	public class StageObjData
	{
		public string sGroupID = "";

		public string name;

		[JsonConverter(typeof(Vector3Converter))]
		public Vector3 position;

		[JsonConverter(typeof(Vector3Converter))]
		public Vector3 scale;

		[JsonConverter(typeof(QuaternionConverter))]
		public Quaternion rotate;

		public string path;

		public string bunldepath;

		public string property;
	}
}
