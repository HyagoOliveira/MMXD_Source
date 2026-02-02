using UnityEngine;

public class ClothForceG : MonoBehaviour
{
	public Transform DirPoint;

	public Transform Cloth1;

	private OrangeCharacter OC;

	private Transform Biptrs;

	private void Start()
	{
		OC = base.transform.root.GetComponent<OrangeCharacter>();
		Biptrs = OrangeBattleUtility.FindChildRecursive(OC.transform, "Bip");
	}

	private void Update()
	{
		Vector3 normalized = (DirPoint.TransformPoint(DirPoint.localPosition) - base.transform.TransformPoint(base.transform.localPosition)).normalized;
		Cloth1.GetComponent<Cloth>().externalAcceleration = normalized;
		base.transform.localPosition = new Vector3(Biptrs.localPosition.x - 0.1f, Biptrs.localPosition.y + 0.3f, Biptrs.localPosition.z);
	}
}
