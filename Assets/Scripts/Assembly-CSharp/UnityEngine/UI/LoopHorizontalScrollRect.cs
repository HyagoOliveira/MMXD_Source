namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Loop Horizontal Scroll Rect", 50)]
	[DisallowMultipleComponent]
	public class LoopHorizontalScrollRect : LoopScrollRectSE
	{
		protected override float GetSize(RectTransform item)
		{
			float num = base.contentSpacing;
			if (m_GridLayout != null)
			{
				return num + m_GridLayout.cellSize.x;
			}
			float x = item.rect.size.x;
			if (x == 0f)
			{
				x = item.sizeDelta.x;
				return num + x;
			}
			return num + x;
		}

		protected override float GetDimension(Vector2 vector)
		{
			return 0f - vector.x;
		}

		protected override Vector2 GetVector(float value)
		{
			return new Vector2(0f - value, 0f);
		}

		protected override void Awake()
		{
			base.Awake();
			directionSign = 1;
			GridLayoutGroup component = base.content.GetComponent<GridLayoutGroup>();
			if (component != null && component.constraint != GridLayoutGroup.Constraint.FixedRowCount)
			{
				Debug.LogError("[LoopHorizontalScrollRect] unsupported GridLayoutGroup constraint");
			}
		}

		protected override bool UpdateItems(Bounds viewBounds, Bounds contentBounds)
		{
			bool result = false;
			if (viewBounds.max.x > contentBounds.max.x)
			{
				float num = NewItemAtEnd();
				float num2 = num;
				while (num > 0f && viewBounds.max.x > contentBounds.max.x + num2)
				{
					num = NewItemAtEnd();
					num2 += num;
				}
				if (num2 > 0f)
				{
					result = true;
				}
			}
			else if (viewBounds.max.x < contentBounds.max.x - threshold)
			{
				float num3 = DeleteItemAtEnd();
				float num4 = num3;
				while (num3 > 0f && viewBounds.max.x < contentBounds.max.x - threshold - num4)
				{
					num3 = DeleteItemAtEnd();
					num4 += num3;
				}
				if (num4 > 0f)
				{
					result = true;
				}
			}
			if (viewBounds.min.x < contentBounds.min.x)
			{
				float num5 = NewItemAtStart();
				float num6 = num5;
				while (num5 > 0f && viewBounds.min.x < contentBounds.min.x - num6)
				{
					num5 = NewItemAtStart();
					num6 += num5;
				}
				if (num6 > 0f)
				{
					result = true;
				}
			}
			else if (viewBounds.min.x > contentBounds.min.x + threshold)
			{
				float num7 = DeleteItemAtStart();
				float num8 = num7;
				while (num7 > 0f && viewBounds.min.x > contentBounds.min.x + threshold + num8)
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
