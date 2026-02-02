using System.Collections.Generic;
using Newtonsoft.Json.Utilities;
using UnityEngine;

public class AotTypeEnforcer : MonoBehaviour
{
	public void Awake()
	{
		AotHelper.EnsureList<int>();
		AotHelper.EnsureList<float>();
		AotHelper.EnsureList<decimal>();
		AotHelper.EnsureList<string>();
		AotHelper.EnsureType<HashSet<int>>();
		AotHelper.EnsureType<HashSet<string>>();
	}
}
