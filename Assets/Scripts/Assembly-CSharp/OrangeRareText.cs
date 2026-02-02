using Coffee.UIExtensions;
using NaughtyAttributes;
using UnityEngine;

public class OrangeRareText : MonoBehaviour
{
	public enum Rare
	{
		DUMMY = 0,
		D = 1,
		C = 2,
		B = 3,
		A = 4,
		S = 5,
		Ss = 6,
		COUNT = 7
	}

	private Rare rare;

	[Required(null)]
	[SerializeField]
	private OrangeText text;

	[Required(null)]
	[SerializeField]
	private UIEffect effect;

	[Required(null)]
	[SerializeField]
	private UIShadow shadow;

	private Color[] colorsText = new Color[7]
	{
		Color.white,
		new Color(0.77254903f, 73f / 85f, 77f / 85f, 1f),
		new Color(47f / 85f, 1f, 74f / 85f, 1f),
		new Color(0.41960785f, 0.8509804f, 1f, 1f),
		new Color(49f / 51f, 31f / 51f, 0.99607843f, 1f),
		new Color(1f, 38f / 51f, 0.4745098f, 1f),
		new Color(1f, 49f / 51f, 22f / 51f, 1f)
	};

	private Color[] colorsBlur = new Color[7]
	{
		Color.white,
		new Color(23f / 85f, 0.5019608f, 53f / 85f, 1f),
		new Color(13f / 85f, 59f / 85f, 52f / 85f, 1f),
		new Color(0.12156863f, 42f / 85f, 0.9137255f, 1f),
		new Color(0.77254903f, 0.050980393f, 1f, 1f),
		new Color(0.83137256f, 0.36078432f, 0.12156863f, 1f),
		new Color(0.827451f, 0.6745098f, 0.2f, 1f)
	};

	public void UpdateaRare(int p_rare)
	{
		if (rare != (Rare)p_rare)
		{
			rare = (Rare)p_rare;
			effect.effectColor = colorsText[p_rare];
			shadow.effectColor = colorsBlur[p_rare];
			text.text = rare.ToString();
		}
	}

	public void Clear()
	{
		rare = Rare.DUMMY;
		text.text = string.Empty;
	}
}
