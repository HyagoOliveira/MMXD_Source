using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CallbackDefs;
using KtxUnity;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class L10nRawImage : RawImage
{
	public enum ImageType
	{
		None = 0,
		Banner = 1,
		Stamp = 2,
		Texture = 3
	}

	public enum ImageEffect
	{
		None = 0,
		Fade = 1
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CLoadBasis_003Ed__28 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncVoidMethodBuilder _003C_003Et__builder;

		public byte[] p_param;

		public L10nRawImage _003C_003E4__this;

		private NativeArray<byte> _003Cna_003E5__2;

		private TaskAwaiter<TextureResult> _003C_003Eu__1;

		private void MoveNext()
		{
			int num = _003C_003E1__state;
			L10nRawImage l10nRawImage = _003C_003E4__this;
			try
			{
				TaskAwaiter<TextureResult> awaiter;
				if (num != 0)
				{
					byte[] array = p_param;
					_003Cna_003E5__2 = new NativeArray<byte>(array, Allocator.Persistent);
					awaiter = new BasisUniversalTexture().LoadFromBytes(_003Cna_003E5__2, MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance).GetAwaiter();
					if (!awaiter.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = awaiter;
						_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
						return;
					}
				}
				else
				{
					awaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(TaskAwaiter<TextureResult>);
					num = (_003C_003E1__state = -1);
				}
				TextureResult result = awaiter.GetResult();
				if (result.errorCode == ErrorCode.Success)
				{
					l10nRawImage.texture = result.texture;
					MonoBehaviourSingleton<LocalizationManager>.Instance.dictCacheTexture[l10nRawImage.TextureName] = l10nRawImage.texture;
					l10nRawImage.DisplayNewTexture();
					GC.Collect();
				}
				_003Cna_003E5__2.Dispose();
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private ImageEffect imgEft;

	private Callback<Texture> m_cb;

	private bool cache = true;

	private readonly bool useBasis = true;

	public ImageType ImgType { get; set; }

	public string TextureName { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public void Init(ImageType p_imageType, string p_textureName, Callback<Texture> p_cb = null, ImageEffect p_imgEft = ImageEffect.None, bool p_cache = true)
	{
		color = Color.gray;
		Texture value = null;
		imgEft = p_imgEft;
		m_cb = p_cb;
		if (MonoBehaviourSingleton<LocalizationManager>.Instance.dictCacheTexture.TryGetValue(p_textureName, out value))
		{
			base.texture = value;
			DisplayNewTexture();
			return;
		}
		cache = p_cache;
		ImgType = p_imageType;
		TextureName = p_textureName;
		if (useBasis)
		{
			MonoBehaviourSingleton<LocalizationManager>.Instance.GetL10nRawImage(ImgType, OrangeWebRequestLoad.LoadType.BASIS_L10N_TEXTURE, true, TextureName, LoadBasisTextureData);
		}
		else
		{
			MonoBehaviourSingleton<LocalizationManager>.Instance.GetL10nRawImage(ImgType, OrangeWebRequestLoad.LoadType.L10N_TEXTURE, true, TextureName, LoadRawTextureData);
		}
	}

	private void LoadRawTextureData(byte[] p_param, string p_path)
	{
		color = Color.gray;
		if (p_param != null)
		{
			Texture2D tex = new Texture2D(Width, Height);
			if (tex.LoadImage(p_param, true))
			{
				base.texture = tex;
			}
		}
		MonoBehaviourSingleton<LocalizationManager>.Instance.dictCacheTexture[TextureName] = base.texture;
		DisplayNewTexture();
	}

	private void DisplayNewTexture()
	{
		ImageEffect imageEffect = imgEft;
		if (imageEffect == ImageEffect.None || imageEffect != ImageEffect.Fade)
		{
			color = Color.white;
			m_cb.CheckTargetToInvoke(base.texture);
			m_cb = null;
		}
		else
		{
			LeanTween.color(base.rectTransform, Color.white, 0.1f).setOnComplete((Action)delegate
			{
				m_cb.CheckTargetToInvoke(base.texture);
				m_cb = null;
			});
		}
	}

	public void UpdateImageImmediate()
	{
		if (ImgType != 0)
		{
			Init(ImgType, TextureName, null, ImageEffect.None, cache);
		}
	}

	protected override void OnDisable()
	{
		if (!cache && !string.IsNullOrEmpty(TextureName))
		{
			MonoBehaviourSingleton<LocalizationManager>.Instance.ClearSingleTextureCache(TextureName);
		}
		Resources.UnloadUnusedAssets();
		base.OnDisable();
	}

	private void LoadBasisTextureData(byte[] p_param, string p_path)
	{
		color = Color.gray;
		if (p_param != null)
		{
			LoadBasis(p_param);
		}
		else
		{
			DisplayNewTexture();
		}
	}

	[AsyncStateMachine(typeof(_003CLoadBasis_003Ed__28))]
	private void LoadBasis(byte[] p_param)
	{
		_003CLoadBasis_003Ed__28 stateMachine = default(_003CLoadBasis_003Ed__28);
		stateMachine._003C_003E4__this = this;
		stateMachine.p_param = p_param;
		stateMachine._003C_003Et__builder = AsyncVoidMethodBuilder.Create();
		stateMachine._003C_003E1__state = -1;
		AsyncVoidMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
		_003C_003Et__builder.Start(ref stateMachine);
	}
}
