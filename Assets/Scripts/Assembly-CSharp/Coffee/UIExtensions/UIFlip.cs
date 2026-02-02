using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[DisallowMultipleComponent]
	[AddComponentMenu("UI/MeshEffectForTextMeshPro/UIFlip", 102)]
	public class UIFlip : BaseMeshEffect
	{
		[Tooltip("Flip horizontally.")]
		[SerializeField]
		private bool m_Horizontal;

		[Tooltip("Flip vertically.")]
		[SerializeField]
		private bool m_Veritical;

		public bool horizontal
		{
			get
			{
				return m_Horizontal;
			}
			set
			{
				m_Horizontal = value;
				SetVerticesDirty();
			}
		}

		public bool vertical
		{
			get
			{
				return m_Veritical;
			}
			set
			{
				m_Veritical = value;
				SetVerticesDirty();
			}
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			RectTransform obj = base.graphic.rectTransform;
			UIVertex vertex = default(UIVertex);
			Vector2 center = obj.rect.center;
			for (int i = 0; i < vh.currentVertCount; i++)
			{
				vh.PopulateUIVertex(ref vertex, i);
				Vector3 position = vertex.position;
				vertex.position = new Vector3(m_Horizontal ? (0f - position.x) : position.x, m_Veritical ? (0f - position.y) : position.y);
				vh.SetUIVertex(vertex, i);
			}
		}
	}
}
