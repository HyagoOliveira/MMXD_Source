using UnityEngine;
using UnityEngine.UI;

public class VisualTargetMark : PoolBaseObject
{
	[SerializeField]
	private Sprite[] sprites;

	[SerializeField]
	private Image imgArrow;

	public RectTransform rt;

	private int type = -1;

	public void SetType(int p_type)
	{
		if (type != p_type)
		{
			type = p_type;
			imgArrow.sprite = sprites[p_type];
		}
	}

	public override void BackToPool()
	{
		type = -1;
		base.BackToPool();
	}
}
