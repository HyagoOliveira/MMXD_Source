using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChipSystem : MonoBehaviour
{
	private readonly string[] arrayAddBodyName = new string[2] { "BodyM", "HandM" };

	private readonly int _TintColor = Shader.PropertyToID("_TintColor");

	private const string materialName = "MeshEffect";

	public SkinnedMeshRenderer[] body;

	public Material mMaterial;

	private List<Material[]> skinnedMaterials = new List<Material[]>();

	public bool isActive;

	public bool isSetColor = true;

	public bool isFistMaterial;

	public bool FitWeapon = true;

	public bool PsobjectAddOnce;

	[ColorUsage(true, true)]
	public Color ActiveColor;

	[ColorUsage(true, true)]
	public Color MeshActiveColor;

	private Vector3 ActivePosition = new Vector3(0f, 1f, 0f);

	private Transform sec_psobject;

	private List<ParticleSystem> ps_list = new List<ParticleSystem>();

	private SkinnedMeshRenderer[] weaponMeshs;

	private void Start()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("Fx_ChipActive", 3);
	}

	public void SetWeaponMesh(CharacterMaterial[] p_characterMaterials)
	{
		if (!FitWeapon || p_characterMaterials == null || p_characterMaterials.Length == 0)
		{
			weaponMeshs = new SkinnedMeshRenderer[0];
		}
		else
		{
			weaponMeshs = GetSkinnedMeshRenderer(p_characterMaterials);
		}
		Init();
	}

	public void ResetBodyInfo()
	{
		body = new SkinnedMeshRenderer[0];
	}

	public void Init()
	{
		if (body.Length != 0)
		{
			return;
		}
		sec_psobject = OrangeBattleUtility.FindChildRecursive(base.transform, "psobject");
		List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
		CharacterMaterial[] components = base.transform.parent.GetComponents<CharacterMaterial>();
		if (components != null)
		{
			CharacterMaterial[] array = components;
			for (int i = 0; i < array.Length; i++)
			{
				Renderer[] renderer = array[i].GetRenderer();
				for (int j = 0; j < renderer.Length; j++)
				{
					SkinnedMeshRenderer skinnedMeshRenderer = renderer[j] as SkinnedMeshRenderer;
					if (!(skinnedMeshRenderer != null))
					{
						continue;
					}
					bool flag = false;
					for (int k = 0; k < arrayAddBodyName.Length; k++)
					{
						if (skinnedMeshRenderer.gameObject.name.StartsWith(arrayAddBodyName[k]))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						list.Add(skinnedMeshRenderer);
					}
				}
			}
		}
		body = list.ToArray();
		for (int l = 0; l < body.Length; l++)
		{
			MeshEftInit(body[l], l);
		}
		if (weaponMeshs != null)
		{
			for (int m = 0; m < weaponMeshs.Length; m++)
			{
				MeshEftInit(weaponMeshs[m], -1, false);
			}
		}
	}

	private void MeshEftInit(SkinnedMeshRenderer skrend, int ii = -1, bool addPsobject = true)
	{
		skinnedMaterials.Add(skrend.sharedMaterials);
		if (!addPsobject || !(sec_psobject != null))
		{
			return;
		}
		if (ii == 0)
		{
			ParticleSystem[] componentsInChildren = sec_psobject.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren)
			{
				particleSystem.transform.gameObject.SetActive(false);
				ParticleSystem.ShapeModule shape = particleSystem.shape;
				if (shape.enabled && skrend != null)
				{
					shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
					shape.skinnedMeshRenderer = skrend;
				}
				particleSystem.transform.gameObject.SetActive(true);
				ps_list.Add(particleSystem);
			}
		}
		else
		{
			if (PsobjectAddOnce)
			{
				return;
			}
			GameObject obj = UnityEngine.Object.Instantiate(sec_psobject.gameObject, base.transform);
			ParticleSystem[] componentsInChildren2 = obj.GetComponentsInChildren<ParticleSystem>();
			obj.transform.localPosition += new Vector3((ii % 2 == 0) ? 0.8f : (-0.8f), 0f, 0f);
			ParticleSystem[] componentsInChildren = componentsInChildren2;
			foreach (ParticleSystem particleSystem2 in componentsInChildren)
			{
				particleSystem2.transform.gameObject.SetActive(false);
				ParticleSystem.ShapeModule shape2 = particleSystem2.shape;
				if (shape2.enabled && skrend != null)
				{
					shape2.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
					shape2.skinnedMeshRenderer = skrend;
				}
				particleSystem2.transform.gameObject.SetActive(true);
				ps_list.Add(particleSystem2);
			}
		}
	}

	private void PlayAllPS()
	{
		for (int i = 0; i < ps_list.Count; i++)
		{
			ps_list[i].Play();
		}
	}

	private void StopAllPS()
	{
		for (int i = 0; i < ps_list.Count; i++)
		{
			ps_list[i].Stop();
		}
	}

	public void ActiveChipSkill(bool value, bool ignoreChipActive = true)
	{
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.scrEffect == 0)
		{
			if (value)
			{
				base.transform.localPosition = ActivePosition;
				if (!ignoreChipActive)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("Fx_ChipActive", base.transform, Quaternion.Euler(0f, 0f, 0f), ActiveColor, Array.Empty<object>());
				}
			}
		}
		else
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
					List<Material> list = skinnedMaterials[i].ToList();
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].name.Contains("MeshEffect"))
						{
							list.RemoveAt(j);
						}
					}
					body[i].sharedMaterials = list.ToArray();
				}
				int num = body.Length;
				if (weaponMeshs != null)
				{
					for (int k = 0; k < weaponMeshs.Length; k++)
					{
						List<Material> list2 = skinnedMaterials[k + num].ToList();
						for (int l = 0; l < list2.Count; l++)
						{
							if (list2[l].name.Contains("MeshEffect"))
							{
								list2.RemoveAt(l);
							}
						}
						weaponMeshs[k].sharedMaterials = list2.ToArray();
					}
				}
				if (sec_psobject != null)
				{
					StopAllPS();
				}
				return;
			}
			base.transform.localPosition = ActivePosition;
			if (!ignoreChipActive)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("Fx_ChipActive", base.transform, Quaternion.Euler(0f, 0f, 0f), ActiveColor, Array.Empty<object>());
			}
			for (int m = 0; m < body.Length; m++)
			{
				if (isFistMaterial)
				{
					body[m].sharedMaterials = new Material[1] { mMaterial };
					continue;
				}
				List<Material> list3 = skinnedMaterials[m].ToList();
				for (int n = 0; n < list3.Count; n++)
				{
					if (list3[n].name.Contains("MeshEffect"))
					{
						list3.RemoveAt(n);
					}
				}
				if (isSetColor)
				{
					mMaterial.SetColor(_TintColor, MeshActiveColor);
				}
				list3.Add(mMaterial);
				body[m].sharedMaterials = list3.ToArray();
			}
			int num2 = body.Length;
			if (weaponMeshs != null)
			{
				for (int num3 = 0; num3 < weaponMeshs.Length; num3++)
				{
					if (isFistMaterial)
					{
						weaponMeshs[num3].sharedMaterials = new Material[1] { mMaterial };
						continue;
					}
					List<Material> list4 = skinnedMaterials[num3 + num2].ToList();
					for (int num4 = 0; num4 < list4.Count; num4++)
					{
						if (list4[num4].name.Contains("MeshEffect"))
						{
							list4.RemoveAt(num4);
						}
					}
					if (isSetColor)
					{
						mMaterial.SetColor(_TintColor, MeshActiveColor);
					}
					list4.Add(mMaterial);
					weaponMeshs[num3].sharedMaterials = list4.ToArray();
				}
			}
			if (sec_psobject != null)
			{
				PlayAllPS();
			}
		}
	}

	public void CloseWeaponEffectOnly()
	{
		if (!FitWeapon || !isActive)
		{
			return;
		}
		int num = body.Length;
		if (weaponMeshs == null)
		{
			return;
		}
		for (int i = 0; i < weaponMeshs.Length; i++)
		{
			List<Material> list = skinnedMaterials[i + num].ToList();
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].name.Contains("MeshEffect"))
				{
					list.RemoveAt(j);
				}
			}
			weaponMeshs[i].sharedMaterials = list.ToArray();
		}
	}

	private SkinnedMeshRenderer[] GetSkinnedMeshRenderer(CharacterMaterial[] p_characterMaterials)
	{
		List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
		foreach (CharacterMaterial characterMaterial in p_characterMaterials)
		{
			if (characterMaterial == null)
			{
				continue;
			}
			Renderer[] renderer = characterMaterial.GetRenderer();
			for (int j = 0; j < renderer.Length; j++)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = renderer[j] as SkinnedMeshRenderer;
				if (skinnedMeshRenderer != null)
				{
					list.Add(skinnedMeshRenderer);
				}
			}
		}
		return list.ToArray();
	}

	public void SetActivePosition(Vector3 pos)
	{
		ActivePosition = pos;
	}
}
