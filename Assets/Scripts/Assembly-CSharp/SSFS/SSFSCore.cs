#define RELEASE
using UnityEngine;

namespace SSFS
{
	public static class SSFSCore
	{
		public static string shaderPath
		{
			get
			{
				return "Sci-Fi/SSFS/Base";
			}
		}

		public static Shader shader
		{
			get
			{
				Shader obj = Shader.Find(shaderPath);
				if (obj == null)
				{
					Debug.LogError("SSFS SHADER NOT FOUND");
				}
				return obj;
			}
		}

		public static Material newMaterial
		{
			get
			{
				Material material = new Material(shader);
				if (material == null)
				{
					Debug.LogError("SSFS MATERIAL CREATION FAILED");
				}
				else
				{
					material.name = "New SSFS Material";
				}
				return material;
			}
		}
	}
}
