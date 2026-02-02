using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	public static class EffectAreaExtensions
	{
		private static readonly Rect rectForCharacter = new Rect(0f, 0f, 1f, 1f);

		private static readonly Vector2[] splitedCharacterPosition = new Vector2[4]
		{
			Vector2.up,
			Vector2.one,
			Vector2.right,
			Vector2.zero
		};

		public static Rect GetEffectArea(this EffectArea area, VertexHelper vh, Rect rectangle, float aspectRatio = -1f)
		{
			Rect result = default(Rect);
			switch (area)
			{
			case EffectArea.RectTransform:
				result = rectangle;
				break;
			case EffectArea.Character:
				result = rectForCharacter;
				break;
			case EffectArea.Fit:
			{
				UIVertex vertex = default(UIVertex);
				float num = float.MaxValue;
				float num2 = float.MaxValue;
				float num3 = float.MinValue;
				float num4 = float.MinValue;
				for (int i = 0; i < vh.currentVertCount; i++)
				{
					vh.PopulateUIVertex(ref vertex, i);
					float x = vertex.position.x;
					float y = vertex.position.y;
					num = Mathf.Min(num, x);
					num2 = Mathf.Min(num2, y);
					num3 = Mathf.Max(num3, x);
					num4 = Mathf.Max(num4, y);
				}
				result.Set(num, num2, num3 - num, num4 - num2);
				break;
			}
			default:
				result = rectangle;
				break;
			}
			if (0f < aspectRatio)
			{
				if (result.width < result.height)
				{
					result.width = result.height * aspectRatio;
				}
				else
				{
					result.height = result.width / aspectRatio;
				}
			}
			return result;
		}

		public static void GetPositionFactor(this EffectArea area, int index, Rect rect, Vector2 position, bool isText, bool isTMPro, out float x, out float y)
		{
			if (isText && area == EffectArea.Character)
			{
				index = (isTMPro ? ((index + 3) % 4) : (index % 4));
				x = splitedCharacterPosition[index].x;
				y = splitedCharacterPosition[index].y;
			}
			else if (area == EffectArea.Fit)
			{
				x = Mathf.Clamp01((position.x - rect.xMin) / rect.width);
				y = Mathf.Clamp01((position.y - rect.yMin) / rect.height);
			}
			else
			{
				x = Mathf.Clamp01(position.x / rect.width + 0.5f);
				y = Mathf.Clamp01(position.y / rect.height + 0.5f);
			}
		}

		public static void GetNormalizedFactor(this EffectArea area, int index, Matrix2x3 matrix, Vector2 position, bool isText, out Vector2 nomalizedPos)
		{
			if (isText && area == EffectArea.Character)
			{
				nomalizedPos = matrix * splitedCharacterPosition[(index + 3) % 4];
			}
			else
			{
				nomalizedPos = matrix * position;
			}
		}
	}
}
