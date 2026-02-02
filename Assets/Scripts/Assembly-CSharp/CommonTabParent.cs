using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonTabParent : MonoBehaviour
{
	private List<CommonTab> tabList = new List<CommonTab>();

	private int currentSelectedTabIndex;

	public bool bPlaySE;

	private void Start()
	{
		int num = 0;
		foreach (Transform item in base.transform)
		{
			CommonTab component = item.gameObject.GetComponent<CommonTab>();
			if (!component)
			{
				continue;
			}
			Button componentInChildren = component.GetComponentInChildren<Button>();
			if ((bool)componentInChildren)
			{
				int tempIndex = num;
				componentInChildren.onClick.AddListener(delegate
				{
					OnClickTab(tempIndex);
				});
			}
			component.Setup(num);
			tabList.Add(component);
			num++;
		}
	}

	private void Update()
	{
	}

	private void OnClickTab(int index)
	{
		currentSelectedTabIndex = index;
	}

	public int GetSelectedTabIndex()
	{
		return currentSelectedTabIndex;
	}

	public CommonTab GetTabByIndex(int index)
	{
		if (index < tabList.Count)
		{
			return tabList[index];
		}
		return null;
	}

	public void SetDefaultTabIndex(int index)
	{
		if (index < base.transform.childCount)
		{
			currentSelectedTabIndex = index;
		}
	}
}
