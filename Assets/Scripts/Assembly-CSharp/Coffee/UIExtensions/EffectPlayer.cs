using System;
using System.Collections.Generic;
using UnityEngine;

namespace Coffee.UIExtensions
{
	[Serializable]
	public class EffectPlayer
	{
		[Header("Effect Player")]
		[Tooltip("Playing.")]
		public bool play;

		[Tooltip("Initial play delay.")]
		[Range(0f, 10f)]
		public float initialPlayDelay;

		[Tooltip("Duration.")]
		[Range(0.01f, 10f)]
		public float duration = 1f;

		[Tooltip("Loop.")]
		public bool loop;

		[Tooltip("Delay before looping.")]
		[Range(0f, 10f)]
		public float loopDelay;

		[Tooltip("Update mode")]
		public AnimatorUpdateMode updateMode;

		private static List<Action> s_UpdateActions;

		private float _time;

		private Action<float> _callback;

		public void OnEnable(Action<float> callback = null)
		{
			if (s_UpdateActions == null)
			{
				s_UpdateActions = new List<Action>();
				Canvas.willRenderCanvases += delegate
				{
					int count = s_UpdateActions.Count;
					for (int i = 0; i < count; i++)
					{
						s_UpdateActions[i]();
					}
				};
			}
			s_UpdateActions.Add(OnWillRenderCanvases);
			if (play)
			{
				_time = 0f - initialPlayDelay;
			}
			else
			{
				_time = 0f;
			}
			_callback = callback;
		}

		public void OnDisable()
		{
			_callback = null;
			s_UpdateActions.Remove(OnWillRenderCanvases);
		}

		public void Play(bool reset, Action<float> callback = null)
		{
			if (reset)
			{
				_time = 0f;
			}
			play = true;
			if (callback != null)
			{
				_callback = callback;
			}
		}

		public void Stop(bool reset)
		{
			if (reset)
			{
				_time = 0f;
				if (_callback != null)
				{
					_callback(_time);
				}
			}
			play = false;
		}

		private void OnWillRenderCanvases()
		{
			if (play && Application.isPlaying && _callback != null)
			{
				_time += ((updateMode == AnimatorUpdateMode.UnscaledTime) ? Time.unscaledDeltaTime : Time.deltaTime);
				float obj = _time / duration;
				if (duration <= _time)
				{
					play = loop;
					_time = (loop ? (0f - loopDelay) : 0f);
				}
				_callback(obj);
			}
		}
	}
}
