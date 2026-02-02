using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RainDropTools : MonoBehaviour
{
	public enum RainDropShaderType
	{
		Expensive = 0,
		Cheap = 1,
		NoDistortion = 2
	}

	public static string SHADER_FORWARD = "RainDrop/Internal/RainDistortion (Forward)";

	public static string SHADER_CHEAP = "RainDrop/Internal/RainDistortion (Mobile)";

	public static string SHADER_NO_DISTORTION = "RainDrop/Internal/RainNoDistortion";

	public static string GetShaderName(RainDropShaderType shaderType)
	{
		switch (shaderType)
		{
		case RainDropShaderType.Expensive:
			return SHADER_FORWARD;
		case RainDropShaderType.Cheap:
			return SHADER_CHEAP;
		case RainDropShaderType.NoDistortion:
			return SHADER_NO_DISTORTION;
		default:
			return "";
		}
	}

	public static Material CreateRainMaterial(RainDropShaderType shaderType, int renderQueue)
	{
		return new Material(Shader.Find(GetShaderName(shaderType)))
		{
			renderQueue = renderQueue
		};
	}

	public static void ApplyRainMaterialValue(Material material, RainDropShaderType shaderType, Texture normalMap, Texture overlayTexture = null, float distortionValue = 0f, Color? overlayColor = null, float reliefValue = 0f, float blur = 0f, float darkness = 0f)
	{
		switch (shaderType)
		{
		case RainDropShaderType.Expensive:
			material.SetColor("_Color", overlayColor ?? Color.white);
			material.SetFloat("_Strength", distortionValue);
			material.SetFloat("_Relief", reliefValue);
			if (blur != 0f)
			{
				material.EnableKeyword("BLUR");
				material.SetFloat("_Blur", blur);
			}
			else
			{
				material.DisableKeyword("BLUR");
				material.SetFloat("_Blur", blur);
			}
			material.SetFloat("_Darkness", darkness);
			material.SetTexture("_Distortion", normalMap);
			material.SetTexture("_ReliefTex", overlayTexture);
			break;
		case RainDropShaderType.Cheap:
			material.SetFloat("_Strength", distortionValue);
			material.SetTexture("_Distortion", normalMap);
			break;
		case RainDropShaderType.NoDistortion:
			material.SetTexture("_MainTex", overlayTexture);
			material.SetTexture("_Distortion", normalMap);
			material.SetColor("_Color", overlayColor ?? Color.white);
			material.SetFloat("_Darkness", darkness);
			material.SetFloat("_Relief", reliefValue);
			break;
		}
	}

	public static Mesh CreateQuadMesh()
	{
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(1f, 1f, 0f),
			new Vector3(1f, -1f, 0f),
			new Vector3(-1f, 1f, 0f),
			new Vector3(-1f, -1f, 0f)
		};
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(1f, 1f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(0f, 0f)
		};
		int[] triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		Mesh mesh = new Mesh();
		mesh.hideFlags = HideFlags.DontSave;
		mesh.name = "Rain Mesh";
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.MarkDynamic();
		mesh.RecalculateBounds();
		return mesh;
	}

	public static Transform CreateHiddenObject(string name, Transform parent)
	{
		GameObject obj = new GameObject();
		obj.name = name;
		obj.transform.parent = parent;
		obj.transform.localScale = Vector3.one;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
		return obj.transform;
	}

	public static float Random(float min, float max)
	{
		return UnityEngine.Random.Range(min, max);
	}

	public static int Random(int min, int max)
	{
		return UnityEngine.Random.Range(min, max);
	}

	public static void DestroyChildren(Transform t)
	{
		foreach (Transform item in t)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	public static void DestroyChildrenImmediate(Transform t)
	{
		foreach (Transform item in t)
		{
			UnityEngine.Object.DestroyImmediate(item.gameObject);
		}
	}

	public static Vector2 GetCameraOrthographicSize(Camera cam)
	{
		float num = cam.orthographicSize * 2f;
		return new Vector2(num * cam.aspect, num);
	}

	public static Vector3 GetSpawnLocalPos(Transform parent, Camera cam, float offsetX, float offsetY)
	{
		Vector2 cameraOrthographicSize = GetCameraOrthographicSize(cam);
		Vector3 zero = Vector3.zero;
		zero = cam.transform.rotation * zero + parent.position;
		zero.x += cameraOrthographicSize.x * offsetX;
		zero.y += cameraOrthographicSize.y * offsetY;
		return parent.InverseTransformPoint(zero);
	}

	public static Vector3 GetGForcedScreenMovement(Transform screenTransform, Vector3 GForce)
	{
		Vector3 vector = Vector3.Project(GForce, screenTransform.up);
		Vector3 vector2 = Vector3.Project(GForce, screenTransform.right);
		Vector3 vector3 = Vector3.Project(GForce, screenTransform.forward);
		Vector3 vector4 = screenTransform.InverseTransformPoint(screenTransform.position + vector);
		Vector3 vector5 = screenTransform.InverseTransformPoint(screenTransform.position + vector2);
		Vector3 vector6 = screenTransform.InverseTransformPoint(screenTransform.position + vector3);
		return new Vector3(vector5.x, vector4.y, vector6.z);
	}

	public static KeyValuePair<T1, T2> GetWeightedElement<T1, T2>(List<KeyValuePair<T1, T2>> list) where T2 : IComparable
	{
		if (list.Count == 0)
		{
			return list.FirstOrDefault();
		}
		float max = (float)list.Sum((KeyValuePair<T1, T2> t) => Convert.ToDouble(t.Value));
		float num = Random(0f, max);
		float num2 = 0f;
		foreach (KeyValuePair<T1, T2> item in list)
		{
			for (float num3 = num2; (double)num3 < Convert.ToDouble(item.Value) + (double)num2; num3 += 1f)
			{
				if (num3 >= num)
				{
					return item;
				}
			}
			num2 += (float)Convert.ToDouble(item.Value);
		}
		return list.First();
	}
}
