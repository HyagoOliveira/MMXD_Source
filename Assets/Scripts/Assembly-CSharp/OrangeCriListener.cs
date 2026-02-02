using CriWare;
using UnityEngine;

public class OrangeCriListener : CriAtomListener
{
	[SerializeField]
	private Vector3 offset = new Vector3(0f, 0f, 0f);

	[SerializeField]
	public GameObject attachObj;

	public OrangeCriSource OCS;

	private void Awake()
	{
		OCS = base.gameObject.AddOrGetComponent<OrangeCriSource>();
		OCS.Initial(OrangeSSType.SYSTEM);
		OCS._currentDis = 0f;
		OCS.IsVisiable = true;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (attachObj != null)
		{
			base.transform.position = attachObj.transform.position + offset;
		}
	}
}
