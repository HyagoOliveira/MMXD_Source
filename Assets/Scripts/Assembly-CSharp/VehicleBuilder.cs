using System.Collections;
using CallbackDefs;
using UnityEngine;

public class VehicleBuilder : MonoBehaviour
{
	private enum VehicleParts
	{
		VehicleObject = 0,
		BodyMesh = 1,
		MAX_PARTS = 2
	}

	public int VehicleID;

	public bool CreateAtStart;

	private Object[] _loadedParts;

	private int _loadedCount;

	private VEHICLE_TABLE _currentVehicleTable;

	private void Start()
	{
		if (CreateAtStart)
		{
			CreateVehicle();
		}
	}

    [System.Obsolete]
    public void CreateVehicle(CallbackObjs pCallBack = null)
	{
		_loadedCount = 0;
		_currentVehicleTable = ManagedSingleton<OrangeDataManager>.Instance.VEHICLE_TABLE_DICT[VehicleID];
		_loadedParts = new Object[2];
		string text = "ridearmorbase";
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/" + text, text, delegate(Object obj)
		{
			_loadedParts[0] = obj;
			_loadedCount++;
		});
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("model/vehicle/" + _currentVehicleTable.s_MODEL, _currentVehicleTable.s_MODEL, delegate(Object obj)
		{
			_loadedParts[1] = obj;
			_loadedCount++;
		});
		StartCoroutine(Build(pCallBack));
	}

    [System.Obsolete]
    private IEnumerator Build(CallbackObjs pCallBack)
	{
		while (_loadedCount != 2)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		GameObject gameObject = Object.Instantiate((GameObject)_loadedParts[0], base.transform.position, Quaternion.identity);
		GameObject obj = Object.Instantiate((GameObject)_loadedParts[1], Vector3.zero, Quaternion.identity);
		obj.name = "model";
		obj.transform.SetParent(gameObject.transform);
		obj.transform.eulerAngles = new Vector3(0f, 90f, 0f);
		obj.transform.localPosition = Vector3.zero;
		gameObject.GetComponent<RideArmorController>().Activate = true;
		Object.Destroy(base.gameObject);
	}
}
