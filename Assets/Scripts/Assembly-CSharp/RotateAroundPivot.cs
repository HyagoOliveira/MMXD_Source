#define RELEASE
using UnityEngine;

public class RotateAroundPivot : MonoBehaviour
{
	public Vector3 Pivot;

	public bool DebugInfo = true;

	public bool RotateX;

	public bool RotateY;

	public bool RotateZ;

	public int speed = 45;

	private void FixedUpdate()
	{
		base.transform.position += base.transform.rotation * Pivot;
		if (RotateX)
		{
			base.transform.rotation *= Quaternion.AngleAxis((float)speed * Time.deltaTime, Vector3.right);
		}
		if (RotateY)
		{
			base.transform.rotation *= Quaternion.AngleAxis((float)speed * Time.deltaTime, Vector3.up);
		}
		if (RotateZ)
		{
			base.transform.rotation *= Quaternion.AngleAxis((float)speed * Time.deltaTime, Vector3.forward);
		}
		base.transform.position -= base.transform.rotation * Pivot;
		if (DebugInfo)
		{
			Debug.DrawRay(base.transform.position, base.transform.rotation * Vector3.up, Color.black);
			Debug.DrawRay(base.transform.position, base.transform.rotation * Vector3.right, Color.black);
			Debug.DrawRay(base.transform.position, base.transform.rotation * Vector3.forward, Color.black);
			Debug.DrawRay(base.transform.position + base.transform.rotation * Pivot, base.transform.rotation * Vector3.up, Color.green);
			Debug.DrawRay(base.transform.position + base.transform.rotation * Pivot, base.transform.rotation * Vector3.right, Color.red);
			Debug.DrawRay(base.transform.position + base.transform.rotation * Pivot, base.transform.rotation * Vector3.forward, Color.blue);
		}
	}
}
