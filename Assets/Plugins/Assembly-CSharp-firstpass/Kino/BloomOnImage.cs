using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Kino
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(RawImage))]
	[AddComponentMenu("Kino Image Effects/BloomOnImage")]
	public class BloomOnImage : MonoBehaviour
	{
		public GameObject _objectWithImage;

		private RenderTexture _destRenderTexture;

		private Material _blendingMaterial;

		[SerializeField]
		[Tooltip("Filters out pixels under this level of brightness.")]
		private float _threshold = 0.8f;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("Makes transition between under/over-threshold gradual.")]
		private float _softKnee = 0.5f;

		[SerializeField]
		[Range(1f, 7f)]
		[Tooltip("Changes extent of veiling effects\nin a screen resolution-independent fashion.")]
		private float _radius = 2.5f;

		[SerializeField]
		[Tooltip("Blend factor of the result image.")]
		private float _intensity = 0.8f;

		[SerializeField]
		[Tooltip("Controls filter quality and buffer resolution.")]
		private bool _highQuality = true;

		[SerializeField]
		[Tooltip("Reduces flashing noise with an additional filter.")]
		private bool _antiFlicker = true;

		[SerializeField]
		[HideInInspector]
		private Shader _shader;

		private Material _material;

		private const int kMaxIterations = 16;

		private RenderTexture[] _blurBuffer1 = new RenderTexture[16];

		private RenderTexture[] _blurBuffer2 = new RenderTexture[16];

		public Color additiveColor = new Color(0.25f, 0.25f, 0.25f, 1f);

		private Texture source;

		private Vector2Int sourceSize;

		private readonly int _Threshold = Shader.PropertyToID("_Threshold");

		private readonly int _Curve = Shader.PropertyToID("_Curve");

		private readonly int _PrefilterOffs = Shader.PropertyToID("_PrefilterOffs");

		private readonly int _SampleScale = Shader.PropertyToID("_SampleScale");

		private readonly int _Intensity = Shader.PropertyToID("_Intensity");

		private readonly int _BaseTex = Shader.PropertyToID("_BaseTex");

		private RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGBHalf;

		private bool isVisable = true;

		public float thresholdGamma
		{
			get
			{
				return Mathf.Max(_threshold, 0f);
			}
			set
			{
				_threshold = value;
			}
		}

		public float thresholdLinear
		{
			get
			{
				return GammaToLinear(thresholdGamma);
			}
			set
			{
				_threshold = LinearToGamma(value);
			}
		}

		public float softKnee
		{
			get
			{
				return _softKnee;
			}
			set
			{
				_softKnee = value;
			}
		}

		public float radius
		{
			get
			{
				return _radius;
			}
			set
			{
				_radius = value;
			}
		}

		public float intensity
		{
			get
			{
				return Mathf.Max(_intensity, 0f);
			}
			set
			{
				_intensity = value;
			}
		}

		public bool highQuality
		{
			get
			{
				return _highQuality;
			}
			set
			{
				_highQuality = value;
			}
		}

		public bool antiFlicker
		{
			get
			{
				return _antiFlicker;
			}
			set
			{
				_antiFlicker = value;
			}
		}

		private float LinearToGamma(float x)
		{
			return Mathf.LinearToGammaSpace(x);
		}

		private float GammaToLinear(float x)
		{
			return Mathf.GammaToLinearSpace(x);
		}

		public void Init(Color color)
		{
			if (_objectWithImage != null)
			{
				additiveColor = color;
				RawImage component = _objectWithImage.GetComponent<RawImage>();
				source = component.mainTexture;
				RectTransform rectTransform = component.rectTransform;
				sourceSize = new Vector2Int(Mathf.CeilToInt(rectTransform.sizeDelta.x), Mathf.CeilToInt(rectTransform.sizeDelta.y));
				RectTransform component2 = GetComponent<RectTransform>();
				component2.sizeDelta = new Vector2(sourceSize.x, sourceSize.y);
				component2.position = new Vector2(0f, 0f);
				RenderTextureDescriptor renderTextureDescriptor = default(RenderTextureDescriptor);
				int width = Mathf.CeilToInt((float)sourceSize.x * 0.7f);
				int height = Mathf.CeilToInt((float)sourceSize.y * 0.7f);
				RenderTextureFormat colorFormat = ((!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)) ? RenderTextureFormat.Default : RenderTextureFormat.ARGBHalf);
				renderTextureDescriptor = new RenderTextureDescriptor(width, height, colorFormat, 24);
				bool isMobilePlatform = Application.isMobilePlatform;
				renderTextureFormat = (isMobilePlatform ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
				renderTextureDescriptor.dimension = TextureDimension.Tex2D;
				renderTextureDescriptor.useMipMap = false;
				renderTextureDescriptor.sRGB = false;
				_destRenderTexture = RenderTexture.GetTemporary(renderTextureDescriptor);
				RawImage component3 = GetComponent<RawImage>();
				component3.texture = _destRenderTexture;
				component3.color = additiveColor;
				component3.raycastTarget = false;
			}
		}

		private void OnEnable()
		{
			Shader shader = Shader.Find("Hidden/Kino/Bloom");
			_material = new Material(shader);
			_material.hideFlags = HideFlags.DontSave;
			Shader shader2 = Shader.Find("Mobile/Particles/Additive");
			_blendingMaterial = new Material(shader2);
			_blendingMaterial.hideFlags = HideFlags.DontSave;
			GetComponent<RawImage>().material = _blendingMaterial;
			isVisable = true;
		}

		private void OnDisable()
		{
			isVisable = false;
		}

		private void OnDestroy()
		{
			_blendingMaterial.hideFlags = HideFlags.None;
			_material.hideFlags = HideFlags.None;
			Object.DestroyImmediate(_blendingMaterial);
			Object.DestroyImmediate(_material);
			_blendingMaterial = null;
			_material = null;
			_objectWithImage = null;
			RenderTexture.ReleaseTemporary(_destRenderTexture);
			_destRenderTexture = null;
		}

		private void Update()
		{
			if (_objectWithImage == null || source == null || !isVisable)
			{
				return;
			}
			int num = sourceSize.x;
			int num2 = sourceSize.y;
			if (!_highQuality)
			{
				num /= 2;
				num2 /= 2;
			}
			float num3 = Mathf.Log(num2, 2f) + _radius - 8f;
			int num4 = (int)num3;
			int num5 = Mathf.Clamp(num4, 1, 16);
			float num6 = thresholdLinear;
			_material.SetFloat(_Threshold, num6);
			float num7 = num6 * _softKnee + 1E-05f;
			Vector3 vector = new Vector3(num6 - num7, num7 * 2f, 0.25f / num7);
			_material.SetVector(_Curve, vector);
			bool flag = !_highQuality && _antiFlicker;
			_material.SetFloat(_PrefilterOffs, flag ? (-0.5f) : 0f);
			_material.SetFloat(_SampleScale, 0.5f + num3 - (float)num4);
			_material.SetFloat(_Intensity, intensity);
			RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, renderTextureFormat);
			int pass = (_antiFlicker ? 1 : 0);
			Graphics.Blit(source, temporary, _material, pass);
			RenderTexture renderTexture = temporary;
			for (int i = 0; i < num5; i++)
			{
				_blurBuffer1[i] = RenderTexture.GetTemporary(renderTexture.width / 2, renderTexture.height / 2, 0, renderTextureFormat);
				pass = ((i == 0) ? (_antiFlicker ? 3 : 2) : 4);
				Graphics.Blit(renderTexture, _blurBuffer1[i], _material, pass);
				renderTexture = _blurBuffer1[i];
			}
			for (int num8 = num5 - 2; num8 >= 0; num8--)
			{
				RenderTexture renderTexture2 = _blurBuffer1[num8];
				_material.SetTexture(_BaseTex, renderTexture2);
				_blurBuffer2[num8] = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, 0, renderTextureFormat);
				pass = (_highQuality ? 6 : 5);
				Graphics.Blit(renderTexture, _blurBuffer2[num8], _material, pass);
				renderTexture = _blurBuffer2[num8];
			}
			_material.SetTexture(_BaseTex, source);
			pass = 9;
			_destRenderTexture.DiscardContents();
			Graphics.Blit(renderTexture, _destRenderTexture, _material, pass);
			for (int j = 0; j < 16; j++)
			{
				if (_blurBuffer1[j] != null)
				{
					RenderTexture.ReleaseTemporary(_blurBuffer1[j]);
				}
				if (_blurBuffer2[j] != null)
				{
					RenderTexture.ReleaseTemporary(_blurBuffer2[j]);
				}
				_blurBuffer1[j] = null;
				_blurBuffer2[j] = null;
			}
			RenderTexture.ReleaseTemporary(temporary);
		}
	}
}
