using UnityEngine;

namespace Kino
{
	//[ExecuteInEditMode]
	//[RequireComponent(typeof(Camera))]
	//[AddComponentMenu("Kino Image Effects/Bloom")]
	//public class Bloom : MonoBehaviour
	//{
	//	private int _Threshold = Shader.PropertyToID("_Threshold");

	//	private int _Curve = Shader.PropertyToID("_Curve");

	//	private int _PrefilterOffs = Shader.PropertyToID("_PrefilterOffs");

	//	private int _SampleScale = Shader.PropertyToID("_SampleScale");

	//	private int _Intensity = Shader.PropertyToID("_Intensity");

	//	private int _BaseTex = Shader.PropertyToID("_BaseTex");

	//	[SerializeField]
	//	[Tooltip("Filters out pixels under this level of brightness.")]
	//	private float _threshold = 0.8f;

	//	[SerializeField]
	//	[Range(0f, 1f)]
	//	[Tooltip("Makes transition between under/over-threshold gradual.")]
	//	private float _softKnee = 0.5f;

	//	[SerializeField]
	//	[Range(1f, 7f)]
	//	[Tooltip("Changes extent of veiling effects\nin a screen resolution-independent fashion.")]
	//	private float _radius = 2.5f;

	//	[SerializeField]
	//	[Tooltip("Blend factor of the result image.")]
	//	private float _intensity = 0.8f;

	//	[SerializeField]
	//	[HideInInspector]
	//	private Shader _shader;

	//	private Material _material;

	//	private const int kMaxIterations = 16;

	//	private RenderTexture[] _blurBuffer1 = new RenderTexture[16];

	//	private RenderTexture[] _blurBuffer2 = new RenderTexture[16];

	//	public float thresholdGamma
	//	{
	//		get
	//		{
	//			return Mathf.Max(_threshold, 0f);
	//		}
	//		set
	//		{
	//			_threshold = value;
	//		}
	//	}

	//	public float thresholdLinear
	//	{
	//		get
	//		{
	//			return GammaToLinear(thresholdGamma);
	//		}
	//		set
	//		{
	//			_threshold = LinearToGamma(value);
	//		}
	//	}

	//	public float softKnee
	//	{
	//		get
	//		{
	//			return _softKnee;
	//		}
	//		set
	//		{
	//			_softKnee = value;
	//		}
	//	}

	//	public float radius
	//	{
	//		get
	//		{
	//			return _radius;
	//		}
	//		set
	//		{
	//			_radius = value;
	//		}
	//	}

	//	public float intensity
	//	{
	//		get
	//		{
	//			return Mathf.Max(_intensity, 0f);
	//		}
	//		set
	//		{
	//			_intensity = value;
	//		}
	//	}

	//	private float LinearToGamma(float x)
	//	{
	//		return Mathf.LinearToGammaSpace(x);
	//	}

	//	private float GammaToLinear(float x)
	//	{
	//		return Mathf.GammaToLinearSpace(x);
	//	}

	//	private void OnEnable()
	//	{
	//		Shader shader = Shader.Find("Hidden/Kino/Bloom");
	//		_material = new Material(shader);
	//		_material.hideFlags = HideFlags.DontSave;
	//	}

	//	private void OnDisable()
	//	{
	//		Object.DestroyImmediate(_material);
	//	}

	//	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	//	{
	//		bool isMobilePlatform = Application.isMobilePlatform;
	//		int width = source.width;
	//		int height = source.height;
	//		width /= 2;
	//		height /= 2;
	//		RenderTextureFormat format = (isMobilePlatform ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
	//		float num = Mathf.Log(height, 2f) + _radius - 8f;
	//		int num2 = (int)num;
	//		int num3 = Mathf.Clamp(num2, 1, 16);
	//		float num4 = thresholdLinear;
	//		_material.SetFloat(_Threshold, num4);
	//		float num5 = num4 * _softKnee + 1E-05f;
	//		Vector3 vector = new Vector3(num4 - num5, num5 * 2f, 0.25f / num5);
	//		_material.SetVector(_Curve, vector);
	//		_material.SetFloat(_PrefilterOffs, 0f);
	//		_material.SetFloat(_SampleScale, 0.5f + num - (float)num2);
	//		_material.SetFloat(_Intensity, intensity);
	//		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, format);
	//		int pass = 0;
	//		Graphics.Blit(source, temporary, _material, pass);
	//		RenderTexture renderTexture = temporary;
	//		for (int i = 0; i < num3; i++)
	//		{
	//			_blurBuffer1[i] = RenderTexture.GetTemporary(renderTexture.width / 2, renderTexture.height / 2, 0, format);
	//			pass = ((i == 0) ? 2 : 4);
	//			Graphics.Blit(renderTexture, _blurBuffer1[i], _material, pass);
	//			renderTexture = _blurBuffer1[i];
	//		}
	//		for (int num6 = num3 - 2; num6 >= 0; num6--)
	//		{
	//			RenderTexture renderTexture2 = _blurBuffer1[num6];
	//			_material.SetTexture(_BaseTex, renderTexture2);
	//			_blurBuffer2[num6] = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, 0, format);
	//			pass = 5;
	//			Graphics.Blit(renderTexture, _blurBuffer2[num6], _material, pass);
	//			renderTexture = _blurBuffer2[num6];
	//		}
	//		_material.SetTexture(_BaseTex, source);
	//		pass = 7;
	//		Graphics.Blit(renderTexture, destination, _material, pass);
	//		for (int j = 0; j < 16; j++)
	//		{
	//			if (_blurBuffer1[j] != null)
	//			{
	//				RenderTexture.ReleaseTemporary(_blurBuffer1[j]);
	//			}
	//			if (_blurBuffer2[j] != null)
	//			{
	//				RenderTexture.ReleaseTemporary(_blurBuffer2[j]);
	//			}
	//			_blurBuffer1[j] = null;
	//			_blurBuffer2[j] = null;
	//		}
	//		RenderTexture.ReleaseTemporary(temporary);
	//	}
	//}
}
