using UnityEngine;

public class RotateSelf : MonoBehaviour
{
	public bool bRotateCenter = true;

	public Vector3 vRotateAxis = Vector3.up;

	public Vector3 vRotateCenter = Vector3.zero;

	public float fRotateSpeed = 1f;

	private Vector3 originpos = Vector3.zero;

	private Quaternion originrotate = Quaternion.identity;

	private bool bInit;

	private void Start()
	{
		originpos = base.transform.localPosition;
		originrotate = base.transform.localRotation;
		if (bRotateCenter)
		{
			vRotateCenter = base.transform.position;
		}
		bInit = true;
	}

	private void Update()
	{
		base.transform.RotateAround(vRotateCenter, vRotateAxis, fRotateSpeed * (Time.deltaTime * 20f));
	}

	public void Init()
	{
		if (bInit)
		{
			base.transform.localPosition = originpos;
			base.transform.localRotation = originrotate;
			if (bRotateCenter)
			{
				vRotateCenter = base.transform.position;
			}
		}
	}
}
