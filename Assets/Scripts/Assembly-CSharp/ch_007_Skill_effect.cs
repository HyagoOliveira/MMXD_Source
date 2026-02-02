using System;
using System.Collections.Generic;
using UnityEngine;

public class ch_007_Skill_effect : MonoBehaviour
{
	public Transform[] body;

	public Material mMaterial;

	private List<Material[]> skinnedMaterials = new List<Material[]>();

	public bool isActive;

	private void Start()
	{
		if (body.Length != 0)
		{
			for (int i = 0; i < body.Length; i++)
			{
				skinnedMaterials.Add(body[i].GetComponent<SkinnedMeshRenderer>().sharedMaterials);
			}
		}
	}

	public void ActiveEffect(bool value, float time)
	{
		if (value)
		{
			ActiveEffect(true);
			LeanTween.value(0f, 1f, time).setOnUpdate((Action<float>)delegate
			{
			}).setIgnoreTimeScale(true)
				.setOnComplete((Action)delegate
				{
					ActiveEffect(false);
				});
		}
	}

	public void ActiveEffect(bool value)
	{
		if (isActive == value)
		{
			return;
		}
		isActive = value;
		if (!value)
		{
			for (int i = 0; i < body.Length; i++)
			{
				body[i].GetComponent<SkinnedMeshRenderer>().sharedMaterials = skinnedMaterials[i];
			}
			return;
		}
		for (int j = 0; j < body.Length; j++)
		{
			body[j].GetComponent<SkinnedMeshRenderer>().sharedMaterials = new Material[1] { mMaterial };
		}
	}
}
