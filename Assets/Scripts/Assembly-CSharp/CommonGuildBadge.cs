using UnityEngine;

public class CommonGuildBadge : MonoBehaviour
{
	[SerializeField]
	private ImageSpriteSwitcher _badge;

	[SerializeField]
	private ImageColorController _badgeFrame;

	public int BadgeIndex
	{
		get
		{
			return _badge.CurrentIndex;
		}
		private set
		{
			_badge.ChangeImage(value);
		}
	}

	public float BadgeColor
	{
		get
		{
			return _badgeFrame.ColorH;
		}
		private set
		{
		}
	}

	public void Setup(int badgeIndex, float badgeColor, float scale)
	{
		base.transform.localScale = new Vector3(scale, scale, 1f);
		Setup(badgeIndex, badgeColor);
	}

	public void Setup(int badgeIndex, float badgeColor)
	{
		SetBadgeIndex(badgeIndex);
		SetBadgeColor(badgeColor);
	}

	public void SetBadgeIndex(int badgeIndex)
	{
		BadgeIndex = badgeIndex;
	}

	public void SetBadgeColor(float badgeColor)
	{
		BadgeColor = badgeColor;
	}
}
