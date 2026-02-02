using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[AddComponentMenu("UI/UIEffect/UITransitionEffect", 5)]
	public class UITransitionEffect : UIEffectBase
	{
		public enum EffectMode
		{
			Fade = 1,
			Cutoff = 2,
			Dissolve = 3
		}

		public const string shaderName = "UI/Hidden/UI-Effect-Transition";

		private static readonly ParameterTexture _ptex = new ParameterTexture(8, 128, "_ParamTex");

		[Tooltip("Effect mode.")]
		[SerializeField]
		private EffectMode m_EffectMode = EffectMode.Cutoff;

		[Tooltip("Effect factor between 0(hidden) and 1(shown).")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_EffectFactor = 1f;

		[Tooltip("Transition texture (single channel texture).")]
		[SerializeField]
		private Texture m_TransitionTexture;

		[Header("Advanced Option")]
		[Tooltip("The area for effect.")]
		[SerializeField]
		private EffectArea m_EffectArea;

		[Tooltip("Keep effect aspect ratio.")]
		[SerializeField]
		private bool m_KeepAspectRatio;

		[Tooltip("Dissolve edge width.")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_DissolveWidth = 0.5f;

		[Tooltip("Dissolve edge softness.")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_DissolveSoftness = 0.5f;

		[Tooltip("Dissolve edge color.")]
		[SerializeField]
		[ColorUsage(false)]
		private Color m_DissolveColor = new Color(0f, 0.25f, 1f);

		[Tooltip("Disable graphic's raycast target on hidden.")]
		[SerializeField]
		private bool m_PassRayOnHidden;

		[Header("Effect Player")]
		[SerializeField]
		private EffectPlayer m_Player;

		private MaterialCache _materialCache;

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

		public Texture transitionTexture
		{
			get
			{
				return m_TransitionTexture;
			}
			set
			{
				if (m_TransitionTexture != value)
				{
					m_TransitionTexture = value;
					if ((bool)base.graphic)
					{
						ModifyMaterial();
					}
				}
			}
		}

		public EffectMode effectMode
		{
			get
			{
				return m_EffectMode;
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
					base.targetGraphic.SetVerticesDirty();
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

		public float dissolveWidth
		{
			get
			{
				return m_DissolveWidth;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(m_DissolveWidth, value))
				{
					m_DissolveWidth = value;
					SetDirty();
				}
			}
		}

		public float dissolveSoftness
		{
			get
			{
				return m_DissolveSoftness;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(m_DissolveSoftness, value))
				{
					m_DissolveSoftness = value;
					SetDirty();
				}
			}
		}

		public Color dissolveColor
		{
			get
			{
				return m_DissolveColor;
			}
			set
			{
				if (m_DissolveColor != value)
				{
					m_DissolveColor = value;
					SetDirty();
				}
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

		public bool passRayOnHidden
		{
			get
			{
				return m_PassRayOnHidden;
			}
			set
			{
				m_PassRayOnHidden = value;
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

		private EffectPlayer _player
		{
			get
			{
				return m_Player ?? (m_Player = new EffectPlayer());
			}
		}

		public void Show(bool reset = true)
		{
			_player.loop = false;
			_player.Play(reset, delegate(float f)
			{
				effectFactor = f;
			});
		}

		public void Hide(bool reset = true)
		{
			_player.loop = false;
			_player.Play(reset, delegate(float f)
			{
				effectFactor = 1f - f;
			});
		}

		public override void ModifyMaterial()
		{
			if (base.isTMPro)
			{
				return;
			}
			ulong num = (ulong)((long)(uint)(m_TransitionTexture ? m_TransitionTexture.GetInstanceID() : 0) + 8589934592L + ((long)m_EffectMode << 36));
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
			if (!m_TransitionTexture)
			{
				material = m_EffectMaterial;
				return;
			}
			if (_materialCache != null && _materialCache.hash == num)
			{
				material = _materialCache.material;
				return;
			}
			_materialCache = MaterialCache.Register(num, m_TransitionTexture, delegate
			{
				Material obj = new Material(m_EffectMaterial);
				obj.name = obj.name + "_" + m_TransitionTexture.name;
				obj.SetTexture("_NoiseTex", m_TransitionTexture);
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
				Texture texture = transitionTexture;
				float aspectRatio = ((m_KeepAspectRatio && (bool)texture) ? ((float)texture.width / (float)texture.height) : (-1f));
				Rect effectArea = m_EffectArea.GetEffectArea(vh, base.rectTransform.rect, aspectRatio);
				UIVertex vertex = default(UIVertex);
				int currentVertCount = vh.currentVertCount;
				for (int i = 0; i < currentVertCount; i++)
				{
					vh.PopulateUIVertex(ref vertex, i);
					float x;
					float y;
					m_EffectArea.GetPositionFactor(i, effectArea, vertex.position, isText, base.isTMPro, out x, out y);
					vertex.uv0 = new Vector2(Packer.ToFloat(vertex.uv0.x, vertex.uv0.y), Packer.ToFloat(x, y, normalizedIndex));
					vh.SetUIVertex(vertex, i);
				}
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			_player.OnEnable();
			_player.loop = false;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			MaterialCache.Unregister(_materialCache);
			_materialCache = null;
			_player.OnDisable();
		}

		protected override void SetDirty()
		{
			Material[] array = materials;
			foreach (Material mat in array)
			{
				ptex.RegisterMaterial(mat);
			}
			ptex.SetData(this, 0, m_EffectFactor);
			if (m_EffectMode == EffectMode.Dissolve)
			{
				ptex.SetData(this, 1, m_DissolveWidth);
				ptex.SetData(this, 2, m_DissolveSoftness);
				ptex.SetData(this, 4, m_DissolveColor.r);
				ptex.SetData(this, 5, m_DissolveColor.g);
				ptex.SetData(this, 6, m_DissolveColor.b);
			}
			if (m_PassRayOnHidden)
			{
				base.targetGraphic.raycastTarget = 0f < m_EffectFactor;
			}
		}
	}
}
