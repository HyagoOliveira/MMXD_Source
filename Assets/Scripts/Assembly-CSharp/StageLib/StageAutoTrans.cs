using System.Collections.Generic;
using UnityEngine;

namespace StageLib
{
	public class StageAutoTrans : MonoBehaviour
	{
		public BoxCollider2D[] refAllB2Ds;

		public MeshRenderer tMeshRenderer;

		public float falpha = 1f;

		public Color tColor = Color.white;

		private void Start()
		{
			List<BoxCollider2D> list = new List<BoxCollider2D>();
			for (int i = 0; i < base.transform.childCount; i++)
			{
				BoxCollider2D component = base.transform.GetChild(i).GetComponent<BoxCollider2D>();
				if (component != null)
				{
					list.Add(component);
				}
			}
			if (list.Count > 0)
			{
				refAllB2Ds = list.ToArray();
			}
			else
			{
				refAllB2Ds = new BoxCollider2D[0];
			}
			tMeshRenderer = GetComponent<MeshRenderer>();
			if (tMeshRenderer.material.shader.name != "StageLib/DiffuseAlpha")
			{
				Material material = new Material(Shader.Find("StageLib/DiffuseAlpha"));
				material.mainTexture = tMeshRenderer.material.mainTexture;
				tMeshRenderer.material = material;
			}
			falpha = 1f;
			tMeshRenderer.material.SetColor("_Color", tColor);
		}

		private void LateUpdate()
		{
			tColor.a = falpha;
			for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
			{
				Vector3 position = StageUpdate.runPlayers[num].transform.position;
				for (int i = 0; i < refAllB2Ds.Length; i++)
				{
					if (refAllB2Ds[i].bounds.Contains(position) && falpha >= 0f)
					{
						falpha -= 0.05f;
						if (falpha < 0f)
						{
							falpha = 0f;
						}
						tColor.a = falpha;
						tMeshRenderer.material.SetColor("_Color", tColor);
						return;
					}
				}
			}
			if (falpha < 1f)
			{
				falpha += 0.05f;
				if (falpha > 1f)
				{
					falpha = 1f;
				}
				tColor.a = falpha;
				tMeshRenderer.material.SetColor("_Color", tColor);
			}
		}
	}
}
