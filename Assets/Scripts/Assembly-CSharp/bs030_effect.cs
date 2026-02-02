using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class bs030_effect : MonoBehaviour
{
	public Transform[] body;

	public Material mMaterial;

	private List<Material[]> skinnedMaterials = new List<Material[]>();

	private bool isActive;

	private const string materialName = "MeshEffect";

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

	private void AddEFXMaterials()
	{
		for (int i = 0; i < body.Length; i++)
		{
			List<Material> list = skinnedMaterials[i].ToList();
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].name.Contains("MeshEffect"))
				{
					list.RemoveAt(j);
				}
			}
			list.Add(mMaterial);
			body[i].GetComponent<SkinnedMeshRenderer>().sharedMaterials = list.ToArray();
		}
	}

	private void RemoveEFXMaterials()
	{
		for (int i = 0; i < body.Length; i++)
		{
			List<Material> list = skinnedMaterials[i].ToList();
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].name.Contains("MeshEffect"))
				{
					list.RemoveAt(j);
				}
			}
			body[i].GetComponent<SkinnedMeshRenderer>().sharedMaterials = list.ToArray();
		}
	}

	public void ActiveEffect(bool value, float time1, float time2)
	{
		if (value)
		{
			Color mColor2 = new Color(1f, 1f, 1f, 1f);
			mMaterial.SetColor("_TintColor", mColor2);
			mMaterial.SetFloat("_R0", 0f);
			mMaterial.SetFloat("_BumpAmt", 0f);
			AddEFXMaterials();
			LeanTween.value(1f, 0f, time1).setOnUpdate(delegate(float f)
			{
				mColor2 = new Color(f, f, f, 1f);
				mMaterial.SetColor("_TintColor", mColor2);
			}).setIgnoreTimeScale(true)
				.setOnComplete((Action)delegate
				{
					RemoveEFXMaterials();
					ActiveEffect(true);
					LeanTween.value(0f, 1f, time2).setOnUpdate(delegate(float f)
					{
						mColor2 = new Color(f, f, f, 1f);
						mMaterial.SetColor("_TintColor", mColor2);
					}).setIgnoreTimeScale(true);
					LeanTween.value(0f, 0.2f, time2).setOnUpdate(delegate(float f)
					{
						mMaterial.SetFloat("_R0", f);
					}).setIgnoreTimeScale(true);
					LeanTween.value(0f, 8f, time2).setOnUpdate(delegate(float f)
					{
						mMaterial.SetFloat("_BumpAmt", f);
					}).setIgnoreTimeScale(true);
				});
			return;
		}
		Color mColor = new Color(1f, 1f, 1f, 1f);
		mMaterial.SetColor("_TintColor", mColor);
		mMaterial.SetFloat("_R0", 0.2f);
		mMaterial.SetFloat("_BumpAmt", 8f);
		LeanTween.value(1f, 0f, time1).setOnUpdate(delegate(float f)
		{
			mColor = new Color(f, f, f, 1f);
			mMaterial.SetColor("_TintColor", mColor);
		}).setIgnoreTimeScale(true);
		LeanTween.value(0.2f, 0f, time1).setOnUpdate(delegate(float f)
		{
			mMaterial.SetFloat("_R0", f);
		}).setIgnoreTimeScale(true);
		LeanTween.value(8f, 0f, time1).setOnUpdate(delegate(float f)
		{
			mMaterial.SetFloat("_BumpAmt", f);
		}).setIgnoreTimeScale(true)
			.setOnComplete((Action)delegate
			{
				ActiveEffect(false);
				AddEFXMaterials();
				LeanTween.value(0f, 1f, time2).setOnUpdate(delegate(float f)
				{
					mColor = new Color(f, f, f, 1f);
					mMaterial.SetColor("_TintColor", mColor);
				}).setIgnoreTimeScale(true)
					.setOnComplete(RemoveEFXMaterials);
			});
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
