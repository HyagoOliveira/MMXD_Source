#define RELEASE
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class PSMeshRendererUpdater : MonoBehaviour
{
	public GameObject MeshObject;

	private const string materialName = "MeshEffect";

	private List<Material[]> rendererMaterials = new List<Material[]>();

	private List<Material[]> skinnedMaterials = new List<Material[]>();

	public bool IsActive = true;

	private bool currentActiveStatus = true;

	private void Update()
	{
		if (currentActiveStatus != IsActive)
		{
			currentActiveStatus = IsActive;
			Activation(currentActiveStatus);
		}
	}

	public void Activation(bool activeStatus)
	{
		if (MeshObject == null)
		{
			return;
		}
		ParticleSystem[] componentsInChildren = MeshObject.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			if (activeStatus)
			{
				particleSystem.Play();
			}
			else
			{
				particleSystem.Stop();
			}
		}
		Light componentInChildren = MeshObject.GetComponentInChildren<Light>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = IsActive;
		}
		MeshRenderer componentInChildren2 = MeshObject.GetComponentInChildren<MeshRenderer>();
		if (componentInChildren2 != null)
		{
			Material material = componentInChildren2.sharedMaterials[componentInChildren2.sharedMaterials.Length - 1];
			Color value = Color.black;
			if (material.HasProperty("_TintColor"))
			{
				value = material.GetColor("_TintColor");
			}
			value.a = (activeStatus ? 1 : 0);
			componentInChildren2.sharedMaterials[componentInChildren2.sharedMaterials.Length - 1].SetColor("_TintColor", value);
		}
		SkinnedMeshRenderer componentInChildren3 = MeshObject.GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren3 != null)
		{
			Material material2 = componentInChildren3.sharedMaterials[componentInChildren3.sharedMaterials.Length - 1];
			Color value2 = Color.black;
			if (material2.HasProperty("_TintColor"))
			{
				value2 = material2.GetColor("_TintColor");
			}
			value2.a = (activeStatus ? 1 : 0);
			componentInChildren3.sharedMaterials[componentInChildren3.sharedMaterials.Length - 1].SetColor("_TintColor", value2);
		}
	}

	public void ActiveEffect(bool value)
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		if (MeshObject == null)
		{
			return;
		}
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem particleSystem in array)
		{
			if (value)
			{
				particleSystem.Play();
			}
			else
			{
				particleSystem.Stop();
			}
		}
		if (!value)
		{
			Activation(true);
			if (MeshObject == null)
			{
				return;
			}
			MeshRenderer[] componentsInChildren2 = MeshObject.GetComponentsInChildren<MeshRenderer>();
			SkinnedMeshRenderer[] componentsInChildren3 = MeshObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				if (rendererMaterials.Count == componentsInChildren2.Length)
				{
					componentsInChildren2[j].sharedMaterials = rendererMaterials[j];
				}
				List<Material> list = componentsInChildren2[j].sharedMaterials.ToList();
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k].name.Contains("MeshEffect"))
					{
						list.RemoveAt(k);
					}
				}
				componentsInChildren2[j].sharedMaterials = list.ToArray();
			}
			for (int l = 0; l < componentsInChildren3.Length; l++)
			{
				if (skinnedMaterials.Count == componentsInChildren3.Length)
				{
					componentsInChildren3[l].sharedMaterials = skinnedMaterials[l];
				}
				List<Material> list2 = componentsInChildren3[l].sharedMaterials.ToList();
				for (int m = 0; m < list2.Count; m++)
				{
					if (list2[m].name.Contains("MeshEffect"))
					{
						list2.RemoveAt(m);
					}
				}
				componentsInChildren3[l].sharedMaterials = list2.ToArray();
			}
			rendererMaterials.Clear();
			skinnedMaterials.Clear();
		}
		IsActive = value;
	}

	public void UpdateMeshEffect()
	{
		rendererMaterials.Clear();
		skinnedMaterials.Clear();
		if (!(MeshObject == null))
		{
			UpdatePSMesh(MeshObject);
			AddMaterialToMesh(MeshObject);
		}
	}

	public void UpdateMeshEffect(GameObject go)
	{
		rendererMaterials.Clear();
		skinnedMaterials.Clear();
		if (go == null)
		{
			Debug.Log("You need set a gameObject");
			return;
		}
		MeshObject = go;
		UpdatePSMesh(MeshObject);
		AddMaterialToMesh(MeshObject);
	}

	private void UpdatePSMesh(GameObject go)
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		MeshRenderer componentInChildren = go.GetComponentInChildren<MeshRenderer>();
		SkinnedMeshRenderer componentInChildren2 = go.GetComponentInChildren<SkinnedMeshRenderer>();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem obj in array)
		{
			obj.transform.gameObject.SetActive(false);
			ParticleSystem.ShapeModule shape = obj.shape;
			if (shape.enabled)
			{
				if (componentInChildren != null)
				{
					shape.shapeType = ParticleSystemShapeType.MeshRenderer;
					shape.meshRenderer = componentInChildren;
				}
				if (componentInChildren2 != null)
				{
					shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
					shape.skinnedMeshRenderer = componentInChildren2;
				}
			}
			obj.transform.gameObject.SetActive(true);
		}
	}

	private void AddMaterialToMesh(GameObject go)
	{
		WFX_MeshMaterialEffect componentInChildren = GetComponentInChildren<WFX_MeshMaterialEffect>();
		if (!(componentInChildren == null))
		{
			MeshRenderer[] componentsInChildren = go.GetComponentsInChildren<MeshRenderer>();
			SkinnedMeshRenderer[] componentsInChildren2 = go.GetComponentsInChildren<SkinnedMeshRenderer>();
			MeshRenderer[] array = componentsInChildren;
			foreach (MeshRenderer meshRenderer in array)
			{
				rendererMaterials.Add(meshRenderer.sharedMaterials);
				meshRenderer.sharedMaterials = AddToSharedMaterial(meshRenderer.sharedMaterials, componentInChildren);
			}
			SkinnedMeshRenderer[] array2 = componentsInChildren2;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in array2)
			{
				skinnedMaterials.Add(skinnedMeshRenderer.sharedMaterials);
				skinnedMeshRenderer.sharedMaterials = AddToSharedMaterial(skinnedMeshRenderer.sharedMaterials, componentInChildren);
			}
		}
	}

	private Material[] AddToSharedMaterial(Material[] sharedMaterials, WFX_MeshMaterialEffect meshMatEffect)
	{
		if (meshMatEffect.IsFirstMaterial)
		{
			return new Material[1] { meshMatEffect.Material };
		}
		List<Material> list = sharedMaterials.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].name.Contains("MeshEffect"))
			{
				list.RemoveAt(i);
			}
		}
		list.Add(meshMatEffect.Material);
		return list.ToArray();
	}

	private void OnDestroy()
	{
		Activation(true);
		if (MeshObject == null)
		{
			return;
		}
		MeshRenderer[] componentsInChildren = MeshObject.GetComponentsInChildren<MeshRenderer>();
		SkinnedMeshRenderer[] componentsInChildren2 = MeshObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (rendererMaterials.Count == componentsInChildren.Length)
			{
				componentsInChildren[i].sharedMaterials = rendererMaterials[i];
			}
			List<Material> list = componentsInChildren[i].sharedMaterials.ToList();
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].name.Contains("MeshEffect"))
				{
					list.RemoveAt(j);
				}
			}
			componentsInChildren[i].sharedMaterials = list.ToArray();
		}
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			if (skinnedMaterials.Count == componentsInChildren2.Length)
			{
				componentsInChildren2[k].sharedMaterials = skinnedMaterials[k];
			}
			List<Material> list2 = componentsInChildren2[k].sharedMaterials.ToList();
			for (int l = 0; l < list2.Count; l++)
			{
				if (list2[l].name.Contains("MeshEffect"))
				{
					list2.RemoveAt(l);
				}
			}
			componentsInChildren2[k].sharedMaterials = list2.ToArray();
		}
		rendererMaterials.Clear();
		skinnedMaterials.Clear();
	}
}
