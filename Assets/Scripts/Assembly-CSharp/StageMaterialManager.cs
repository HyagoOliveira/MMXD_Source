using Better;
using UnityEngine;

public class StageMaterialManager : MonoBehaviourSingleton<StageMaterialManager>
{
	private static Dictionary<string, Material> MaterialDict = new Dictionary<string, Material>();

	private static Dictionary<string, Texture> MaterialTextureDict = new Dictionary<string, Texture>();

	private bool bIsInit;

	public void Create()
	{
		if (!bIsInit)
		{
			Clear();
			MaterialDict.Add("empty", new Material(Shader.Find("Transparent/Diffuse")));
			MaterialDict.Add("StageLib/StageStandardObj", new Material(Shader.Find("StageLib/StageStandardObj")));
			MaterialDict.Add("StageLib/DiffuseAlpha", new Material(Shader.Find("StageLib/DiffuseAlpha")));
			bIsInit = true;
		}
	}

	public static Material Get(string key)
	{
		Material value = null;
		if (!MaterialDict.TryGetValue(key, out value))
		{
			MonoBehaviourSingleton<StageMaterialManager>.Instance.Create();
			if (MaterialDict.TryGetValue(key, out value))
			{
				return value;
			}
			value = MaterialDict["empty"];
		}
		return value;
	}

	public static void SetTexture(string key, Texture texture)
	{
		if (!MaterialTextureDict.ContainsKey(key))
		{
			MaterialTextureDict.Add(key, texture);
		}
	}

	public static bool TryGetTexture(string key, out Texture texture)
	{
		return MaterialTextureDict.TryGetValue(key, out texture);
	}

	public void Clear()
	{
		foreach (Material value in MaterialDict.Values)
		{
			Object.Destroy(value);
		}
		foreach (Texture value2 in MaterialTextureDict.Values)
		{
			Object.Destroy(value2);
		}
		MaterialDict.Clear();
		MaterialTextureDict.Clear();
		bIsInit = false;
	}
}
