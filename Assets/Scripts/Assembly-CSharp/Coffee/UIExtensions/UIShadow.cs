using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[RequireComponent(typeof(Graphic))]
	[AddComponentMenu("UI/UIEffect/UIShadow", 100)]
	public class UIShadow : BaseMeshEffect, IParameterTexture
	{
		[Serializable]
		[Obsolete]
		public class AdditionalShadow
		{
			[FormerlySerializedAs("shadowBlur")]
			[Range(0f, 1f)]
			public float blur = 0.25f;

			[FormerlySerializedAs("shadowMode")]
			public ShadowStyle style = ShadowStyle.Shadow;

			[FormerlySerializedAs("shadowColor")]
			public Color effectColor = Color.black;

			public Vector2 effectDistance = new Vector2(1f, -1f);

			public bool useGraphicAlpha = true;
		}

		[Tooltip("How far is the blurring shadow from the graphic.")]
		[FormerlySerializedAs("m_Blur")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_BlurFactor = 1f;

		[Tooltip("Shadow effect style.")]
		[SerializeField]
		private ShadowStyle m_Style = ShadowStyle.Shadow;

		[HideInInspector]
		[Obsolete]
		[SerializeField]
		private List<AdditionalShadow> m_AdditionalShadows = new List<AdditionalShadow>();

		[SerializeField]
		private Color m_EffectColor = new Color(0f, 0f, 0f, 0.5f);

		[SerializeField]
		private Vector2 m_EffectDistance = new Vector2(1f, -1f);

		[SerializeField]
		private bool m_UseGraphicAlpha = true;

		private const float kMaxEffectDistance = 600f;

		private int _graphicVertexCount;

		private static readonly List<UIShadow> tmpShadows = new List<UIShadow>();

		private UIEffect _uiEffect;

		private static readonly List<UIVertex> s_Verts = new List<UIVertex>(4096);

		public Color effectColor
		{
			get
			{
				return m_EffectColor;
			}
			set
			{
				m_EffectColor = value;
				if (base.graphic != null)
				{
					base.graphic.SetVerticesDirty();
				}
			}
		}

		public Vector2 effectDistance
		{
			get
			{
				return m_EffectDistance;
			}
			set
			{
				if (value.x > 600f)
				{
					value.x = 600f;
				}
				if (value.x < -600f)
				{
					value.x = -600f;
				}
				if (value.y > 600f)
				{
					value.y = 600f;
				}
				if (value.y < -600f)
				{
					value.y = -600f;
				}
				if (!(m_EffectDistance == value))
				{
					m_EffectDistance = value;
					if (base.graphic != null)
					{
						base.graphic.SetVerticesDirty();
					}
				}
			}
		}

		public bool useGraphicAlpha
		{
			get
			{
				return m_UseGraphicAlpha;
			}
			set
			{
				m_UseGraphicAlpha = value;
				if (base.graphic != null)
				{
					base.graphic.SetVerticesDirty();
				}
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
				m_BlurFactor = Mathf.Clamp(value, 0f, 2f);
				_SetDirty();
			}
		}

		public float blurFactor
		{
			get
			{
				return m_BlurFactor;
			}
			set
			{
				m_BlurFactor = Mathf.Clamp(value, 0f, 2f);
				_SetDirty();
			}
		}

		public ShadowStyle style
		{
			get
			{
				return m_Style;
			}
			set
			{
				m_Style = value;
				_SetDirty();
			}
		}

		public int parameterIndex { get; set; }

		public ParameterTexture ptex { get; private set; }

		protected override void OnEnable()
		{
			base.OnEnable();
			_uiEffect = GetComponent<UIEffect>();
			if ((bool)_uiEffect)
			{
				ptex = _uiEffect.ptex;
				ptex.Register(this);
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			_uiEffect = null;
			if (ptex != null)
			{
				ptex.Unregister(this);
				ptex = null;
			}
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!base.isActiveAndEnabled || vh.currentVertCount <= 0 || m_Style == ShadowStyle.None)
			{
				return;
			}
			vh.GetUIVertexStream(s_Verts);
			GetComponents(tmpShadows);
			foreach (UIShadow tmpShadow in tmpShadows)
			{
				if (!tmpShadow.isActiveAndEnabled)
				{
					continue;
				}
				if (!(tmpShadow == this))
				{
					break;
				}
				foreach (UIShadow tmpShadow2 in tmpShadows)
				{
					tmpShadow2._graphicVertexCount = s_Verts.Count;
				}
				break;
			}
			tmpShadows.Clear();
			_uiEffect = _uiEffect ?? GetComponent<UIEffect>();
			int start = s_Verts.Count - _graphicVertexCount;
			int end = s_Verts.Count;
			if (ptex != null && (bool)_uiEffect && _uiEffect.isActiveAndEnabled)
			{
				ptex.SetData(this, 0, _uiEffect.effectFactor);
				ptex.SetData(this, 1, byte.MaxValue);
				ptex.SetData(this, 2, m_BlurFactor);
			}
			_ApplyShadow(s_Verts, effectColor, ref start, ref end, effectDistance, style, useGraphicAlpha);
			vh.Clear();
			vh.AddUIVertexTriangleStream(s_Verts);
			s_Verts.Clear();
		}

		private void _ApplyShadow(List<UIVertex> verts, Color color, ref int start, ref int end, Vector2 effectDistance, ShadowStyle style, bool useGraphicAlpha)
		{
			if (style != 0 && !(color.a <= 0f))
			{
				_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, effectDistance.x, effectDistance.y, useGraphicAlpha);
				if (ShadowStyle.Shadow3 == style)
				{
					_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, effectDistance.x, 0f, useGraphicAlpha);
					_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, 0f, effectDistance.y, useGraphicAlpha);
				}
				else if (ShadowStyle.Outline == style)
				{
					_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, effectDistance.x, 0f - effectDistance.y, useGraphicAlpha);
					_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, 0f - effectDistance.x, effectDistance.y, useGraphicAlpha);
					_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, 0f - effectDistance.x, 0f - effectDistance.y, useGraphicAlpha);
				}
				else if (ShadowStyle.Outline8 == style)
				{
					_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, effectDistance.x, 0f - effectDistance.y, useGraphicAlpha);
					_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, 0f - effectDistance.x, effectDistance.y, useGraphicAlpha);
					_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, 0f - effectDistance.x, 0f - effectDistance.y, useGraphicAlpha);
					_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, 0f - effectDistance.x, 0f, useGraphicAlpha);
					_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, 0f, 0f - effectDistance.y, useGraphicAlpha);
					_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, effectDistance.x, 0f, useGraphicAlpha);
					_ApplyShadowZeroAlloc(s_Verts, color, ref start, ref end, 0f, effectDistance.y, useGraphicAlpha);
				}
			}
		}

		private void _ApplyShadowZeroAlloc(List<UIVertex> verts, Color color, ref int start, ref int end, float x, float y, bool useGraphicAlpha)
		{
			int num = end - start;
			int num2 = verts.Count + num;
			if (verts.Capacity < num2)
			{
				verts.Capacity *= 2;
			}
			float num3 = ((ptex != null && (bool)_uiEffect && _uiEffect.isActiveAndEnabled) ? ptex.GetNormalizedIndex(this) : (-1f));
			UIVertex item = default(UIVertex);
			for (int i = 0; i < num; i++)
			{
				verts.Add(item);
			}
			int num4 = verts.Count - 1;
			while (num <= num4)
			{
				verts[num4] = verts[num4 - num];
				num4--;
			}
			for (int j = 0; j < num; j++)
			{
				item = verts[j + start + num];
				Vector3 position = item.position;
				item.position.Set(position.x + x, position.y + y, position.z);
				Color color2 = effectColor;
				color2.a = (useGraphicAlpha ? (color.a * (float)(int)item.color.a / 255f) : color.a);
				item.color = color2;
				if (0f <= num3)
				{
					item.uv0 = new Vector2(item.uv0.x, num3);
				}
				verts[j] = item;
			}
			start = end;
			end = verts.Count;
		}

		private void _SetDirty()
		{
			if ((bool)base.graphic)
			{
				base.graphic.SetVerticesDirty();
			}
		}
	}
}
