using UnityEngine;
using UnityEngine.UI;

public class RankingItemUI : MonoBehaviour
{
	private int TargetIndex;

	public Text mRank;

	public Text mName;

	public Text mScore;

	public Image[] mBadge;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetTargetIndex(int idx)
	{
		TargetIndex = idx;
	}

	public int GetTargetIndex()
	{
		return TargetIndex;
	}

	public void SetItemData(int rank, string name, int score)
	{
		mRank.text = string.Concat(rank + 1);
		mName.text = name;
		mScore.text = string.Concat(score);
		for (int i = 0; i < mBadge.Length; i++)
		{
			mBadge[i].enabled = false;
			if (rank == i)
			{
				mBadge[i].enabled = true;
			}
		}
	}
}
