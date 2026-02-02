using UnityEngine;
using UnityEngine.UI;

public class RankIconBase : MonoBehaviour
{
	[SerializeField]
	private Image[] RankIconImages;

	[SerializeField]
	private Image[] RankStarImages;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Setup(int rank, int star)
	{
		for (int i = 0; i < RankIconImages.Length; i++)
		{
			bool active = i == rank - 1;
			RankIconImages[i].gameObject.SetActive(active);
		}
		for (int j = 0; j < RankStarImages.Length; j++)
		{
			bool active2 = j < star;
			RankStarImages[j].gameObject.SetActive(active2);
		}
	}
}
