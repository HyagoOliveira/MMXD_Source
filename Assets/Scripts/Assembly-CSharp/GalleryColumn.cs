#define RELEASE
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GalleryColumn : ScrollIndexCallback
{
	public int nCount = 6;

	private const int cellWidth = 270;

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	public override void BackToPool()
	{
		while (base.transform.childCount != 0)
		{
			Object.DestroyImmediate(base.transform.GetChild(0).transform.gameObject);
		}
		base.gameObject.SetActive(false);
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
	}

	public override void ScrollCellIndex(int p_idx)
	{
		base.gameObject.SetActive(true);
		IllustrationUI componentInParent = GetComponentInParent<IllustrationUI>();
		if (componentInParent == null)
		{
			return;
		}
		while (base.transform.childCount != 0)
		{
			Object.DestroyImmediate(base.transform.GetChild(0).transform.gameObject);
		}
		nCount = componentInParent.nColumeCnt;
		int num = p_idx * nCount;
		int num2 = num + nCount;
		int num3 = (int)(componentInParent.m_eCharacter - 1);
		Dictionary<int, GalleryHelper.GalleryCellInfo> dictionary = ManagedSingleton<GalleryHelper>.Instance.GalleryCellInfos[num3];
		if (dictionary.Count == 0)
		{
			Debug.LogWarning("GalleryUI: no cell ENABLED!!");
			return;
		}
		RectTransform component = base.transform.GetComponent<RectTransform>();
		HorizontalLayoutGroup component2 = GetComponent<HorizontalLayoutGroup>();
		if (componentInParent.m_eCharacter == GalleryType.Card)
		{
			component.sizeDelta = new Vector2(component.rect.width, 400f);
			component2.padding.left = 20;
			component2.padding.right = 20;
			component2.spacing = 32f;
			for (int i = num; i < num2 && i < dictionary.Count; i++)
			{
				Object.Instantiate(componentInParent.galleryCardCell, base.transform, false).Setup(dictionary.ElementAt(i).Value);
			}
		}
		else
		{
			component.sizeDelta = new Vector2(component.rect.width, 315f);
			component2.padding.left = 10;
			component2.padding.right = 10;
			component2.spacing = 7f;
			for (int j = num; j < num2 && j < dictionary.Count; j++)
			{
				Object.Instantiate(componentInParent.galleryCell, base.transform, false).Setup(dictionary.ElementAt(j).Value);
			}
		}
	}
}
