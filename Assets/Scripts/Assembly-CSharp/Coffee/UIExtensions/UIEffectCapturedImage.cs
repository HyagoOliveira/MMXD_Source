using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[AddComponentMenu("UI/UIEffect/UIEffectCapturedImage", 200)]
	public class UIEffectCapturedImage : RawImage
	{
		public enum DesamplingRate
		{
			None = 0,
			x1 = 1,
			x2 = 2,
			x4 = 4,
			x8 = 8
		}

		public const string shaderName = "UI/Hidden/UI-EffectCapture";

		[Tooltip("Effect factor between 0(no effect) and 1(complete effect).")]
		[FormerlySerializedAs("m_ToneLevel")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_EffectFactor = 1f;

		[Tooltip("Color effect factor between 0(no effect) and 1(complete effect).")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_ColorFactor = 1f;

		[Tooltip("How far is the blurring from the graphic.")]
		[FormerlySerializedAs("m_Blur")]
		[SerializeField]
		[Range(0f, 1f)]
		private float m_BlurFactor = 1f;

		[Tooltip("Effect mode.")]
		[FormerlySerializedAs("m_ToneMode")]
		[SerializeField]
		private EffectMode m_EffectMode;

		[Tooltip("Color effect mode.")]
		[SerializeField]
		private ColorMode m_ColorMode;

		[Tooltip("Blur effect mode.")]
		[SerializeField]
		private BlurMode m_BlurMode = BlurMode.DetailBlur;

		[Tooltip("Color for the color effect.")]
		[SerializeField]
		private Color m_EffectColor = Color.white;

		[Tooltip("Desampling rate of the generated RenderTexture.")]
		[SerializeField]
		private DesamplingRate m_DesamplingRate = DesamplingRate.x1;

		[Tooltip("Desampling rate of reduction buffer to apply effect.")]
		[SerializeField]
		private DesamplingRate m_ReductionRate = DesamplingRate.x1;

		[Tooltip("FilterMode for capturing.")]
		[SerializeField]
		private FilterMode m_FilterMode = FilterMode.Bilinear;

		[Tooltip("Effect material.")]
		[SerializeField]
		private Material m_EffectMaterial;

		[Tooltip("Blur iterations.")]
		[FormerlySerializedAs("m_Iterations")]
		[SerializeField]
		[Range(1f, 8f)]
		private int m_BlurIterations = 3;

		[Tooltip("Fits graphic size to screen on captured.")]
		[FormerlySerializedAs("m_KeepCanvasSize")]
		[SerializeField]
		private bool m_FitToScreen = true;

		[Tooltip("Capture automatically on enable.")]
		[SerializeField]
		private bool m_CaptureOnEnable;

		private RenderTexture _rt;

		private RenderTargetIdentifier _rtId;

		private static int s_CopyId;

		private static int s_EffectId1;

		private static int s_EffectId2;

		private static int s_EffectFactorId;

		private static int s_ColorFactorId;

		private static CommandBuffer s_CommandBuffer;

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
				m_BlurFactor = Mathf.Clamp(value, 0f, 4f);
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
				m_BlurFactor = Mathf.Clamp(value, 0f, 4f);
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
				return m_EffectColor;
			}
			set
			{
				m_EffectColor = value;
			}
		}

		public virtual Material effectMaterial
		{
			get
			{
				return m_EffectMaterial;
			}
		}

		public DesamplingRate desamplingRate
		{
			get
			{
				return m_DesamplingRate;
			}
			set
			{
				m_DesamplingRate = value;
			}
		}

		public DesamplingRate reductionRate
		{
			get
			{
				return m_ReductionRate;
			}
			set
			{
				m_ReductionRate = value;
			}
		}

		public FilterMode filterMode
		{
			get
			{
				return m_FilterMode;
			}
			set
			{
				m_FilterMode = value;
			}
		}

		public RenderTexture capturedTexture
		{
			get
			{
				return _rt;
			}
		}

		[Obsolete("Use blurIterations instead (UnityUpgradable) -> blurIterations")]
		public int iterations
		{
			get
			{
				return m_BlurIterations;
			}
			set
			{
				m_BlurIterations = value;
			}
		}

		public int blurIterations
		{
			get
			{
				return m_BlurIterations;
			}
			set
			{
				m_BlurIterations = value;
			}
		}

		[Obsolete("Use fitToScreen instead (UnityUpgradable) -> fitToScreen")]
		public bool keepCanvasSize
		{
			get
			{
				return m_FitToScreen;
			}
			set
			{
				m_FitToScreen = value;
			}
		}

		public bool fitToScreen
		{
			get
			{
				return m_FitToScreen;
			}
			set
			{
				m_FitToScreen = value;
			}
		}

		[Obsolete]
		public RenderTexture targetTexture
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public bool captureOnEnable
		{
			get
			{
				return m_CaptureOnEnable;
			}
			set
			{
				m_CaptureOnEnable = value;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (m_CaptureOnEnable && Application.isPlaying)
			{
				Capture();
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (m_CaptureOnEnable && Application.isPlaying)
			{
				_Release(false);
				base.texture = null;
			}
		}

		protected override void OnDestroy()
		{
			Release();
			base.OnDestroy();
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			if (base.texture == null || this.color.a < 0.003921569f || base.canvasRenderer.GetAlpha() < 0.003921569f)
			{
				vh.Clear();
				return;
			}
			base.OnPopulateMesh(vh);
			int currentVertCount = vh.currentVertCount;
			UIVertex vertex = default(UIVertex);
			Color color = this.color;
			for (int i = 0; i < currentVertCount; i++)
			{
				vh.PopulateUIVertex(ref vertex, i);
				vertex.color = color;
				vh.SetUIVertex(vertex, i);
			}
		}

		public void GetDesamplingSize(DesamplingRate rate, out int w, out int h)
		{
			w = Screen.width;
			h = Screen.height;
			if (rate != 0)
			{
				float num = (float)w / (float)h;
				if (w < h)
				{
					h = Mathf.ClosestPowerOfTwo(h / (int)rate);
					w = Mathf.CeilToInt((float)h * num);
				}
				else
				{
					w = Mathf.ClosestPowerOfTwo(w / (int)rate);
					h = Mathf.CeilToInt((float)w / num);
				}
			}
		}

		public void Capture()
		{
			Canvas rootCanvas = base.canvas.rootCanvas;
			if (m_FitToScreen)
			{
				RectTransform rectTransform = rootCanvas.transform as RectTransform;
				Vector2 size = rectTransform.rect.size;
				base.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
				base.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
				base.rectTransform.position = rectTransform.position;
			}
			if (s_CopyId == 0)
			{
				s_CopyId = Shader.PropertyToID("_UIEffectCapturedImage_ScreenCopyId");
				s_EffectId1 = Shader.PropertyToID("_UIEffectCapturedImage_EffectId1");
				s_EffectId2 = Shader.PropertyToID("_UIEffectCapturedImage_EffectId2");
				s_EffectFactorId = Shader.PropertyToID("_EffectFactor");
				s_ColorFactorId = Shader.PropertyToID("_ColorFactor");
				s_CommandBuffer = new CommandBuffer();
			}
			int w;
			int h;
			GetDesamplingSize(m_DesamplingRate, out w, out h);
			if ((bool)_rt && (_rt.width != w || _rt.height != h))
			{
				_Release(ref _rt);
			}
			if (_rt == null)
			{
				_rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
				_rt.filterMode = m_FilterMode;
				_rt.useMipMap = false;
				_rt.wrapMode = TextureWrapMode.Clamp;
				_rtId = new RenderTargetIdentifier(_rt);
			}
			SetupCommandBuffer();
		}

		private void SetupCommandBuffer()
		{
			Material mat = m_EffectMaterial;
			if (s_CommandBuffer == null)
			{
				s_CommandBuffer = new CommandBuffer();
			}
			int w;
			int h;
			GetDesamplingSize(DesamplingRate.None, out w, out h);
			s_CommandBuffer.GetTemporaryRT(s_CopyId, w, h, 0, m_FilterMode);
			s_CommandBuffer.Blit(BuiltinRenderTextureType.BindableTexture, s_CopyId);
			s_CommandBuffer.SetGlobalVector(s_EffectFactorId, new Vector4(m_EffectFactor, 0f));
			s_CommandBuffer.SetGlobalVector(s_ColorFactorId, new Vector4(m_EffectColor.r, m_EffectColor.g, m_EffectColor.b, m_EffectColor.a));
			GetDesamplingSize(m_ReductionRate, out w, out h);
			s_CommandBuffer.GetTemporaryRT(s_EffectId1, w, h, 0, m_FilterMode);
			s_CommandBuffer.Blit(s_CopyId, s_EffectId1, mat, 0);
			s_CommandBuffer.ReleaseTemporaryRT(s_CopyId);
			if (m_BlurMode != 0)
			{
				s_CommandBuffer.GetTemporaryRT(s_EffectId2, w, h, 0, m_FilterMode);
				for (int i = 0; i < m_BlurIterations; i++)
				{
					s_CommandBuffer.SetGlobalVector(s_EffectFactorId, new Vector4(m_BlurFactor, 0f));
					s_CommandBuffer.Blit(s_EffectId1, s_EffectId2, mat, 1);
					s_CommandBuffer.SetGlobalVector(s_EffectFactorId, new Vector4(0f, m_BlurFactor));
					s_CommandBuffer.Blit(s_EffectId2, s_EffectId1, mat, 1);
				}
				s_CommandBuffer.ReleaseTemporaryRT(s_EffectId2);
			}
			s_CommandBuffer.Blit(s_EffectId1, _rtId);
			s_CommandBuffer.ReleaseTemporaryRT(s_EffectId1);
			base.canvas.rootCanvas.GetComponent<CanvasScaler>().StartCoroutine(_CoUpdateTextureOnNextFrame());
		}

		public void Release()
		{
			_Release(true);
			base.texture = null;
		}

		private void _Release(bool releaseRT)
		{
			if (releaseRT)
			{
				base.texture = null;
				_Release(ref _rt);
			}
			if (s_CommandBuffer != null)
			{
				s_CommandBuffer.Clear();
				if (releaseRT)
				{
					s_CommandBuffer.Release();
					s_CommandBuffer = null;
				}
			}
		}

		[Conditional("UNITY_EDITOR")]
		private void _SetDirty()
		{
		}

		private void _Release(ref RenderTexture obj)
		{
			if ((bool)obj)
			{
				obj.Release();
				RenderTexture.ReleaseTemporary(obj);
				obj = null;
			}
		}

		private IEnumerator _CoUpdateTextureOnNextFrame()
		{
			yield return new WaitForEndOfFrame();
			UpdateTexture();
		}

		private void UpdateTexture()
		{
			Graphics.ExecuteCommandBuffer(s_CommandBuffer);
			_Release(false);
			base.texture = capturedTexture;
		}
	}
}
