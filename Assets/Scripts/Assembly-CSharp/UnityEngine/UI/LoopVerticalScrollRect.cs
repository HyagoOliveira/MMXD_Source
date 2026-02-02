namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Loop Vertical Scroll Rect", 51)]
	[DisallowMultipleComponent]
	public class LoopVerticalScrollRect : LoopScrollRectSE
	{
		protected override float GetSize(RectTransform item)
		{
			float num = base.contentSpacing;
			if (m_GridLayout != null)
			{
				return num + m_GridLayout.cellSize.y;
			}
			float y = item.rect.size.y;
			if (y == 0f)
			{
				y = item.sizeDelta.y;
				return num + y;
			}
			return num + y;
		}

		protected override float GetDimension(Vector2 vector)
		{
			return vector.y;
		}

		protected override Vector2 GetVector(float value)
		{
			return new Vector2(0f, value);
		}

		protected override void Awake()
		{
			base.Awake();
			directionSign = -1;
			GridLayoutGroup component = base.content.GetComponent<GridLayoutGroup>();
			if (component != null && component.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
			{
				Debug.LogError("[LoopHorizontalScrollRect] unsupported GridLayoutGroup constraint");
			}
		}

		protected override bool UpdateItems(Bounds viewBounds, Bounds contentBounds)
		{
			bool result = false;
			if (viewBounds.min.y < contentBounds.min.y)
			{
				float num = NewItemAtEnd();
				float num2 = num;
				while (num > 0f && viewBounds.min.y < contentBounds.min.y - num2)
				{
					num = NewItemAtEnd();
					num2 += num;
				}
				if (num2 > 0f)
				{
					result = true;
				}
			}
			else if (viewBounds.min.y > contentBounds.min.y + threshold)
			{
				float num3 = DeleteItemAtEnd();
				float num4 = num3;
				while (num3 > 0f && viewBounds.min.y > contentBounds.min.y + threshold + num4)
				{
					num3 = DeleteItemAtEnd();
					num4 += num3;
				}
				if (num4 > 0f)
				{
					result = true;
				}
			}
			if (viewBounds.max.y > contentBounds.max.y)
			{
				float num5 = NewItemAtStart();
				float num6 = num5;
				while (num5 > 0f && viewBounds.max.y > contentBounds.max.y + num6)
				{
					num5 = NewItemAtStart();
					num6 += num5;
				}
				if (num6 > 0f)
				{
					result = true;
				}
			}
			else if (viewBounds.max.y < contentBounds.max.y - threshold)
			{
				float num7 = DeleteItemAtStart();
				float num8 = num7;
				while (num7 > 0f && viewBounds.max.y < contentBounds.max.y - threshold - num8)
				{
					num7 = DeleteItemAtStart();
					num8 += num7;
				}
				if (num8 > 0f)
				{
					result = true;
				}
			}
			return result;
		}
	}
}
