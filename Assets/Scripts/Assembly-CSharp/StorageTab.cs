using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using UnityEngine;

public class StorageTab : MonoBehaviour
{
	[Space]
	[BoxGroup("Color")]
	[SerializeField]
	private Color[] mainColors = new Color[2]
	{
		new Color(0.5372549f, 0.8235294f, 1f, 1f),
		new Color(1f, 0.9490196f, 0f, 1f)
	};

	[BoxGroup("Color")]
	[SerializeField]
	private Color[] mainOutlineColors = new Color[2]
	{
		new Color(16f / 85f, 14f / 51f, 28f / 85f, 1f),
		new Color(0.59607846f, 22f / 51f, 1f / 17f, 1f)
	};

	[BoxGroup("Color")]
	[SerializeField]
	private Color[] subColors = new Color[2]
	{
		new Color(73f / 85f, 79f / 85f, 1f, 1f),
		new Color(1f, 0.9490196f, 0f, 1f)
	};

	[BoxGroup("Color")]
	[SerializeField]
	private Color[] subOutlineColors = new Color[2]
	{
		new Color(0.02745098f, 3f / 85f, 2f / 51f, 1f),
		new Color(3f / 85f, 3f / 85f, 2f / 51f, 1f)
	};

	[Space]
	[BoxGroup("Sprite")]
	[SerializeField]
	private Sprite[] mainSprites = new Sprite[2];

	[BoxGroup("Sprite")]
	[SerializeField]
	private Sprite[] subSprites = new Sprite[2];

	private Vector3[] scales = new Vector3[2]
	{
		new Vector3(1f, 1f, 1f),
		new Vector3(1.1f, 1.1f, 1f)
	};

	[SerializeField]
	private StorageCompUnit unitMain;

	[SerializeField]
	private StorageCompUnit unitSub;

	[SerializeField]
	private RectTransform thisRt;

	[SerializeField]
	private RectTransform subRt;
    [System.Obsolete]
    private CallbackObjs m_cb;

	private List<StorageCompUnit> listChild = new List<StorageCompUnit>();

	private StorageInfo storageInfo;

	public StorageInfo StorageInfo
	{
		get
		{
			return storageInfo;
		}
	}

    [System.Obsolete]
    public void Setup(StorageInfo p_storageInfo, CallbackObj callbackEx)
	{
		this.storageInfo = p_storageInfo;
		if (this.storageInfo.Sub != null)
		{
			StorageInfo[] sub = this.storageInfo.Sub;
			foreach (StorageInfo storageInfo in sub)
			{
				storageInfo.Parent = this.storageInfo;
				StorageCompUnit storageCompUnit = Object.Instantiate(unitSub, subRt.transform);
				storageCompUnit.SetUnit(storageInfo, callbackEx);
				listChild.Add(storageCompUnit);
			}
		}
		unitSub.gameObject.SetActive(false);
		unitMain.SetUnit(this.storageInfo, callbackEx);
		subRt.RebuildLayout();
		thisRt.RebuildLayout();
	}

	public void UpdateTabBtn(StorageInfo p_storageInfo, int subIdx = 0)
	{
		if (storageInfo == p_storageInfo)
		{
			SetChildsActive(true);
			SetMainState(true);
			if (listChild.Count > 0)
			{
				listChild[subIdx].CanPlaySE = false;
				listChild[subIdx].OnClick();
			}
		}
		else if (p_storageInfo.Parent != null && p_storageInfo.Parent == storageInfo)
		{
			for (int i = 0; i < listChild.Count; i++)
			{
				StorageCompUnit unit = listChild[i];
				SetSubState(p_storageInfo == unit.StorageInfo, ref unit);
			}
		}
		else
		{
			SetMainState(false);
			SetChildsActive(false);
		}
		subRt.RebuildLayout();
	}

	private void SetMainState(bool p_isClick)
	{
		int num = (p_isClick ? 1 : 0);
		unitMain.Btn.interactable = !p_isClick;
		unitMain.ImgBg.sprite = mainSprites[num];
		unitMain.Text.color = mainColors[num];
		unitMain.UiShadow.effectColor = mainOutlineColors[num];
		unitMain.Btn.transform.localScale = scales[num];
	}

	private void SetSubState(bool p_isClick, ref StorageCompUnit unit)
	{
		int num = (p_isClick ? 1 : 0);
		unit.Btn.interactable = !p_isClick;
		unit.ImgBg.sprite = subSprites[num];
		unit.Text.color = subColors[num];
		unit.UiShadow.effectColor = subOutlineColors[num];
	}

	private void SetChildsActive(bool p_active)
	{
		foreach (StorageCompUnit item in listChild)
		{
			item.gameObject.SetActive(p_active);
		}
	}

	public void UpdateHint()
	{
		unitMain.UpdateHint();
		for (int i = 0; i < listChild.Count; i++)
		{
			listChild[i].UpdateHint();
		}
	}
}
