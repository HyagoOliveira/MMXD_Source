using UnityEngine;

namespace Crystal
{
	public class SafeArea : MonoBehaviour
	{
		private RectTransform panel;

		public bool ConformX = true;

		public bool ConformY = true;

		private void Awake()
		{
			panel = GetComponent<RectTransform>();
		}

		public Vector2 ApplySafeArea(Rect p_r)
		{
			if (!base.gameObject.activeSelf)
			{
				return Vector2.zero;
			}
			Rect rect = p_r;
			Vector2Int vector2Int = new Vector2Int(MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenWidth, MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenHeight);
			if (!ConformX)
			{
				rect.x = 0f;
				rect.width = vector2Int.x;
			}
			if (!ConformY)
			{
				rect.y = 0f;
				rect.height = vector2Int.y;
			}
			Vector2 position = rect.position;
			Vector2 anchorMax = rect.position + rect.size;
			position.x /= vector2Int.x;
			position.y /= vector2Int.y;
			anchorMax.x /= vector2Int.x;
			anchorMax.y /= vector2Int.y;
			panel.anchorMin = position;
			panel.anchorMax = anchorMax;
			return new Vector2(rect.x, rect.y);
		}
	}
}
