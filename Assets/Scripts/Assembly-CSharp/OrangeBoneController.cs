using System.Collections.Generic;
using UnityEngine;

public class OrangeBoneController : MonoBehaviour
{
	[SerializeField]
	private List<OrangeBoneScaleData> listScale = new List<OrangeBoneScaleData>();

	[SerializeField]
	private List<OrangeBonePositionZData> listPositionZ = new List<OrangeBonePositionZData>();

	private void LateUpdate()
	{
		foreach (OrangeBoneScaleData item in listScale)
		{
			item.transform.localScale = item.Scale;
		}
		foreach (OrangeBonePositionZData item2 in listPositionZ)
		{
			Vector3 localPosition = item2.transform.localPosition;
			item2.transform.localPosition = new Vector3(localPosition.x, localPosition.y, item2.z);
		}
	}
}
