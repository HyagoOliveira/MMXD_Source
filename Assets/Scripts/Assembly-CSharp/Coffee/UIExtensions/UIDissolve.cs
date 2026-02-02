using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[AddComponentMenu("UI/UIEffect/UIDissolve", 3)]
	public class UIDissolve : UIEffectBase
	{
		public const string shaderName = "UI/Hidden/UI-Effect-Dissolve";

		private static readonly ParameterTexture _ptex = new ParameterTexture(8, 128, "_ParamTex");

		[Tooltip("Current location[0-1] for dissolve effect. 0 is not dissolved, 1 is completely dissolved.")]
		[FormerlySerializedAs("m_Location")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_EffectFactor = 0.5f;

		[Tooltip("Edge width.")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_Width = 0.5f;

		[Tooltip("Edge softness.")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_Softness = 0.5f;

		[Tooltip("Edge color.")]
		[SerializeField]
		[ColorUsage(false)]
		private Color m_Color = new Color(0f, 0.25f, 1f);

		[Tooltip("Edge color effect mode.")]
		[SerializeField]
		private ColorMode m_ColorMode = ColorMode.Add;

		[Tooltip("Noise texture for dissolving (single channel texture).")]
		[SerializeField]
		private Texture m_NoiseTexture;

		[Header("Advanced Option")]
		[Tooltip("The area for effect.")]
		[SerializeField]
		protected EffectArea m_EffectArea;

		[Tooltip("Keep effect aspect ratio.")]
		[SerializeField]
		private bool m_KeepAspectRatio;

		[Header("Effect Player")]
		[SerializeField]
		private EffectPlayer m_Player;

		[Tooltip("Reverse the dissolve effect.")]
		[FormerlySerializedAs("m_ReverseAnimation")]
		[SerializeField]
		private bool m_Reverse;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		[Range(0.1f, 10f)]
		private float m_Duration = 1f;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		private AnimatorUpdateMode m_UpdateMode;

		private MaterialCache _materialCache;

		[Obsolete("Use effectFactor instead (UnityUpgradable) -> effectFactor")]
		public float location
		{
			get
			{
				return m_EffectFactor;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(m_EffectFactor, value))
				{
					m_EffectFactor = value;
					SetDirty();
				}
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
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(m_EffectFactor, value))
				{
					m_EffectFactor = value;
					SetDirty();
				}
			}
		}

		public float width
		{
			get
			{
				return m_Width;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(m_Width, value))
				{
					m_Width = value;
					SetDirty();
				}
			}
		}

		public float softness
		{
			get
			{
				return m_Softness;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(m_Softness, value))
				{
					m_Softness = value;
					SetDirty();
				}
			}
		}

		public Color color
		{
			get
			{
				return m_Color;
			}
			set
			{
				if (m_Color != value)
				{
					m_Color = value;
					SetDirty();
				}
			}
		}

		public Texture noiseTexture
		{
			get
			{
				return m_NoiseTexture ?? material.GetTexture("_NoiseTex");
			}
			set
			{
				if (m_NoiseTexture != value)
				{
					m_NoiseTexture = value;
					if ((bool)base.graphic)
					{
						ModifyMaterial();
					}
				}
			}
		}

		public EffectArea effectArea
		{
			get
			{
				return m_EffectArea;
			}
			set
			{
				if (m_EffectArea != value)
				{
					m_EffectArea = value;
					SetVerticesDirty();
				}
			}
		}

		public bool keepAspectRatio
		{
			get
			{
				return m_KeepAspectRatio;
			}
			set
			{
				if (m_KeepAspectRatio != value)
				{
					m_KeepAspectRatio = value;
					SetVerticesDirty();
				}
			}
		}

		public ColorMode colorMode
		{
			get
			{
				return m_ColorMode;
			}
		}

		[Obsolete("Use Play/Stop method instead")]
		public bool play
		{
			get
			{
				return _player.play;
			}
			set
			{
				_player.play = value;
			}
		}

		[Obsolete]
		public bool loop
		{
			get
			{
				return _player.loop;
			}
			set
			{
				_player.loop = value;
			}
		}

		public float duration
		{
			get
			{
				return _player.duration;
			}
			set
			{
				_player.duration = Mathf.Max(value, 0.1f);
			}
		}

		[Obsolete]
		public float loopDelay
		{
			get
			{
				return _player.loopDelay;
			}
			set
			{
				_player.loopDelay = Mathf.Max(value, 0f);
			}
		}

		public AnimatorUpdateMode updateMode
		{
			get
			{
				return _player.updateMode;
			}
			set
			{
				_player.updateMode = value;
			}
		}

		public bool reverse
		{
			get
			{
				return m_Reverse;
			}
			set
			{
				m_Reverse = value;
			}
		}

		public override ParameterTexture ptex
		{
			get
			{
				return _ptex;
			}
		}

		private EffectPlayer _player
		{
			get
			{
				return m_Player ?? (m_Player = new EffectPlayer());
			}
		}

		public override void ModifyMaterial()
		{
			if (base.isTMPro)
			{
				return;
			}
			ulong num = (ulong)((long)(uint)(m_NoiseTexture ? m_NoiseTexture.GetInstanceID() : 0) + 4294967296L + ((long)m_ColorMode << 36));
			if (_materialCache != null && (_materialCache.hash != num || !base.isActiveAndEnabled || !m_EffectMaterial))
			{
				MaterialCache.Unregister(_materialCache);
				_materialCache = null;
			}
			if (!base.isActiveAndEnabled || !m_EffectMaterial)
			{
				material = null;
				return;
			}
			if (!m_NoiseTexture)
			{
				material = m_EffectMaterial;
				return;
			}
			if (_materialCache != null && _materialCache.hash == num)
			{
				material = _materialCache.material;
				return;
			}
			_materialCache = MaterialCache.Register(num, m_NoiseTexture, delegate
			{
				Material obj = new Material(m_EffectMaterial);
				obj.name = obj.name + "_" + m_NoiseTexture.name;
				obj.SetTexture("_NoiseTex", m_NoiseTexture);
				return obj;
			});
			material = _materialCache.material;
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (base.isActiveAndEnabled)
			{
				bool isText = base.isTMPro || base.graphic is Text;
				float normalizedIndex = ptex.GetNormalizedIndex(this);
				Texture texture = noiseTexture;
				float aspectRatio = ((m_KeepAspectRatio && (bool)texture) ? ((float)texture.width / (float)texture.height) : (-1f));
				Rect rect = m_EffectArea.GetEffectArea(vh, base.rectTransform.rect, aspectRatio);
				UIVertex vertex = default(UIVertex);
				int currentVertCount = vh.currentVertCount;
				for (int i = 0; i < currentVertCount; i++)
				{
					vh.PopulateUIVertex(ref vertex, i);
					float x;
					float y;
					m_EffectArea.GetPositionFactor(i, rect, vertex.position, isText, base.isTMPro, out x, out y);
					vertex.uv0 = new Vector2(Packer.ToFloat(vertex.uv0.x, vertex.uv0.y), Packer.ToFloat(x, y, normalizedIndex));
					vh.SetUIVertex(vertex, i);
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
			ptex.SetData(this, 1, m_Width);
			ptex.SetData(this, 2, m_Softness);
			ptex.SetData(this, 4, m_Color.r);
			ptex.SetData(this, 5, m_Color.g);
			ptex.SetData(this, 6, m_Color.b);
		}

		public void Play(bool reset = true)
		{
			_player.Play(reset);
		}

		public void Stop(bool reset = true)
		{
			_player.Stop(reset);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			_player.OnEnable(delegate(float f)
			{
				effectFactor = (m_Reverse ? (1f - f) : f);
			});
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			MaterialCache.Unregister(_materialCache);
			_materialCache = null;
			_player.OnDisable();
		}
	}
}
