using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LoopInfo
{
	[SerializeField]
	public OrangeCriPoint point;

	[SerializeField]
	public List<OrangeCriSource> listSource = new List<OrangeCriSource>();
}
