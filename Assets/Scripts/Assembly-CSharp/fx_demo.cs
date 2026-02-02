using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class fx_demo : MonoBehaviour
{
	private int index;

	private FileInfo[] fileInfo;

	private List<GameObject> mNowObj;

	public void addObject(int min, int max)
	{
	}

	private void Start()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + "/Main/Art/Fx/CSLO/Prefabs");
		fileInfo = directoryInfo.GetFiles("*.prefab");
		mNowObj = new List<GameObject>();
		addObject(0, 50);
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 10f, 50f, 50f), "up 50") && index > 0)
		{
			index--;
			addObject(index * 50, (index + 1) * 50);
		}
		if (GUI.Button(new Rect(10f, 70f, 50f, 30f), "next 50"))
		{
			if ((index + 2) * 50 < fileInfo.Length)
			{
				index++;
				addObject(index * 50, (index + 1) * 50);
			}
			else
			{
				addObject(index * 50, fileInfo.Length);
			}
		}
	}
}
