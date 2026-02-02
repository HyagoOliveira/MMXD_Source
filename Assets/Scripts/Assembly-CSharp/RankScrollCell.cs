using UnityEngine;
using UnityEngine.UI;

public class RankScrollCell : ScrollIndexCallback
{
	[SerializeField]
	private GameObject GLG;

	[SerializeField]
	private Text RankText;

	[SerializeField]
	private Image[] TabImgs;

	[SerializeField]
	private Image IconImg;

	private int idx;

	private Color32[] colors = new Color32[8]
	{
		new Color32(0, 186, byte.MaxValue, byte.MaxValue),
		new Color32(0, 186, byte.MaxValue, byte.MaxValue),
		new Color32(0, 186, byte.MaxValue, byte.MaxValue),
		new Color32(251, 219, 214, byte.MaxValue),
		new Color32(216, 192, byte.MaxValue, byte.MaxValue),
		new Color32(216, 192, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, 230, 93, byte.MaxValue),
		new Color32(byte.MaxValue, 230, 93, byte.MaxValue)
	};

	private void Start()
	{
	}

	private void Update()
	{
	}

	public override void ScrollCellIndex(int p_idx)
	{
		idx = p_idx;
	}

	public GameObject GetGLG()
	{
		return GLG;
	}

	public void SetRankText(int rank, string str)
	{
		rank = ((rank <= 0) ? 1 : rank);
		for (int i = 0; i < TabImgs.Length; i++)
		{
			bool active = i + 1 == rank;
			TabImgs[i].gameObject.SetActive(active);
		}
		IconImg.color = colors[rank - 1];
		RankText.color = colors[rank - 1];
		RankText.text = str;
	}
}
