using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[ExecuteInEditMode]
	public abstract class BaseMeshEffect : UIBehaviour, IMeshModifier
	{
		private static readonly Material[] s_EmptyMaterials = new Material[0];

		private bool _initialized;

		private CanvasRenderer _canvasRenderer;

		private RectTransform _rectTransform;

		private Graphic _graphic;

		private Material[] _materials = new Material[1];

		public Graphic graphic
		{
			get
			{
				Initialize();
				return _graphic;
			}
		}

		public CanvasRenderer canvasRenderer
		{
			get
			{
				Initialize();
				return _canvasRenderer;
			}
		}

		public RectTransform rectTransform
		{
			get
			{
				Initialize();
				return _rectTransform;
			}
		}

		public virtual AdditionalCanvasShaderChannels requiredChannels
		{
			get
			{
				return AdditionalCanvasShaderChannels.None;
			}
		}

		public bool isTMPro
		{
			get
			{
				return false;
			}
		}

		public virtual Material material
		{
			get
			{
				if ((bool)graphic)
				{
					return graphic.material;
				}
				return null;
			}
			set
			{
				if ((bool)graphic)
				{
					graphic.material = value;
				}
			}
		}

		public virtual Material[] materials
		{
			get
			{
				if ((bool)graphic)
				{
					_materials[0] = graphic.material;
					return _materials;
				}
				return s_EmptyMaterials;
			}
		}

		protected virtual bool isLegacyMeshModifier
		{
			get
			{
				return false;
			}
		}

		public virtual void ModifyMesh(Mesh mesh)
		{
		}

		public virtual void ModifyMesh(VertexHelper vh)
		{
		}

		public virtual void SetVerticesDirty()
		{
			if ((bool)graphic)
			{
				graphic.SetVerticesDirty();
			}
		}

		public void ShowTMProWarning(Shader shader, Shader mobileShader, Shader spriteShader, Action<Material> onCreatedMaterial)
		{
		}

		protected virtual void Initialize()
		{
			if (!_initialized)
			{
				_initialized = true;
				_graphic = _graphic ?? GetComponent<Graphic>();
				_canvasRenderer = _canvasRenderer ?? GetComponent<CanvasRenderer>();
				_rectTransform = _rectTransform ?? GetComponent<RectTransform>();
			}
		}

		protected override void OnEnable()
		{
			_initialized = false;
			SetVerticesDirty();
			if ((bool)graphic)
			{
				AdditionalCanvasShaderChannels additionalCanvasShaderChannels = requiredChannels;
				Canvas canvas = graphic.canvas;
				if ((bool)canvas)
				{
					AdditionalCanvasShaderChannels additionalCanvasShaderChannel = canvas.additionalShaderChannels & additionalCanvasShaderChannels;
				}
			}
		}

		protected override void OnDisable()
		{
			SetVerticesDirty();
		}

		protected virtual void LateUpdate()
		{
		}

		protected override void OnDidApplyAnimationProperties()
		{
			SetVerticesDirty();
		}
	}
}
