using UnityEngine;
using UnityEngine.UI;

public class CardSkillColor : MonoBehaviour
{
	[SerializeField]
	private Image[] ColorImages;

	private int GetImageIndex(int idx)
	{
		switch (idx)
		{
		case 1:
			return 0;
		case 2:
			return 1;
		case 4:
			return 2;
		case 8:
			return 3;
		case 16:
			return 4;
		default:
			return -1;
		}
	}

	public void SetImage(int idx)
	{
		for (int i = 0; i < ColorImages.Length; i++)
		{
			ColorImages[i].gameObject.SetActive(i == GetImageIndex(idx));
		}
	}
}
