using System;
using System.Collections.Generic;
using UnityEngine;

public class ModelTransform : MonoBehaviour
{
	[Serializable]
	public class ModelTransformData
	{
		public List<string> listEnableModel = new List<string>();

		public int nStatus;
	}

	public int nForwardSpeed = 5000;

	public int nBackSpeed = 3000;

	public int nUpSpeed = 2000;

	public int nDownSpeed = 2000;

	public float fTransfromDis = 2.5f;

	public List<ModelTransformData> listMTDs = new List<ModelTransformData>();
}
