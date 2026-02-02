using System;
using System.Collections.Generic;
using UnityEngine;

public class CH116_Demo_EggMaterial : MonoBehaviour
{
	[SerializeField]
	private CharacterMaterial characterMaterial;

	private void Start()
	{
		if (characterMaterial == null)
		{
			characterMaterial = GetComponent<CharacterMaterial>();
		}
		UpdateRandTexture();
	}

	public void UpdateRandTexture()
	{
		if (!characterMaterial || !characterMaterial.IsRenderersExist())
		{
			return;
		}
		Renderer[] renderer = characterMaterial.GetRenderer();
		Texture[] otherTextures = characterMaterial.GetOtherTextures();
		System.Random random = new System.Random();
		int num = Mathf.Min(otherTextures.Length, renderer.Length);
		List<int> list = new List<int>();
		for (int i = 0; i < num; i++)
		{
			list.Add(i);
		}
		for (int num2 = num - 1; num2 > 0; num2--)
		{
			int index = random.Next(num2 + 1);
			int value = list[num2];
			list[num2] = list[index];
			list[index] = value;
		}
		OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
		for (int j = 0; j < list.Count; j++)
		{
			Renderer renderer2 = renderer[j];
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			renderer2.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetTexture(instance.i_MainTex, otherTextures[list[j]]);
			renderer2.SetPropertyBlock(materialPropertyBlock);
			if ((bool)renderer2.GetComponent<CharacterMaterialChildPass>())
			{
				renderer2.GetComponent<CharacterMaterialChildPass>().Mpb = materialPropertyBlock;
			}
		}
	}
}
