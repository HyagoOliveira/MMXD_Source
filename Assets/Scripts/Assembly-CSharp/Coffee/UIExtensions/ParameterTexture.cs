using System;
using System.Collections.Generic;
using UnityEngine;

namespace Coffee.UIExtensions
{
	[Serializable]
	public class ParameterTexture
	{
		private Texture2D _texture;

		private bool _needUpload;

		private int _propertyId;

		private readonly string _propertyName;

		private readonly int _channels;

		private readonly int _instanceLimit;

		private readonly byte[] _data;

		private readonly Stack<int> _stack;

		private static List<Action> updates;

		public ParameterTexture(int channels, int instanceLimit, string propertyName)
		{
			_propertyName = propertyName;
			_channels = ((channels - 1) / 4 + 1) * 4;
			_instanceLimit = ((instanceLimit - 1) / 2 + 1) * 2;
			_data = new byte[_channels * _instanceLimit];
			_stack = new Stack<int>(_instanceLimit);
			for (int i = 1; i < _instanceLimit + 1; i++)
			{
				_stack.Push(i);
			}
		}

		public void Register(IParameterTexture target)
		{
			Initialize();
			if (target.parameterIndex <= 0 && 0 < _stack.Count)
			{
				target.parameterIndex = _stack.Pop();
			}
		}

		public void Unregister(IParameterTexture target)
		{
			if (0 < target.parameterIndex)
			{
				_stack.Push(target.parameterIndex);
				target.parameterIndex = 0;
			}
		}

		public void SetData(IParameterTexture target, int channelId, byte value)
		{
			int num = (target.parameterIndex - 1) * _channels + channelId;
			if (0 < target.parameterIndex && _data[num] != value)
			{
				_data[num] = value;
				_needUpload = true;
			}
		}

		public void SetData(IParameterTexture target, int channelId, float value)
		{
			SetData(target, channelId, (byte)(Mathf.Clamp01(value) * 255f));
		}

		public void RegisterMaterial(Material mat)
		{
			if (_propertyId == 0)
			{
				_propertyId = Shader.PropertyToID(_propertyName);
			}
			if ((bool)mat)
			{
				mat.SetTexture(_propertyId, _texture);
			}
		}

		public float GetNormalizedIndex(IParameterTexture target)
		{
			return ((float)target.parameterIndex - 0.5f) / (float)_instanceLimit;
		}

		private void Initialize()
		{
			if (updates == null)
			{
				updates = new List<Action>();
				Canvas.willRenderCanvases += delegate
				{
					int count = updates.Count;
					for (int i = 0; i < count; i++)
					{
						updates[i]();
					}
				};
			}
			if (!_texture)
			{
				bool linear = QualitySettings.activeColorSpace == ColorSpace.Linear;
				_texture = new Texture2D(_channels / 4, _instanceLimit, TextureFormat.RGBA32, false, linear);
				_texture.filterMode = FilterMode.Point;
				_texture.wrapMode = TextureWrapMode.Clamp;
				updates.Add(UpdateParameterTexture);
				_needUpload = true;
			}
		}

		private void UpdateParameterTexture()
		{
			if (_needUpload && (bool)_texture)
			{
				_needUpload = false;
				_texture.LoadRawTextureData(_data);
				_texture.Apply(false, false);
			}
		}
	}
}
