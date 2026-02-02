using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarClearComponent : MonoBehaviour
{
	public enum Group
	{
		horizontal = 0,
		vertical = 1
	}

	[SerializeField]
	private Group group;

	[SerializeField]
	private Image firstImg;

	[SerializeField]
	private int Amount;

	[SerializeField]
	private Sprite[] sprStar = new Sprite[2];

	[SerializeField]
	private float spacingX;

	private List<Image> listStar = new List<Image>();

	public bool lazyLoading;

	private void Awake()
	{
		if (!lazyLoading)
		{
			InstantiateChild();
		}
	}

	private void InstantiateChild()
	{
		Transform parent = firstImg.transform.parent;
		listStar.Add(firstImg);
		Vector2 spacing = GetSpacing();
		for (int i = 1; i < Amount; i++)
		{
			Image image = Object.Instantiate(firstImg, parent);
			image.rectTransform.anchoredPosition += i * spacing;
			listStar.Add(image);
		}
	}

	private Vector2 GetSpacing()
	{
		Group group = this.group;
		if (group == Group.horizontal || group != Group.vertical)
		{
			return new Vector2(spacingX, 0f);
		}
		return new Vector2(0f, spacingX);
	}

	public void SetActiveStar(int p_activeAmount)
	{
		if (listStar.Count == 0)
		{
			InstantiateChild();
		}
		for (int i = 0; i < listStar.Count; i++)
		{
			listStar[i].sprite = ((p_activeAmount > i) ? sprStar[1] : sprStar[0]);
		}
	}
}
