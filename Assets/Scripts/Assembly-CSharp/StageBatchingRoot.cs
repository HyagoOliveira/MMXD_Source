using System.Collections.Generic;
using StageLib;
using UnityEngine;
using UnityEngine.Rendering;

public class StageBatchingRoot : MonoBehaviour
{
	private bool activeShadow;

	public void Start()
	{
		activeShadow = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.sw;
		OnBatching();
	}

	private void OnBatching()
	{
		Dictionary<string, List<GameObject>> dict = new Dictionary<string, List<GameObject>>();
		FindChildForBatching(base.transform, ref dict);
		foreach (List<GameObject> value in dict.Values)
		{
			if (value.Count >= 2)
			{
				StaticBatchingUtility.Combine(value.ToArray(), base.gameObject);
			}
		}
	}

	private void FindChildForBatching(Transform p_transform, ref Dictionary<string, List<GameObject>> dict)
	{
		if ((bool)p_transform.GetComponent<MapObjEvent>() || (bool)p_transform.GetComponent<MapCollisionEvent>())
		{
			return;
		}
		string empty = string.Empty;
		for (int i = 0; i < p_transform.childCount; i++)
		{
			Transform child = p_transform.GetChild(i);
			bool activeSelf = child.gameObject.activeSelf;
			if (!activeSelf)
			{
				child.gameObject.SetActive(true);
			}
			if ((bool)child.GetComponentInChildren<FallingFloor>() || (bool)child.GetComponentInChildren<RaycastController>() || (bool)child.GetComponent<Animation>())
			{
				child.gameObject.SetActive(activeSelf);
				continue;
			}
			if ((bool)child.GetComponent<MapObjEvent>())
			{
				child.gameObject.SetActive(activeSelf);
				continue;
			}
			MapCollisionEvent component = child.GetComponent<MapCollisionEvent>();
			if (component != null && component.mapEvent != MapCollisionEvent.MapCollisionEnum.COLLISION_TRACK && component.mapEvent != MapCollisionEvent.MapCollisionEnum.COLLISION_QUICKSAND)
			{
				StageSceneObjParam[] componentsInChildren = base.transform.GetComponentsInChildren<StageSceneObjParam>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].InitMeshMaterial();
				}
				child.gameObject.SetActive(activeSelf);
				continue;
			}
			MeshFilter component2 = child.GetComponent<MeshFilter>();
			if ((bool)component2 && component2.sharedMesh != null && component2.sharedMesh.isReadable)
			{
				MeshRenderer component3 = child.GetComponent<MeshRenderer>();
				if ((bool)component3)
				{
					if (!activeShadow && component3.shadowCastingMode != ShadowCastingMode.ShadowsOnly)
					{
						component3.shadowCastingMode = ShadowCastingMode.Off;
					}
					if (component3.sharedMaterial != null)
					{
						empty = component3.sharedMaterial.name;
						if (!dict.ContainsKey(empty))
						{
							dict.Add(empty, new List<GameObject>());
						}
						dict[empty].Add(component3.gameObject);
					}
				}
			}
			FindChildForBatching(child, ref dict);
			child.gameObject.SetActive(activeSelf);
		}
	}
}
