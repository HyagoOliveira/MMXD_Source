using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[AddComponentMenu("UI/UIEffect/UIHsvModifier", 4)]
	public class UIHsvModifier : UIEffectBase
	{
		public const string shaderName = "UI/Hidden/UI-Effect-HSV";

		private static readonly ParameterTexture _ptex = new ParameterTexture(7, 128, "_ParamTex");

		[Header("Target")]
		[Tooltip("Target color to affect hsv shift.")]
		[SerializeField]
		[ColorUsage(false)]
		private Color m_TargetColor = Color.red;

		[Tooltip("Color range to affect hsv shift [0 ~ 1].")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_Range = 0.1f;

		[Header("Adjustment")]
		[Tooltip("Hue shift [-0.5 ~ 0.5].")]
		[SerializeField]
		[Range(-0.5f, 0.5f)]
		private float m_Hue;

		[Tooltip("Saturation shift [-0.5 ~ 0.5].")]
		[SerializeField]
		[Range(-0.5f, 0.5f)]
		private float m_Saturation;

		[Tooltip("Value shift [-0.5 ~ 0.5].")]
		[SerializeField]
		[Range(-0.5f, 0.5f)]
		private float m_Value;

		public Color targetColor
		{
			get
			{
				return m_TargetColor;
			}
			set
			{
				if (m_TargetColor != value)
				{
					m_TargetColor = value;
					SetDirty();
				}
			}
		}

		public float range
		{
			get
			{
				return m_Range;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(m_Range, value))
				{
					m_Range = value;
					SetDirty();
				}
			}
		}

		public float saturation
		{
			get
			{
				return m_Saturation;
			}
			set
			{
				value = Mathf.Clamp(value, -0.5f, 0.5f);
				if (!Mathf.Approximately(m_Saturation, value))
				{
					m_Saturation = value;
					SetDirty();
				}
			}
		}

		public float value
		{
			get
			{
				return m_Value;
			}
			set
			{
				value = Mathf.Clamp(value, -0.5f, 0.5f);
				if (!Mathf.Approximately(m_Value, value))
				{
					m_Value = value;
					SetDirty();
				}
			}
		}

		public float hue
		{
			get
			{
				return m_Hue;
			}
			set
			{
				value = Mathf.Clamp(value, -0.5f, 0.5f);
				if (!Mathf.Approximately(m_Hue, value))
				{
					m_Hue = value;
					SetDirty();
				}
			}
		}

		public override ParameterTexture ptex
		{
			get
			{
				return _ptex;
			}
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (base.isActiveAndEnabled)
			{
				float normalizedIndex = ptex.GetNormalizedIndex(this);
				UIVertex vertex = default(UIVertex);
				int currentVertCount = vh.currentVertCount;
				for (int i = 0; i < currentVertCount; i++)
				{
					vh.PopulateUIVertex(ref vertex, i);
					vertex.uv0 = new Vector2(Packer.ToFloat(vertex.uv0.x, vertex.uv0.y), normalizedIndex);
					vh.SetUIVertex(vertex, i);
				}
			}
		}

		protected override void SetDirty()
		{
			float H;
			float S;
			float V;
			Color.RGBToHSV(m_TargetColor, out H, out S, out V);
			Material[] array = materials;
			foreach (Material mat in array)
			{
				ptex.RegisterMaterial(mat);
			}
			ptex.SetData(this, 0, H);
			ptex.SetData(this, 1, S);
			ptex.SetData(this, 2, V);
			ptex.SetData(this, 3, m_Range);
			ptex.SetData(this, 4, m_Hue + 0.5f);
			ptex.SetData(this, 5, m_Saturation + 0.5f);
			ptex.SetData(this, 6, m_Value + 0.5f);
		}
	}
}
