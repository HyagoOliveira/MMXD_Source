#define RELEASE
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageComponent : MonoBehaviour
{
	private List<StorageInfo> listStorages = new List<StorageInfo>();

	private List<StorageTab> listStorageTab = new List<StorageTab>();

	[SerializeField]
	private RectTransform rt;

	[SerializeField]
	private ContentSizeFitter layout;

	[SerializeField]
	private StorageTab InstTab;

	public void Setup(List<StorageInfo> p_listStorages, int defaultIdx = 0, int defaultSubIdx = 0)
	{
		listStorages = p_listStorages;
		for (int i = 0; i < listStorages.Count; i++)
		{
			StorageInfo storageInfo = listStorages[i];
			storageInfo.AnimAddDelay = 0.05f * (float)i;
			CreateTabs(storageInfo);
		}
		InstTab.gameObject.SetActive(false);
		if (defaultIdx >= listStorageTab.Count)
		{
			Debug.LogWarning("[StorageComponent]defaultIdx is out of range " + defaultIdx);
			defaultIdx = 0;
		}
		else if (defaultIdx < 0)
		{
			Debug.LogWarning("[StorageComponent]defaultIdx is out of range : " + defaultIdx);
			defaultIdx = 0;
		}
		StorageInfo storageInfo2 = listStorages[defaultIdx];
		if (storageInfo2.Sub.Length != 0 && defaultSubIdx >= storageInfo2.Sub.Length)
		{
			Debug.LogWarning("[StorageComponent]defaultSubIdx is out of range.");
			defaultSubIdx = 0;
		}
		foreach (StorageTab item in listStorageTab)
		{
			item.UpdateTabBtn(storageInfo2, defaultSubIdx);
		}
		if (storageInfo2.Sub == null || storageInfo2.Sub.Length < 1)
		{
			storageInfo2.ClickCb.CheckTargetToInvoke(storageInfo2);
		}
		rt.RebuildLayout();
	}

	public void CreateTabs(StorageInfo storageInfo)
	{
		StorageTab storageTab = Object.Instantiate(InstTab, base.transform);
		storageTab.Setup(storageInfo, UpdateUnitState);
		listStorageTab.Add(storageTab);
	}

	private void UpdateUnitState(object p_param)
	{
		StorageInfo p_storageInfo = p_param as StorageInfo;
		foreach (StorageTab item in listStorageTab)
		{
			item.UpdateTabBtn(p_storageInfo);
		}
		rt.RebuildLayout();
	}

	public void UpdateHint()
	{
		foreach (StorageTab item in listStorageTab)
		{
			item.UpdateHint();
		}
	}
}
