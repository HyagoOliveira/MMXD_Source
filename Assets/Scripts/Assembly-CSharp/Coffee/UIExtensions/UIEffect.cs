using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Graphic))]
	[DisallowMultipleComponent]
	[AddComponentMenu("UI/UIEffect/UIEffect", 1)]
	public class UIEffect : UIEffectBase
	{
		public enum BlurEx
		{
			None = 0,
			Ex = 1
		}

		public const string shaderName = "UI/Hidden/UI-Effect";

		private static readonly ParameterTexture _ptex = new ParameterTexture(4, 1024, "_ParamTex");

		[FormerlySerializedAs("m_ToneLevel")]
		[Tooltip("Effect factor between 0(no effect) and 1(complete effect).")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_EffectFactor = 1f;

		[Tooltip("Color effect factor between 0(no effect) and 1(complete effect).")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_ColorFactor = 1f;

		[FormerlySerializedAs("m_Blur")]
		[Tooltip("How far is the blurring from the graphic.")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_BlurFactor = 1f;

		[FormerlySerializedAs("m_ToneMode")]
		[Tooltip("Effect mode")]
		[SerializeField]
		private EffectMode m_EffectMode;

		[Tooltip("Color effect mode")]
		[SerializeField]
		private ColorMode m_ColorMode;

		[Tooltip("Blur effect mode")]
		[SerializeField]
		private BlurMode m_BlurMode;

		[Tooltip("Advanced blurring remove common artifacts in the blur effect for uGUI.")]
		[SerializeField]
		private bool m_AdvancedBlur;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_ShadowBlur = 1f;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		private ShadowStyle m_ShadowStyle;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		private Color m_ShadowColor = Color.black;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		private Vector2 m_EffectDistance = new Vector2(1f, -1f);

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		private bool m_UseGraphicAlpha = true;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		private Color m_EffectColor = Color.white;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		private List<UIShadow.AdditionalShadow> m_AdditionalShadows = new List<UIShadow.AdditionalShadow>();

		public override AdditionalCanvasShaderChannels requiredChannels
		{
			get
			{
				if (advancedBlur)
				{
					if (!base.isTMPro)
					{
						return AdditionalCanvasShaderChannels.TexCoord1;
					}
					return AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
				}
				return AdditionalCanvasShaderChannels.None;
			}
		}

		[Obsolete("Use effectFactor instead (UnityUpgradable) -> effectFactor")]
		public float toneLevel
		{
			get
			{
				return m_EffectFactor;
			}
			set
			{
				m_EffectFactor = Mathf.Clamp(value, 0f, 1f);
				SetDirty();
			}
		}

		public float effectFactor
		{
			get
			{
				return m_EffectFactor;
			}
			set
			{
				m_EffectFactor = Mathf.Clamp(value, 0f, 1f);
				SetDirty();
			}
		}

		public float colorFactor
		{
			get
			{
				return m_ColorFactor;
			}
			set
			{
				m_ColorFactor = Mathf.Clamp(value, 0f, 1f);
				SetDirty();
			}
		}

		[Obsolete("Use blurFactor instead (UnityUpgradable) -> blurFactor")]
		public float blur
		{
			get
			{
				return m_BlurFactor;
			}
			set
			{
				m_BlurFactor = Mathf.Clamp(value, 0f, 1f);
				SetDirty();
			}
		}

		[Obsolete("Use effectFactor instead (UnityUpgradable) -> effectFactor")]
		public float blurFactor
		{
			get
			{
				return m_BlurFactor;
			}
			set
			{
				m_BlurFactor = Mathf.Clamp(value, 0f, 1f);
				SetDirty();
			}
		}

		[Obsolete("Use effectMode instead (UnityUpgradable) -> effectMode")]
		public EffectMode toneMode
		{
			get
			{
				return m_EffectMode;
			}
		}

		public EffectMode effectMode
		{
			get
			{
				return m_EffectMode;
			}
		}

		public ColorMode colorMode
		{
			get
			{
				return m_ColorMode;
			}
		}

		public BlurMode blurMode
		{
			get
			{
				return m_BlurMode;
			}
		}

		public Color effectColor
		{
			get
			{
				return base.graphic.color;
			}
			set
			{
				base.graphic.color = value;
				SetDirty();
			}
		}

		public override ParameterTexture ptex
		{
			get
			{
				return _ptex;
			}
		}

		public bool advancedBlur
		{
			get
			{
				if (!base.isTMPro)
				{
					return m_AdvancedBlur;
				}
				if ((bool)material)
				{
					return material.IsKeywordEnabled("EX");
				}
				return false;
			}
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			float normalizedIndex = ptex.GetNormalizedIndex(this);
			if (m_BlurMode != 0 && advancedBlur)
			{
				vh.GetUIVertexStream(UIEffectBase.tempVerts);
				vh.Clear();
				int count = UIEffectBase.tempVerts.Count;
				int num = ((base.targetGraphic is Text || base.isTMPro) ? 6 : count);
				Rect posBounds = default(Rect);
				Rect uvBounds = default(Rect);
				Vector3 a = default(Vector3);
				Vector3 vector = default(Vector3);
				Vector3 vector2 = default(Vector3);
				float num2 = (float)blurMode * 6f * 2f;
				for (int i = 0; i < count; i += num)
				{
					GetBounds(UIEffectBase.tempVerts, i, num, ref posBounds, ref uvBounds, true);
					Vector2 vector3 = new Vector2(Packer.ToFloat(uvBounds.xMin, uvBounds.yMin), Packer.ToFloat(uvBounds.xMax, uvBounds.yMax));
					for (int j = 0; j < num; j += 6)
					{
						Vector3 position = UIEffectBase.tempVerts[i + j + 1].position;
						Vector3 position2 = UIEffectBase.tempVerts[i + j + 4].position;
						bool flag = num == 6 || !posBounds.Contains(position) || !posBounds.Contains(position2);
						if (flag)
						{
							Vector3 vector4 = UIEffectBase.tempVerts[i + j + 1].uv0;
							Vector3 vector5 = UIEffectBase.tempVerts[i + j + 4].uv0;
							Vector3 vector6 = (position + position2) / 2f;
							Vector3 vector7 = (vector4 + vector5) / 2f;
							a = position - position2;
							a.x = 1f + num2 / Mathf.Abs(a.x);
							a.y = 1f + num2 / Mathf.Abs(a.y);
							a.z = 1f + num2 / Mathf.Abs(a.z);
							vector = vector6 - Vector3.Scale(a, vector6);
							vector2 = vector7 - Vector3.Scale(a, vector7);
						}
						for (int k = 0; k < 6; k++)
						{
							UIVertex value = UIEffectBase.tempVerts[i + j + k];
							Vector3 position3 = value.position;
							Vector2 uv = value.uv0;
							if (flag && (position3.x < posBounds.xMin || posBounds.xMax < position3.x))
							{
								position3.x = position3.x * a.x + vector.x;
								uv.x = uv.x * a.x + vector2.x;
							}
							if (flag && (position3.y < posBounds.yMin || posBounds.yMax < position3.y))
							{
								position3.y = position3.y * a.y + vector.y;
								uv.y = uv.y * a.y + vector2.y;
							}
							value.uv0 = new Vector2(Packer.ToFloat((uv.x + 0.5f) / 2f, (uv.y + 0.5f) / 2f), normalizedIndex);
							value.position = position3;
							if (base.isTMPro)
							{
								value.uv2 = vector3;
							}
							else
							{
								value.uv1 = vector3;
							}
							UIEffectBase.tempVerts[i + j + k] = value;
						}
					}
				}
				vh.AddUIVertexTriangleStream(UIEffectBase.tempVerts);
				UIEffectBase.tempVerts.Clear();
			}
			else
			{
				int currentVertCount = vh.currentVertCount;
				UIVertex vertex = default(UIVertex);
				for (int l = 0; l < currentVertCount; l++)
				{
					vh.PopulateUIVertex(ref vertex, l);
					Vector2 uv2 = vertex.uv0;
					vertex.uv0 = new Vector2(Packer.ToFloat((uv2.x + 0.5f) / 2f, (uv2.y + 0.5f) / 2f), normalizedIndex);
					vh.SetUIVertex(vertex, l);
				}
			}
		}

		protected override void SetDirty()
		{
			Material[] array = materials;
			foreach (Material mat in array)
			{
				ptex.RegisterMaterial(mat);
			}
			ptex.SetData(this, 0, m_EffectFactor);
			ptex.SetData(this, 1, m_ColorFactor);
			ptex.SetData(this, 2, m_BlurFactor);
		}

		private static void GetBounds(List<UIVertex> verts, int start, int count, ref Rect posBounds, ref Rect uvBounds, bool global)
		{
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
			Vector2 vector3 = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector4 = new Vector2(float.MinValue, float.MinValue);
			for (int i = start; i < start + count; i++)
			{
				UIVertex uIVertex = verts[i];
				Vector2 uv = uIVertex.uv0;
				Vector3 position = uIVertex.position;
				if (vector.x >= position.x && vector.y >= position.y)
				{
					vector = position;
				}
				else if (vector2.x <= position.x && vector2.y <= position.y)
				{
					vector2 = position;
				}
				if (vector3.x >= uv.x && vector3.y >= uv.y)
				{
					vector3 = uv;
				}
				else if (vector4.x <= uv.x && vector4.y <= uv.y)
				{
					vector4 = uv;
				}
			}
			posBounds.Set(vector.x + 0.001f, vector.y + 0.001f, vector2.x - vector.x - 0.002f, vector2.y - vector.y - 0.002f);
			uvBounds.Set(vector3.x, vector3.y, vector4.x - vector3.x, vector4.y - vector3.y);
		}
	}
}
