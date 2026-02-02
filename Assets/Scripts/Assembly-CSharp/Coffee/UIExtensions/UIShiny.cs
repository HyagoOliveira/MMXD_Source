using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[AddComponentMenu("UI/UIEffect/UIShiny", 2)]
	public class UIShiny : UIEffectBase
	{
		public const string shaderName = "UI/Hidden/UI-Effect-Shiny";

		private static readonly ParameterTexture _ptex = new ParameterTexture(8, 128, "_ParamTex");

		[Tooltip("Location for shiny effect.")]
		[FormerlySerializedAs("m_Location")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_EffectFactor;

		[Tooltip("Width for shiny effect.")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_Width = 0.25f;

		[Tooltip("Rotation for shiny effect.")]
		[SerializeField]
		[Range(-180f, 180f)]
		private float m_Rotation;

		[Tooltip("Softness for shiny effect.")]
		[SerializeField]
		[Range(0.01f, 1f)]
		private float m_Softness = 1f;

		[Tooltip("Brightness for shiny effect.")]
		[FormerlySerializedAs("m_Alpha")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_Brightness = 1f;

		[Tooltip("Gloss factor for shiny effect.")]
		[FormerlySerializedAs("m_Highlight")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_Gloss = 1f;

		[Header("Advanced Option")]
		[Tooltip("The area for effect.")]
		[SerializeField]
		protected EffectArea m_EffectArea;

		[SerializeField]
		private EffectPlayer m_Player;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		private bool m_Play;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		private bool m_Loop;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		[Range(0.1f, 10f)]
		private float m_Duration = 1f;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		[Range(0f, 10f)]
		private float m_LoopDelay = 1f;

		[Obsolete]
		[HideInInspector]
		[SerializeField]
		private AnimatorUpdateMode m_UpdateMode;

		private float _lastRotation;

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
				value = Mathf.Clamp(value, 0.01f, 1f);
				if (!Mathf.Approximately(m_Softness, value))
				{
					m_Softness = value;
					SetDirty();
				}
			}
		}

		[Obsolete("Use brightness instead (UnityUpgradable) -> brightness")]
		public float alpha
		{
			get
			{
				return m_Brightness;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(m_Brightness, value))
				{
					m_Brightness = value;
					SetDirty();
				}
			}
		}

		public float brightness
		{
			get
			{
				return m_Brightness;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(m_Brightness, value))
				{
					m_Brightness = value;
					SetDirty();
				}
			}
		}

		[Obsolete("Use gloss instead (UnityUpgradable) -> gloss")]
		public float highlight
		{
			get
			{
				return m_Gloss;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(m_Gloss, value))
				{
					m_Gloss = value;
					SetDirty();
				}
			}
		}

		public float gloss
		{
			get
			{
				return m_Gloss;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, 1f);
				if (!Mathf.Approximately(m_Gloss, value))
				{
					m_Gloss = value;
					SetDirty();
				}
			}
		}

		public float rotation
		{
			get
			{
				return m_Rotation;
			}
			set
			{
				if (!Mathf.Approximately(m_Rotation, value))
				{
					m_Rotation = (_lastRotation = value);
					SetVerticesDirty();
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

		protected override void OnEnable()
		{
			base.OnEnable();
			_player.OnEnable(delegate(float f)
			{
				effectFactor = f;
			});
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			_player.OnDisable();
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (base.isActiveAndEnabled)
			{
				bool isText = base.isTMPro || base.graphic is Text;
				float normalizedIndex = ptex.GetNormalizedIndex(this);
				Rect rect = m_EffectArea.GetEffectArea(vh, base.rectTransform.rect);
				float f = m_Rotation * ((float)Math.PI / 180f);
				Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
				vector.x *= rect.height / rect.width;
				vector = vector.normalized;
				UIVertex vertex = default(UIVertex);
				Matrix2x3 matrix = new Matrix2x3(rect, vector.x, vector.y);
				for (int i = 0; i < vh.currentVertCount; i++)
				{
					vh.PopulateUIVertex(ref vertex, i);
					Vector2 nomalizedPos;
					m_EffectArea.GetNormalizedFactor(i, matrix, vertex.position, isText, out nomalizedPos);
					vertex.uv0 = new Vector2(Packer.ToFloat(vertex.uv0.x, vertex.uv0.y), Packer.ToFloat(nomalizedPos.y, normalizedIndex));
					vh.SetUIVertex(vertex, i);
				}
			}
		}

		public void Play(bool reset = true)
		{
			_player.Play(reset);
		}

		public void Stop(bool reset = true)
		{
			_player.Stop(reset);
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
			ptex.SetData(this, 3, m_Brightness);
			ptex.SetData(this, 4, m_Gloss);
			if (!Mathf.Approximately(_lastRotation, m_Rotation) && (bool)base.targetGraphic)
			{
				_lastRotation = m_Rotation;
				SetVerticesDirty();
			}
		}
	}
}
