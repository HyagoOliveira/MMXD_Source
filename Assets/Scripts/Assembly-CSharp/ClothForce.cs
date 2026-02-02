using UnityEngine;

public class ClothForce : MonoBehaviour
{
	public Transform DirPoint;

	public Transform DirPoint1;

	public Transform Cloth1;

	public Transform Cloth2;

	public Transform Efx_trv;

	public Transform Efx_trv1;

	private void OnRenderObject()
	{
	}

	private void Update()
	{
		Vector3 normalized = (DirPoint.TransformPoint(DirPoint.localPosition) - base.transform.TransformPoint(base.transform.localPosition)).normalized;
		Vector3 normalized2 = (DirPoint1.TransformPoint(DirPoint1.localPosition) - base.transform.TransformPoint(base.transform.localPosition)).normalized;
		Cloth1.GetComponent<Cloth>().externalAcceleration = normalized;
		Cloth2.GetComponent<Cloth>().externalAcceleration = normalized2;
		Efx_trv.localPosition = Cloth1.GetComponent<Cloth>().vertices[OrangeBattleUtility.Random(70, 80)];
		Efx_trv1.localPosition = Cloth2.GetComponent<Cloth>().vertices[OrangeBattleUtility.Random(70, 80)];
	}
}
