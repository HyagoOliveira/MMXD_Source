using System;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[DisallowMultipleComponent]
	[AddComponentMenu("UI/MeshEffectForTextMeshPro/UIGradient", 101)]
	public class UIGradient : BaseMeshEffect
	{
		public enum Direction
		{
			Horizontal = 0,
			Vertical = 1,
			Angle = 2,
			Diagonal = 3
		}

		public enum GradientStyle
		{
			Rect = 0,
			Fit = 1,
			Split = 2
		}

		private struct Matrix2x3
		{
			public float m00;

			public float m01;

			public float m02;

			public float m10;

			public float m11;

			public float m12;

			public Matrix2x3(Rect rect, float cos, float sin)
			{
				float num = (0f - rect.xMin) / rect.width - 0.5f;
				float num2 = (0f - rect.yMin) / rect.height - 0.5f;
				m00 = cos / rect.width;
				m01 = (0f - sin) / rect.height;
				m02 = num * cos - num2 * sin + 0.5f;
				m10 = sin / rect.width;
				m11 = cos / rect.height;
				m12 = num * sin + num2 * cos + 0.5f;
			}

			public static Vector2 operator *(Matrix2x3 m, Vector2 v)
			{
				return new Vector2(m.m00 * v.x + m.m01 * v.y + m.m02, m.m10 * v.x + m.m11 * v.y + m.m12);
			}
		}

		[Tooltip("Gradient Direction.")]
		[SerializeField]
		private Direction m_Direction;

		[Tooltip("Color1: Top or Left.")]
		[SerializeField]
		private Color m_Color1 = Color.white;

		[Tooltip("Color2: Bottom or Right.")]
		[SerializeField]
		private Color m_Color2 = Color.white;

		[Tooltip("Color3: For diagonal.")]
		[SerializeField]
		private Color m_Color3 = Color.white;

		[Tooltip("Color4: For diagonal.")]
		[SerializeField]
		private Color m_Color4 = Color.white;

		[Tooltip("Gradient rotation.")]
		[SerializeField]
		[Range(-180f, 180f)]
		private float m_Rotation;

		[Tooltip("Gradient offset for Horizontal, Vertical or Angle.")]
		[SerializeField]
		[Range(-1f, 1f)]
		private float m_Offset1;

		[Tooltip("Gradient offset for Diagonal.")]
		[SerializeField]
		[Range(-1f, 1f)]
		private float m_Offset2;

		[Tooltip("Gradient style for Text.")]
		[SerializeField]
		private GradientStyle m_GradientStyle;

		[Tooltip("Color space to correct color.")]
		[SerializeField]
		private ColorSpace m_ColorSpace = ColorSpace.Uninitialized;

		[Tooltip("Ignore aspect ratio.")]
		[SerializeField]
		private bool m_IgnoreAspectRatio = true;

		private static readonly Vector2[] s_SplitedCharacterPosition = new Vector2[4]
		{
			Vector2.up,
			Vector2.one,
			Vector2.right,
			Vector2.zero
		};

		public Graphic targetGraphic
		{
			get
			{
				return base.graphic;
			}
		}

		public Direction direction
		{
			get
			{
				return m_Direction;
			}
			set
			{
				if (m_Direction != value)
				{
					m_Direction = value;
					SetVerticesDirty();
				}
			}
		}

		public Color color1
		{
			get
			{
				return m_Color1;
			}
			set
			{
				if (m_Color1 != value)
				{
					m_Color1 = value;
					SetVerticesDirty();
				}
			}
		}

		public Color color2
		{
			get
			{
				return m_Color2;
			}
			set
			{
				if (m_Color2 != value)
				{
					m_Color2 = value;
					SetVerticesDirty();
				}
			}
		}

		public Color color3
		{
			get
			{
				return m_Color3;
			}
			set
			{
				if (m_Color3 != value)
				{
					m_Color3 = value;
					SetVerticesDirty();
				}
			}
		}

		public Color color4
		{
			get
			{
				return m_Color4;
			}
			set
			{
				if (m_Color4 != value)
				{
					m_Color4 = value;
					SetVerticesDirty();
				}
			}
		}

		public float rotation
		{
			get
			{
				if (m_Direction != 0)
				{
					if (m_Direction != Direction.Vertical)
					{
						return m_Rotation;
					}
					return 0f;
				}
				return -90f;
			}
			set
			{
				if (!Mathf.Approximately(m_Rotation, value))
				{
					m_Rotation = value;
					SetVerticesDirty();
				}
			}
		}

		public float offset
		{
			get
			{
				return m_Offset1;
			}
			set
			{
				if (m_Offset1 != value)
				{
					m_Offset1 = value;
					SetVerticesDirty();
				}
			}
		}

		public Vector2 offset2
		{
			get
			{
				return new Vector2(m_Offset2, m_Offset1);
			}
			set
			{
				if (m_Offset1 != value.y || m_Offset2 != value.x)
				{
					m_Offset1 = value.y;
					m_Offset2 = value.x;
					SetVerticesDirty();
				}
			}
		}

		public GradientStyle gradientStyle
		{
			get
			{
				return m_GradientStyle;
			}
			set
			{
				if (m_GradientStyle != value)
				{
					m_GradientStyle = value;
					SetVerticesDirty();
				}
			}
		}

		public ColorSpace colorSpace
		{
			get
			{
				return m_ColorSpace;
			}
			set
			{
				if (m_ColorSpace != value)
				{
					m_ColorSpace = value;
					SetVerticesDirty();
				}
			}
		}

		public bool ignoreAspectRatio
		{
			get
			{
				return m_IgnoreAspectRatio;
			}
			set
			{
				if (m_IgnoreAspectRatio != value)
				{
					m_IgnoreAspectRatio = value;
					SetVerticesDirty();
				}
			}
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!IsActive())
			{
				return;
			}
			Rect rect = default(Rect);
			UIVertex vertex = default(UIVertex);
			if (m_GradientStyle == GradientStyle.Rect)
			{
				rect = base.graphic.rectTransform.rect;
			}
			else if (m_GradientStyle == GradientStyle.Split)
			{
				rect.Set(0f, 0f, 1f, 1f);
			}
			else if (m_GradientStyle == GradientStyle.Fit)
			{
				float xMin = (rect.yMin = float.MaxValue);
				rect.xMin = xMin;
				xMin = (rect.yMax = float.MinValue);
				rect.xMax = xMin;
				for (int i = 0; i < vh.currentVertCount; i++)
				{
					vh.PopulateUIVertex(ref vertex, i);
					rect.xMin = Mathf.Min(rect.xMin, vertex.position.x);
					rect.yMin = Mathf.Min(rect.yMin, vertex.position.y);
					rect.xMax = Mathf.Max(rect.xMax, vertex.position.x);
					rect.yMax = Mathf.Max(rect.yMax, vertex.position.y);
				}
			}
			float f = rotation * ((float)Math.PI / 180f);
			Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
			if (!m_IgnoreAspectRatio && Direction.Angle <= m_Direction)
			{
				vector.x *= rect.height / rect.width;
				vector = vector.normalized;
			}
			Matrix2x3 matrix2x = new Matrix2x3(rect, vector.x, vector.y);
			for (int j = 0; j < vh.currentVertCount; j++)
			{
				vh.PopulateUIVertex(ref vertex, j);
				Vector2 vector2 = ((m_GradientStyle != GradientStyle.Split) ? (matrix2x * vertex.position + offset2) : (matrix2x * s_SplitedCharacterPosition[j % 4] + offset2));
				Color color = ((direction != Direction.Diagonal) ? Color.LerpUnclamped(m_Color2, m_Color1, vector2.y) : Color.LerpUnclamped(Color.LerpUnclamped(m_Color1, m_Color2, vector2.x), Color.LerpUnclamped(m_Color3, m_Color4, vector2.x), vector2.y));
				ref Color32 color2 = ref vertex.color;
				color2 *= ((m_ColorSpace == ColorSpace.Gamma) ? color.gamma : ((m_ColorSpace == ColorSpace.Linear) ? color.linear : color));
				vh.SetUIVertex(vertex, j);
			}
		}
	}
}
