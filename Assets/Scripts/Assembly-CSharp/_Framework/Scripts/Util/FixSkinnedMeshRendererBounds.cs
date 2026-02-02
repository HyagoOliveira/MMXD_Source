using UnityEngine;

namespace _Framework.Scripts.Util
{
	public class FixSkinnedMeshRendererBounds : MonoBehaviour
	{
		[SerializeField]
		private MeshRenderer skinnedMeshRenderer;

		private void OnRenderObject()
		{
			skinnedMeshRenderer.GetComponent<MeshFilter>().mesh.vertices = base.transform.GetComponent<Cloth>().vertices;
		}
	}
}
