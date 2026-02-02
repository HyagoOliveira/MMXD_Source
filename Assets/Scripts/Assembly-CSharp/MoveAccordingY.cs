using System.Collections.Generic;
using UnityEngine;

public class MoveAccordingY : MonoBehaviour
{
	public GameObject[] MoveNode;

	private float fSetY;

	private bool bInit;

	public void SetYAndMove(float fSet)
	{
		if (MoveNode == null || MoveNode.Length == 0)
		{
			return;
		}
		if (!bInit)
		{
			bInit = true;
			List<GameObject> list = new List<GameObject>();
			list.AddRange(MoveNode);
			list.Sort(delegate(GameObject a, GameObject b)
			{
				if (a.transform.position.y > b.transform.position.y)
				{
					return -1;
				}
				return (a.transform.position.y < b.transform.position.y) ? 1 : 0;
			});
			MoveNode = list.ToArray();
		}
		for (int i = 0; i < MoveNode.Length - 1; i++)
		{
			float y = MoveNode[i].transform.position.y;
			float y2 = MoveNode[i + 1].transform.position.y;
			if (y > fSet && fSet > y2)
			{
				float num = 1f - (y - fSet) / (y - y2);
				Vector3 position = MoveNode[i].transform.position;
				Vector3 position2 = MoveNode[i + 1].transform.position;
				Vector3 position3 = position2 + num * (position - position2);
				base.transform.position = position3;
				break;
			}
		}
	}
}
