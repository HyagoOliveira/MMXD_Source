using UnityEngine;
using UnityEngine.UI;

public class RankNameStars : MonoBehaviour
{
	public enum RANK
	{
		DUMMY = 0,
		D = 1,
		C = 2,
		B = 3,
		A = 4,
		S = 5,
		SS = 6
	}

	private RANK currentRank = RANK.D;

	[SerializeField]
	private Image[] rankImages;

	[SerializeField]
	private Text[] nameTexts;

	[SerializeField]
	private Image[] starImages;

	[SerializeField]
	private Image[] flareImages;

	[SerializeField]
	private GameObject sparkleObject;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Setup(RANK rank, string name, int numStars)
	{
		int num = 0;
		Image[] array = rankImages;
		foreach (Image obj in array)
		{
			bool flag = num + 1 == (int)rank;
			obj.gameObject.SetActive(flag);
			nameTexts[num].gameObject.SetActive(flag);
			if (flag)
			{
				nameTexts[num].text = name;
			}
			num++;
		}
		num = 0;
		array = starImages;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(num < numStars);
			if (rank == RANK.S || rank == RANK.SS)
			{
				flareImages[num].gameObject.SetActive(num < numStars);
			}
			else
			{
				flareImages[num].gameObject.SetActive(false);
			}
			num++;
		}
		bool active = rank == RANK.S || rank == RANK.SS;
		if ((bool)sparkleObject)
		{
			sparkleObject.SetActive(active);
		}
	}

	public Vector3 GetStarPosition(int index)
	{
		if (index >= starImages.Length)
		{
			return Vector3.zero;
		}
		return starImages[index].transform.position;
	}
}
