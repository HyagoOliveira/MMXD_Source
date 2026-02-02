using UnityEngine;

namespace StageLib
{
	public class PosShower : MonoBehaviour
	{
		[HideInInspector]
		public Color ShowColor = Color.yellow;

		[HideInInspector]
		public Vector3 rect = new Vector3(1f, 1f, 1f);

		[HideInInspector]
		public bool bWire = true;

		private void OnDrawGizmos()
		{
			Gizmos.color = ShowColor;
			Gizmos.DrawSphere(base.transform.position, 0.1f);
			if (bWire)
			{
				Gizmos.DrawWireCube(base.transform.position, rect);
			}
			else
			{
				Gizmos.DrawCube(base.transform.position, rect);
			}
		}
	}
}
